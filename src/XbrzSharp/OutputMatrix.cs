// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
using Xbrz.Scaler;

namespace Xbrz;

/// <summary>Accesses a scale×scale output block with top-left at the current position.</summary>
internal sealed class OutputMatrix
{
    private readonly int n;
    private readonly int[] outArray;
    private readonly int outWidth;
    private readonly MatrixRotation rot;
    private int offset;
    private RotationDegree rotDeg = RotationDegree.Rot0;

    public OutputMatrix(int scale, int[] outArray, int outWidth)
    {
        n = scale;
        this.outArray = outArray;
        this.outWidth = outWidth;
        rot = MatrixRotation.Of(scale);
    }

    public void PositionY(int y) => offset = n * y * outWidth;

    public void IncrementX() => offset += n;

    public void SetRotationDegree(RotationDegree deg) => rotDeg = deg;

    private int Position(int i, int j)
    {
        byte ij = rot.Calc(rotDeg, i, j);
        int iOld = ij >> MatrixRotation.HalfByte & 0xF;
        int jOld = ij & 0xF;
        return offset + jOld + (iOld * outWidth);
    }

    public void Set(int i, int j, int val) => outArray[Position(i, j)] = val;

    public void Blend(int i, int j, int m, int nFrac, int col, bool withAlpha)
    {
        int pos = Position(i, j);
        outArray[pos] = withAlpha
            ? AbstractScaler.AlphaGradArgb(m, nFrac, outArray[pos], col)
            : AbstractScaler.AlphaGradRgb(m, nFrac, outArray[pos], col);
    }

    public void FillBlock(int col)
    {
        for (int y = 0, trg = offset; y < n; ++y, trg += outWidth)
            for (int x = 0; x < n; ++x)
                outArray[trg + x] = col;
    }
}
