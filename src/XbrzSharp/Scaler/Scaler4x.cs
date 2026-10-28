// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.Scaler;

internal sealed class Scaler4x(bool withAlpha) : AbstractScaler(4, withAlpha)
{
    public override void BlendLineShallow(int col, OutputMatrix outMatrix)
    {
        Blend(outMatrix, Scale - 1, 0, 1, 4, col);
        Blend(outMatrix, Scale - 2, 2, 1, 4, col);
        Blend(outMatrix, Scale - 1, 1, 3, 4, col);
        Blend(outMatrix, Scale - 2, 3, 3, 4, col);
        outMatrix.Set(Scale - 1, 2, col);
        outMatrix.Set(Scale - 1, 3, col);
    }

    public override void BlendLineSteep(int col, OutputMatrix outMatrix)
    {
        Blend(outMatrix, 0, Scale - 1, 1, 4, col);
        Blend(outMatrix, 2, Scale - 2, 1, 4, col);
        Blend(outMatrix, 1, Scale - 1, 3, 4, col);
        Blend(outMatrix, 3, Scale - 2, 3, 4, col);
        outMatrix.Set(2, Scale - 1, col);
        outMatrix.Set(3, Scale - 1, col);
    }

    public override void BlendLineSteepAndShallow(int col, OutputMatrix outMatrix)
    {
        Blend(outMatrix, 3, 1, 3, 4, col);
        Blend(outMatrix, 1, 3, 3, 4, col);
        Blend(outMatrix, 3, 0, 1, 4, col);
        Blend(outMatrix, 0, 3, 1, 4, col);
        Blend(outMatrix, 2, 2, 1, 3, col);
        outMatrix.Set(3, 3, col);
        outMatrix.Set(3, 2, col);
        outMatrix.Set(2, 3, col);
    }

    public override void BlendLineDiagonal(int col, OutputMatrix outMatrix)
    {
        Blend(outMatrix, Scale - 1, Scale / 2, 1, 2, col);
        Blend(outMatrix, Scale - 2, (Scale / 2) + 1, 1, 2, col);
        outMatrix.Set(Scale - 1, Scale - 1, col);
    }

    public override void BlendCorner(int col, OutputMatrix outMatrix)
    {
        Blend(outMatrix, 3, 3, 68, 100, col);
        Blend(outMatrix, 3, 2, 9, 100, col);
        Blend(outMatrix, 2, 3, 9, 100, col);
    }
}
