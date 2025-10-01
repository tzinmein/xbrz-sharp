// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.ColorDistance;

internal class ColorDistanceYCbCrInteger(double lumaWeight) : ColorDistanceYCbCr(lumaWeight)
{
    private const long PRECISION = 100_000L;
    private const long PRECISION_SQ = PRECISION * PRECISION;
    private readonly long lumaWeightInt = (long)(PRECISION * lumaWeight);
    private const long k_b_int = (long)(PRECISION * 0.0593);
    private const long k_r_int = (long)(PRECISION * 0.2627);
    private const long k_g_int = PRECISION - k_b_int - k_r_int;
    private const long denom_b = 2 * (PRECISION - k_b_int);
    private const long denom_r = 2 * (PRECISION - k_r_int);

    public override double Calc(int pix1, int pix2)
    {
        long r_diff = Color.GetRed(pix1) - Color.GetRed(pix2);
        long g_diff = Color.GetGreen(pix1) - Color.GetGreen(pix2);
        long b_diff = Color.GetBlue(pix1) - Color.GetBlue(pix2);
        long y = (k_r_int * r_diff) + (k_g_int * g_diff) + (k_b_int * b_diff);
        long c_b = PRECISION * ((PRECISION * b_diff) - y) / denom_b;
        long c_r = PRECISION * ((PRECISION * r_diff) - y) / denom_r;
        return Math.Sqrt((Square(lumaWeightInt == PRECISION ? y : y * lumaWeightInt / PRECISION) + Square(c_b) + Square(c_r)) / (double)PRECISION_SQ);
    }

    private static long Square(long value) => value * value;
}
