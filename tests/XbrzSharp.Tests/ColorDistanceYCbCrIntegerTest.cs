// xBrzTool / Test / Helper file
// Licensed under 0BSD
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xbrz.ColorDistance;

namespace xBrzNet.Tests;

public class ColorDistanceYCbCrIntegerTest
{
    private const double DefaultDelta = 0.000005;
    private static readonly ColorDistance colorDistance = new ColorDistanceYCbCrInteger(1).Calc;

    public static TheoryData<string, string, double, double> Data()
    {
        double[] deltas = [
            DefaultDelta,
            DefaultDelta,
            DefaultDelta,
            DefaultDelta,
            DefaultDelta,
            0.014,
            0.043,
            0.005,
            DefaultDelta,
            0.003,
            DefaultDelta,
            DefaultDelta,
            DefaultDelta,
            DefaultDelta,
            DefaultDelta,
            0.003,
            DefaultDelta,
            0.005,
            0.043,
            0.012,
            DefaultDelta,
            DefaultDelta,
            DefaultDelta,
            DefaultDelta,
            DefaultDelta
        ];

        var baseData = ColorDistanceTest.Data().ToArray();
        var theoryData = new TheoryData<string, string, double, double>();
        for (int i = 0; i < baseData.Length; i++)
        {
            var row = baseData[i];
            theoryData.Add((string)row[0], (string)row[1], (double)row[2], deltas[i]);
        }
        return theoryData;
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void Calc(string pix1, string pix2, double expectedDistance, double floatDelta)
    {
        int color1 = HtmlToArgb(pix1);
        int color2 = HtmlToArgb(pix2);
        double actual = colorDistance(color1, color2);
        Assert.InRange(actual, expectedDistance - floatDelta, expectedDistance + floatDelta);
    }

    private static int HtmlToArgb(string html)
    {
        var color = SixLabors.ImageSharp.Color.Parse(html).ToPixel<Rgba32>();
        return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
    }
}
