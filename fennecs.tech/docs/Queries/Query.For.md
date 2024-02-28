# `Query<>.For(RefAction)`
# `Query<>.For<U>(RefAction)`

Single-theaded, synchronous Runner Methods on Queries with 1 or more [Stream Types](Stream%20Types.md).

For each Entity in the Query, calls the appropriate `RefAction`, with or without passing an uniform data parameter depending on the overload.

Takes a [`RefAction`](RefAction.md) or [`RefActionU<>`](RefAction.md) as delegate parameter.

The Runner is executed directly on the calling thread. Until the runner returns, the World is in `WorldMode.Deferred`, meaning structural changes are applied once the Runner has finished.

## Basic Operation

::: code-group
```cs [For(...) plain]
myQuery.For((ref Vector3 position, ref Vector3 velocity) => 
{
    velocity += Vector3.DOWN * 0.016f;
}); // actual ValueTuple being passed in
```

```cs [For&lt;U&gt;(...) with uniform]
myQuery.For((ref Vector3 position, ref Vector3 velocity, float dtUniform) => 
{
    velocity += Vector3.DOWN * dtUniform;
}, Time.deltaTime); 

```
:::


::: tip
The function passed as `RefAction` can be `static`, even if they are written anonymous delegates or lambda expressions! This reduces the allocation of memory for a closure or context to zero in most cases.

Consider adding the keyword `static` where you can.
:::


## Differences in Calling Conventions

RefActions can be passed to runners in several ways. Choose based on your preferred style and conciseness - but here's a lineup to compare options.

::: code-group
```cs [lambda]
// The classic. Fast to write, fast to execute.
// ⚠️ Allocates memory for closure on each call!
myQuery.For((ref Vector3 position, ref Vector3 velocity) => 
{
    velocity += Vector3.DOWN * 0.016f;
});
```

```cs [🥇 static lambda]
// Fast, very flexible. Use For<U>+uniform delegate to "capture" values.
// ✅ No additional memory allocation!
myQuery.For(static (ref Vector3 position, ref Vector3 velocity) =>
{
    velocity += Vector3.DOWN * 0.016f;
});
```

```cs [🎖️ static method]
// Fastest, most readable, most refactorable, best code re-use.
// Use Uniform variant to "capture" values.
// ✅ No additional memory allocation!
myQuery.For(Physics.ApplyGravity); 

```


```cs [🥉 named method]
// Much improved readability and code re-use.
// Executes slightly faster than lambda.
// ⚠️ Allocates memory for 'this' on each call!
myQuery.For(this.ApplyGravity); 

```

```cs [🥈 (static) delegate]
// Slightly faster than lambda expression in some benchmarks, but no other upside.
// ⚠️ Allocates memory for closure on each call!
myQuery.For(delegate (ref Vector3 position, ref Vector3 velocity) 
{
    velocity += Vector3.DOWN * 0.016f;
});

// Slightly faster than lambda expression in some benchmarks.
// ✅ No additional memory allocation!
myQuery.For(static delegate (ref Vector3 position, ref Vector3 velocity) 
{
    velocity += Vector3.DOWN * 0.016f;
});
```
:::


::: tip
Don't skimp on static functions just because you need data from your current context! 

A **Uniform** can be anything: a primitive type like `int`, a `struct`, a `class`, and also the new `System.ValueTuple`. The latter makes it possible to capture arbitrary data, and provide it in a named way and allocation-free into `static` anonymous or named functions, without having to declare a struct somewhere else.

```cs
myQuery.For(static (ref Vector3 position, ref Vector3 velocity, 
/* ValueTuple! */ (Vector3 gravity, float deltaTime) uniform) =>          
{
    velocity += uniform.gravity * uniform.deltaTime;
}, (Vector3.DOWN, Time.deltaTime)); // actual ValueTuple being passed in
```

:::

