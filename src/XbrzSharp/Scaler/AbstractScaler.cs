// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.Scaler;

internal abstract class AbstractScaler(int scale, IColorGradient colorGradient) : IScaler
{
    public int Scale { get; } = scale;

    protected AbstractScaler(int scale, bool withAlpha)
        : this(scale, withAlpha ? ColorGradientFactory.GradientARGB() : ColorGradientFactory.GradientRGB())
    {
    }

    protected int AlphaGrad(int M, int N, int pixBack, int pixFront)
        => colorGradient.AlphaGrad(M, N, pixBack, pixFront);

    // IScaler interface
    public abstract void BlendLineShallow(int col, OutputMatrix outMatrix);
    public abstract void BlendLineSteep(int col, OutputMatrix outMatrix);
    public abstract void BlendLineSteepAndShallow(int col, OutputMatrix outMatrix);
    public abstract void BlendLineDiagonal(int col, OutputMatrix outMatrix);
    public abstract void BlendCorner(int col, OutputMatrix outMatrix);
}
