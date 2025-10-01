// xBrzTool / Test / Helper file
// Licensed under 0BSD
//
// Copyright (c) 2025 Ho Tzin Mein
//
// For the full license text, see LICENSE.md in the repository
using Xbrz;

namespace xBrzNet.Tests;

public class Scaler2xTest : AbstractScalerTest
{
    private static readonly XbrzScaler XbrzScaler2 = new(2);
    protected override XbrzScaler XbrzScaler() => XbrzScaler2;
}

public class Scaler3xTest : AbstractScalerTest
{
    private static readonly XbrzScaler XbrzScaler3 = new(3);
    protected override XbrzScaler XbrzScaler() => XbrzScaler3;
}

public class Scaler4xTest : AbstractScalerTest
{
    private static readonly XbrzScaler XbrzScaler4 = new(4);
    protected override XbrzScaler XbrzScaler() => XbrzScaler4;
}

public class Scaler5xTest : AbstractScalerTest
{
    private static readonly XbrzScaler XbrzScaler5 = new(5);
    protected override XbrzScaler XbrzScaler() => XbrzScaler5;
}

public class Scaler6xTest : AbstractScalerTest
{
    private static readonly XbrzScaler XbrzScaler6 = new(6);
    protected override XbrzScaler XbrzScaler() => XbrzScaler6;
}
