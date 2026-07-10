---
title: Microcode
order: 1
outline: [2, 3]

head:
  - - meta
    - name: description
      content: How subnormal floats and FP assists can silently make identical code 3x slower - a fennecs benchmark ghost story, with reproducible code.
---

# The Ghost in the Microcode

::: info :neofox_floof_mug: MMMH, REAL CODE
This **RUNS**! The benchmark quoted below lives in the repo at `src/fennecs.benchmarks/`, and you can summon the ghost yourself:

```shell
dotnet run -c Release -f net10.0 -- --filter "*Microcode*"
```

:::

## Premise

One of our benchmarks was haunted. 

:neofox_blobfoxghostfloof:

It ran at a brisk **~490µs** for exactly twelve thousand ticks... and then flipped to **~1.6ms**. Deterministically.

We interrogated every usual suspect. The JIT? *Alibi*  –  the flip survived `DOTNET_TieredCompilation=0`, PGO off, OSR off, even `AggressiveOptimization`. Thermal throttling? *Alibi*  –  a pure register spin-loop timed identical before and after, and the slowness survived a 20-second nap. The GC? *Alibi*  –  the arrays never moved an inch, zero page faults, and relocating everything via LOH compaction cured nothing.

But wait... copying the data into **fresh arrays** made the *same code* fast again? Then, the fresh copies, after enough simulation ticks of their own, *became haunted too.*

The curse wasn't in the code or in the memory... it was **in the values**?!

### The Culprit: Subnormal Floats

A `float` normally can't get smaller than about `1.18e-38`. Below that lurks a special zone  –  **subnormal** (a.k.a. *denormal*) numbers  –  where the format gives up its implicit leading bit to eke out a little more range before flushing to zero. Mathematically noble! But your CPU's fast floating-point circuitry doesn't handle them: it bails out into **microcode**, a trap politely called an **FP assist**, costing tens to *hundreds* of cycles for that one instruction.

And here's the truly cursed part:

::: danger :neofox_peek_knife: YOU NEVER HAVE TO *STORE* ONE
Our haunted data never contained a single subnormal  –  we scanned all 100,000 entities to check. The smallest stored component was a perfectly legal `1.99e-25`.

But square that  –  as `Vector3.Dot`, `Length()`, and `Normalize()` all do internally  –  and you get `1e-50`: **deep** in subnormal territory. Every intermediate product and sum in the hot loop triggered an assist. The penalty is invisible in your data and invisible in your code. It only exists *mid-instruction*.
:::

Our workload was a little physics integrator with gravity, a speed clamp, and a normalize. That system has an attractor: *terminal velocity, straight down*. The lateral velocity components decayed exponentially  –  `1e-4` → `5e-16` → `1.8e-29` → gone  –  and at ~12,000 ticks, enough entities had crossed below `~1e-19` (where squaring underflows) that microcode ate the whole frame budget.

The benchmark had been faithfully simulating **rain**. :neofox_sob:

### The Recipe

To capture the ghost in a jar, this benchmark runs the *identical* workload over two Worlds with identical layout and iteration order. The only difference: one World's velocities are around `1.0`, the other's are around `1e-25`  –  healthy food and cursed food, same kitchen:

::: code-group
<<< ../../../src/fennecs.benchmarks/ECS/MicrocodeBenchmarks.cs {cs:line-numbers} [MicrocodeBenchmarks.cs]
:::

### The Damage

| Method  | EntityCount | Mean       | StdDev   | Median     | Ratio     |
|-------- |------------ |-----------:|---------:|-----------:|----------:|
| Healthy | 100000      |   449.1 μs |  1.85 μs |   448.4 μs |  1.00     |
| Cursed  | 100000      | 1,510.8 μs | 26.45 μs | 1,504.1 μs |  **3.36** |

> *(AMD Ryzen 9 5900X, .NET 10, BenchmarkDotNet; some CPUs punish assists far harder.)*

Eek - it's **3.36× slower.** Same instructions & access patterns all around, only the *magnitude of the float values* was different.

### How Meals Get Cursed

Nobody seasons their data with `1e-25` on purpose. It happens wherever values decay exponentially and nothing stops them:

- **Damping & drag**  –  `velocity *= 0.98f` every frame is a one-way trip to subnormal town (~4,000 frames from `1.0`).
- **Attractors**  –  our gravity + normalize combo; anything that converges lateral components toward zero.
- **Fade-outs**  –  particle alphas, audio envelopes, smoothing filters, exponential moving averages.
- **Lerp-toward-target**  –  `x = Lerp(x, target, 0.1f)` never *reaches* the target; the error just gets tinier and tinier and *tinier*...

### The Antidotes

::: tip :neofox_knives: KEEP YOUR DYNAMICS STATIONARY
The best fix is upstream: design simulation loops whose values can't decay without bound. Clamp magnitudes from **below** as well as above, or use dynamics (like pure rotations) that preserve magnitude. Our fixed benchmark workload does exactly this.
:::

::: tip :neofox_thumbsup: FLUSH TINY VALUES YOURSELF
.NET gives managed code no access to the CPU's flush-to-zero / denormals-are-zero flags, so snap the values yourself  –  one branch per entity is *vastly* cheaper than a handful of assists per entity:

```csharp
if (velocity.LengthSquared() < 1e-30f) velocity = Vector3.Zero;
```

:::

::: tip :neofox_magnify: DIAGNOSE WITH A SCAN
`float.IsSubnormal(x)` exists! But remember the twist above  –  also check whether values are *small enough that their squares or products* go subnormal (below `~1e-19` for squares).
:::

### :neofox_cofe: The moral of the story

When identical code runs at wildly different speeds, remember there are *three* inputs to performance: the code, the memory... and the **values**. The third one can be haunted.
