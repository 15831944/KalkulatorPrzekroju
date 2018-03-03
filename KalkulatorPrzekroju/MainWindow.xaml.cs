﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using OxyPlot;
using OxyPlot.Series;
using Microsoft.Win32;
using System.Globalization;


namespace KalkulatorPrzekroju
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Serializable]
    public partial class MainWindow : Window
    {
        Factors wspolczynniki;
        Steel stal = new Steel(Steel.classes.B500B);
        Section section1,tempSection1;
        Section section2, tempSection2;
        Stirrups stirrups1, tempStir1;
        Stirrups stirrups2, tempStir2;
        MySettings ustawienia;
        MainPlotView diagram_ULS_VN;
        MainPlotView diagram_ULS_MN;
        MainPlotView diagram_SLS_Crack;
        MainPlotView diagram_SLS_Stressess;
        SavedFile thisInstance;
        CreepParams section1crp;
        CreepParams section2crp;
        bool correctData;

        double[][] tabSLS_ConcreteStress;
        double[][] tabSLS_SteelStress;
        double[][] tabVRd1;
        double[][] tabVRd2;
        double[][] tabVRd3;
        double[][] tabVRd4;
        double[][] tabVRdc1;
        double[][] tabSLS_NonCrack;
        double[][] tabSLS_Crack;
        double[][] tab2_ULS;
        double[][] tab1_ULS;
        double[][] tabSLS_Crack_L;
        double[][] tabSLS_Crack_R;

        List<CasePoint> points_MN;
        List<CasePoint> points_VN;
        List<CasePoint> points_SLS_QPR;
        List<CasePoint> points_SLS_CHR;

        string format = "0.###";
        string thisFile = "";
        string defaultTitle = "Concrete Regular Section Designer CRSD";
        string defaultExt = "CRSD files (*.crsd)|*.crdsd|All files (*.*)|*.*";

        public MainWindow()
        {
            InitializeComponent();
            this.Title = defaultTitle;
            wspolczynniki = new Factors(Factors.Settings.zachowane);
            SetControlls();
            ustawienia = new MySettings(Source.zapisane);
            
            //ustawienia = new MySettings(Source.domyslne);
            //ustawienia.SaveToFile();
        }

        private void SetControlls()
        {
            List<double> lista_srednic = LoadBarDiameters();
            foreach (var item in lista_srednic)
            {
                comboBox_diameter_As1_1.Items.Add(item);
                comboBox_diameter_As2_1.Items.Add(item);
                comboBox_diameter_As1_2.Items.Add(item);
                comboBox_diameter_As2_2.Items.Add(item);
                comboBox_diameter_AsStir_1.Items.Add(item);
                comboBox_diameter_AsStir_2.Items.Add(item);
                comboBox_diameter_Circ_1.Items.Add(item);
                comboBox_diameter_Circ_2.Items.Add(item);
            }
            comboBox_diameter_As1_1.SelectedIndex = 4;
            comboBox_diameter_As2_1.SelectedIndex = 4;
            comboBox_diameter_As1_2.SelectedIndex = 4;
            comboBox_diameter_As2_2.SelectedIndex = 4;
            comboBox_diameter_AsStir_1.SelectedIndex = 4;
            comboBox_diameter_AsStir_2.SelectedIndex = 4;
            comboBox_diameter_Circ_1.SelectedIndex = 4;
            comboBox_diameter_Circ_2.SelectedIndex = 4;

            comboBox_As1_spac_no_1.Items.Add("spacing");
            comboBox_As1_spac_no_1.Items.Add("no of bars");
            comboBox_As2_spac_no_1.ItemsSource = comboBox_As1_spac_no_1.Items;
            comboBox_As1_spac_no_2.ItemsSource = comboBox_As1_spac_no_1.Items;
            comboBox_As2_spac_no_2.ItemsSource = comboBox_As1_spac_no_1.Items;

            comboBox_As1_spac_no_1.SelectedIndex = 0;
            comboBox_As2_spac_no_1.SelectedIndex = 0;
            comboBox_As1_spac_no_2.SelectedIndex = 0;
            comboBox_As2_spac_no_2.SelectedIndex = 0;

            for (int i = 0; i <= (int)Concrete.classes.C90_105; i++)
            {
                string name = new Concrete((Concrete.classes)i).Name;
                comboBox_Concrete_1.Items.Add(name);
                comboBox_Concrete_2.Items.Add(name);
            }
            comboBox_Concrete_1.SelectedIndex = 0;
            comboBox_Concrete_2.SelectedIndex = 0;

            foreach (var item in stal.steelNames)
            {
                comboBox_Steel_1.Items.Add(item);
                comboBox_Steel_2.Items.Add(item);
            }
            comboBox_Steel_1.SelectedIndex = 0;
            comboBox_Steel_2.SelectedIndex = 0;

            comboBox_DesignSituation_1.Items.Add("Accidental");
            comboBox_DesignSituation_1.Items.Add("Persistent & Transient");
            comboBox_DesignSituation_2.ItemsSource = comboBox_DesignSituation_1.Items;
            comboBox_DesignSituation_1.SelectedIndex = 1;
            comboBox_DesignSituation_2.SelectedIndex = 1;


        }
        // załadowanie średnic pretow z pliku
        private List<double> LoadBarDiameters()
        {
            List<double> lista = new List<double>();
            try
            {
                using (StreamReader input = new StreamReader(@"data/bar_diameters.txt"))
                {
                    string line;
                    while ((line = input.ReadLine()) != null)
                    {
                    	double diameter;
                        Double.TryParse(line, out diameter);
                        lista.Add(diameter);
                    }
                }
                lista.Sort();

            }
            catch (Exception)
            {
                MessageBox.Show("Nie udało się wczytać pliku.", "Loading failed",
                    MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            return lista;
        }


        // OPROGRAMOWANIE KONTROLEK
        private void ComboBox_As1_spac_no_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_As1_spac_no_1.SelectedIndex == 0)
            {
                label_spac_no_As1_1.Visibility = Visibility.Visible;
            }
            else if (comboBox_As1_spac_no_1.SelectedIndex == 1)
            {
                label_spac_no_As1_1.Visibility = Visibility.Hidden;
            }
            comboBox_As2_spac_no_1.SelectedIndex = comboBox_As1_spac_no_1.SelectedIndex;
        }

        private void ComboBox_As2_spac_no_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_As2_spac_no_1.SelectedIndex == 0)
            {
                label_spac_no_As2_1.Visibility = Visibility.Visible;
            }
            else if (comboBox_As2_spac_no_1.SelectedIndex == 1)
            {
                label_spac_no_As2_1.Visibility = Visibility.Hidden;
            }
            comboBox_As1_spac_no_1.SelectedIndex = comboBox_As2_spac_no_1.SelectedIndex;
        }

        private void ComboBox_As1_spac_no_2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_As1_spac_no_2.SelectedIndex == 0)
            {
                label_spac_no_As1_2.Visibility = Visibility.Visible;
            }
            else if (comboBox_As1_spac_no_2.SelectedIndex == 1)
            {
                label_spac_no_As1_2.Visibility = Visibility.Hidden;
            }
            comboBox_As2_spac_no_2.SelectedIndex = comboBox_As1_spac_no_2.SelectedIndex;
        }

        private void ComboBox_As2_spac_no_2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_As2_spac_no_2.SelectedIndex == 0)
            {
                label_spac_no_As2_2.Visibility = Visibility.Visible;
            }
            else if (comboBox_As2_spac_no_2.SelectedIndex == 1)
            {
                label_spac_no_As2_2.Visibility = Visibility.Hidden;
            }
            comboBox_As1_spac_no_2.SelectedIndex = comboBox_As2_spac_no_2.SelectedIndex;
        }
		        
		private void radioBut_RectCirc_Checked(object sender, RoutedEventArgs e)
		{
			if (radioBut_Circular_sec1.IsChecked == true) {
				grid_Geom_Circ1.Visibility = Visibility.Visible;
				grid_Geom_Rect1.Visibility = Visibility.Hidden;
				grid_Reo_Circ1.Visibility = Visibility.Visible;
				grid_Reo_Rect1.Visibility = Visibility.Hidden;
			} else if (radioBut_Rectangle_sec1.IsChecked == true) {
				grid_Geom_Circ1.Visibility = Visibility.Hidden;
				grid_Geom_Rect1.Visibility = Visibility.Visible;
				grid_Reo_Circ1.Visibility = Visibility.Hidden;
				grid_Reo_Rect1.Visibility = Visibility.Visible;
			} 
			
			if (radioBut_Circular_sec2.IsChecked == true) {
				grid_Geom_Circ2.Visibility = Visibility.Visible;
				grid_Geom_Rect2.Visibility = Visibility.Hidden;
				grid_Reo_Circ2.Visibility = Visibility.Visible;
				grid_Reo_Rect2.Visibility = Visibility.Hidden;
			} else if (radioBut_Rectangle_sec2.IsChecked == true) {
				grid_Geom_Circ2.Visibility = Visibility.Hidden;
				grid_Geom_Rect2.Visibility = Visibility.Visible;
				grid_Reo_Circ2.Visibility = Visibility.Hidden;
				grid_Reo_Rect2.Visibility = Visibility.Visible;
			}
			ShowToUpdate();
		}
		
        //oprogramowanie menu
        private void MenuItemSettingsFactors_Click(object sender, RoutedEventArgs e)
        {
            WindowFactors settingsWindow = new WindowFactors();
            settingsWindow.Show(wspolczynniki);
            button_UpdateGraph.IsEnabled = true;
        }

        private void MenuItemSettingsDisplay_Click(object sender, RoutedEventArgs e)
        {
            Window_DisplaySet displaySettingsWindow = new Window_DisplaySet(ustawienia);
            displaySettingsWindow.Show();
            button_UpdateGraph.IsEnabled = true;
        }

        private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            if (String.Equals(thisFile, ""))
            {
                MenuItem_SaveAs_Click(sender, e);
            }
            else
            {
                SavedFile instance = new SavedFile();
                SaveToInstance(instance);

                using (Stream output = File.Create(thisFile))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(output, instance);
                }
                thisInstance = instance;
                ShowToUpdate();

                MessageBox.Show("Saved!", "Saving", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void MenuItem_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = defaultExt
            };
            string newFile="";
            if (saveFileDialog1.ShowDialog() == true)
            {
                SavedFile instance = new SavedFile();
                SaveToInstance(instance);

                using (Stream output = File.Create(saveFileDialog1.FileName))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(output, instance);
                }
                MessageBox.Show("Saved!", "Saving", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                thisInstance = instance;
                newFile = saveFileDialog1.FileName;
                ShowToUpdate();
            }
            else
                MessageBox.Show("File not saved!", "Saving", MessageBoxButton.OK, MessageBoxImage.Error);
            
            this.Title = defaultTitle + " (" + newFile + ")";
            thisFile = newFile;
        }

        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = defaultExt
            };
            if (openFileDialog1.ShowDialog() == true)
            {
                SavedFile instance;

                using (Stream input = File.Open(openFileDialog1.FileName, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    instance = (SavedFile)formatter.Deserialize(input);
                    this.Title += " (" + openFileDialog1.FileName + ")";
                    thisFile = openFileDialog1.FileName;
                }

                ReadFromInstance(instance);
                Refresh_SLS_Crack_Graph();
                Refresh_SLS_Stresses_Graph();
                Refresh_ULS_MN_Graph();
                Refresh_ULS_VN_Graph();
                dataGrid_ULS_MN.ItemsSource = points_MN;
                dataGrid_ULS_VN.ItemsSource = points_VN;
                dataGrid_SLS_CHR.ItemsSource = points_SLS_CHR;
                dataGrid_SLS_QPR.ItemsSource = points_SLS_QPR;

                thisInstance = instance;
            }
        }
		//koniec oprogramowanie menu
		
        //kontrola wprowadzania danych przez uzytkownika
        private void TextBox_ToDouble_LostFocus(object sender, RoutedEventArgs e)
        {
        	TextBox tb = sender as TextBox;
        	double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
            ShowToUpdate();
            //CorrectData();
        }
        
        private void TextBox_ToInt_LostFocus(object sender, RoutedEventArgs e)
        {
        	TextBox tb = sender as TextBox;
        	int input;
            Int32.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
            ShowToUpdate();
            //CorrectData();
        }

       /*private void TextBox_Cr1_TextChanged(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            int input;
            Int32.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
            //section1crp = null;
            //ShowToUpdate();
        }

        private void TextBox_Cr2_TextChanged(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            int input;
            Int32.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
            //section2crp = null;
            //ShowToUpdate();
        }*/

        private void ListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ShowToUpdate();
        }

        private void DataGrid_ULS_MN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh_ULS_MN_Graph();
        }

        private void DataGrid_SLS_CHR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh_ULS_VN_Graph();
        }

        private void DataGrid_SLS_QPR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh_SLS_Crack_Graph();
        }

        private void DataGrid_ULS_VN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh_SLS_Stresses_Graph();
        }
        ///koniec kontrola wprowadzania danych

        //PRZYCISKI
        private void Button_UpdateGraph_Click(object sender, RoutedEventArgs e)
        {
        	if (CorrectData()) {
        	
            section1 = CreateSection(1);
            section2 = CreateSection(2);
            stirrups1 = CreateStirrups(1);
            stirrups2 = CreateStirrups(2);

            //Thread thr = new Thread(CalcCurves);
            //thr.Start();
            CalcCurves();

            Refresh_ULS_MN_Graph();
            Refresh_ULS_VN_Graph();
            Refresh_SLS_Crack_Graph();
            Refresh_SLS_Stresses_Graph();

            ShowToUpdate();
        	}
        }

        private void Button_Preview_Click(object sender, RoutedEventArgs e)
        {
        	if (CorrectData()) {
        	
            section1 = CreateSection(1);
            section2 = CreateSection(2);
            stirrups1 = CreateStirrups(1);
            stirrups2 = CreateStirrups(2);

            CalcCurves();

            int ti = tabControl1.SelectedIndex;

            Preview window;
            window = new Preview(ti,section1.draw,section2.draw);
        	}
        }

        private void Button_Import_MN_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 0;
            List<CasePoint> temp_points_MN = ReadFileCSV();
            if (temp_points_MN.Count > 0)
            {
                points_MN = temp_points_MN;
                dataGrid_ULS_MN.ItemsSource = points_MN;
            }
            Refresh_ULS_MN_Graph();
        }

        private void Button_Import_VN_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 1;
            List<CasePoint> temp_points_VN = ReadFileCSV();
            if (temp_points_VN.Count > 0)
            {
                points_VN = temp_points_VN;
                dataGrid_ULS_VN.ItemsSource = points_VN;
            }
            Refresh_ULS_VN_Graph();
        }

        private void Button_Import_QPR_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 2;
            List<CasePoint> temp_points_QPR = ReadFileCSV();
            if (temp_points_QPR.Count > 0)
            {
                points_SLS_QPR = temp_points_QPR;
                dataGrid_SLS_QPR.ItemsSource = points_SLS_QPR;
            }
            Refresh_SLS_Crack_Graph();
        }

        private void Button_Import_CHR_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 3;
            List<CasePoint> temp_points_CHR = ReadFileCSV();
            if (temp_points_CHR.Count > 0)
            {
                points_SLS_CHR = temp_points_CHR;
                dataGrid_SLS_CHR.ItemsSource = points_SLS_CHR;
            }
            Refresh_SLS_Stresses_Graph();
        }

        private void Button_Delete_MN_Click(object sender, RoutedEventArgs e)
        {
            if (diagram_ULS_MN != null)
            {
                PlotView_ULS_MN.Model = null;
                diagram_ULS_MN.RemoveSerie("ULS Case");
                PlotView_ULS_MN.Model = diagram_ULS_MN.wykres;
            }
        }

        private void Button_Delete_VN_Click(object sender, RoutedEventArgs e)
        {
            if (diagram_ULS_VN != null)
            {
                PlotView_ULS_VN.Model = null;
                diagram_ULS_VN.RemoveSerie("ULS Case");
                PlotView_ULS_VN.Model = diagram_ULS_VN.wykres;
            }
        }

        private void Button_Delete_QPR_Click(object sender, RoutedEventArgs e)
        {
            if (diagram_SLS_Crack != null)
            {
                PlotView_SLS_Crack.Model = null;
                diagram_SLS_Crack.RemoveSerie("SLS QPR Case");
                PlotView_SLS_Crack.Model = diagram_SLS_Crack.wykres;
            }
        }

        private void Button_Delete_CHR_Click(object sender, RoutedEventArgs e)
        {
            if (diagram_SLS_Crack != null)
            {
                PlotView_SLS_Stresess.Model = null;
                diagram_SLS_Stressess.RemoveSerie("SLS CHR Case");
                PlotView_SLS_Stresess.Model = diagram_SLS_Stressess.wykres;
            }
        }

        private void button_CalcCreep1_Click(object sender, RoutedEventArgs e)
        {
        	if (CorrectData()) {
        	
            section1 = CreateSection(1);
            section2 = CreateSection(2);
            stirrups1 = CreateStirrups(1);
            stirrups2 = CreateStirrups(2);

            CalcCurves();

            CreepWindow creepwin1 = new CreepWindow();
            double creep1;
            Double.TryParse(textBox_creep1.Text, out creep1);
            if(section1crp == null)
            {
                section1crp = new CreepParams(70, 1000, 14, 10000, 0, false, false);
            }
            creepwin1.Show(section1.AcTotal, section1.CurrentConcrete.fcm, creep1, section1crp);
            textBox_creep1.Text = creepwin1.CrCoeff.ToString("F3");
            section1crp = creepwin1.crp;
            ShowToUpdate();	
        	}
        }

        private void button_CalcCreep2_Click(object sender, RoutedEventArgs e)
        {
        	if (CorrectData()) {
        	
            section1 = CreateSection(1);
            section2 = CreateSection(2);
            stirrups1 = CreateStirrups(1);
            stirrups2 = CreateStirrups(2);

            CalcCurves();

            CreepWindow creepwin2 = new CreepWindow();
            double creep2;
            Double.TryParse(textBox_creep2.Text, out creep2);
            if (section2crp == null)
            {
                section1crp = new CreepParams(70, 1000, 14, 10000, 0, false, false);
            }
            creepwin2.Show(section2.AcTotal, section2.CurrentConcrete.fcm, creep2, section2crp);
            textBox_creep2.Text = creepwin2.CrCoeff.ToString("F3");
            section2crp = creepwin2.crp;
            ShowToUpdate();
        	}
        }

        private void button_SaveToPDF_ULS_MN_Click(object sender, RoutedEventArgs e)
        {
            ExportToPDF(diagram_ULS_MN, "EN-1992 ULS Resistance - Curvature Axial Force [kN] / Bending Moment [kNm]");
        }

        private void button_SaveToPDF_ULS_VN_Click(object sender, RoutedEventArgs e)
        {
            ExportToPDF(diagram_ULS_VN, "EN-1992 ULS Resistance - Curvature Axial Force [kN] / Shear Force [kN]");
        }

        private void button_SaveToPDF_SLS_Crack_Click(object sender, RoutedEventArgs e)
        {
            ExportToPDF(diagram_SLS_Crack, "EN-1992 SLS Range - Curvature Axial Force [kN] / Bending Moment [kNm]");
        }

        private void button_SaveToPDF_SLS_Stresses_Click(object sender, RoutedEventArgs e)
        {
            ExportToPDF(diagram_SLS_Stressess, "EN-1992 SLS Range - Curvature Axial Force [kN] / Bending Moment [kNm]");
        }
        
        private void checkBox_ULS_MN_upperRange_Click(object sender, RoutedEventArgs e)
        {
            textBox_ULS_MN_upperRange.IsEnabled = !(bool)checkBox_ULS_MN_upperRange.IsChecked;
        }

        private void checkBox_ULS_MN_lowerRange_Click(object sender, RoutedEventArgs e)
        {
            textBox_ULS_MN_lowerRange.IsEnabled = !(bool)checkBox_ULS_MN_lowerRange.IsChecked;
        }
        
        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            ShowToUpdate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (thisFile != "")
            {
                string lastChar = thisFile.Substring(thisFile.Length - 1);
                if (lastChar != "*")
                {
                    return;
                }
            }

            MessageBoxResult result = MessageBox.Show("Do you want to save your work?", "Saving", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                MenuItem_Save_Click(sender, null);
                return;
            }
            else if (result == MessageBoxResult.No)
            {
                return;
            }

        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            string text = "Thank you for using this application. Hope you like it! \n\nAuthors (algorithms, code and interface):\nTomasz Ślusarczyk \nMarcin Obara\n\n" +
                "In case of any question or willing to contact: tom.slusarczyk@gmail.com \n\nAll rights reserved. \nPoland, Kraków, 3rd Fabruary 2018";
            MessageBox.Show(text, "About...", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        // KONIEC OPROGRAMOWANIA KONTROLEK

        private Section CreateSection(int i)
        {
			Section section;
			if (i == 1) {
				if (radioBut_Rectangle_sec1.IsChecked == true) {
					if (comboBox_As1_spac_no_1.Text == "spacing") {
						section = new RectangleSection(
							new Concrete((Concrete.classes)comboBox_Concrete_1.SelectedIndex),
							new Steel((Steel.classes)comboBox_Steel_1.SelectedIndex),
							Double.Parse(textBox_width_1.Text),
							Double.Parse(textBox_height_1.Text),
							Double.Parse(comboBox_diameter_As1_1.Text),
							Double.Parse(textBox_spac_no_As1_1.Text),
							Double.Parse(textBox_cover_As1_1.Text),
							Double.Parse(comboBox_diameter_As2_1.Text),
							Double.Parse(textBox_spac_no_As2_1.Text),
							Double.Parse(textBox_cover_As2_1.Text),
							CreateStirrups(1)
						);
					} else if (comboBox_As1_spac_no_1.Text == "no of bars") {
						section = new RectangleSection(
							new Concrete((Concrete.classes)comboBox_Concrete_1.SelectedIndex),
							new Steel((Steel.classes)comboBox_Steel_1.SelectedIndex),
							Double.Parse(textBox_width_1.Text),
							Double.Parse(textBox_height_1.Text),
							Double.Parse(comboBox_diameter_As1_1.Text),
							Int32.Parse(textBox_spac_no_As1_1.Text),
							Double.Parse(textBox_cover_As1_1.Text),
							Double.Parse(comboBox_diameter_As2_1.Text),
							Int32.Parse(textBox_spac_no_As2_1.Text),
							Double.Parse(textBox_cover_As2_1.Text),
							CreateStirrups(1)
						);
                    }
                    else
						section = null;
				} else if (radioBut_Circular_sec1.IsChecked == true) {
					section = new CircleSection(
						new Concrete((Concrete.classes)comboBox_Concrete_1.SelectedIndex),
						new Steel((Steel.classes)comboBox_Steel_1.SelectedIndex),
						Double.Parse(textBox_diameter_1.Text),
						Double.Parse(comboBox_diameter_Circ_1.Text),
						Double.Parse(textBox_cover_Circ_1.Text),
						Int32.Parse(textBox_no_Circ_1.Text));
				} else
						section = null;

                double fi1 = 0;
                Double.TryParse(textBox_creep1.Text, out fi1);
                section.SetCreepFactor(fi1, (bool)checkBox_creep1_4concrete.IsChecked, (bool)checkBox_creep1_4steel.IsChecked, (bool)checkBox_creep1_4width.IsChecked);
            }
            else if (i == 2) {
				if (radioBut_Rectangle_sec2.IsChecked == true) {
					if (comboBox_As1_spac_no_2.Text == "spacing") {
						section = new RectangleSection(
							new Concrete((Concrete.classes)comboBox_Concrete_2.SelectedIndex),
							new Steel((Steel.classes)comboBox_Steel_2.SelectedIndex),
							Double.Parse(textBox_width_2.Text),
							Double.Parse(textBox_height_2.Text),
							Double.Parse(comboBox_diameter_As1_2.Text),
							Double.Parse(textBox_spac_no_As1_2.Text),
							Double.Parse(textBox_cover_As1_2.Text),
							Double.Parse(comboBox_diameter_As2_2.Text),
							Double.Parse(textBox_spac_no_As2_2.Text),
							Double.Parse(textBox_cover_As2_2.Text),
							CreateStirrups(2)
						);
					} else if (comboBox_As1_spac_no_2.Text == "no of bars") {
						section = new RectangleSection(
							new Concrete((Concrete.classes)comboBox_Concrete_2.SelectedIndex),
							new Steel((Steel.classes)comboBox_Steel_2.SelectedIndex),
							Double.Parse(textBox_width_2.Text),
							Double.Parse(textBox_height_2.Text),
							Double.Parse(comboBox_diameter_As1_2.Text),
							Int32.Parse(textBox_spac_no_As1_2.Text),
							Double.Parse(textBox_cover_As1_2.Text),
							Double.Parse(comboBox_diameter_As2_2.Text),
							Int32.Parse(textBox_spac_no_As2_2.Text),
							Double.Parse(textBox_cover_As2_2.Text),
							CreateStirrups(2)
						);
					} else
						section = null;
				} else if (radioBut_Circular_sec2.IsChecked == true) {
					section = new CircleSection(
						new Concrete((Concrete.classes)comboBox_Concrete_2.SelectedIndex),
						new Steel((Steel.classes)comboBox_Steel_2.SelectedIndex),
						Double.Parse(textBox_diameter_2.Text),
						Double.Parse(comboBox_diameter_Circ_2.Text),
						Double.Parse(textBox_cover_Circ_2.Text),
						Int32.Parse(textBox_no_Circ_2.Text));
				} else
						section = null;

                double fi2 = 0;
                Double.TryParse(textBox_creep2.Text, out fi2);
                section.SetCreepFactor(fi2, (bool)checkBox_creep2_4concrete.IsChecked, (bool)checkBox_creep2_4steel.IsChecked, (bool)checkBox_creep2_4width.IsChecked);

            } else
				section = null;

            return section;
		}

        private Stirrups CreateStirrups(int i)
        {
            Stirrups stirrups;
            if (i == 1)
            {
                stirrups = new Stirrups(
                    int.Parse(textBox_legs_1.Text),
                    double.Parse(comboBox_diameter_AsStir_1.Text),
                    new Steel((Steel.classes)comboBox_Steel_1.SelectedIndex),
                    double.Parse(textBox_stir_spacing_1.Text),
                    double.Parse(textBox_stir_angle_1.Text)
                    );
            }
            else if (i == 2)
            {
                stirrups = new Stirrups(
                    int.Parse(textBox_legs_2.Text),
                    double.Parse(comboBox_diameter_AsStir_2.Text),
                    new Steel((Steel.classes)comboBox_Steel_2.SelectedIndex),
                    double.Parse(textBox_stir_spacing_2.Text),
                    double.Parse(textBox_stir_angle_2.Text)
                    );
            }
            else
                stirrups = null;
            return stirrups;
        }
        
        private void CalcCurves()
        {
            ULS_Set factors = new ULS_Set(wspolczynniki, (DesignSituation)comboBox_DesignSituation_1.SelectedIndex);

            tab1_ULS = section1.ULS_MN_Curve(factors, wspolczynniki.NoOfPoints);

            tab2_ULS = section2.ULS_MN_Curve(factors, wspolczynniki.NoOfPoints);

            //tabSLS_Crack = section1.SLS_Crack_Curve(wspolczynniki, true);
            tabSLS_Crack_L = section1.SLS_Crack_OneSide_Curve(wspolczynniki, true, true);
            tabSLS_Crack_R = section1.SLS_Crack_OneSide_Curve(wspolczynniki, true, false);

            tabSLS_NonCrack = section1.SLS_Crack_Curve(wspolczynniki, false);
            
            tabVRdc1 = section1.ULS_VRdcN_Curve(factors, wspolczynniki.NoOfPoints);

            tabVRd1 = section1.ULS_VRdN_Curve(factors, 1.0, wspolczynniki.NoOfPoints);

            tabVRd2 = section1.ULS_VRdN_Curve(factors, 0.75, wspolczynniki.NoOfPoints);

            tabVRd3 = section1.ULS_VRdN_Curve(factors, 0.5, wspolczynniki.NoOfPoints);

            tabVRd4 = section1.ULS_VRdN_Curve(factors, 0.25, wspolczynniki.NoOfPoints);

            tabSLS_SteelStress = section1.SLS_StressSteel_Curve(wspolczynniki);

            tabSLS_ConcreteStress = section1.SLS_StressConcrete_Curve(wspolczynniki);

        }

        private void Refresh_ULS_MN_Graph()
        {
            PlotView_ULS_MN.Model = null;

            diagram_ULS_MN = new MainPlotView();

            if (tab1_ULS != null)
            {
                diagram_ULS_MN.RemoveSerie("Section 1");
                diagram_ULS_MN.AddLineSerie(tab1_ULS, "Section 1", ustawienia.ULSMN_Section1LineColor.GetMedia(), ustawienia.ULSMN_Section1LineWeight);
            }

            if (tab2_ULS != null)
            {
                diagram_ULS_MN.RemoveSerie("Section 2");
                diagram_ULS_MN.AddLineSerie(tab2_ULS, "Section 2", ustawienia.ULSMN_Section2LineColor.GetMedia(), ustawienia.ULSMN_Section2LineWeight);
            }

            if (points_MN != null)
            {
                diagram_ULS_MN.RemoveSerie("ULS Case");
                diagram_ULS_MN.AddPointSerie(points_MN, "ULS Case", ustawienia.ULSMN_DataPointColor.GetMedia(), ustawienia.ULSMN_DataPointWeight);
            }
            PlotView_ULS_MN.Model = diagram_ULS_MN.wykres;
            button_SaveToPDF_ULS_MN.Visibility = Visibility.Visible;
        }

        private void Refresh_ULS_VN_Graph()
        {
            PlotView_ULS_VN.Model = null;

            diagram_ULS_VN = new MainPlotView();

            if (tabVRdc1 != null)
            {
                diagram_ULS_VN.RemoveSerie("Section 1 - VRd.c");
                diagram_ULS_VN.AddLineSerie(tabVRdc1, "Section 1 - VRd.c", ustawienia.ULSVN_VrdcLineColor.GetMedia(), ustawienia.ULSVN_VrdcLineWeight);
            }

            Color thisColor = ustawienia.ULSVN_VrdLineColor.GetMedia();

            if (tabVRd1 != null)
            {
                diagram_ULS_VN.RemoveSerie("Section 1 - VRd.s - 100% MRd");
                diagram_ULS_VN.AddLineSerie(tabVRd1, "Section 1 - VRd.s - 100% MRd", ustawienia.ULSVN_VrdLineColor.GetMedia(), ustawienia.ULSVN_VrdLineWeight);
            }

            if (tabVRd2 != null)
            {
                diagram_ULS_VN.RemoveSerie("Section 1 - VRd.s - 75% MRd");
                float partR = thisColor.ScR * 0.75f;
                float partG = thisColor.ScG * 0.75f;
                float partB = thisColor.ScB * 0.75f;
                Color myColor = Color.FromScRgb(thisColor.ScA, partR, partG, partB);
                diagram_ULS_VN.AddLineSerie(tabVRd2, "Section 1 - VRd.s - 75% MRd", myColor, ustawienia.ULSVN_VrdLineWeight);
            }

            if (tabVRd3 != null)
            {
                diagram_ULS_VN.RemoveSerie("Section 1 - VRd.s - 50% MRd");
                float partR = thisColor.ScR * 0.5f;
                float partG = thisColor.ScG * 0.5f;
                float partB = thisColor.ScB * 0.5f;
                Color myColor = Color.FromScRgb(thisColor.ScA, partR, partG, partB);
                diagram_ULS_VN.AddLineSerie(tabVRd3, "Section 1 - VRd.s - 50% MRd", myColor, ustawienia.ULSVN_VrdLineWeight);
            }

            if (tabVRd4 != null)
            {
                diagram_ULS_VN.RemoveSerie("Section 1 - VRd.s - 25% MRd");
                float partR = thisColor.ScR * 0.25f;
                float partG = thisColor.ScG * 0.25f;
                float partB = thisColor.ScB * 0.25f;
                Color myColor = Color.FromScRgb(thisColor.ScA, partR, partG, partB);
                diagram_ULS_VN.AddLineSerie(tabVRd4, "Section 1 - VRd.s - 25% MRd", myColor, ustawienia.ULSVN_VrdLineWeight);
            }

            if (points_VN != null)
            {
                diagram_ULS_VN.RemoveSerie("ULS Case");
                diagram_ULS_VN.AddPointSerie(points_VN, "ULS Case", ustawienia.ULSVN_DataPointColor.GetMedia(), ustawienia.ULSVN_DataPointWeight);
            }
            PlotView_ULS_VN.Model = diagram_ULS_VN.wykres;
            button_SaveToPDF_ULS_VN.Visibility = Visibility.Visible;
        }

        private void Refresh_SLS_Crack_Graph()
        {
            PlotView_SLS_Crack.Model = null;

            diagram_SLS_Crack = new MainPlotView();

            if (tabSLS_NonCrack != null)
            {
                diagram_SLS_Crack.RemoveSerie("Section 1 - non-cracked");
                diagram_SLS_Crack.AddLineSerie(tabSLS_NonCrack, "Section 1 - non-cracked", ustawienia.SLS_Crack_NonCracked_LineColor.GetMedia(), ustawienia.SLS_Crack_NonCracked_LineWeight);
            }

            if (tabSLS_Crack != null)
            {
                //diagram_SLS_Crack.RemoveSerie("Section 1 - w.max = " + wspolczynniki.Crack_wklim + " mm");
                //diagram_SLS_Crack.AddLineSerie(tabSLS_Crack, "Section 1 - w.max = " + wspolczynniki.Crack_wklim + " mm", ustawienia.SLS_Crack_Cracked_LineColor.GetMedia(), ustawienia.SLS_Crack_Cracked_LineWeight);
            }

            if (tabSLS_Crack_R != null && tabSLS_Crack_L != null)
            {
                diagram_SLS_Crack.RemoveSerie("Section 1 bottom - wk.lim = " + wspolczynniki.Crack_wklim + " mm");
                diagram_SLS_Crack.RemoveSerie("Section 1 top - wk.lim = " + wspolczynniki.Crack_wklim + " mm");
                diagram_SLS_Crack.AddLineSerie(tabSLS_Crack_R, "Section 1 bottom - wk.lim = " + wspolczynniki.Crack_wklim + " mm", ustawienia.SLS_Crack_Cracked_Right_LineColor.GetMedia(), ustawienia.SLS_Crack_Cracked_LineWeight);
                diagram_SLS_Crack.AddLineSerie(tabSLS_Crack_L, "Section 1 top - wk.lim = " + wspolczynniki.Crack_wklim + " mm", ustawienia.SLS_Crack_Cracked_Left_LineColor.GetMedia(), ustawienia.SLS_Crack_Cracked_LineWeight);
            }

            if (points_SLS_QPR != null)
            {
                diagram_SLS_Crack.RemoveSerie("SLS QPR Case");
                diagram_SLS_Crack.AddPointSerie(points_SLS_QPR, "SLS QPR Case", ustawienia.SLS_Crack_DataPointColor.GetMedia(), ustawienia.SLS_Crack_DataPointWeight);
            }
            PlotView_SLS_Crack.Model = diagram_SLS_Crack.wykres;
            button_SaveToPDF_SLS_Crack.Visibility = Visibility.Visible;
        }

        private void Refresh_SLS_Stresses_Graph()
        {
            PlotView_SLS_Stresess.Model = null;

            diagram_SLS_Stressess = new MainPlotView();

            if (tabSLS_ConcreteStress != null)
            {
                diagram_SLS_Stressess.RemoveSerie("Section 1 - Concrete stress");
                diagram_SLS_Stressess.AddLineSerie(tabSLS_ConcreteStress, "Section 1 - Concrete stress", ustawienia.SLS_ConcreteStress_LineColor.GetMedia(), ustawienia.SLS_ConcreteStress_LineWeight);
            }

            if (tabSLS_SteelStress != null)
            {
                diagram_SLS_Stressess.RemoveSerie("Section 1 - Steel stress");
                diagram_SLS_Stressess.AddLineSerie(tabSLS_SteelStress, "Section 1 - Steel stress", ustawienia.SLS_SteelStress_LineColor.GetMedia(), ustawienia.SLS_SteelStress_LineWeight);
            }
            if (points_SLS_CHR != null)
            {
                diagram_SLS_Stressess.RemoveSerie("SLS CHR Case");
                diagram_SLS_Stressess.AddPointSerie(points_SLS_CHR, "SLS CHR Case", ustawienia.SLS_Stress_DataPointColor.GetMedia(), ustawienia.SLS_Stress_DataPointWeight);
            }
            PlotView_SLS_Stresess.Model = diagram_SLS_Stressess.wykres;
            button_SaveToPDF_SLS_Stresses.Visibility = Visibility.Visible;
        }

        private List<CasePoint> ReadFileCSV()
        {
            List<CasePoint> taLista = new List<CasePoint>();
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "Text files (.txt)|*.txt|CSV Files (.csv)|*.csv",
                FilterIndex = 2,
                FileName = "*.csv"
            };

            string path = "";

            if ((bool)openFileDialog1.ShowDialog())
            {
                path = openFileDialog1.FileName;

                try
                {
                    string separator = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
                    char columnSeparator;

                    if (String.Equals(separator, "."))
                    {
                        columnSeparator = ',';
                    }
                    else
                    {
                        columnSeparator = ';';
                    }

                    string line;
                    int counter = 0;
                    StreamReader file = new StreamReader(@path);
                    while ((line = file.ReadLine()) != null)
                    {
                        counter++;

                        if (counter != 1)
                        {
                            string[] dataLine;
                            dataLine = line.Split(new char[] { columnSeparator });
                            taLista.Add(new CasePoint(counter - 1, Double.Parse(dataLine[0]), Double.Parse(dataLine[1])));
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Cannot load file!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return taLista;
        }

        private void ReadFromInstance(SavedFile instance)
        {
            section1 = instance.section1;
            section2 = instance.section2;

            stirrups1 = instance.stirrups1;
            stirrups2 = instance.stirrups2;

            textBox_height_1.Text = instance.section1_h;
            textBox_height_2.Text = instance.section2_h;
            textBox_width_1.Text = instance.section1_b;
            textBox_width_2.Text = instance.section2_b;
            textBox_cover_As1_1.Text = instance.section1_c1;
            textBox_cover_As2_1.Text = instance.section1_c2;
            textBox_cover_As1_2.Text = instance.section2_c1;
            textBox_cover_As2_2.Text = instance.section2_c2;

            textBox_diameter_1.Text = instance.section1_diameter;
            textBox_diameter_2.Text = instance.section2_diameter;
            textBox_cover_Circ_1.Text = instance.section1_cover;
            textBox_cover_Circ_2.Text = instance.section2_cover;

            comboBox_diameter_Circ_1.SelectedIndex = instance.section1_diameterBars;
            comboBox_diameter_Circ_2.SelectedIndex = instance.section2_diameterBars;
            textBox_no_Circ_1.Text = instance.section1_noOfBars;
            textBox_no_Circ_2.Text = instance.section2_noOfBars;

            comboBox_diameter_As1_1.SelectedIndex = instance.diameter_As1_1;
            comboBox_diameter_As2_1.SelectedIndex = instance.diameter_As2_1;
            comboBox_diameter_As1_2.SelectedIndex = instance.diameter_As1_2;
            comboBox_diameter_As2_2.SelectedIndex = instance.diameter_As2_2;
            comboBox_As1_spac_no_1.SelectedIndex = instance.section1_As1_noOfBars;
            comboBox_As2_spac_no_1.SelectedIndex = instance.section1_As2_noOfBars;
            comboBox_As1_spac_no_2.SelectedIndex = instance.section2_As1_noOfBars;
            comboBox_As2_spac_no_2.SelectedIndex = instance.section2_As2_noOfBars;
            textBox_spac_no_As1_1.Text = instance.spac_no_As1_1;
            textBox_spac_no_As2_1.Text = instance.spac_no_As2_1;
            textBox_spac_no_As1_2.Text = instance.spac_no_As1_2;
            textBox_spac_no_As2_2.Text = instance.spac_no_As2_2;

            comboBox_Concrete_1.SelectedIndex = instance.concrete1;
            comboBox_Concrete_2.SelectedIndex = instance.concrete2;
            comboBox_Steel_1.SelectedIndex = instance.steel1;
            comboBox_Steel_2.SelectedIndex = instance.steel2;
            comboBox_DesignSituation_1.SelectedIndex = instance.section1DS;
            comboBox_DesignSituation_2.SelectedIndex = instance.section2DS;

            comboBox_diameter_AsStir_1.SelectedIndex = instance.diameter_stir_s1;
            comboBox_diameter_AsStir_2.SelectedIndex = instance.diameter_stir_s2;
            textBox_legs_1.Text = instance.legs_stir_s1;
            textBox_legs_2.Text = instance.legs_stir_s2;
            textBox_stir_spacing_1.Text = instance.spacing_stir_s1;
            textBox_stir_spacing_2.Text = instance.spacing_stir_s2;
            textBox_stir_angle_1.Text = instance.angle_stir_s1;
            textBox_stir_angle_2.Text = instance.angle_stir_s2;

            tabSLS_ConcreteStress = instance.tabSLS_ConcreteStress;
            tabSLS_SteelStress = instance.tabSLS_SteelStress;
            tabVRd1 = instance.tabVRd1;
            tabVRd2 = instance.tabVRd2;
            tabVRd3 = instance.tabVRd3;
            tabVRd4 = instance.tabVRd4;
            tabVRdc1 = instance.tabVRdc1;
            tabSLS_NonCrack = instance.tabSLS_NonCrack;
            tabSLS_Crack = instance.tabSLS_Crack;
            tab1_ULS = instance.tab1_ULS;
            tab2_ULS = instance.tab2_ULS;

            points_MN = instance.points_MN;
            points_VN = instance.points_VN;
            points_SLS_QPR = instance.points_SLS_QPR;
            points_SLS_CHR = instance.points_SLS_CHR;

            textBox_creep1.Text = instance.creep1.ToString();
            textBox_creep2.Text = instance.creep2.ToString();

            checkBox_creep1_4concrete.IsChecked = instance.consider4concrete1;
            checkBox_creep1_4steel.IsChecked = instance.consider4steel1;
            checkBox_creep1_4width.IsChecked = instance.consider4crack1;
            checkBox_creep2_4concrete.IsChecked = instance.consider4concrete2;
            checkBox_creep2_4steel.IsChecked = instance.consider4steel2;
            checkBox_creep2_4width.IsChecked = instance.consider4crack2;
        }
        
        private void SaveToInstance(SavedFile instance)
        {
            instance.section1 = section1;
            instance.section2 = section2;

            instance.stirrups1 = stirrups1;
            instance.stirrups2 = stirrups2;

            instance.section1_h = textBox_height_1.Text;
            instance.section2_h = textBox_height_2.Text;
            instance.section1_b = textBox_width_1.Text;
            instance.section2_b = textBox_width_1.Text;
            instance.section1_c1 = textBox_cover_As1_1.Text;
            instance.section1_c2 = textBox_cover_As2_1.Text;
            instance.section2_c1 = textBox_cover_As1_2.Text;
            instance.section2_c2 = textBox_cover_As2_2.Text;

            instance.section1_diameter = textBox_diameter_1.Text;
            instance.section2_diameter = textBox_diameter_2.Text;
            instance.section1_cover = textBox_cover_Circ_1.Text;
            instance.section2_cover = textBox_cover_Circ_2.Text;

            instance.section1_diameterBars = comboBox_diameter_Circ_1.SelectedIndex;
            instance.section2_diameterBars = comboBox_diameter_Circ_2.SelectedIndex;
            instance.section1_noOfBars = textBox_no_Circ_1.Text;
            instance.section2_noOfBars = textBox_no_Circ_2.Text;

            instance.diameter_As1_1 = comboBox_diameter_As1_1.SelectedIndex;
            instance.diameter_As2_1 = comboBox_diameter_As2_1.SelectedIndex;
            instance.diameter_As1_2 = comboBox_diameter_As1_2.SelectedIndex;
            instance.diameter_As2_2 = comboBox_diameter_As2_2.SelectedIndex;
            instance.section1_As1_noOfBars = comboBox_As1_spac_no_1.SelectedIndex;
            instance.section1_As2_noOfBars = comboBox_As2_spac_no_1.SelectedIndex;
            instance.section2_As1_noOfBars = comboBox_As1_spac_no_2.SelectedIndex;
            instance.section2_As2_noOfBars = comboBox_As2_spac_no_2.SelectedIndex;
            instance.spac_no_As1_1 = textBox_spac_no_As1_1.Text;
            instance.spac_no_As2_1 = textBox_spac_no_As1_2.Text;
            instance.spac_no_As1_2 = textBox_spac_no_As2_1.Text;
            instance.spac_no_As2_2 = textBox_spac_no_As2_2.Text;

            instance.concrete1 = comboBox_Concrete_1.SelectedIndex;
            instance.concrete2 = comboBox_Concrete_2.SelectedIndex;

            instance.steel1 = comboBox_Steel_1.SelectedIndex;
            instance.steel2 = comboBox_Steel_2.SelectedIndex;

            instance.section1DS = comboBox_DesignSituation_1.SelectedIndex;
            instance.section2DS = comboBox_DesignSituation_2.SelectedIndex;

            instance.diameter_stir_s1 = comboBox_diameter_AsStir_1.SelectedIndex;
            instance.diameter_stir_s2 = comboBox_diameter_AsStir_2.SelectedIndex;
            instance.legs_stir_s1 = textBox_legs_1.Text;
            instance.legs_stir_s2 = textBox_legs_2.Text;
            instance.spacing_stir_s1 = textBox_stir_spacing_1.Text;
            instance.spacing_stir_s2 = textBox_stir_spacing_2.Text;
            instance.angle_stir_s1 = textBox_stir_angle_1.Text;
            instance.angle_stir_s2 = textBox_stir_angle_2.Text;

            instance.tabSLS_ConcreteStress = tabSLS_ConcreteStress;
            instance.tabSLS_SteelStress = tabSLS_SteelStress;
            instance.tabVRd1 = tabVRd1;
            instance.tabVRd2 = tabVRd2;
            instance.tabVRd3 = tabVRd3;
            instance.tabVRd4 = tabVRd4;
            instance.tabVRdc1 = tabVRdc1;
            instance.tabSLS_NonCrack = tabSLS_NonCrack;
            instance.tabSLS_Crack = tabSLS_Crack;
            instance.tab1_ULS = tab1_ULS;
            instance.tab2_ULS = tab2_ULS;

            instance.points_MN = points_MN;
            instance.points_SLS_CHR = points_SLS_CHR;
            instance.points_SLS_QPR = points_SLS_QPR;
            instance.points_VN = points_VN;

            instance.creep1 = textBox_creep1.Text;
            instance.creep2 = textBox_creep2.Text;

            instance.consider4concrete1 = (bool)checkBox_creep1_4concrete.IsChecked;
            instance.consider4steel1 = (bool)checkBox_creep1_4steel.IsChecked;
            instance.consider4crack1 = (bool)checkBox_creep1_4width.IsChecked;
            instance.consider4concrete2 = (bool)checkBox_creep2_4concrete.IsChecked;
            instance.consider4steel2 = (bool)checkBox_creep2_4steel.IsChecked;
            instance.consider4crack2 = (bool)checkBox_creep2_4width.IsChecked;
        }

        private bool GraphIsUpToDate()
        {
			tempSection1 = CreateSection(1);
			tempSection2 = CreateSection(2);
			tempStir1 = CreateStirrups(1);
			tempStir2 = CreateStirrups(2);
			try {
				if (Equals(tempSection1, section1) &&
				    Equals(tempSection2, section2) &&
				    Equals(tempStir1, stirrups1) &&
				    Equals(tempStir2, stirrups2)) {
					return true;
				}
				return false;
				
			} catch (Exception) {
            	
				return false;
			}
		}
        
        private void ShowToUpdate()
        {
            if (GraphIsUpToDate())
                button_UpdateGraph.IsEnabled = false;
            else
                button_UpdateGraph.IsEnabled = true;

            if (thisInstance != null)
            {
            	try {
                if (Equals(thisInstance.section1, tempSection1) &&
                    Equals(thisInstance.section2, tempSection2) &&
                    Equals(thisInstance.stirrups1, tempStir1) &&
                    Equals(thisInstance.stirrups2, tempStir2) &&
                    Equals(thisInstance.points_MN, points_MN) &&
                    Equals(thisInstance.points_SLS_CHR, points_SLS_CHR) &&
                    Equals(thisInstance.points_SLS_QPR, points_SLS_QPR) &&
                    Equals(thisInstance.points_VN, points_VN)
                    )
                {
                    Title = defaultTitle + " (" + thisFile + ")";
                }
                else
                    Title = defaultTitle + " (" + thisFile + ")" + " *";
            		
            	} catch (Exception) {
            		Title = defaultTitle + " (" + thisFile + ")" + " *";
            	}
            }
        }

        private void ExportToPDF(MainPlotView model, string heading)
        {
            if (model.wykres != null)
            {
                model.wykres.Title = heading;

                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
                };

                if (saveFileDialog1.ShowDialog() == true)
                {
                    using (var stream = File.Create(saveFileDialog1.FileName))
                    {
                        double factor = 1.4;
                        var pdfExporter = new PdfExporter { Width = factor * model.wykres.Width, Height = factor * model.wykres.Height };
                        pdfExporter.Export(model.wykres, stream);

                        Process.Start(saveFileDialog1.FileName);
                    }
                }
                model.wykres.Title = null;
            }
            else
                MessageBox.Show("No picture to save!", "Saving as PDF", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        private bool CorrectData(){
        	if (tabControl1.SelectedIndex == 0) {
        		if(radioBut_Rectangle_sec1.IsChecked == true) {
        			double h,b,c1,c2,fi1,fi2;
        			Double.TryParse(textBox_height_1.Text, out h);
        			Double.TryParse(textBox_width_1.Text, out b);
        			Double.TryParse(textBox_cover_As1_1.Text, out c1);
        			Double.TryParse(textBox_cover_As2_1.Text, out c2);
        			Double.TryParse(comboBox_diameter_As1_1.Text, out fi1);
        			Double.TryParse(comboBox_diameter_As2_1.Text, out fi2);
        			
        			if (h < (c1+c2+fi1+fi2) || b< (2*Math.Max(c1,c2)+Math.Max(fi1,fi2))) {
        				MessageBox.Show("Incorrect section geometry", "Section 1", MessageBoxButton.OK, MessageBoxImage.Error);
        				return false;
        			}
        		}
        		if (radioBut_Circular_sec1.IsChecked == true) {
        			double d,c,fi;
        			Double.TryParse(textBox_diameter_1.Text, out d);
        			Double.TryParse(textBox_cover_Circ_1.Text, out c);
        			Double.TryParse(comboBox_diameter_Circ_1.Text, out fi);
        			
        			if (d < (c+fi)) {
        				MessageBox.Show("Incorrect section geometry", "Section 1", MessageBoxButton.OK, MessageBoxImage.Error);
        				return false;
        			}
        		}

        	}
        	
        	 if (tabControl1.SelectedIndex == 1) {
        		if(radioBut_Rectangle_sec2.IsChecked == true) {
        			double h,b,c1,c2,fi1,fi2;
        			Double.TryParse(textBox_height_2.Text, out h);
        			Double.TryParse(textBox_width_2.Text, out b);
        			Double.TryParse(textBox_cover_As1_2.Text, out c1);
        			Double.TryParse(textBox_cover_As2_2.Text, out c2);
        			Double.TryParse(comboBox_diameter_As1_2.Text, out fi1);
        			Double.TryParse(comboBox_diameter_As2_2.Text, out fi2);
        			
        			if (h < (c1+c2+fi1+fi2) || b< (2*Math.Max(c1,c2)+Math.Max(fi1,fi2))) {
        				MessageBox.Show("Incorrect section geometry", "Section 2", MessageBoxButton.OK, MessageBoxImage.Error);
        				return false;
        			}
        		}
        		if (radioBut_Circular_sec1.IsChecked == true) {
        			double d,c,fi;
        			Double.TryParse(textBox_diameter_2.Text, out d);
        			Double.TryParse(textBox_cover_Circ_2.Text, out c);
        			Double.TryParse(comboBox_diameter_Circ_2.Text, out fi);
        			
        			if (d < (c+fi)) {
        				MessageBox.Show("Incorrect section geometry", "Section 2", MessageBoxButton.OK, MessageBoxImage.Error);
        				return false;
        			}
        		}

        	}
        	return true;
        }

    }
}
