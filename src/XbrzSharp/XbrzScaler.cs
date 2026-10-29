// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Xbrz.ColorDistance;
using Xbrz.Scaler;

namespace Xbrz;

public partial class XbrzScaler
{
    private const int SoftMaxArrayLength = int.MaxValue - 8;

    private readonly AbstractScaler scaler;
    private readonly ColorDistance.ColorDistance? customDist;
    private readonly bool withAlpha;
    private readonly double lumaWeight;
    private readonly double equalTol;
    private readonly double equalTolSq;
    private readonly double centerBias;
    private readonly double dominantTol;
    private readonly double steepTol;
    private readonly double steepTolSq;
    private readonly bool useDistSq; // safe for default YCbCr without alpha wrap

    public ScalerCfg Cfg { get; }
    public int Factor => scaler.Scale;

    public XbrzScaler(int factor) : this(factor, true) { }
    public XbrzScaler(int factor, bool withAlpha) : this(factor, withAlpha, new ScalerCfg()) { }

    public XbrzScaler(int factor, bool withAlpha, ScalerCfg cfg)
        : this(Create(factor, withAlpha, cfg, customDist: null)) { }

    public XbrzScaler(int factor, bool withAlpha, ScalerCfg cfg, ColorDistance.ColorDistance dist)
        : this(Create(factor, withAlpha, cfg, customDist: dist ?? throw new ArgumentNullException(nameof(dist)))) { }

    private XbrzScaler(in InitState state)
    {
        scaler = state.Scaler;
        customDist = state.CustomDist;
        Cfg = state.Cfg;
        withAlpha = state.WithAlpha;
        lumaWeight = state.LumaWeight;
        equalTol = state.EqualTol;
        equalTolSq = state.EqualTolSq;
        centerBias = state.CenterBias;
        dominantTol = state.DominantTol;
        steepTol = state.SteepTol;
        steepTolSq = state.SteepTolSq;
        useDistSq = state.UseDistSq;
    }

    private readonly record struct InitState(
        AbstractScaler Scaler,
        ColorDistance.ColorDistance? CustomDist,
        ScalerCfg Cfg,
        bool WithAlpha,
        double LumaWeight,
        double EqualTol,
        double EqualTolSq,
        double CenterBias,
        double DominantTol,
        double SteepTol,
        double SteepTolSq,
        bool UseDistSq);

    private static InitState Create(int factor, bool withAlpha, ScalerCfg cfg, ColorDistance.ColorDistance? customDist)
    {
        ArgumentNullException.ThrowIfNull(cfg);
        double equalTol = cfg.EqualColorTolerance;
        double steepTol = cfg.SteepDirectionThreshold;
        return new InitState(
            Scaler: ScalerFactory.ForFactor(factor, withAlpha),
            CustomDist: customDist,
            Cfg: cfg,
            WithAlpha: withAlpha,
            LumaWeight: cfg.LuminanceWeight,
            EqualTol: equalTol,
            EqualTolSq: equalTol * equalTol,
            CenterBias: cfg.CenterDirectionBias,
            DominantTol: cfg.DominantDirectionThreshold,
            SteepTol: steepTol,
            SteepTolSq: steepTol * steepTol,
            UseDistSq: customDist is null && !withAlpha);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double WrapAlpha(int pix1, int pix2, double d)
    {
        int a1 = Color.GetAlpha(pix1);
        int a2 = Color.GetAlpha(pix2);
        return a1 < a2 ? (a1 / 255.0 * d) + (a2 - a1)
                       : (a2 / 255.0 * d) + (a1 - a2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double Dist(int pix1, int pix2)
    {
        double d = customDist is null
            ? YCbCrMath.Distance(lumaWeight, pix1, pix2)
            : customDist(pix1, pix2);
        return withAlpha ? WrapAlpha(pix1, pix2, d) : d;
    }

    /// <summary>
    /// Comparison metric for order / threshold tests. Uses DistSq when safe
    /// (default YCbCr, no alpha wrap); otherwise Dist. Not valid for summing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double Cmp(int pix1, int pix2)
        => useDistSq ? YCbCrMath.DistanceSquared(lumaWeight, pix1, pix2) : Dist(pix1, pix2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Eq(int pix1, int pix2)
        => useDistSq
            ? YCbCrMath.DistanceSquared(lumaWeight, pix1, pix2) < equalTolSq
            : Dist(pix1, pix2) < equalTol;

    private void PreProcessCorners(Kernel4x4 ker, ref BlendResult result)
    {
        result.Reset();
        if ((ker.f == ker.g && ker.j == ker.k) || (ker.f == ker.j && ker.g == ker.k))
            return;
        // Sums of distances — must use Dist (not DistSq / Cmp).
        double jg = Dist(ker.i, ker.f) + Dist(ker.f, ker.c) + Dist(ker.n, ker.k) + Dist(ker.k, ker.h) + (centerBias * Dist(ker.j, ker.g));
        double fk = Dist(ker.e, ker.j) + Dist(ker.j, ker.o) + Dist(ker.b, ker.g) + Dist(ker.g, ker.l) + (centerBias * Dist(ker.f, ker.k));
        if (jg < fk)
        {
            bool dominantGradient = dominantTol * jg < fk;
            if (ker.f != ker.g && ker.f != ker.j)
                result.BlendF = dominantGradient ? BlendType.Dominant : BlendType.Normal;
            if (ker.k != ker.j && ker.k != ker.g)
                result.BlendK = dominantGradient ? BlendType.Dominant : BlendType.Normal;
        }
        else if (fk < jg)
        {
            bool dominantGradient = dominantTol * fk < jg;
            if (ker.j != ker.f && ker.j != ker.k)
                result.BlendJ = dominantGradient ? BlendType.Dominant : BlendType.Normal;
            if (ker.g != ker.f && ker.g != ker.k)
                result.BlendG = dominantGradient ? BlendType.Dominant : BlendType.Normal;
        }
    }

    private void BlendPixel(RotationDegree rotDeg, Kernel3x3 ker, OutputMatrix outMatrix, byte blendInfo)
    {
        byte blend = BlendInfo.Rotate(blendInfo, rotDeg);
        BlendType bottomR = BlendInfo.GetBottomR(blend);
        if (bottomR < BlendType.Normal)
            return;

        ker.SetRotation(rotDeg);
        outMatrix.SetRotationDegree(rotDeg);
        int e = ker.E, f = ker.F, h = ker.H, g = ker.G, c = ker.C, i = ker.I;

        bool doLineBlend = bottomR >= BlendType.Dominant
            || (BlendInfo.GetTopR(blend) == BlendType.None || Eq(e, g))
            && (BlendInfo.GetBottomL(blend) == BlendType.None || Eq(e, c))
            && (Eq(e, i) || !Eq(g, h) || !Eq(h, i) || !Eq(i, f) || !Eq(f, c));

        int px = Cmp(e, f) <= Cmp(e, h) ? f : h;
        if (!doLineBlend)
        {
            scaler.BlendCorner(px, outMatrix);
            return;
        }

        // Dist: steepTol * d1 <= d2; DistSq: steepTol² * d1² <= d2²
        double steepFactor = useDistSq ? steepTolSq : steepTol;
        double fg = Cmp(f, g);
        double hc = Cmp(h, c);
        bool haveShallowLine = steepFactor * fg <= hc && e != g && ker.D != g;
        bool haveSteepLine = steepFactor * hc <= fg && e != c && ker.B != c;

        if (haveShallowLine && haveSteepLine)
            scaler.BlendLineSteepAndShallow(px, outMatrix);
        else if (haveShallowLine)
            scaler.BlendLineShallow(px, outMatrix);
        else if (haveSteepLine)
            scaler.BlendLineSteep(px, outMatrix);
        else
            scaler.BlendLineDiagonal(px, outMatrix);
    }

    public int[] ScaleImage(int[] src, int[]? trg, int srcWidth, int srcHeight)
    {
        trg ??= new int[TargetArraySize(srcWidth, srcHeight, Factor)];
        ScaleImage(src, trg, srcWidth, srcHeight, 0, srcHeight);
        return trg;
    }

    public void ScaleImage(int[] src, int[] trg, int srcWidth, int srcHeight, int yFirst, int yLast)
    {
        yFirst = Math.Max(yFirst, 0);
        yLast = Math.Min(yLast, srcHeight);
        if (yFirst >= yLast || srcWidth <= 0)
            return;

        int scale = scaler.Scale;
        int outWidth = srcWidth * scale;
        var pool = ArrayPool<byte>.Shared;

        // Stripe consecutive rows so each worker can roll preProcBuf (Java/C++ style)
        // instead of re-preprocessing y-1 on every row.
        Parallel.ForEach(Partitioner.Create(yFirst, yLast), range =>
        {
            byte[] preProcBuf = pool.Rent(srcWidth);
            try
            {
                var ker4 = new Kernel4x4(src, srcWidth, srcHeight, withAlpha);
                var outMatrix = new OutputMatrix(scale, trg, outWidth);
                var ker3 = ker4.Kernel3x3;
                BlendResult res = default;

                // Seed top corners for the first row of this stripe from yStart - 1.
                ker4.PositionY(range.Item1 - 1);
                PreProcessCorners(ker4, ref res);
                BlendInfo.ClearAddTopL(preProcBuf, 0, res.BlendK);
                for (int x = 0; x < srcWidth; ++x)
                {
                    ker4.ShiftAndReadDhlp(x);
                    PreProcessCorners(ker4, ref res);
                    BlendInfo.AddTopR(preProcBuf, x, res.BlendJ);
                    if (x + 1 < srcWidth)
                        BlendInfo.ClearAddTopL(preProcBuf, x + 1, res.BlendK);
                }

                for (int y = range.Item1; y < range.Item2; ++y)
                {
                    outMatrix.PositionY(y);
                    ker4.PositionY(y);
                    PreProcessCorners(ker4, ref res);
                    byte blendXy1 = (byte)res.BlendK;
                    BlendInfo.AddBottomL(preProcBuf, 0, res.BlendG);
                    for (int x = 0; x < srcWidth; ++x, outMatrix.IncrementX())
                    {
                        ker4.ShiftAndReadDhlp(x);
                        PreProcessCorners(ker4, ref res);
                        byte blendXy = BlendInfo.AddBottomR(preProcBuf[x], res.BlendF);
                        blendXy1 = BlendInfo.AddTopR(blendXy1, res.BlendJ);
                        preProcBuf[x] = blendXy1; // tops for (x, y+1) — next row
                        if (x + 1 < srcWidth)
                        {
                            blendXy1 = (byte)res.BlendK;
                            BlendInfo.AddBottomL(preProcBuf, x + 1, res.BlendG);
                        }
                        outMatrix.FillBlock(ker4.f);
                        if (BlendInfo.BlendingNeeded(blendXy))
                        {
                            BlendPixel(RotationDegree.Rot0, ker3, outMatrix, blendXy);
                            BlendPixel(RotationDegree.Rot90, ker3, outMatrix, blendXy);
                            BlendPixel(RotationDegree.Rot180, ker3, outMatrix, blendXy);
                            BlendPixel(RotationDegree.Rot270, ker3, outMatrix, blendXy);
                        }
                    }
                }
            }
            finally
            {
                pool.Return(preProcBuf);
            }
        });
    }

    public static int[] ScaleImage(int factor, bool hasAlpha, int[] src, int[]? trg, int srcWidth, int srcHeight)
        => new XbrzScaler(factor, hasAlpha).ScaleImage(src, trg, srcWidth, srcHeight);

    public static int TargetArraySize(int sourceWidth, int sourceHeight, int factor)
    {
        string Message() => $"Target size exceeds implementation limits (sourceWidth: {sourceWidth}, sourceHeight: {sourceHeight}, scaleFactor: {factor})";
        try
        {
            checked
            {
                int targetSize = sourceWidth * factor * sourceHeight * factor;
                if (targetSize > SoftMaxArrayLength)
                    throw new OutOfMemoryException(Message());
                return targetSize;
            }
        }
        catch (OverflowException)
        {
            throw new OutOfMemoryException(Message());
        }
    }
}
