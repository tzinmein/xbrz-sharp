// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz;

internal sealed class Kernel4x4
{
    public int a, b, c, e, f, g, i, j, k, m, n, o, d, h, l, p;
    private int[] src = [];
    private int srcWidth, srcHeight;
    private bool withAlpha;
    private int s_m1, s_0, s_p1, s_p2;
    private readonly Kernel3x3 ker3;
    private Kernel4x4() { ker3 = new Kernel3x3(this); }
    private static readonly ThreadLocal<Kernel4x4> instance = new();
    public static Kernel4x4 Instance(int[] src, int srcWidth, int srcHeight, bool withAlpha)
    {
        var kernel = instance.Value;
        if (kernel == null)
        {
            kernel = new Kernel4x4();
            instance.Value = kernel;
        }
        kernel.src = src;
        kernel.srcWidth = srcWidth;
        kernel.srcHeight = srcHeight;
        kernel.withAlpha = withAlpha;
        return kernel;
    }
    public Kernel3x3 Kernel3x3() => ker3;
    public void PositionY(int y)
    {
        if (withAlpha) PositionYTransparent(y); else PositionYDuplicate(y);
        ReadDhlp(-4); a = d; e = h; i = l; m = p;
        ReadDhlp(-3); b = d; f = h; j = l; n = p;
        ReadDhlp(-2); c = d; g = h; k = l; o = p;
        ReadDhlp(-1);
    }
    private void PositionYTransparent(int y)
    {
        s_m1 = 0 <= y - 1 && y - 1 < srcHeight ? srcWidth * (y - 1) : -1;
        s_0  = 0 <= y     && y     < srcHeight ? srcWidth * y       : -1;
        s_p1 = 0 <= y + 1 && y + 1 < srcHeight ? srcWidth * (y + 1) : -1;
        s_p2 = 0 <= y + 2 && y + 2 < srcHeight ? srcWidth * (y + 2) : -1;
    }
    private void PositionYDuplicate(int y)
    {
        s_m1 = srcWidth * Clamp(y - 1, 0, srcHeight - 1);
        s_0  = srcWidth * Clamp(y,     0, srcHeight - 1);
        s_p1 = srcWidth * Clamp(y + 1, 0, srcHeight - 1);
        s_p2 = srcWidth * Clamp(y + 2, 0, srcHeight - 1);
    }
    private static int Clamp(int v, int lo, int hi) => v < lo ? lo : v > hi ? hi : v;
    public void ReadDhlp(int x)
    {
        if (withAlpha) ReadDhlpTransparent(x); else ReadDhlpDuplicate(x);
    }
    private void ReadDhlpTransparent(int x)
    {
        int x_p2 = x + 2;
        if (0 <= x_p2 && x_p2 < srcWidth)
        {
            d = s_m1 >= 0 ? src[s_m1 + x_p2] : 0;
            h = s_0  >= 0 ? src[s_0  + x_p2] : 0;
            l = s_p1 >= 0 ? src[s_p1 + x_p2] : 0;
            p = s_p2 >= 0 ? src[s_p2 + x_p2] : 0;
        }
        else { d = h = l = p = 0; }
    }
    private void ReadDhlpDuplicate(int x)
    {
        int xc_p2 = Clamp(x + 2, 0, srcWidth - 1);
        d = src[s_m1 + xc_p2];
        h = src[s_0  + xc_p2];
        l = src[s_p1 + xc_p2];
        p = src[s_p2 + xc_p2];
    }
    public void Shift()
    {
        a = b; e = f; i = j; m = n;
        b = c; f = g; j = k; n = o;
        c = d; g = h; k = l; o = p;
    }
}
