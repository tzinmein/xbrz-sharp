// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz;

internal sealed class BlendResult
{
    public byte blend_f, blend_g, blend_j, blend_k;
    private BlendResult() { }
    private static readonly ThreadLocal<BlendResult> instance = new();
    public static BlendResult Instance()
    {
        var result = instance.Value;
        if (result == null)
        {
            result = new BlendResult();
            instance.Value = result;
        }
        return result;
    }
    public void Reset() { blend_f = blend_g = blend_j = blend_k = BlendType.BLEND_NONE; }
}
