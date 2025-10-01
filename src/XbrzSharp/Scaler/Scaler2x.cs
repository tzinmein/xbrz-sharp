// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.Scaler;

internal class Scaler2x(bool withAlpha) : AbstractScaler(2, withAlpha)
{
    public override void BlendLineShallow(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(Scale - 1, 0, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(Scale - 1, 1, refVal => AlphaGrad(3, 4, refVal, col));
    }
    public override void BlendLineSteep(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(0, Scale - 1, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(1, Scale - 1, refVal => AlphaGrad(3, 4, refVal, col));
    }
    public override void BlendLineSteepAndShallow(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(1, 0, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(0, 1, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(1, 1, refVal => AlphaGrad(5, 6, refVal, col));
    }
    public override void BlendLineDiagonal(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(1, 1, refVal => AlphaGrad(1, 2, refVal, col));
    }
    public override void BlendCorner(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(1, 1, refVal => AlphaGrad(21, 100, refVal, col));
    }
}
