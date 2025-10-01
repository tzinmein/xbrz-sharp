// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz.Scaler;

internal static class ScalerFactory
{
    public static IScaler ForFactor(int factor, bool withAlpha)
    {
        return factor switch
        {
            2 => new Scaler2x(withAlpha),
            3 => new Scaler3x(withAlpha),
            4 => new Scaler4x(withAlpha),
            5 => new Scaler5x(withAlpha),
            6 => new Scaler6x(withAlpha),
            _ => throw new ArgumentException($"Illegal scaling factor: {factor}")
        };
    }
}
