// xBrzNet - .NET port of xBRZ
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

    private const int SOFT_MAX_ARRAY_LENGTH = int.MaxValue - 8;
    private const bool DEBUG = false;

    private readonly IScaler scaler;
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

        if (this.dist == null)
            throw new InvalidOperationException("ColorDistance delegate is null after construction!");
    }

    public int Factor() => scaler.Scale;
    public int Scale() => Factor();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double Dist(int pix1, int pix2) => dist(pix1, pix2);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Eq(int pix1, int pix2) => Dist(pix1, pix2) < cfg.EqualColorTolerance;

    private void PreProcessCorners(Kernel4x4 ker, BlendResult result)
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
                result.blend_f = dominantGradient ? BlendType.BLEND_DOMINANT : BlendType.BLEND_NORMAL;
            if (ker.k != ker.j && ker.k != ker.g)
                result.blend_k = dominantGradient ? BlendType.BLEND_DOMINANT : BlendType.BLEND_NORMAL;
        }
        else if (fk < jg)
        {
            bool dominantGradient = cfg.DominantDirectionThreshold * fk < jg;
            if (ker.j != ker.f && ker.j != ker.k)
                result.blend_j = dominantGradient ? BlendType.BLEND_DOMINANT : BlendType.BLEND_NORMAL;
            if (ker.g != ker.f && ker.g != ker.k)
                result.blend_g = dominantGradient ? BlendType.BLEND_DOMINANT : BlendType.BLEND_NORMAL;
        }
    }

    private void BlendPixel(RotationDegree rotDeg, Kernel3x3 ker, OutputMatrix outMatrix, byte blendInfo)
    {
        byte blend = BlendInfo.Rotate(blendInfo, rotDeg);
        if (BlendInfo.GetBottomR(blend) >= BlendType.BLEND_NORMAL)
        {
            ker.RotDeg(rotDeg);
            outMatrix.SetRotationDegree(rotDeg);
            int e = ker.e();
            int f = ker.f();
            int h = ker.h();
            int g = ker.g();
            int c = ker.c();
            int i = ker.i();
            bool doLineBlend;
            if (BlendInfo.GetBottomR(blend) >= BlendType.BLEND_DOMINANT)
                doLineBlend = true;
            else if (BlendInfo.GetTopR(blend) != BlendType.BLEND_NONE && !Eq(e, g))
                doLineBlend = false;
            else if (BlendInfo.GetBottomL(blend) != BlendType.BLEND_NONE && !Eq(e, c))
                doLineBlend = false;
            else if (!Eq(e, i) && Eq(g, h) && Eq(h, i) && Eq(i, f) && Eq(f, c))
                doLineBlend = false;
            else
                doLineBlend = true;
            int px = Dist(e, f) <= Dist(e, h) ? f : h;
            if (doLineBlend)
            {
                double fg = Dist(f, g);
                double hc = Dist(h, c);
                bool haveShallowLine = cfg.SteepDirectionThreshold * fg <= hc && e != g && ker.d() != g;
                bool haveSteepLine = cfg.SteepDirectionThreshold * hc <= fg && e != c && ker.b() != c;
                if (haveShallowLine)
                {
                    if (haveSteepLine)
                        scaler.BlendLineSteepAndShallow(px, outMatrix);
                    else
                        scaler.BlendLineShallow(px, outMatrix);
                }
                else
                {
                    if (haveSteepLine)
                        scaler.BlendLineSteep(px, outMatrix);
                    else
                        scaler.BlendLineDiagonal(px, outMatrix);
                }
            }
            else
            {
                scaler.BlendCorner(px, outMatrix);
            }
        }
    }

    public int[] ScaleImage(int[] src, int[] trg, int srcWidth, int srcHeight)
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

        Parallel.For(yFirst, yLast, y =>
        {
            var pool = ArrayPool<byte>.Shared;
            byte[] preProcBuf = pool.Rent(srcWidth);
            try
            {
                var ker4 = Kernel4x4.Instance(src, srcWidth, srcHeight, withAlpha);
                var outMatrix = OutputMatrix.Instance(scaler.Scale, trg, srcWidth * scaler.Scale);
                var res = BlendResult.Instance();

                ker4.PositionY(y - 1);
                PreProcessCorners(ker4, res);
                BlendInfo.ClearAddTopL(preProcBuf, 0, res.blend_k);
                for (int x = 0; x < srcWidth; ++x)
                {
                    ker4.Shift();
                    ker4.ReadDhlp(x);
                    PreProcessCorners(ker4, res);
                    BlendInfo.AddTopR(preProcBuf, x, res.blend_j);
                    if (x + 1 < srcWidth)
                        BlendInfo.ClearAddTopL(preProcBuf, x + 1, res.blend_k);
                }
                var ker3 = ker4.Kernel3x3();
                outMatrix.PositionY(y);
                ker4.PositionY(y);
                byte blend_xy1;
                PreProcessCorners(ker4, res);
                blend_xy1 = BlendInfo.ClearAddTopL(res.blend_k);
                BlendInfo.AddBottomL(preProcBuf, 0, res.blend_g);
                for (int x = 0; x < srcWidth; ++x, outMatrix.IncrementX())
                {
                    ker4.Shift();
                    ker4.ReadDhlp(x);
                    byte blend_xy = preProcBuf[x];
                    PreProcessCorners(ker4, res);
                    blend_xy = BlendInfo.AddBottomR(blend_xy, res.blend_f);
                    blend_xy1 = BlendInfo.AddTopR(blend_xy1, res.blend_j);
                    preProcBuf[x] = blend_xy1;
                    if (x + 1 < srcWidth)
                    {
                        blend_xy1 = BlendInfo.ClearAddTopL(res.blend_k);
                        BlendInfo.AddBottomL(preProcBuf, x + 1, res.blend_g);
                    }
                    outMatrix.FillBlock(ker4.f);
                    if (BlendInfo.BlendingNeeded(blend_xy))
                    {
                        BlendPixel(RotationDegree.Rot0, ker3, outMatrix, blend_xy);
                        BlendPixel(RotationDegree.Rot90, ker3, outMatrix, blend_xy);
                        BlendPixel(RotationDegree.Rot180, ker3, outMatrix, blend_xy);
                        BlendPixel(RotationDegree.Rot270, ker3, outMatrix, blend_xy);
                    }
                }
            }
            finally
            {
                pool.Return(preProcBuf);
            }
        });
    }

    public static int[] ScaleImage(int factor, bool hasAlpha, int[] src, int[] trg, int srcWidth, int srcHeight)
    {
        return new XbrzScaler(factor, hasAlpha).ScaleImage(src, trg, srcWidth, srcHeight);
    }

    public static int TargetArraySize(int sourceWidth, int sourceHeight, int factor)
    {
        string Message() => $"Target size exceeds implementation limits (sourceWidth: {sourceWidth}, sourceHeight: {sourceHeight}, scaleFactor: {factor})";
        try
        {
            checked
            {
                int targetSize = checked(sourceWidth * factor * sourceHeight * factor);
                if (targetSize > SOFT_MAX_ARRAY_LENGTH)
                    throw new OutOfMemoryException(Message());
                return targetSize;
            }
        }
        catch (OverflowException)
        {
            if (DEBUG)
                Console.Error.WriteLine("DEBUG: Overflow in TargetArraySize");
            throw new OutOfMemoryException(Message());
        }
    }
}
