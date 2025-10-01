// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
using System.Collections.Concurrent;

namespace Xbrz.ColorDistance;

internal class ColorDistanceYCbCrBuffered : ColorDistanceYCbCr
{
    private const int diffSize = 9;
    private readonly int sigBits;
    private readonly int adjBits;
    private readonly float[] diffToDist;
    private static readonly ConcurrentDictionary<int, WeakReference<ColorDistanceYCbCrBuffered>> instances = new();

    public ColorDistanceYCbCrBuffered(int sigBits) : base(1)
    {
        if (sigBits < 2 || sigBits > 8)
            throw new ArgumentException($"Illegal sigBits: {sigBits}");
        this.sigBits = sigBits;
        adjBits = diffSize - sigBits;
        diffToDist = new float[1 << 3 * sigBits];
        int bitMask = (1 << sigBits) - 1;
        for (int i = 0, len = diffToDist.Length; i < len; i++)
        {
            int r_diff = i >> (sigBits << 1) & bitMask;
            int g_diff = i >> sigBits & bitMask;
            int b_diff = i & bitMask;
            r_diff = (r_diff << adjBits) - 255 + (1 << adjBits - 1);
            g_diff = (g_diff << adjBits) - 255 + (1 << adjBits - 1);
            b_diff = (b_diff << adjBits) - 255 + (1 << adjBits - 1);
            double y = (k_r * r_diff) + (k_g * g_diff) + (k_b * b_diff);
            double c_b = scale_b * (b_diff - y);
            double c_r = scale_r * (r_diff - y);
            diffToDist[i] = (float)Math.Sqrt(Square(y) + Square(c_b) + Square(c_r));
        }
    }

    public static ColorDistanceYCbCrBuffered Instance(int sigBits)
    {
        ColorDistanceYCbCrBuffered obj = null;
        instances.AddOrUpdate(sigBits,
            k => new WeakReference<ColorDistanceYCbCrBuffered>(obj = new ColorDistanceYCbCrBuffered(k)),
            (k, existingRef) =>
            {
                if (existingRef == null || !existingRef.TryGetTarget(out obj))
                    existingRef = new WeakReference<ColorDistanceYCbCrBuffered>(obj = new ColorDistanceYCbCrBuffered(k));
                return existingRef;
            });
        return obj;
    }

    public override double Calc(int pix1, int pix2)
    {
        int r_diff = Color.GetRed(pix1) - Color.GetRed(pix2) + 255;
        int g_diff = Color.GetGreen(pix1) - Color.GetGreen(pix2) + 255;
        int b_diff = Color.GetBlue(pix1) - Color.GetBlue(pix2) + 255;
        int index = r_diff >> adjBits << (sigBits << 1) |
                    g_diff >> adjBits << sigBits |
                    b_diff >> adjBits;
        return diffToDist[index];
    }
}
