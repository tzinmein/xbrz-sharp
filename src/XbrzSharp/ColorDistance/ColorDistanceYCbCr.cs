// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.ColorDistance;

internal class ColorDistanceYCbCr(double lumaWeight)
{
    protected readonly double lumaWeight = lumaWeight;
    protected const double k_b = 0.0593;
    protected const double k_r = 0.2627;
    protected const double k_g = 1 - k_b - k_r;
    protected const double scale_b = 0.5 / (1 - k_b);
    protected const double scale_r = 0.5 / (1 - k_r);

    public virtual double Calc(int pix1, int pix2)
    {
        int r_diff = Color.GetRed(pix1) - Color.GetRed(pix2);
        int g_diff = Color.GetGreen(pix1) - Color.GetGreen(pix2);
        int b_diff = Color.GetBlue(pix1) - Color.GetBlue(pix2);
        double y = (k_r * r_diff) + (k_g * g_diff) + (k_b * b_diff);
        double c_b = scale_b * (b_diff - y);
        double c_r = scale_r * (r_diff - y);
        return Math.Sqrt(Square(lumaWeight == 1.0 ? y : lumaWeight * y) + Square(c_b) + Square(c_r));
    }

    protected static double Square(double value) => value * value;
}
