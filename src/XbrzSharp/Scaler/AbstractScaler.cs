// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.Scaler;

internal abstract class AbstractScaler
{
    private readonly bool withAlpha;

    protected AbstractScaler(int scale, bool withAlpha)
    {
        Scale = scale;
        this.withAlpha = withAlpha;
    }

    public int Scale { get; }

    protected void Blend(OutputMatrix outMatrix, int i, int j, int m, int n, int col)
        => outMatrix.Blend(i, j, m, n, col, withAlpha);

    private static int CalcRgb(int m, int n, int colFront, int colBack)
        => ((colFront * m) + (colBack * (n - m))) / n;

    private static int CalcArgb(int weightFront, int weightBack, int weightSum, int colFront, int colBack)
        => ((colFront * weightFront) + (colBack * weightBack)) / weightSum;

    internal static int AlphaGradRgb(int m, int n, int pixBack, int pixFront)
        => Color.MakePixel(
            CalcRgb(m, n, Color.GetRed(pixFront), Color.GetRed(pixBack)),
            CalcRgb(m, n, Color.GetGreen(pixFront), Color.GetGreen(pixBack)),
            CalcRgb(m, n, Color.GetBlue(pixFront), Color.GetBlue(pixBack)));

    internal static int AlphaGradArgb(int m, int n, int pixBack, int pixFront)
    {
        int weightFront = Color.GetAlpha(pixFront) * m;
        int weightBack = Color.GetAlpha(pixBack) * (n - m);
        int weightSum = weightFront + weightBack;
        if (weightSum == 0)
            return 0;
        return Color.MakePixel(
            weightSum / n,
            CalcArgb(weightFront, weightBack, weightSum, Color.GetRed(pixFront), Color.GetRed(pixBack)),
            CalcArgb(weightFront, weightBack, weightSum, Color.GetGreen(pixFront), Color.GetGreen(pixBack)),
            CalcArgb(weightFront, weightBack, weightSum, Color.GetBlue(pixFront), Color.GetBlue(pixBack)));
    }

    public abstract void BlendLineShallow(int col, OutputMatrix outMatrix);
    public abstract void BlendLineSteep(int col, OutputMatrix outMatrix);
    public abstract void BlendLineSteepAndShallow(int col, OutputMatrix outMatrix);
    public abstract void BlendLineDiagonal(int col, OutputMatrix outMatrix);
    public abstract void BlendCorner(int col, OutputMatrix outMatrix);
}
