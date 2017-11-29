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
        Section section1;
        Section section2;
        Stirrups stirrups1;
        Stirrups stirrups2;
        MySettings ustawienia;
        MainPlotView diagram_ULS_VN;
        MainPlotView diagram_ULS_MN;
        MainPlotView diagram_SLS_Crack;
        MainPlotView diagram_SLS_Stressess;

        double[][] tabSLS_ConcreteStress;
        double[][] tabSLS_SteelStress;
        double[][] tabVRd1;
        double[][] tabVRdc1;
        double[][] tabSLS_NonCrack;
        double[][] tabSLS_Crack;
        double[][] tab2_ULS;
        double[][] tab1_ULS;

        List<CasePoint> points_MN;
        List<CasePoint> points_VN;
        List<CasePoint> points_SLS_QPR;
        List<CasePoint> points_SLS_CHR;

        string format = "0.##";

        public MainWindow()
        {
            InitializeComponent();
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
            }
            comboBox_diameter_As1_1.SelectedIndex = 4;
            comboBox_diameter_As2_1.SelectedIndex = 4;
            comboBox_diameter_As1_2.SelectedIndex = 4;
            comboBox_diameter_As2_2.SelectedIndex = 4;
            comboBox_diameter_AsStir_1.SelectedIndex = 4;
            comboBox_diameter_AsStir_2.SelectedIndex = 4;

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


        /// OPROGRAMOWANIE KONTROLEK
        private void comboBox_As1_spac_no_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_As1_spac_no_1.SelectedIndex == 0)
            {
                label_spac_no_As1_1.Visibility = Visibility.Visible;
            }
            else if (comboBox_As1_spac_no_1.SelectedIndex == 1)
            {
                label_spac_no_As1_1.Visibility = Visibility.Hidden;
            }
        }

        private void comboBox_As2_spac_no_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_As2_spac_no_1.SelectedIndex == 0)
            {
                label_spac_no_As2_1.Visibility = Visibility.Visible;
            }
            else if (comboBox_As2_spac_no_1.SelectedIndex == 1)
            {
                label_spac_no_As2_1.Visibility = Visibility.Hidden;
            }
        }

        private void comboBox_As1_spac_no_2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_As1_spac_no_2.SelectedIndex == 0)
            {
                label_spac_no_As1_2.Visibility = Visibility.Visible;
            }
            else if (comboBox_As1_spac_no_2.SelectedIndex == 1)
            {
                label_spac_no_As1_2.Visibility = Visibility.Hidden;
            }
        }

        private void comboBox_As2_spac_no_2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_As2_spac_no_2.SelectedIndex == 0)
            {
                label_spac_no_As2_2.Visibility = Visibility.Visible;
            }
            else if (comboBox_As2_spac_no_2.SelectedIndex == 1)
            {
                label_spac_no_As2_2.Visibility = Visibility.Hidden;
            }
        }

        //oprogramowanie menu
        private void MenuItemSettingsFactors_Click(object sender, RoutedEventArgs e)
        {
            WindowFactors settingsWindow = new WindowFactors();
            settingsWindow.Show(wspolczynniki);
        }

        private void MenuItemSettingsDisplay_Click(object sender, RoutedEventArgs e)
        {
            Window_DisplaySet displaySettingsWindow = new Window_DisplaySet(ustawienia);
            displaySettingsWindow.Show();
        }

        private void menuItem_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void menuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            if (saveFileDialog1.ShowDialog() == true)
            {
                using (Stream output = File.Create(saveFileDialog1.FileName))
                {
                    //BinaryFormatter formatter = new BinaryFormatter();
                    //formatter.Serialize(output, this);
                }
            }
        }

        ///kontrola wprowadzania danych przez uzytkownika
        private void textBox_width_1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_width_1;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_height_1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_height_1;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_cover_As1_1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_cover_As1_1;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_cover_As2_1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_cover_As2_1;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_spac_no_As1_1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_spac_no_As1_1;
            if (comboBox_As1_spac_no_1.SelectedIndex == 0)
            {
                double input;
                Double.TryParse(tb.Text, out input);
                tb.Text = input.ToString(format);
            }
            else if (comboBox_As1_spac_no_1.SelectedIndex == 1)
            {
                int input;
                Int32.TryParse(tb.Text, out input);
                tb.Text = input.ToString(format);
            }

        }

        private void textBox_spac_no_As2_1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_spac_no_As2_1;
            if (comboBox_As2_spac_no_1.SelectedIndex == 0)
            {
                double input;
                Double.TryParse(tb.Text, out input);
                tb.Text = input.ToString(format);
            }
            else if (comboBox_As2_spac_no_1.SelectedIndex == 1)
            {
                int input;
                Int32.TryParse(tb.Text, out input);
                tb.Text = input.ToString(format);
            }
        }

        private void textBox_width_2_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_width_2;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_height_2_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_height_2;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_cover_As1_2_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_cover_As1_2;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_cover_As2_2_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_cover_As2_2;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_spac_no_As1_2_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_spac_no_As1_2;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_spac_no_As2_2_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_spac_no_As2_2;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_legs_2_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_legs_2;
            int input;
            Int32.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_stir_spacing_2_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_stir_spacing_2;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_stir_angle_2_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_stir_angle_2;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_legs_1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_legs_1;
            int input;
            Int32.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_stir_spacing_1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_stir_spacing_1;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_stir_angle_1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_stir_angle_1;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_RH_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_RH;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_u_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_u;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_Cst_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_Cst;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }

        private void textBox_Cse_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = textBox_Cse;
            double input;
            Double.TryParse(tb.Text, out input);
            tb.Text = input.ToString(format);
        }
        
        private void dataGrid_ULS_MN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh_ULS_MN_Graph();
        }

        private void dataGrid_SLS_CHR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh_ULS_VN_Graph();
        }

        private void dataGrid_SLS_QPR_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh_SLS_Crack_Graph();
        }

        private void dataGrid_ULS_VN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh_SLS_Stresses_Graph();
        }
        ///koniec kontrola wprowadzania danych

        //PRZYCISKI
        private void button_UpdateGraph_Click(object sender, RoutedEventArgs e)
        {
            TakeData();
            CalcCurves();

            Refresh_ULS_MN_Graph();
            Refresh_ULS_VN_Graph();
            Refresh_SLS_Crack_Graph();
            Refresh_SLS_Stresses_Graph();
        }

        private void button_Creep1_Click(object sender, RoutedEventArgs e)
        {
            //double[] crCoeff = { 40, 1, 100, 10000 };
            double divide = 10;
            double[] cr = new double[Convert.ToInt32(divide)];
            double[] day = new double[Convert.ToInt32(divide)];
            for (double i = 1; i <= divide; i++)
            {
                cr[Convert.ToInt32(i - 1)] = CreepCoefficient.CreepCoefficientCalc(section1, Double.Parse(textBox_RH.Text), Double.Parse(textBox_u.Text), Double.Parse(textBox_Cst.Text), Double.Parse(textBox_Cst.Text) + ((i - 1) / (divide - 1)) * (Double.Parse(textBox_Cse.Text) - Double.Parse(textBox_Cst.Text)), 0);
                day[Convert.ToInt32(i - 1)] = Double.Parse(textBox_Cst.Text) + ((i - 1) / (divide - 1)) * (Double.Parse(textBox_Cse.Text) - Double.Parse(textBox_Cst.Text));
            }
            CreepWindow crWindow = new CreepWindow();
            crWindow.Show(cr, day);
        }

        private void button_Creep2_Click(object sender, RoutedEventArgs e)
        {
            //double[] crCoeff = { 40, 1, 100, 10000 };
            double divide = 10;
            double[] cr = new double[Convert.ToInt32(divide)];
            double[] day = new double[Convert.ToInt32(divide)];
            for (double i = 1; i <= divide; i++)
            {
                cr[Convert.ToInt32(i - 1)] = CreepCoefficient.CreepCoefficientCalc(section2, Double.Parse(textBox_RH.Text), Double.Parse(textBox_u.Text), Double.Parse(textBox_Cst.Text), Double.Parse(textBox_Cst.Text) + ((i - 1) / (divide - 1)) * (Double.Parse(textBox_Cse.Text) - Double.Parse(textBox_Cst.Text)), 0);
                day[Convert.ToInt32(i - 1)] = Double.Parse(textBox_Cst.Text) + ((i - 1) / (divide - 1)) * (Double.Parse(textBox_Cse.Text) - Double.Parse(textBox_Cst.Text));
            }
            CreepWindow crWindow = new CreepWindow();
            crWindow.Show(cr, day);
        }

        private void button_Import_MN_Click(object sender, RoutedEventArgs e)
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

        private void button_Import_VN_Click(object sender, RoutedEventArgs e)
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

        private void button_Import_QPR_Click(object sender, RoutedEventArgs e)
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

        private void button_Import_CHR_Click(object sender, RoutedEventArgs e)
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

        private void button_Delete_MN_Click(object sender, RoutedEventArgs e)
        {
            if (diagram_ULS_MN != null)
            {
                PlotView_ULS_MN.Model = null;
                diagram_ULS_MN.RemoveSerie("ULS Case");
                PlotView_ULS_MN.Model = diagram_ULS_MN.wykres;
            }
        }

        private void button_Delete_VN_Click(object sender, RoutedEventArgs e)
        {
            if (diagram_ULS_VN != null)
            {
                PlotView_ULS_VN.Model = null;
                diagram_ULS_VN.RemoveSerie("ULS Case");
                PlotView_ULS_VN.Model = diagram_ULS_VN.wykres;
            }
        }

        private void button_Delete_QPR_Click(object sender, RoutedEventArgs e)
        {
            if (diagram_SLS_Crack != null)
            {
                PlotView_SLS_Crack.Model = null;
                diagram_SLS_Crack.RemoveSerie("SLS QPR Case");
                PlotView_SLS_Crack.Model = diagram_SLS_Crack.wykres;
            }
        }

        private void button_Delete_CHR_Click(object sender, RoutedEventArgs e)
        {
            if (diagram_SLS_Crack != null)
            {
                PlotView_SLS_Stresess.Model = null;
                diagram_SLS_Stressess.RemoveSerie("SLS CHR Case");
                PlotView_SLS_Stresess.Model = diagram_SLS_Stressess.wykres;
            }
        }

        // KONIEC OPROGRAMOWANIA KONTROLEK

        private void TakeData()
        {

            if (comboBox_As2_spac_no_1.Text == "spacing")
            {
                section1 = new Section(
                   new Concrete((Concrete.classes)comboBox_Concrete_1.SelectedIndex),
                   new Steel((Steel.classes)comboBox_Steel_1.SelectedIndex),
                   Double.Parse(textBox_width_1.Text),
                   Double.Parse(textBox_height_1.Text),
                   Double.Parse(comboBox_diameter_As1_1.Text),
                   Double.Parse(textBox_spac_no_As1_1.Text),
                   Double.Parse(textBox_cover_As1_1.Text),
                   Double.Parse(comboBox_diameter_As2_1.Text),
                   Double.Parse(textBox_spac_no_As2_1.Text),
                   Double.Parse(textBox_cover_As2_1.Text)
                   );
            }
            else if (comboBox_As2_spac_no_1.Text == "no of bars")
            {
                section1 = new Section(
                   new Concrete((Concrete.classes)comboBox_Concrete_1.SelectedIndex),
                   new Steel((Steel.classes)comboBox_Steel_1.SelectedIndex),
                   Double.Parse(textBox_width_1.Text),
                   Double.Parse(textBox_height_1.Text),
                   Double.Parse(comboBox_diameter_As1_1.Text),
                   Int32.Parse(textBox_spac_no_As1_1.Text),
                   Double.Parse(textBox_cover_As1_1.Text),
                   Double.Parse(comboBox_diameter_As2_1.Text),
                   Int32.Parse(textBox_spac_no_As2_1.Text),
                   Double.Parse(textBox_cover_As2_1.Text)
                   );
            }

            if (comboBox_As2_spac_no_2.Text == "spacing")
            {
                section2 = new Section(
                   new Concrete((Concrete.classes)comboBox_Concrete_2.SelectedIndex),
                   new Steel((Steel.classes)comboBox_Steel_2.SelectedIndex),
                   Double.Parse(textBox_width_2.Text),
                   Double.Parse(textBox_height_2.Text),
                   Double.Parse(comboBox_diameter_As1_2.Text),
                   Double.Parse(textBox_spac_no_As1_2.Text),
                   Double.Parse(textBox_cover_As1_2.Text),
                   Double.Parse(comboBox_diameter_As2_2.Text),
                   Double.Parse(textBox_spac_no_As2_2.Text),
                   Double.Parse(textBox_cover_As2_2.Text)
                   );
            }
            else if (comboBox_As2_spac_no_2.Text == "no of bars")
            {
                section2 = new Section(
                   new Concrete((Concrete.classes)comboBox_Concrete_2.SelectedIndex),
                   new Steel((Steel.classes)comboBox_Steel_2.SelectedIndex),
                   Double.Parse(textBox_width_2.Text),
                   Double.Parse(textBox_height_2.Text),
                   Double.Parse(comboBox_diameter_As1_2.Text),
                   Int32.Parse(textBox_spac_no_As1_2.Text),
                   Double.Parse(textBox_cover_As1_2.Text),
                   Double.Parse(comboBox_diameter_As2_2.Text),
                   Int32.Parse(textBox_spac_no_As2_2.Text),
                   Double.Parse(textBox_cover_As2_2.Text)
                   );
            }

            stirrups1 = new Stirrups(
                Int32.Parse(textBox_legs_1.Text),
                Double.Parse(comboBox_diameter_AsStir_1.Text),
                section1.currentSteel,
                Double.Parse(textBox_stir_spacing_1.Text),
                Double.Parse(textBox_stir_angle_1.Text)
                );

            stirrups2 = new Stirrups(
                Int32.Parse(textBox_legs_2.Text),
                Double.Parse(comboBox_diameter_AsStir_2.Text),
                section2.currentSteel,
                Double.Parse(textBox_stir_spacing_2.Text),
                Double.Parse(textBox_stir_angle_2.Text)
                );
        }

        private void CalcCurves()
        {
            tab1_ULS = ULS.GetULS_MN_Curve(
                section1,
                (ULS.DesignSituation)comboBox_DesignSituation_1.SelectedIndex,
                wspolczynniki.NoOfPoints
                );

            tab2_ULS = ULS.GetULS_MN_Curve(
                section2,
                (ULS.DesignSituation)comboBox_DesignSituation_2.SelectedIndex,
                wspolczynniki.NoOfPoints
                );

            tabSLS_Crack = SLS.GetSLS_Crack_Curve(
                section1,
                wspolczynniki.NoOfPoints,
                wspolczynniki.Crack_wklim,
                wspolczynniki.Crack_kt,
                wspolczynniki.Crack_k1
                );

            tabSLS_NonCrack = SLS.GetSLS_Crack_Curve(
                section1,
                wspolczynniki.NoOfPoints,
                0,
                wspolczynniki.Crack_kt,
                wspolczynniki.Crack_k1
                );

            tabVRdc1 = ULS.GetULS_VRdcN_Curve(
                section1,
                (ULS.DesignSituation)comboBox_DesignSituation_1.SelectedIndex,
                wspolczynniki.NoOfPoints
                );

            tabVRd1 = ULS.GetULS_VRdN_Curve(
                section1,
                (ULS.DesignSituation)comboBox_DesignSituation_1.SelectedIndex,
                wspolczynniki.NoOfPoints,
                stirrups1
                );

            tabSLS_SteelStress = SLS.GetSLS_StressSteel_Curve(
                     section1,
                     wspolczynniki.NoOfPoints,
                     wspolczynniki.Stresses_k3
                     );

            tabSLS_ConcreteStress = SLS.GetSLS_StressConcrete_Curve(
                     section1,
                     wspolczynniki.NoOfPoints,
                     wspolczynniki.Stresses_k1
                     );

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

            if (tabVRd1 != null)
            {
                diagram_ULS_VN.RemoveSerie("Section 1 - VRd.s");
                diagram_ULS_VN.AddLineSerie(tabVRd1, "Section 1 - VRd.s", ustawienia.ULSVN_VrdLineColor.GetMedia(), ustawienia.ULSVN_VrdLineWeight);
            }

            if (points_VN != null)
            {
                diagram_ULS_VN.RemoveSerie("ULS Case");
                diagram_ULS_VN.AddPointSerie(points_VN, "ULS Case", ustawienia.ULSVN_DataPointColor.GetMedia(), ustawienia.ULSVN_DataPointWeight);
            }
            PlotView_ULS_VN.Model = diagram_ULS_VN.wykres;
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
                diagram_SLS_Crack.RemoveSerie("Section 1 - w.max = " + wspolczynniki.Crack_wklim + " mm");
                diagram_SLS_Crack.AddLineSerie(tabSLS_Crack, "Section 1 - w.max = " + wspolczynniki.Crack_wklim + " mm", ustawienia.SLS_Crack_Cracked_LineColor.GetMedia(), ustawienia.SLS_Crack_Cracked_LineWeight);
            }

            if (points_SLS_QPR != null)
            {
                diagram_SLS_Crack.RemoveSerie("SLS QPR Case");
                diagram_SLS_Crack.AddPointSerie(points_SLS_QPR, "SLS QPR Case", ustawienia.SLS_Crack_DataPointColor.GetMedia(), ustawienia.SLS_Crack_DataPointWeight);
            }
            PlotView_SLS_Crack.Model = diagram_SLS_Crack.wykres;
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
        }

        private List<CasePoint> ReadFileCSV()
        {
            List<CasePoint> taLista = new List<CasePoint>();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Text files (.txt)|*.txt|CSV Files (.csv)|*.csv";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.FileName = "*.csv";

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
    }
}
