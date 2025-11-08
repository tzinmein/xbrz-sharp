using BenchmarkDotNet.Attributes;
using Xbrz;

namespace XbrzSharp.Benchmarks;

public enum ImageContentType
{
    Solid,
    Gradient,
    Random
}

[MemoryDiagnoser]
public class XbrzScalerBenchmarks
{
    [Params(2, 3, 4, 5, 6)]
    public int ScaleFactor { get; set; }

    [Params(32, 256, 1024)]
    public int ImageSize { get; set; }

    [Params(true, false)]
    public bool WithAlpha { get; set; }

    [Params(ImageContentType.Solid, ImageContentType.Gradient, ImageContentType.Random)]
    public ImageContentType ContentType { get; set; }

    private int[] src;
    private int[] trg;
    private XbrzScaler scaler;
    private Random rng;

    [GlobalSetup]
    public void Setup()
    {
        int width = ImageSize;
        int height = ImageSize;
        src = new int[width * height];
        trg = new int[width * height * ScaleFactor * ScaleFactor];
        rng = new Random(42);
        switch (ContentType)
        {
            case ImageContentType.Solid:
                int color = WithAlpha ? unchecked((int)0xFF336699) : 0x336699;
                for (int i = 0; i < src.Length; i++)
                    src[i] = color;
                break;
            case ImageContentType.Gradient:
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int r = (x * 255) / (width - 1);
                    int g = (y * 255) / (height - 1);
                    int b = 128;
                    int a = WithAlpha ? 255 : 0;
                    src[y * width + x] = (a << 24) | (r << 16) | (g << 8) | b;
                }
                break;
            case ImageContentType.Random:
                for (int i = 0; i < src.Length; i++)
                {
                    int a = WithAlpha ? rng.Next(256) : 0;
                    int r = rng.Next(256);
                    int g = rng.Next(256);
                    int b = rng.Next(256);
                    src[i] = (a << 24) | (r << 16) | (g << 8) | b;
                }
                break;
        }
        scaler = new XbrzScaler(ScaleFactor, WithAlpha);
    }

    [Benchmark]
    public void Scale()
    {
        scaler.ScaleImage(src, trg, ImageSize, ImageSize);
    }
}
