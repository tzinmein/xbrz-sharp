// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz;

internal struct BlendResult
{
    public BlendType BlendF, BlendG, BlendJ, BlendK;

    public void Reset() => BlendF = BlendG = BlendJ = BlendK = BlendType.None;
}
