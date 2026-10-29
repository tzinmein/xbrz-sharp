// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.ColorDistance;

public delegate double ColorDistance(int pix1, int pix2);

internal static class ColorDistanceFactory
{
    public static ColorDistance Rgb() => static (pix1, pix2) =>
    {
        int rDiff = Color.GetRed(pix1) - Color.GetRed(pix2);
        int gDiff = Color.GetGreen(pix1) - Color.GetGreen(pix2);
        int bDiff = Color.GetBlue(pix1) - Color.GetBlue(pix2);
        return Math.Sqrt((rDiff * rDiff) + (gDiff * gDiff) + (bDiff * bDiff));
    };

    public static ColorDistance YCbCr(double lumaWeight)
        => (pix1, pix2) => YCbCrMath.Distance(lumaWeight, pix1, pix2);

    public static ColorDistance IntegerYCbCr(double lumaWeight)
        => new ColorDistanceYCbCrInteger(lumaWeight).Calc;

    public static ColorDistance BufferedYCbCr(int sigBits)
        => new ColorDistanceYCbCrBuffered(sigBits).Calc;
}
