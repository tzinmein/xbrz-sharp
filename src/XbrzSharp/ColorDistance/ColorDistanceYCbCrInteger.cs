// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.ColorDistance;

internal sealed class ColorDistanceYCbCrInteger
{
    private const long Precision = 100_000L;
    private const long PrecisionSq = Precision * Precision;
    private readonly long lumaWeightInt;
    private const long KbInt = (long)(Precision * 0.0593);
    private const long KrInt = (long)(Precision * 0.2627);
    private const long KgInt = Precision - KbInt - KrInt;
    private const long DenomB = 2 * (Precision - KbInt);
    private const long DenomR = 2 * (Precision - KrInt);

    public ColorDistanceYCbCrInteger(double lumaWeight)
        => lumaWeightInt = (long)(Precision * lumaWeight);

    public double Calc(int pix1, int pix2)
    {
        long rDiff = Color.GetRed(pix1) - Color.GetRed(pix2);
        long gDiff = Color.GetGreen(pix1) - Color.GetGreen(pix2);
        long bDiff = Color.GetBlue(pix1) - Color.GetBlue(pix2);
        long y = (KrInt * rDiff) + (KgInt * gDiff) + (KbInt * bDiff);
        long cb = Precision * ((Precision * bDiff) - y) / DenomB;
        long cr = Precision * ((Precision * rDiff) - y) / DenomR;
        long weightedY = lumaWeightInt == Precision ? y : y * lumaWeightInt / Precision;
        return Math.Sqrt((Square(weightedY) + Square(cb) + Square(cr)) / (double)PrecisionSq);
    }

    private static long Square(long value) => value * value;
}
