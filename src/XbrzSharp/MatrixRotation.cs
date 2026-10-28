// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz;

internal sealed class MatrixRotation
{
    public const int HalfByte = sizeof(byte) * 8 / 2;

    private static readonly MatrixRotation[] Cache =
    [
        null!, // unused index 0
        null!, // unused index 1
        new(2),
        new(3),
        new(4),
        new(5),
        new(6),
    ];

    private readonly int n;
    private readonly int nSq;
    private readonly byte[] lookup;

    private MatrixRotation(int n)
    {
        this.n = n;
        nSq = n * n;
        lookup = new byte[4 * nSq];
        for (int rotDeg = 0; rotDeg < 4; rotDeg++)
        {
            int offset = rotDeg * nSq;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    lookup[offset + (i * n) + j] = CalcRecursive(rotDeg, (byte)(i << HalfByte | j));
            }
        }
    }

    public static MatrixRotation Of(int n)
    {
        if ((uint)n - 2u > 4u)
            throw new ArgumentOutOfRangeException(nameof(n), "Scale must be 2–6");
        return Cache[n];
    }

    private byte CalcRecursive(int rotDeg, byte ij)
    {
        if (rotDeg == 0)
            return ij;
        byte ijOld = CalcRecursive(rotDeg - 1, ij);
        int jOld = ijOld & 0xF;
        int iOld = ijOld >> HalfByte & 0xF;
        return (byte)((n - 1 - jOld) << HalfByte | iOld);
    }

    public byte Calc(RotationDegree rotDeg, int i, int j)
        => lookup[(int)rotDeg * nSq + (i * n) + j];
}
