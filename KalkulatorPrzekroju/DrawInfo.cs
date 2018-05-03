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
        public GeometryGroup Dimv;
        public double f1;
        public double f2;
        public double c1;
        public double c2;
        public double sp1;
        public double sp2;
        public double size;
        public double vert;
        public double hor;
        public double[] bindtop1;
        public double[] bindtop2;
        public double[] bindbot1;
        public double[] bindbot2;



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
            this.Shape = CreateShape(H, B);
            this.Reinforcement = CreateBar(H, B, f1, f2, c1, c2, sp1, sp2, true, bySpacing);
            this.size = Math.Max(this.H, this.B);
            this.vert = h;
            this.hor = b;
            this.bindtop1 = new double[] { B / 2, H / 2 };
            this.bindtop2 = new double[] { B / 2, -H / 2 };
            this.bindbot1 = new double[] { -B / 2, H / 2 };
            this.bindbot2 = new double[] { B / 2, H / 2 };
        }

        public DrawInfo(double Dd, double no1, double fi1, double co1)
        {
            this.D = Dd;
            this.f1 = fi1;
            this.c1 = co1;
            isRectangle = false;
            bySpacing = false;
            this.Shape = CreateShape(D);
            this.Reinforcement = CreateBar(D, f1, c1, no1);
            this.size = this.D;
            this.vert = D;
            this.hor = D;
            this.bindtop1 = new double[] { D / 2, D / 2 };
            this.bindtop2 = new double[] { D / 2, -D / 2 };
            this.bindbot1 = new double[] { -D / 2, D / 2 };
            this.bindbot2 = new double[] { D / 2, D / 2 };

        }

        public GeometryGroup CreateShape(double H, double B)
        {
            GeometryGroup aaa = new GeometryGroup();
            RectangleGeometry aa = new RectangleGeometry();
            aa.Rect = new Rect(-B / 2, -H / 2, B, H);
            aaa.Children.Add(aa);
            return aaa;
        }

        public GeometryGroup CreateShape(double D)
        {
            GeometryGroup aaa = new GeometryGroup();
            EllipseGeometry bb = new EllipseGeometry();
            bb.Center = new Point(0, 0);
            bb.RadiusX = D / 2;
            bb.RadiusY = D / 2;
            aaa.Children.Add(bb);
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
                    bb.Center = new Point(edge1 + i * sp1 - b / 2, h - c1 - f1 / 2 - h / 2);
                    bb.RadiusX = f1 / 2;
                    bb.RadiusY = f1 / 2;
                    bbb.Children.Add(bb);
                }
                for (double j = 0; j <= no2; j++)
                {
                    EllipseGeometry cc = new EllipseGeometry();
                    cc.Center = new Point(edge2 + j * sp2 - b / 2, c2 + f2 / 2 - h / 2);
                    cc.RadiusX = f2 / 2;
                    cc.RadiusY = f2 / 2;
                    bbb.Children.Add(cc);
                }
            }
            return bbb;
        }

        public GeometryGroup CreateBar(double d, double f1, double c1, double no1)
        {
            GeometryGroup bbb = new GeometryGroup();
            {
                double divide = 360 / no1;
                double edge1 = c1 + f1 / 2;

                for (double i = 0; i < no1; i++)
                {
                    EllipseGeometry bb = new EllipseGeometry();
                    bb.Center = new Point((d - c1*2 - f1/2)/2 * Math.Cos((Math.PI / 180) * i * divide), (d - c1 * 2 - f1/2) / 2 * Math.Sin((Math.PI / 180) * i * divide));
                    bb.RadiusX = f1 / 2;
                    bb.RadiusY = f1 / 2;
                    bbb.Children.Add(bb);
                }
            }
            return bbb;
        }
    }
}