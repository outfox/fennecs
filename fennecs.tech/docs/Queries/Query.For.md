# `Query<>.For(RefAction)`
# `Query<>.For<U>(RefAction)`

::: info THE CLASSIC
"**For**" is always there "**For U**"... and _gets it done_ in a quick, predictable, reliable way.  
:::

## Description
Single-theaded, synchronous Runner Methods on Queries with 1 or more [Stream Types](StreamTypes.md).

For each Entity in the Query, calls the appropriate `RefAction`, with or without passing an uniform data parameter depending on the overload.

Each `For`-Runner takes a [`RefAction`](Delegates.md#refaction-and-refactionu) or [`RefActionU<>`](Delegates.md#refaction-and-refactionu) as delegate parameter. The Type Parameters for the Actions are the Stream Types of the Query, or a [prefix subset](Query.1-5.md#prefix-subsets).

The Runner is executed directly on the calling thread. Until the runner returns, the World is in `WorldMode.Deferred`, meaning structural changes are applied once the Runner has finished.

### Basic Operation

Call a Runner on a Query to have it execute the delegate you're passing in. You may optionally have the Runner pass in a Uniform data item that you can provide.

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
The function passed as `RefAction` can be `static`, even if they are written anonymous delegates or lambda expressions! This reduces the allocation of memory for a closure or context to zero in most cases. Consider adding the keyword `static` where you can.
:::

### Performance Considerations in Calling Conventions

RefActions can be passed to runners in several ways. Choose based on your preferred code style and desired conciseness. Here's a lineup to compare options.

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


### Uniforms, Shmuniforms

Shader programmers are going to love these, but the classical programmer might be scratching their head in wonder and ask: "But why? A lambda is so flexible! The whole universe is my ~~oyster~~ closure."

::: warning :neofox_bongo_down: ALL CONVENTIONS ARE BEAUTIFUL
And yet... don't skimp on static functions just because you need data from your current context! 🦊 Memory allocations can fragment your heap and will slow down your game or simulation. 
:::

But amazingly, a **Uniform** can be anything: a primitive type like `int`, a `struct`, a `class`, and also the new `System.ValueTuple`. The latter makes it possible to capture arbitrary data, and provide it in a readable, named, *and allocation-free* way into your `static` anonymous or named functions, without having to declare a struct somewhere else.

```cs
// Using System.ValueTuple for the win!
myQuery.For(static (ref Vector3 position, ref Vector3 velocity, 
/* ValueTuple! */ (Vector3 gravity, float deltaTime) uniform) =>          
{
    velocity += uniform.gravity * uniform.deltaTime;
}, (Vector3.DOWN, Time.deltaTime)); //...and the actual ValueTuple being passed in
```


