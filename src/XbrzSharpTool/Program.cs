// xBrzTool / Test / Helper file
// Licensed under 0BSD
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using Xbrz;

namespace XbrzSharpTool;

static class Program
{
    static void PrintUsage()
    {
        Console.Error.WriteLine("Usage: xbrz <source> [scaling_factor]");
    }

    static int[] ImageToArgbArray(Image<Rgba32> img)
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
                    pixels[y * width + x] = px.ToArgb();
                }
            }
        });
        return pixels;
    }

    static Image<Rgba32> ArgbArrayToImage(int[] pixels, int width, int height)
    {
        var img = new Image<Rgba32>(width, height);
        img.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    int argb = pixels[y * width + x];
                    row[x] = Rgba32FromArgb(argb);
                }
            }
        });
        return img;
    }

    static Rgba32 Rgba32FromArgb(int argb)
    {
        byte a = (byte)(argb >> 24 & 0xFF);
        byte r = (byte)(argb >> 16 & 0xFF);
        byte g = (byte)(argb >> 8 & 0xFF);
        byte b = (byte)(argb & 0xFF);
        return new Rgba32(r, g, b, a);
    }

    static int ToArgb(this Rgba32 color)
    {
        return color.A << 24 | color.R << 16 | color.G << 8 | color.B;
    }

    static Image<Rgba32> ScaleImage(string sourcePath, int factor)
    {
        using var image = Image.Load<Rgba32>(sourcePath);
        int srcWidth = image.Width;
        int srcHeight = image.Height;
        int[] srcPixels = ImageToArgbArray(image);
        int destWidth = srcWidth * factor;
        int destHeight = srcHeight * factor;
        bool hasAlpha = true; // Rgba32 always has alpha
        var scaler = new XbrzScaler(factor, hasAlpha);
        int[] destPixels = scaler.ScaleImage(srcPixels, null, srcWidth, srcHeight);
        return ArgbArrayToImage(destPixels, destWidth, destHeight);
    }

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            Environment.Exit(1);
        }

        int factor = ParseScalingFactor(args);
        string source = args[0];

        if (Directory.Exists(source))
        {
            ProcessDirectory(source, factor);
        }
        else
        {
            ProcessFile(source, factor);
        }
    }

    static int ParseScalingFactor(string[] args)
    {
        int factor = 2;
        if (args.Length > 1)
        {
            if (!int.TryParse(args[1], out factor))
            {
                Console.Error.WriteLine("Invalid scaling factor.");
                PrintUsage();
                Environment.Exit(2);
            }
        }
        return factor;
    }

    static void ProcessDirectory(string directory, int factor)
    {
        var extensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff" };
        var files = Directory.EnumerateFiles(directory)
            .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

        foreach (var file in files)
        {
            try
            {
                ProcessFile(file, factor);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to process '{file}': {ex.Message}");
            }
        }
    }

    static void ProcessFile(string file, int factor)
    {
        string targetBase = Path.Combine(
            Path.GetDirectoryName(file) ?? "",
            Path.GetFileNameWithoutExtension(file)
        );
        string target = $"{targetBase}@{factor}x.png";

        using var scaled = ScaleImage(file, factor);
        scaled.Save(target, new PngEncoder());
        Console.WriteLine(target);
    }
}