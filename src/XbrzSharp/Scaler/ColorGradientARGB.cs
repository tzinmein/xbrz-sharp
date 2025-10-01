// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.Scaler;

internal class ColorGradientARGB : IColorGradient
{
    private static int CalcColor(int weightFront, int weightBack, int weightSum, int colFront, int colBack)
        => ((colFront * weightFront) + (colBack * weightBack)) / weightSum;
    public int AlphaGrad(int M, int N, int pixBack, int pixFront)
    {
        int weightFront = Color.GetAlpha(pixFront) * M;
        int weightBack = Color.GetAlpha(pixBack) * (N - M);
        int weightSum = weightFront + weightBack;
        if (weightSum == 0)
            return 0;
        return Color.MakePixel(
            weightSum / N,
            CalcColor(weightFront, weightBack, weightSum, Color.GetRed(pixFront), Color.GetRed(pixBack)),
            CalcColor(weightFront, weightBack, weightSum, Color.GetGreen(pixFront), Color.GetGreen(pixBack)),
            CalcColor(weightFront, weightBack, weightSum, Color.GetBlue(pixFront), Color.GetBlue(pixBack))
        );
    }
}
