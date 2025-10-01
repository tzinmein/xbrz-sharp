# XbrzSharp

![.NET Build & Test](https://github.com/tzinmein/xbrz-sharp/workflows/.NET%20Build,%20Test,%20and%20Publish/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/XbrzSharp.svg?style=flat-square)](https://www.nuget.org/packages/XbrzSharp/)
[![License](https://img.shields.io/badge/License-GPL%203.0%20%2B%200BSD-blue.svg)](LICENSE.md)
![.NET](https://img.shields.io/badge/.NET-8.0-blue)

[xBRZ](https://sourceforge.net/projects/xbrz/): "Scale by rules" – high quality image upscaling filter by Zenju.

This is a .NET 8 port of xBRZ 1.8, based on the [Java port by Stanio](https://github.com/stanio/xbzr-java).

**Important:** This project is **distinct** from another .NET port [xBRZ.NET](https://github.com/Helion-Engine/xBRZ.NET) (namespace `xBRZNet`).

Right ow it's a very straightforward port. Performance optimizations and more idiomatic .NET code may be added in the future.

Copyright (c) 2025 Ho Tzin Mein

## Projects

- **XbrzSharp** – Core xBRZ upscaling logic (library)
- **XbrzSharpTool** – Command-line tool for batch image upscaling

## Usage

### Command-Line Tool

```sh
dotnet run --project XbrzSharpTool -- <source> [scaling_factor]
```

- `<source>`: Path to the input image file or directory. If a directory is specified, all supported images in the directory will be upscaled.
- `[scaling_factor]`: Optional integer (default: 2). Must be between 2 and 6.

The output will be saved as `<source-image>@<factor>x.png` in the same directory as the input file(s).

### Library API Example

You can use the `XbrzSharp` library in your own .NET projects for programmatic upscaling:

```csharp
using System.Drawing;
using System.Drawing.Imaging;
using Xbrz;

// Load your image as a Bitmap
using var bitmap = new Bitmap("input.png");

// Convert to ARGB int[] array
int width = bitmap.Width, height = bitmap.Height;
int[] srcPixels = new int[width * height];
var rect = new Rectangle(0, 0, width, height);
var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
System.Runtime.InteropServices.Marshal.Copy(data.Scan0, srcPixels, 0, srcPixels.Length);
bitmap.UnlockBits(data);

// Perform scaling
int factor = 3;
var scaler = new XbrzScaler(factor, withAlpha: true);
int[] scaledPixels = scaler.ScaleImage(srcPixels, null, width, height);

// Convert back to Bitmap
int scaledWidth = width * factor, scaledHeight = height * factor;
var scaledBitmap = new Bitmap(scaledWidth, scaledHeight, PixelFormat.Format32bppArgb);
var scaledRect = new Rectangle(0, 0, scaledWidth, scaledHeight);
var scaledData = scaledBitmap.LockBits(scaledRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
System.Runtime.InteropServices.Marshal.Copy(scaledPixels, 0, scaledData.Scan0, scaledPixels.Length);
scaledBitmap.UnlockBits(scaledData);

// Save the result
scaledBitmap.Save("output@3x.png");
```
## Attribution for Test Assets

Test inputs and expected outputs are copied from the xBRZ Java port by Stanio:

Java port repository: https://github.com/stanio/xbzr-java

## License

- **Core library (`xBrzNet`)**: [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html)
- **Other code (tools, tests, helpers, documentation)**: [BSD Zero Clause License](https://spdx.org/licenses/0BSD.html)

See [LICENSE](LICENSE.md) for full license texts.
