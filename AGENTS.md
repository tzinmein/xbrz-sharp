# Agent guide — xbrz-sharp

.NET 10 port of **xBRZ 1.8** (Zenju), originally based on [Stanio’s Java port](https://github.com/stanio/xbrz-java). Distinct from Helion’s [xBRZ.NET](https://github.com/Helion-Engine/xBRZ.NET) (namespace `xBRZNet`).

**Pixel output** is gated by golden tests. **Code organization** should be idiomatic C# — do not preserve Java layout, naming, or singleton patterns for fidelity’s sake.

## Layout

| Path | Role | License |
|------|------|---------|
| `src/XbrzSharp/` | Core library (NuGet `XbrzSharp`) | GPL-3.0 |
| `src/XbrzSharpTool/` | CLI batch upscaler (ImageSharp I/O) | 0BSD |
| `tests/XbrzSharp.Tests/` | xUnit + golden PNGs (from Java port) | 0BSD |
| `tests/XbrzSharp.Benchmarks/` | BenchmarkDotNet | 0BSD |
| `XbrzSharp.slnx` | Solution | — |
| `.github/workflows/dotnet.yml` | Restore → Release build → test → pack → NuGet push | — |

Do not commit a local `xbrz-java-master/` tree (gitignored).

## Commands

```sh
dotnet build
dotnet test
dotnet test --filter "FullyQualifiedName~Scaler2xTest"
dotnet run --project src/XbrzSharpTool -- <source> [factor]   # factor 2–6
dotnet run -c Release --project tests/XbrzSharp.Benchmarks
dotnet pack src/XbrzSharp/XbrzSharp.csproj -c Release -o ./artifacts
```

Target framework: **net10.0** everywhere. Nullable and implicit usings are enabled.

## Public API (keep stable)

- Namespace root: **`Xbrz`** (not `XbrzSharp`)
- Primary type: `Xbrz.XbrzScaler` + nested `ScalerCfg` record; scale via `Factor` property
- Pixels: packed **ARGB** `int[]` (A in high byte)
- Factors: **2–6** via `ScalerFactory`
- `ScaleImage(src, trg, w, h)` — `trg` may be `null` (allocated)
- `ColorDistance` delegate is public; **`ColorDistanceFactory` is internal** (tests via `InternalsVisibleTo`)

Default `ScalerCfg`: luminance `1`, equalColorTolerance `30`, centerDirectionBias `4`, dominantDirectionThreshold `3.6`, steepDirectionThreshold `2.2`.

## Architecture (library)

```
XbrzScaler
  ├─ Scaler/Scaler{2–6}x (per-factor AlphaGrad tables)
  ├─ ColorDistance (YCbCr / Integer / Buffered / RGB / WithAlpha)
  ├─ Kernel4x4 → Kernel3x3
  ├─ OutputMatrix, BlendInfo, BlendResult
  └─ Parallel.For per source row (per-worker locals)
```

Keep `Scaler2x`–`Scaler6x` as separate types (shared plumbing OK; do not merge math tables into one mega-scaler).

## Behavior to preserve

Do not casually change (breaks goldens):

- YCbCr constants and `ScalerCfg` defaults / blend thresholds
- Per-factor `AlphaGrad` fractions in `ScalerNx`
- Corner preprocess + four rotation blends
- Edge policy (transparent vs duplicate when `withAlpha`)
- Golden PNGs under `tests/XbrzSharp.Tests/test/` or test deviation limits

After algorithm or scaler changes: full `dotnet test`. Never loosen tolerances to hide drift.

## Tests

Goldens (`name@{2–6}xbrz.png`). Typical deviations:

- Alpha fixtures (`basic-shapes`, `down-arrow`, `open-folder`): **0.0001%**
- `gbamockup` no-alpha: **0.001%**
- Alternate distances (RGB / buffered): up to **3.0%**

## Conventions

- Core files: GPL header (`XbrzSharp` / GPL-3.0)
- Tool/tests: 0BSD header
- Prefer idiomatic C# (structs, properties, Parallel.For localInit, no Java-style ThreadLocal singletons)
- Prefer fixing nullable properly over `!` / warning suppressions
- Keep ImageSharp **out** of the core library csproj (tool/tests only)

## CI / packaging

- Push `master` → beta pack `1.1.0-beta.$RUN_NUMBER` and publish
- Tag `vX.Y.Z` → version `X.Y.Z` and publish
- Secret: `XBRZ_NUGET_KEY`
- NuGet license expression: `GPL-3.0-only`

## Agent do / don’t

| Do | Don’t |
|----|--------|
| Preserve pixel output; prove with `dotnet test` | Change blend math / thresholds without golden proof |
| Prefer clear idiomatic C# structure | Mirror Java organization for its own sake |
| Keep dual-license headers correct for the tree | Add deps to the packaged library lightly |
| Docs, nullable cleanup, tool UX, CI polish | Confuse this repo with Helion `xBRZ.NET` |
| Intentional public API expansion (e.g. factory) | Commit/push/publish unless asked |
