// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.Scaler;

internal sealed class Scaler3x(bool withAlpha) : AbstractScaler(3, withAlpha)
{
    public override void BlendLineShallow(int col, OutputMatrix outMatrix)
    {
        Blend(outMatrix, Scale - 1, 0, 1, 4, col);
        Blend(outMatrix, Scale - 2, 2, 1, 4, col);
        Blend(outMatrix, Scale - 1, 1, 3, 4, col);
        outMatrix.Set(Scale - 1, 2, col);
    }

    public override void BlendLineSteep(int col, OutputMatrix outMatrix)
    {
        Blend(outMatrix, 0, Scale - 1, 1, 4, col);
        Blend(outMatrix, 2, Scale - 2, 1, 4, col);
        Blend(outMatrix, 1, Scale - 1, 3, 4, col);
        outMatrix.Set(2, Scale - 1, col);
    }

    public override void BlendLineSteepAndShallow(int col, OutputMatrix outMatrix)
    {
        Blend(outMatrix, 2, 0, 1, 4, col);
        Blend(outMatrix, 0, 2, 1, 4, col);
        Blend(outMatrix, 2, 1, 3, 4, col);
        Blend(outMatrix, 1, 2, 3, 4, col);
        outMatrix.Set(2, 2, col);
    }

    public override void BlendLineDiagonal(int col, OutputMatrix outMatrix)
    {
        Blend(outMatrix, 1, 2, 1, 8, col);
        Blend(outMatrix, 2, 1, 1, 8, col);
        Blend(outMatrix, 2, 2, 7, 8, col);
    }

    public override void BlendCorner(int col, OutputMatrix outMatrix)
        => Blend(outMatrix, 2, 2, 45, 100, col);
}
