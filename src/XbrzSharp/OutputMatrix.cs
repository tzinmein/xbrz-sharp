// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz;

// Access matrix area, top-left at current position
internal sealed class OutputMatrix
{
    private int N;
    private int[] outArray = [];
    private int outWidth;
    private int offset;
    private RotationDegree rotDeg = RotationDegree.Rot0;
    private MatrixRotation rot = null!;

    private OutputMatrix() { }

    private static readonly ThreadLocal<OutputMatrix> instance = new();

    public static OutputMatrix Instance(int N, int[] outArray, int outWidth)
    {
        var matrix = instance.Value;
        if (matrix == null)
        {
            matrix = new OutputMatrix();
            instance.Value = matrix;
        }
        matrix.N = N;
        matrix.outArray = outArray;
        matrix.outWidth = outWidth;
        matrix.rot = MatrixRotation.Of(N);
        return matrix;
    }

    public void PositionY(int y)
    {
        offset = N * y * outWidth;
    }

    public void IncrementX()
    {
        offset += N;
    }

    public void SetRotationDegree(RotationDegree deg)
    {
        rotDeg = deg;
    }

    private int Position(int I, int J)
    {
        byte IJ_old = rot.Calc(rotDeg, I, J);
        int I_old = IJ_old >> MatrixRotation.HalfByte & 0xF;
        int J_old = IJ_old & 0xF;
        return offset + J_old + (I_old * outWidth);
    }

    public void Set(int I, int J, int val)
    {
        outArray[Position(I, J)] = val;
    }

    public void Set(int I, int J, Func<int, int> func)
    {
        int pos = Position(I, J);
        outArray[pos] = func(outArray[pos]);
    }

    // Fill block of size scale * scale with the given color
    public void FillBlock(int col)
    {
        FillBlock(col, N, N);
    }

    public void FillBlock(int col, int blockWidth, int blockHeight)
    {
        for (int y = 0, trg = (y * outWidth) + offset; y < blockHeight; ++y, trg += outWidth)
            for (int x = 0; x < blockWidth; ++x)
                outArray[trg + x] = col;
    }
}
