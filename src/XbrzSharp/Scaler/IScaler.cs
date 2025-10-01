// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.Scaler;

internal interface IScaler
{
    int Scale { get; }
    void BlendLineShallow(int col, OutputMatrix outMatrix);
    void BlendLineSteep(int col, OutputMatrix outMatrix);
    void BlendLineSteepAndShallow(int col, OutputMatrix outMatrix);
    void BlendLineDiagonal(int col, OutputMatrix outMatrix);
    void BlendCorner(int col, OutputMatrix outMatrix);
}
