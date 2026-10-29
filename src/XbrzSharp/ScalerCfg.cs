// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz;

public partial class XbrzScaler
{
    public sealed record ScalerCfg(
        double LuminanceWeight = 1,
        double EqualColorTolerance = 30,
        double CenterDirectionBias = 4,
        double DominantDirectionThreshold = 3.6,
        double SteepDirectionThreshold = 2.2)
    {
        public ScalerCfg WithLuminanceWeight(double luminanceWeight)
            => this with { LuminanceWeight = luminanceWeight };

        public ScalerCfg WithEqualColorTolerance(double equalColorTolerance)
            => this with { EqualColorTolerance = equalColorTolerance };

        public ScalerCfg WithCenterDirectionBias(double centerDirectionBias)
            => this with { CenterDirectionBias = centerDirectionBias };

        public ScalerCfg WithDominantDirectionThreshold(double dominantDirectionThreshold)
            => this with { DominantDirectionThreshold = dominantDirectionThreshold };

        public ScalerCfg WithSteepDirectionThreshold(double steepDirectionThreshold)
            => this with { SteepDirectionThreshold = steepDirectionThreshold };
    }
}
