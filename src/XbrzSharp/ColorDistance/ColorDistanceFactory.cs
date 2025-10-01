// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.ColorDistance;

// Delegate for color distance calculation
public delegate double ColorDistance(int pix1, int pix2);

internal static class ColorDistanceFactory
{
    public static ColorDistance Rgb() => (pix1, pix2) =>
    {
        int r_diff = Color.GetRed(pix1) - Color.GetRed(pix2);
        int g_diff = Color.GetGreen(pix1) - Color.GetGreen(pix2);
        int b_diff = Color.GetBlue(pix1) - Color.GetBlue(pix2);
        return Math.Sqrt((r_diff * r_diff) + (g_diff * g_diff) + (b_diff * b_diff));
    };

    public static ColorDistance YCbCr(double lumaWeight) => new ColorDistanceYCbCr(lumaWeight).Calc;
    public static ColorDistance IntegerYCbCr(double lumaWeight) => new ColorDistanceYCbCrInteger(lumaWeight).Calc;
    public static ColorDistance BufferedYCbCr(int sigBits) => ColorDistanceYCbCrBuffered.Instance(sigBits).Calc;
    public static ColorDistance WithAlpha(ColorDistance dist) => (pix1, pix2) =>
    {
        int a1 = Color.GetAlpha(pix1);
        int a2 = Color.GetAlpha(pix2);
        double d = dist(pix1, pix2);
        return a1 < a2 ? (a1 / 255.0 * d) + (a2 - a1)
                         : (a2 / 255.0 * d) + (a1 - a2);
    };
}
