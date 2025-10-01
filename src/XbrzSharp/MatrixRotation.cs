// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
using System.Collections.Concurrent;

namespace Xbrz;

internal sealed class MatrixRotation
{
    public const int HalfByte = sizeof(byte) * 8 / 2;
    private readonly int N;
    private readonly int Nsq;
    private readonly byte[] lookup;
    public MatrixRotation(int N)
    {
        this.N = N;
        Nsq = N * N;
        if (N <= 0 || N >= 16)
            throw new ArgumentException("N should be > 0 and < 16");
        byte[] lookup = new byte[4 * Nsq];
        for (int rotDeg = 0; rotDeg < 4; rotDeg++)
        {
            int offset = rotDeg * Nsq;
            for (int I = 0; I < N; I++)
            {
                for (int J = 0; J < N; J++)
                {
                    lookup[offset + (I * N) + J] = Calc(rotDeg, (byte)(I << HalfByte | J));
                }
            }
        }
        this.lookup = lookup;
    }

    private static readonly ConcurrentDictionary<int, MatrixRotation> instance = new();
    public static MatrixRotation Of(int N)
    {
        return instance.GetOrAdd(N, n => new MatrixRotation(n));
    }

    private byte Calc(int rotDeg, byte IJ)
    {
        if (rotDeg == 0)
            return IJ;
        byte IJ_old = Calc(rotDeg - 1, IJ);
        int J_old = IJ_old & 0xF;
        int I_old = IJ_old >> HalfByte & 0xF;
        int rot_I = N - 1 - J_old;
        int rot_J = I_old;
        return (byte)(rot_I << HalfByte | rot_J);
    }

    public byte Calc(RotationDegree rotDeg, int I, int J)
    {
        int offset = (int)rotDeg * Nsq;
        return lookup[offset + (I * N) + J];
    }
}
