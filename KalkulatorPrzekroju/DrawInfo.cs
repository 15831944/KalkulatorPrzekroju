﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media;

namespace KalkulatorPrzekroju
{
    [Serializable]
    public class DrawInfo
    {
        public double H;
        public double B;
        public double D;
        public GeometryGroup Shape;
        public bool isRectangle;
        public bool bySpacing;
        public GeometryGroup Reinforcement;
        public double f1;
        public double f2;
        public double c1;
        public double c2;
        public double sp1;
        public double sp2;

        public DrawInfo()
        {
            isRectangle = false;
            bySpacing = false;
        }

        public DrawInfo(double h, double b, bool spac, double fi1, double fi2, double co1, double co2, double s1, double s2)
        {
            this.H = h;
            this.B = b;
            this.f1 = fi1;
            this.f2 = fi2;
            this.c1 = co1;
            this.c2 = co2;
            this.sp1 = s1;
            this.sp2 = s2;
            isRectangle = true;
            bySpacing = spac;
            this.Shape = CreateShape(H, B, 0, true);
            this.Reinforcement = CreateBar(H, B, f1, f2, c1, c2, sp1, sp2, true, bySpacing);
        }

        public DrawInfo(double Dd)
        {
            this.D = Dd;
            isRectangle = false;
            bySpacing = false;
            this.Shape = CreateShape(0, 0, D, false);
            this.Reinforcement = CreateBar(D, f1, c1, sp1);
        }

        public GeometryGroup CreateShape(double H, double B, double D, bool isR)
        {
            GeometryGroup aaa = new GeometryGroup();
            if (isR)
            {
                RectangleGeometry aa = new RectangleGeometry();
                aa.Rect = new Rect(0, 0, B, H);
                aaa.Children.Add(aa);
            }
            else
            {
                EllipseGeometry bb = new EllipseGeometry();
                bb.Center = new Point(D/2, D / 2);
                bb.RadiusX = D / 2;
                bb.RadiusY = D / 2;
                aaa.Children.Add(bb);
            }
            return aaa;
        }

        public GeometryGroup CreateBar(double h, double b, double f1, double f2, double c1, double c2, double sp1, double sp2, bool isR, bool byS)
        {

            int no1;
            int no2;

            GeometryGroup bbb = new GeometryGroup();
            if (isR)
            {
                double edge1;
                double edge2;

                if (byS)
                {
                    no1 = (b % sp1 == 0) ? (no1 = (int)b / (int)sp1 - 1) : (no1 = (int)b) / ((int)sp1);
                    no2 = (b % sp2 == 0) ? (no2 = (int)b / (int)sp2 - 1) : (no2 = (int)b) / ((int)sp2);
                    edge1 = (b - no1 * sp1) / 2;
                    edge2 = (b - no2 * sp2) / 2;
                }
                else
                {
                    no1 = ((int)(b - 2 * c1 - f1 / 2)) / ((int)sp1);
                    no2 = ((int)(b - 2 * c2 - f2 / 2)) / ((int)sp2);
                    sp1 = (b - 2 * c1 - f1) / (no1);
                    sp2 = (b - 2 * c2 - f2) / (no2);
                    edge1 = c1 + f1 / 2;
                    edge2 = c2 + f2 / 2;
                }

                for (double i = 0; i <= no1; i++)
                {
                    EllipseGeometry bb = new EllipseGeometry();
                    bb.Center = new Point(edge1 + i * sp1, h - c1 - f1 / 2);
                    bb.RadiusX = f1 / 2;
                    bb.RadiusY = f1 / 2;
                    bbb.Children.Add(bb);
                }
                for (double j = 0; j <= no2; j++)
                {
                    EllipseGeometry cc = new EllipseGeometry();
                    cc.Center = new Point(edge2 + j * sp2, c2 + f2 / 2);
                    cc.RadiusX = f2 / 2;
                    cc.RadiusY = f2 / 2;
                    bbb.Children.Add(cc);
                }
            }
            return bbb;
        }

        public GeometryGroup CreateBar(double d, double f1, double c1, double sp1)
        {
            GeometryGroup bbb = new GeometryGroup();
            {
                double edge1 = c1 + f1 / 2;

                /*for (int i = 0; i < b / sp1; i++)
                {
                    EllipseGeometry bb = new EllipseGeometry();
                    bb.Center = new Point(edge1 + i * sp1, c1 + f1 / 2);
                    bb.RadiusX = f1 / 2;
                    bb.RadiusY = f1 / 2;
                    bbb.Children.Add(bb);
                }
                for (int i = 0; i < b / sp2; i++)
                {
                    EllipseGeometry bb = new EllipseGeometry();
                    bb.Center = new Point(edge2 + i * sp2, h - c2 + f2 / 2);
                    bb.RadiusX = f2 / 2;
                    bb.RadiusY = f2 / 2;
                    bbb.Children.Add(bb);
                }*/
            }
            return bbb;
        }
    }
}