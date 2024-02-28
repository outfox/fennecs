# `Query.For(RefAction)`
# `Query.For<U>(RefAction)`

For each entity in the query, calls the appropriate `RefAction`, with or without passing an uniform data parameter depending on the overload.

The processing is synchronous and on the single calling thread.

::: tip
The function passed as `RefAction` can be `static`, even if anonymous.

Uniform parameters can be `ValueTuples`, which makes it possible to capture arbitrary data allocation-free into `static` anonymous or named functions.

Example:
```cs
myQuery.For(static (ref Vector3 position, ref Vector3 velocity, 
/* ValueTuple! */ (Vector3 gravity, float deltaTime) uniform) =>          
{
    velocity += uniform.gravity * uniform.deltaTime;
}, (Vector3.DOWN, Time.deltaTime)); // ValueTuple lets type U of For<U> to be inferred.
```
:::

