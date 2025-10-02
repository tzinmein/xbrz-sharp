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

public class ColorDistanceTest
{
    private const double DefaultDelta = 0.000005;
    private static readonly ColorDistance colorDistance = ColorDistanceFactory.YCbCr(1);

    public static IEnumerable<object[]> Data()
    {
        yield return new object[] { "#000000", "#000000",   0.0,      DefaultDelta };
        yield return new object[] { "#000000", "#404040",  64.0,      DefaultDelta };
        yield return new object[] { "#000000", "#808080", 128.0,      DefaultDelta };
        yield return new object[] { "#000000", "#C0C0C0", 192.0,      DefaultDelta };
        yield return new object[] { "#000000", "#FFFFFF", 255.0,      DefaultDelta };
        yield return new object[] { "#403050", "#000000",  56.226890, DefaultDelta };
        yield return new object[] { "#403050", "#404040",  18.236254, DefaultDelta };
        yield return new object[] { "#403050", "#808080",  75.469587, DefaultDelta };
        yield return new object[] { "#403050", "#C0B0D0", 128.0,      DefaultDelta };
        yield return new object[] { "#403050", "#FFFFFF", 201.482146, DefaultDelta };
        yield return new object[] { "#808080", "#000000", 128.0,      DefaultDelta };
        yield return new object[] { "#808080", "#404040",  64.0,      DefaultDelta };
        yield return new object[] { "#808080", "#808080",   0.0,      DefaultDelta };
        yield return new object[] { "#808080", "#C0C0C0",  64.0,      DefaultDelta };
        yield return new object[] { "#808080", "#FFFFFF", 127.0,      DefaultDelta };
        yield return new object[] { "#C0D0B0", "#000000", 202.479267, DefaultDelta };
        yield return new object[] { "#C0D0B0", "#405030", 128.0,      DefaultDelta };
        yield return new object[] { "#C0D0B0", "#808080",  75.469587, DefaultDelta };
        yield return new object[] { "#C0D0B0", "#C0C0C0",  18.236254, DefaultDelta };
        yield return new object[] { "#C0D0B0", "#FFFFFF",  55.265375, DefaultDelta };
        yield return new object[] { "#FFFFFF", "#000000", 255.0,      DefaultDelta };
        yield return new object[] { "#FFFFFF", "#404040", 191.0,      DefaultDelta };
        yield return new object[] { "#FFFFFF", "#808080", 127.0,      DefaultDelta };
        yield return new object[] { "#FFFFFF", "#C0C0C0",  63.0,      DefaultDelta };
        yield return new object[] { "#FFFFFF", "#FFFFFF",   0.0,      DefaultDelta };
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
        var color = Color.Parse(html).ToPixel<Rgba32>();
        return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
    }
}
