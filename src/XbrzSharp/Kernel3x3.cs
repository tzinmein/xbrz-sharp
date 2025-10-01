// xBrzNet - .NET port of xBRZ
// Core library licensed under GPL-3.0
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
namespace Xbrz;

internal sealed class Kernel3x3(Kernel4x4 ker4)
{
    private readonly Kernel4x4 ker4 = ker4;
    private RotationDegree rotDeg = RotationDegree.Rot0;

    public int a() => rotDeg switch { RotationDegree.Rot90 => ker4.i, RotationDegree.Rot180 => ker4.k, RotationDegree.Rot270 => ker4.c, _ => ker4.a };
    public int b() => rotDeg switch { RotationDegree.Rot90 => ker4.e, RotationDegree.Rot180 => ker4.j, RotationDegree.Rot270 => ker4.g, _ => ker4.b };
    public int c() => rotDeg switch { RotationDegree.Rot90 => ker4.a, RotationDegree.Rot180 => ker4.i, RotationDegree.Rot270 => ker4.k, _ => ker4.c };
    public int d() => rotDeg switch { RotationDegree.Rot90 => ker4.j, RotationDegree.Rot180 => ker4.g, RotationDegree.Rot270 => ker4.b, _ => ker4.e };
    public int e() => ker4.f;
    public int f() => rotDeg switch { RotationDegree.Rot90 => ker4.b, RotationDegree.Rot180 => ker4.e, RotationDegree.Rot270 => ker4.j, _ => ker4.g };
    public int g() => rotDeg switch { RotationDegree.Rot90 => ker4.k, RotationDegree.Rot180 => ker4.c, RotationDegree.Rot270 => ker4.a, _ => ker4.i };
    public int h() => rotDeg switch { RotationDegree.Rot90 => ker4.g, RotationDegree.Rot180 => ker4.b, RotationDegree.Rot270 => ker4.e, _ => ker4.j };
    public int i() => rotDeg switch { RotationDegree.Rot90 => ker4.c, RotationDegree.Rot180 => ker4.a, RotationDegree.Rot270 => ker4.i, _ => ker4.k };
    public void RotDeg(RotationDegree deg) { rotDeg = deg; }
}
