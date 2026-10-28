// XbrzSharp - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
using System.Buffers;
using System.Runtime.CompilerServices;
using Xbrz.ColorDistance;
using Xbrz.Scaler;

namespace Xbrz;

public class XbrzScaler
{
    public sealed class ScalerCfg(double luminanceWeight, double equalColorTolerance, double centerDirectionBias, double dominantDirectionThreshold, double steepDirectionThreshold)
    {
        public double LuminanceWeight { get; } = luminanceWeight;
        public double EqualColorTolerance { get; } = equalColorTolerance;
        public double CenterDirectionBias { get; } = centerDirectionBias;
        public double DominantDirectionThreshold { get; } = dominantDirectionThreshold;
        public double SteepDirectionThreshold { get; } = steepDirectionThreshold;

        public ScalerCfg() : this(1, 30, 4, 3.6, 2.2) { }

        public ScalerCfg WithLuminanceWeight(double luminanceWeight) => new(luminanceWeight, EqualColorTolerance, CenterDirectionBias, DominantDirectionThreshold, SteepDirectionThreshold);
        public ScalerCfg WithEqualColorTolerance(double equalColorTolerance) => new(LuminanceWeight, equalColorTolerance, CenterDirectionBias, DominantDirectionThreshold, SteepDirectionThreshold);
        public ScalerCfg WithCenterDirectionBias(double centerDirectionBias) => new(LuminanceWeight, EqualColorTolerance, centerDirectionBias, DominantDirectionThreshold, SteepDirectionThreshold);
        public ScalerCfg WithDominantDirectionThreshold(double dominantDirectionThreshold) => new(LuminanceWeight, EqualColorTolerance, CenterDirectionBias, dominantDirectionThreshold, SteepDirectionThreshold);
        public ScalerCfg WithSteepDirectionThreshold(double steepDirectionThreshold) => new(LuminanceWeight, EqualColorTolerance, CenterDirectionBias, DominantDirectionThreshold, steepDirectionThreshold);
    }

    private const int SoftMaxArrayLength = int.MaxValue - 8;

    private readonly AbstractScaler scaler;
    private readonly ColorDistance.ColorDistance dist;
    private readonly ScalerCfg cfg;
    private readonly bool withAlpha;

    public XbrzScaler(int factor) : this(factor, true) { }
    public XbrzScaler(int factor, bool withAlpha) : this(factor, withAlpha, new ScalerCfg()) { }
    public XbrzScaler(int factor, bool withAlpha, ScalerCfg cfg)
        : this(factor, withAlpha, cfg, ColorDistanceFactory.YCbCr(cfg.LuminanceWeight)) { }

    public XbrzScaler(int factor, bool withAlpha, ScalerCfg cfg, ColorDistance.ColorDistance dist)
    {
        scaler = ScalerFactory.ForFactor(factor, withAlpha);
        this.cfg = cfg;
        this.dist = withAlpha ? ColorDistanceFactory.WithAlpha(dist) : dist;
        this.withAlpha = withAlpha;
    }

    public int Factor() => scaler.Scale;
    public int Scale() => Factor();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double Dist(int pix1, int pix2) => dist(pix1, pix2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Eq(int pix1, int pix2) => Dist(pix1, pix2) < cfg.EqualColorTolerance;

    private void PreProcessCorners(Kernel4x4 ker, ref BlendResult result)
    {
        result.Reset();
        if ((ker.f == ker.g && ker.j == ker.k) || (ker.f == ker.j && ker.g == ker.k))
            return;
        double jg = Dist(ker.i, ker.f) + Dist(ker.f, ker.c) + Dist(ker.n, ker.k) + Dist(ker.k, ker.h) + (cfg.CenterDirectionBias * Dist(ker.j, ker.g));
        double fk = Dist(ker.e, ker.j) + Dist(ker.j, ker.o) + Dist(ker.b, ker.g) + Dist(ker.g, ker.l) + (cfg.CenterDirectionBias * Dist(ker.f, ker.k));
        if (jg < fk)
        {
            bool dominantGradient = cfg.DominantDirectionThreshold * jg < fk;
            if (ker.f != ker.g && ker.f != ker.j)
                result.BlendF = dominantGradient ? BlendType.Dominant : BlendType.Normal;
            if (ker.k != ker.j && ker.k != ker.g)
                result.BlendK = dominantGradient ? BlendType.Dominant : BlendType.Normal;
        }
        else if (fk < jg)
        {
            bool dominantGradient = cfg.DominantDirectionThreshold * fk < jg;
            if (ker.j != ker.f && ker.j != ker.k)
                result.BlendJ = dominantGradient ? BlendType.Dominant : BlendType.Normal;
            if (ker.g != ker.f && ker.g != ker.k)
                result.BlendG = dominantGradient ? BlendType.Dominant : BlendType.Normal;
        }
    }

    private void BlendPixel(RotationDegree rotDeg, Kernel3x3 ker, OutputMatrix outMatrix, byte blendInfo)
    {
        byte blend = BlendInfo.Rotate(blendInfo, rotDeg);
        if (BlendInfo.GetBottomR(blend) < BlendType.Normal)
            return;

        ker.SetRotation(rotDeg);
        outMatrix.SetRotationDegree(rotDeg);
        int e = ker.E;
        int f = ker.F;
        int h = ker.H;
        int g = ker.G;
        int c = ker.C;
        int i = ker.I;
        bool doLineBlend;
        if (BlendInfo.GetBottomR(blend) >= BlendType.Dominant)
            doLineBlend = true;
        else if (BlendInfo.GetTopR(blend) != BlendType.None && !Eq(e, g))
            doLineBlend = false;
        else if (BlendInfo.GetBottomL(blend) != BlendType.None && !Eq(e, c))
            doLineBlend = false;
        else if (!Eq(e, i) && Eq(g, h) && Eq(h, i) && Eq(i, f) && Eq(f, c))
            doLineBlend = false;
        else
            doLineBlend = true;

        int px = Dist(e, f) <= Dist(e, h) ? f : h;
        if (!doLineBlend)
        {
            scaler.BlendCorner(px, outMatrix);
            return;
        }

        double fg = Dist(f, g);
        double hc = Dist(h, c);
        bool haveShallowLine = cfg.SteepDirectionThreshold * fg <= hc && e != g && ker.D != g;
        bool haveSteepLine = cfg.SteepDirectionThreshold * hc <= fg && e != c && ker.B != c;
        if (haveShallowLine)
        {
            if (haveSteepLine)
                scaler.BlendLineSteepAndShallow(px, outMatrix);
            else
                scaler.BlendLineShallow(px, outMatrix);
        }
        else if (haveSteepLine)
        {
            scaler.BlendLineSteep(px, outMatrix);
        }
        else
        {
            scaler.BlendLineDiagonal(px, outMatrix);
        }
    }

    public int[] ScaleImage(int[] src, int[]? trg, int srcWidth, int srcHeight)
    {
        trg ??= new int[TargetArraySize(srcWidth, srcHeight, Factor())];
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

        Parallel.For(
            yFirst,
            yLast,
            localInit: () =>
            {
                byte[] preProcBuf = pool.Rent(srcWidth);
                var ker4 = new Kernel4x4(src, srcWidth, srcHeight, withAlpha);
                var outMatrix = new OutputMatrix(scale, trg, outWidth);
                return (preProcBuf, ker4, outMatrix);
            },
            body: (y, _, state) =>
            {
                var (preProcBuf, ker4, outMatrix) = state;
                BlendResult res = default;

                ker4.PositionY(y - 1);
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

                var ker3 = ker4.Kernel3x3;
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
                    preProcBuf[x] = blendXy1;
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

                return state;
            },
            localFinally: state => pool.Return(state.preProcBuf));
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
