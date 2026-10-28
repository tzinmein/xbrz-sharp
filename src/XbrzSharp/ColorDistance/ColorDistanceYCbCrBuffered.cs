// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.ColorDistance;

internal sealed class ColorDistanceYCbCrBuffered
{
    private const int DiffSize = 9;
    private readonly int sigBits;
    private readonly int adjBits;
    private readonly float[] diffToDist;

    // sigBits 5 ≈ 128KB; sigBits 8 ≈ 64MB
    public ColorDistanceYCbCrBuffered(int sigBits)
    {
        if (sigBits < 2 || sigBits > 8)
            throw new ArgumentException($"Illegal sigBits: {sigBits}");
        this.sigBits = sigBits;
        adjBits = DiffSize - sigBits;
        diffToDist = new float[1 << 3 * sigBits];
        int bitMask = (1 << sigBits) - 1;
        for (int i = 0, len = diffToDist.Length; i < len; i++)
        {
            int rDiff = i >> (sigBits << 1) & bitMask;
            int gDiff = i >> sigBits & bitMask;
            int bDiff = i & bitMask;
            rDiff = (rDiff << adjBits) - 255 + (1 << adjBits - 1);
            gDiff = (gDiff << adjBits) - 255 + (1 << adjBits - 1);
            bDiff = (bDiff << adjBits) - 255 + (1 << adjBits - 1);
            double y = (YCbCrMath.Kr * rDiff) + (YCbCrMath.Kg * gDiff) + (YCbCrMath.Kb * bDiff);
            double cb = YCbCrMath.ScaleB * (bDiff - y);
            double cr = YCbCrMath.ScaleR * (rDiff - y);
            diffToDist[i] = (float)Math.Sqrt(YCbCrMath.Square(y) + YCbCrMath.Square(cb) + YCbCrMath.Square(cr));
        }
    }

    public double Calc(int pix1, int pix2)
    {
        int rDiff = Color.GetRed(pix1) - Color.GetRed(pix2) + 255;
        int gDiff = Color.GetGreen(pix1) - Color.GetGreen(pix2) + 255;
        int bDiff = Color.GetBlue(pix1) - Color.GetBlue(pix2) + 255;
        int index = rDiff >> adjBits << (sigBits << 1) |
                    gDiff >> adjBits << sigBits |
                    bDiff >> adjBits;
        return diffToDist[index];
    }
}
