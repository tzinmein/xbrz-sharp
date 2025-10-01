// xBrzTool / Test / Helper file
// Licensed under 0BSD
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xbrz;

namespace xBrzNet.Tests;

public abstract class AbstractScalerTest
{
    protected abstract XbrzScaler XbrzScaler();

    [Theory]
    [InlineData("test/basic-shapes", 0.0001)]
    [InlineData("test/down-arrow", 0.0001)]
    [InlineData("test/open-folder", 0.0001)]
    public void BasicShapesAndAlphaImages(string name, double deviation)
    {
        TestImage(name, deviation);
    }

    [Fact]
    public void FullPicture()
    {
        // Test with alpha disabled, deviation 0.001
        TestImage(new XbrzScaler(XbrzScaler().Factor(), false), "test/gbamockup", 0.001);
    }

    [Fact]
    public void ColorDistanceRGB()
    {
        var scaler = new XbrzScaler(XbrzScaler().Factor(), false, new XbrzScaler.ScalerCfg(), Xbrz.ColorDistance.ColorDistanceFactory.Rgb());
        TestImage(scaler, "test/gbamockup", 3.0);
    }

    [Fact]
    public void ColorDistanceYCbCrBuffered()
    {
        var scaler = new XbrzScaler(XbrzScaler().Factor(), false, new XbrzScaler.ScalerCfg(), Xbrz.ColorDistance.ColorDistanceFactory.BufferedYCbCr(5));
        TestImage(scaler, "test/gbamockup", 3.0);
    }

    [Fact]
    public void AlphaNoAlpha()
    {
        var source = LoadImage("test/gbamockup.png");
        int srcWidth = source.Width;
        int srcHeight = source.Height;
        int[] srcPixels = GetPixels(source);
        int factor = XbrzScaler().Factor();
        int destWidth = srcWidth * factor;
        int destHeight = srcHeight * factor;
        int[] destPixels = new int[destWidth * destHeight];
        new XbrzScaler(factor, true).ScaleImage(srcPixels, destPixels, srcWidth, srcHeight);
        int[] dest2Pixels = new int[destWidth * destHeight];
        new XbrzScaler(factor, false).ScaleImage(srcPixels, dest2Pixels, srcWidth, srcHeight);
        AssertPixels($"AlphaNoAlpha-{factor}x", destPixels, dest2Pixels, 1.0);
    }

    protected void TestImage(string name, double deviation)
        => TestImage(XbrzScaler(), name, deviation);

    protected void TestImage(XbrzScaler scaler, string imageName, double deviation)
    {
        int factor = scaler.Factor();
        var source = LoadImage($"{imageName}.png");
        int srcWidth = source.Width;
        int srcHeight = source.Height;
        int[] srcPixels = GetPixels(source);
        int destWidth = srcWidth * factor;
        int destHeight = srcHeight * factor;
        int[] destPixels = new int[destWidth * destHeight];
        scaler.ScaleImage(srcPixels, destPixels, srcWidth, srcHeight);
        var reference = LoadImage($"{imageName}@{factor}xbrz.png");
        int[] refPixels = GetPixels(reference);
        AssertPixels($"{imageName}-{factor}x", destPixels, refPixels, deviation);
    }

    private static Image<Rgba32> LoadImage(string relativePath)
    {
        // Assumes test images are copied to output directory
        var path = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
        return Image.Load<Rgba32>(path);
    }

    private static int[] GetPixels(Image<Rgba32> img)
    {
        int width = img.Width;
        int height = img.Height;
        int[] pixels = new int[width * height];
        img.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    Rgba32 px = row[x];
                    pixels[y * width + x] = (px.A << 24) | (px.R << 16) | (px.G << 8) | px.B;
                }
            }
        });
        return pixels;
    }

    private static void AssertPixels(string name, int[] destPixels, int[] refPixels, double deviation)
    {
        Assert.Equal(refPixels.Length, destPixels.Length);
        int mismatch = 0;
        for (int i = 0; i < refPixels.Length; i++)
        {
            if (destPixels[i] != refPixels[i])
                mismatch++;
        }
        double percent = mismatch * 100.0 / destPixels.Length;
        string message = $"Pixel mismatch: {mismatch} ({percent:F3}%) [{name}]";
        Console.WriteLine(message);
        Assert.True(percent <= deviation, message);
    }
}
