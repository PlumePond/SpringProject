

using System;
using Microsoft.Xna.Framework;

public static class Extensions
{
        /// <summary>
    /// Convert HSV to RGB
    /// h is from 0-360
    /// s,v values are 0-1
    /// r,g,b values are 0-255
    /// Based upon http://ilab.usc.edu/wiki/index.php/HSV_And_H2SV_Color_Space#HSV_Transformation_C_.2F_C.2B.2B_Code_2
    /// </summary>
    public static Color FromHSV(double h, double S, double V)
    {
        // ######################################################################
        // T. Nathan Mundhenk
        // mundhenk@usc.edu
        // C/C++ Macro HSV to RGB

        double H = h;
        while (H < 0) { H += 360; };
        while (H >= 360) { H -= 360; };
        double R, G, B;
        if (V <= 0)
            { R = G = B = 0; }
        else if (S <= 0)
        {
            R = G = B = V;
        }
        else
        {
            double hf = H / 60.0;
            int i = (int)Math.Floor(hf);
            double f = hf - i;
            double pv = V * (1 - S);
            double qv = V * (1 - S * f);
            double tv = V * (1 - S * (1 - f));
            switch (i)
            {

            // Red is the dominant color

            case 0:
                R = V;
                G = tv;
                B = pv;
                break;

            // Green is the dominant color

            case 1:
                R = qv;
                G = V;
                B = pv;
                break;
            case 2:
                R = pv;
                G = V;
                B = tv;
                break;

            // Blue is the dominant color

            case 3:
                R = pv;
                G = qv;
                B = V;
                break;
            case 4:
                R = tv;
                G = pv;
                B = V;
                break;

            // Red is the dominant color

            case 5:
                R = V;
                G = pv;
                B = qv;
                break;

            // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

            case 6:
                R = V;
                G = tv;
                B = pv;
                break;
            case -1:
                R = V;
                G = pv;
                B = qv;
                break;

            // The color is not defined, we should throw an error.

            default:
                //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                R = G = B = V; // Just pretend its black/white
                break;
            }
        }
            int r = Clamp((int)(R * 255.0));
            int g = Clamp((int)(G * 255.0));
            int b = Clamp((int)(B * 255.0));

            return new Color(r, g, b);
    }

    public static HSV ToHSV(Color color)
    {
        byte r = color.R;
        byte g = color.G;
        byte b = color.B;

        double max = Math.Max(Math.Max(r, g), b);
        double min = Math.Min(Math.Min(r, g), b);
        double delta = max - min;

        double h = 0;
        double s = 0; 
        double v = max / 255.0;

        if (max != 0)
        {
            s = delta / max;
        }

        if (s != 0)
        {
            if (r == max)
            {
                h = (g - b) / delta;
            }
            else if (g == max)
            {
                h = 2 + (b - r) / delta;
            }
            else if (b == max)
            {
                h = 4 + (r - g) / delta;
            }

            h *= 60;
            
            if (h < 0)
            {
                h += 360;
            }
        }

        return new HSV(h, s * 100, v * 100); // H in degrees, S and V as percentages
    }

    public struct HSV
    {
        public double H;
        public double S;
        public double V;

        public HSV(double h, double s, double v)
        {
            H = h;
            S = s;
            V = v;
        }
    }

    static int Clamp(int i)
    {
        if (i < 0) return 0;
        if (i > 255) return 255;
        return i;
    }
}