// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz;

internal static class BlendInfo
{
    public static byte Rotate(byte b, RotationDegree rotDeg) => rotDeg switch
    {
        RotationDegree.Rot90 => (byte)((b << 2 & 0xFF) | (b & 0xFF) >> 6),
        RotationDegree.Rot180 => (byte)((b << 4 & 0xFF) | (b & 0xFF) >> 4),
        RotationDegree.Rot270 => (byte)((b << 6 & 0xFF) | (b & 0xFF) >> 2),
        _ => b
    };

    public static bool BlendingNeeded(byte b) => b != (byte)BlendType.None;
    public static BlendType GetTopR(byte b) => (BlendType)(0x3 & b >> 2);
    public static BlendType GetBottomR(byte b) => (BlendType)(0x3 & b >> 4);
    public static BlendType GetBottomL(byte b) => (BlendType)(0x3 & b >> 6);

    public static byte AddTopR(byte b, BlendType bt) => (byte)(b | (byte)bt << 2);
    public static byte AddBottomR(byte b, BlendType bt) => (byte)(b | (byte)bt << 4);

    public static void ClearAddTopL(byte[] buf, int i, BlendType bt) => buf[i] = (byte)bt;
    public static void AddTopR(byte[] buf, int i, BlendType bt) => buf[i] |= (byte)((byte)bt << 2);
    public static void AddBottomL(byte[] buf, int i, BlendType bt) => buf[i] |= (byte)((byte)bt << 6);
}
