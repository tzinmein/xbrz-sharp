// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.Scaler;

internal class Scaler3x(bool withAlpha) : AbstractScaler(3, withAlpha)
{
    public override void BlendLineShallow(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(Scale - 1, 0, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(Scale - 2, 2, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(Scale - 1, 1, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(Scale - 1, 2, col);
    }
    public override void BlendLineSteep(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(0, Scale - 1, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(2, Scale - 2, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(1, Scale - 1, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(2, Scale - 1, col);
    }
    public override void BlendLineSteepAndShallow(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(2, 0, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(0, 2, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(2, 1, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(1, 2, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(2, 2, col);
    }
    public override void BlendLineDiagonal(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(1, 2, refVal => AlphaGrad(1, 8, refVal, col));
        outMatrix.Set(2, 1, refVal => AlphaGrad(1, 8, refVal, col));
        outMatrix.Set(2, 2, refVal => AlphaGrad(7, 8, refVal, col));
    }
    public override void BlendCorner(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(2, 2, refVal => AlphaGrad(45, 100, refVal, col));
    }
}
