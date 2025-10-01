// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.Scaler;

internal class Scaler6x(bool withAlpha) : AbstractScaler(6, withAlpha)
{
    public override void BlendLineShallow(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(Scale - 1, 0, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(Scale - 2, 2, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(Scale - 3, 4, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(Scale - 1, 1, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(Scale - 2, 3, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(Scale - 3, 5, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(Scale - 1, 2, col);
        outMatrix.Set(Scale - 1, 3, col);
        outMatrix.Set(Scale - 1, 4, col);
        outMatrix.Set(Scale - 1, 5, col);
        outMatrix.Set(Scale - 2, 4, col);
        outMatrix.Set(Scale - 2, 5, col);
    }
    public override void BlendLineSteep(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(0, Scale - 1, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(2, Scale - 2, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(4, Scale - 3, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(1, Scale - 1, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(3, Scale - 2, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(5, Scale - 3, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(2, Scale - 1, col);
        outMatrix.Set(3, Scale - 1, col);
        outMatrix.Set(4, Scale - 1, col);
        outMatrix.Set(5, Scale - 1, col);
        outMatrix.Set(4, Scale - 2, col);
        outMatrix.Set(5, Scale - 2, col);
    }
    public override void BlendLineSteepAndShallow(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(0, Scale - 1, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(2, Scale - 2, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(1, Scale - 1, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(3, Scale - 2, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(Scale - 1, 0, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(Scale - 2, 2, refVal => AlphaGrad(1, 4, refVal, col));
        outMatrix.Set(Scale - 1, 1, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(Scale - 2, 3, refVal => AlphaGrad(3, 4, refVal, col));
        outMatrix.Set(2, Scale - 1, col);
        outMatrix.Set(3, Scale - 1, col);
        outMatrix.Set(4, Scale - 1, col);
        outMatrix.Set(5, Scale - 1, col);
        outMatrix.Set(4, Scale - 2, col);
        outMatrix.Set(5, Scale - 2, col);
        outMatrix.Set(Scale - 1, 2, col);
        outMatrix.Set(Scale - 1, 3, col);
    }
    public override void BlendLineDiagonal(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(Scale - 1, Scale / 2, refVal => AlphaGrad(1, 2, refVal, col));
        outMatrix.Set(Scale - 2, (Scale / 2) + 1, refVal => AlphaGrad(1, 2, refVal, col));
        outMatrix.Set(Scale - 3, (Scale / 2) + 2, refVal => AlphaGrad(1, 2, refVal, col));
        outMatrix.Set(Scale - 2, Scale - 1, col);
        outMatrix.Set(Scale - 1, Scale - 1, col);
        outMatrix.Set(Scale - 1, Scale - 2, col);
    }
    public override void BlendCorner(int col, OutputMatrix outMatrix)
    {
        outMatrix.Set(5, 5, refVal => AlphaGrad(97, 100, refVal, col));
        outMatrix.Set(4, 5, refVal => AlphaGrad(42, 100, refVal, col));
        outMatrix.Set(5, 4, refVal => AlphaGrad(42, 100, refVal, col));
        outMatrix.Set(5, 3, refVal => AlphaGrad(6, 100, refVal, col));
        outMatrix.Set(3, 5, refVal => AlphaGrad(6, 100, refVal, col));
    }
}
