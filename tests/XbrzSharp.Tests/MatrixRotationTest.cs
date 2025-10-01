// xBrzTool / Test / Helper file
// Licensed under 0BSD
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
using Xbrz;

namespace xBrzNet.Tests;

public class MatrixRotationTest
{
    private const int N = 4;
    private static readonly MatrixRotation m4x4 = MatrixRotation.Of(4);

    [Fact]
    public void Matrix4Rotation0()
    {
        const RotationDegree rot0 = RotationDegree.Rot0;
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                Assert.Equal(i << 4 | j, m4x4.Calc(rot0, i, j) & 0xFF);
            }
        }
    }

    [Fact]
    public void Matrix4Rotation90()
    {
        const RotationDegree rot90 = RotationDegree.Rot90;
        int[] expected =
        [
            0x30, 0x20, 0x10, 0x00,
            0x31, 0x21, 0x11, 0x01,
            0x32, 0x22, 0x12, 0x02,
            0x33, 0x23, 0x13, 0x03
        ];
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                Assert.Equal(expected[(i * N) + j], m4x4.Calc(rot90, i, j) & 0xFF);
            }
        }
    }

    [Fact]
    public void Matrix4Rotation180()
    {
        const RotationDegree rot180 = RotationDegree.Rot180;
        int[] expected =
        [
            0x33, 0x32, 0x31, 0x30,
            0x23, 0x22, 0x21, 0x20,
            0x13, 0x12, 0x11, 0x10,
            0x03, 0x02, 0x01, 0x00
        ];
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                Assert.Equal(expected[(i * N) + j], m4x4.Calc(rot180, i, j) & 0xFF);
            }
        }
    }

    [Fact]
    public void Matrix4Rotation270()
    {
        const RotationDegree rot270 = RotationDegree.Rot270;
        int[] expected =
        [
            0x03, 0x13, 0x23, 0x33,
            0x02, 0x12, 0x22, 0x32,
            0x01, 0x11, 0x21, 0x31,
            0x00, 0x10, 0x20, 0x30
        ];
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                Assert.Equal(expected[(i * N) + j], m4x4.Calc(rot270, i, j) & 0xFF);
            }
        }
    }

    [Fact]
    public void Matrix3Rotation270()
    {
        const int n = 3;
        MatrixRotation m3x3 = MatrixRotation.Of(n);
        const RotationDegree rot270 = RotationDegree.Rot270;
        int[] expected =
        [
            0x2, 0x12, 0x22,
            0x1, 0x11, 0x21,
            0x0, 0x10, 0x20
        ];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                Assert.Equal(expected[(i * n) + j], m3x3.Calc(rot270, i, j) & 0xFF);
            }
        }
    }
}
