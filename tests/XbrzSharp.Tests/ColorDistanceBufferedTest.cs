// xBrzTool / Test / Helper file
// Licensed under 0BSD
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
using Xbrz.ColorDistance;

namespace xBrzNet.Tests;

public class ColorDistanceBufferedTest
{
    private const double DefaultDelta = 8.0; // full 8 bits - 5 bits = 3 bits
    private static readonly ColorDistance colorDistance = ColorDistanceFactory.BufferedYCbCr(5);

    public static IEnumerable<object[]> Data()
    {
        // Example data, replace with actual ColorDistanceTest.data() equivalent
        // Each row: color1, color2, expected, delta
        yield return new object[] { unchecked((int)0xFF000000), unchecked((int)0xFF000000), 0.0, DefaultDelta }; // black vs black
        yield return new object[] { unchecked((int)0xFFFFFFFF), unchecked((int)0xFF000000), 255.0, DefaultDelta }; // white vs black (example)
        // Add more test cases as needed
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void TestColorDistance(int color1, int color2, double expected, double delta)
    {
        double actual = colorDistance(color1, color2);
        Assert.InRange(actual, expected - delta, expected + delta);
    }
}
