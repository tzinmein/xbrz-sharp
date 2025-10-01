// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz;

internal static class Color
{
    public static int GetAlpha(int pix) => pix >> 24 & 0xFF;
    public static int GetRed(int pix)   => pix >> 16 & 0xFF;
    public static int GetGreen(int pix) => pix >> 8  & 0xFF;
    public static int GetBlue(int pix)  => pix >> 0  & 0xFF;

    public static int MakePixel(int a, int r, int g, int b) => a << 24 | r << 16 | g << 8 | b;
    public static int MakePixel(int r, int g, int b) => 0xFF << 24 | r << 16 | g << 8 | b;
}
