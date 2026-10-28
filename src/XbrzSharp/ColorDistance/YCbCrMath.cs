// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.ColorDistance;

/// <summary>ITU-R BT.2020 YCbCr constants and distance helper.</summary>
internal static class YCbCrMath
{
    public const double Kb = 0.0593;
    public const double Kr = 0.2627;
    public const double Kg = 1 - Kb - Kr;
    public const double ScaleB = 0.5 / (1 - Kb);
    public const double ScaleR = 0.5 / (1 - Kr);

    public static double Distance(double lumaWeight, int pix1, int pix2)
    {
        int rDiff = Color.GetRed(pix1) - Color.GetRed(pix2);
        int gDiff = Color.GetGreen(pix1) - Color.GetGreen(pix2);
        int bDiff = Color.GetBlue(pix1) - Color.GetBlue(pix2);
        double y = (Kr * rDiff) + (Kg * gDiff) + (Kb * bDiff);
        double cb = ScaleB * (bDiff - y);
        double cr = ScaleR * (rDiff - y);
        double weightedY = lumaWeight == 1.0 ? y : lumaWeight * y;
        return Math.Sqrt(Square(weightedY) + Square(cb) + Square(cr));
    }

    public static double Square(double value) => value * value;
}
