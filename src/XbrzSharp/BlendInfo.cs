// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz;

internal static class BlendInfo
{
    public static byte Rotate(byte b, RotationDegree rotDeg)
    {
        return rotDeg switch
        {
            RotationDegree.Rot90 => (byte)((b << 2 & 0xFF) | (b & 0xFF) >> 6),
            RotationDegree.Rot180 => (byte)((b << 4 & 0xFF) | (b & 0xFF) >> 4),
            RotationDegree.Rot270 => (byte)((b << 6 & 0xFF) | (b & 0xFF) >> 2),
            _ => b
        };
    }
    public static bool BlendingNeeded(byte b) => b != BlendType.BLEND_NONE;
    public static byte GetTopR(byte b) => (byte)(0x3 & b >> 2);
    public static byte GetBottomR(byte b) => (byte)(0x3 & b >> 4);
    public static byte GetBottomL(byte b) => (byte)(0x3 & b >> 6);
    public static byte ClearAddTopL(byte bt) => bt;
    public static byte AddTopR(byte b, byte bt) => (byte)(b | bt << 2);
    public static byte AddBottomR(byte b, byte bt) => (byte)(b | bt << 4);
    public static void ClearAddTopL(byte[] buf, int i, byte bt) { buf[i] = bt; }
    public static void AddTopR(byte[] buf, int i, byte bt) { buf[i] |= (byte)(bt << 2); }
    public static void AddBottomL(byte[] buf, int i, byte bt) { buf[i] |= (byte)(bt << 6); }
}
