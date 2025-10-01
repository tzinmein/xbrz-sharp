// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.Scaler;

internal class ColorGradientRGB : IColorGradient
{
    private static int CalcColor(int M, int N, int colFront, int colBack)
        => ((colFront * M) + (colBack * (N - M))) / N;
    public int AlphaGrad(int M, int N, int pixBack, int pixFront)
    {
        return Color.MakePixel(
            CalcColor(M, N, Color.GetRed(pixFront), Color.GetRed(pixBack)),
            CalcColor(M, N, Color.GetGreen(pixFront), Color.GetGreen(pixBack)),
            CalcColor(M, N, Color.GetBlue(pixFront), Color.GetBlue(pixBack))
        );
    }
}
