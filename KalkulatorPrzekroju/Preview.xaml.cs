﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KalkulatorPrzekroju
{
    /// <summary>
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class Preview : Window
    {
        public Preview(int ind, DrawInfo s1, DrawInfo s2)
        {
            InitializeComponent();
            this.Width = 500;
            this.Height = this.Width;
            this.Show();
            Rectangle rsec;
            Ellipse csec;

            SetBackground();

            if (ind == 0)
            {
                if (s1.isRectangle)
                {
                    double W = s1.B;
                    double H = s1.H;
                    rsec = DrawRectangle(H, W);
                    PreviewCanvas.Children.Add(rsec);
                    Canvas.SetTop(rsec, 50);
                    Canvas.SetLeft(rsec, 50);
                }
                else
                {
                    double D = s1.D;
                    csec = DrawCircle(D);
                    PreviewCanvas.Children.Add(csec);
                    Canvas.SetTop(csec, 50);
                    Canvas.SetLeft(csec, 50);
                }
            }
            else if (ind == 1)
            {
                if (s2.isRectangle)
                {
                    double W = s2.B;
                    double H = s2.H;
                    rsec = DrawRectangle(H, W);
                    PreviewCanvas.Children.Add(rsec);
                    Canvas.SetTop(rsec, 50);
                    Canvas.SetLeft(rsec, 50);
                }
                else
                {
                    double D = s2.D;
                    csec = DrawCircle(D);
                    PreviewCanvas.Children.Add(csec);
                    Canvas.SetTop(csec, 50);
                    Canvas.SetLeft(csec, 50);
                }
            }
            else Console.WriteLine("Not Good");
        }

        void Preview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //StretchVbox();
            ScaleTransform myScale = new ScaleTransform();
            TransformGroup myTransform = new TransformGroup();
            myTransform.Children.Add(myScale);
            myScale.ScaleX = this.ActualWidth/500;
            myScale.ScaleY = myScale.ScaleX;
            PreviewCanvas.RenderTransform = myTransform;
        }

        private void StretchVbox()
    {
        Viewbox Outline = new Viewbox();
        Outline.StretchDirection = StretchDirection.Both;
        Outline.Stretch = Stretch.UniformToFill;
        Outline.MaxWidth = 1200;
        Outline.MaxHeight = 1200;
    }

        private void SetBackground()
    {
        LinearGradientBrush bkground = new LinearGradientBrush();
        bkground.StartPoint = new Point(0, 0);
        bkground.EndPoint = new Point(0, 1);
        GradientStop whiteGS = new GradientStop();
        whiteGS.Color = Colors.White;
        whiteGS.Offset = 0.0;
        bkground.GradientStops.Add(whiteGS);
        GradientStop grayGS = new GradientStop();
        grayGS.Color = Colors.LightGray;
        grayGS.Offset = 1.0;
        bkground.GradientStops.Add(grayGS);

        PreviewCanvas.Background = bkground;
    }

        private Rectangle DrawRectangle(double H, double B)
    {
        double aH = PreviewCanvas.ActualHeight;
        double aB = PreviewCanvas.ActualWidth;

        double trH;
        double trB;

        if (H / aH > B / aB)
        {
            trH = 0.8 * aH;
            trB = 0.8 * (aH / H) * B;
        }
        else
        {
            trB = 0.8 * aB;
            trH = 0.8 * (aB / B) * H;
        }

            Rectangle rsec = new Rectangle();
            rsec.Height = trH;
            rsec.Width = trB;
            rsec.Stroke = new SolidColorBrush(Colors.Black);
            rsec.StrokeThickness = 1;
            rsec.Fill = new SolidColorBrush(Colors.LightGoldenrodYellow);
            return rsec;
    }

        private Ellipse DrawCircle(double D)
        {
            double aH = PreviewCanvas.ActualHeight;
            double aB = PreviewCanvas.ActualWidth;

            double trH;
            double trB;

            if (D / aH > D / aB)
            {
                trH = 0.8 * aH;
                trB = 0.8 * (aH / D) * D;
            }
            else
            {
                trB = 0.8 * aB;
                trH = 0.8 * (aB / D) * D;
            }

            Ellipse csec = new Ellipse();
            csec.Height = trH;
            csec.Width = trB;
            csec.Stroke = new SolidColorBrush(Colors.Black);
            csec.StrokeThickness = 1;
            csec.Fill = new SolidColorBrush(Colors.LightGoldenrodYellow);
            return csec;
        }
    }
}
