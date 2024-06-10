---
title: Delegates 
order: 5
---
# Delegates
Runner methods on Steam Queries expect delegates (Actions) to call. The delegate signatures mirror the count and order of the Query's Stream Types.

## `ComponentAction<>` and `UniformComponentAction<>`
These are invoked by [`Stream<>.For`](Stream.For.md) and [`Stream<>.Job`](Stream.Job.md). The Uniforms are contravariant, which helps with code reuse when you refactor your anonymous, named, or static method signatures to take broader data types.

::: code-group
```cs [basic]
delegate void ComponentAction<C0>(ref C0 comp0);
delegate void ComponentAction<C0, C1>(ref C0 comp0, ref C1 comp1);
delegate void ComponentAction<C0, C1, C2>(ref C0 comp0, ref C1 comp1, ref C2 comp2);
delegate void ComponentAction<C0, C1, C2, C3>(ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3);
delegate void ComponentAction<C0, C1, C2, C3, C4>(ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3, ref C4 comp4);
```

```cs [with uniform]
delegate void UniformComponentAction<in U, C0>(U uniform, ref C0 comp0);
delegate void UniformComponentAction<in U, C0, C1>(U uniform, ref C0 comp0, ref C1 comp1);  
delegate void UniformComponentAction<in U, C0, C1, C2>(U uniform, ref C0 comp0, ref C1 comp1, ref C2 comp2);
delegate void UniformComponentAction<in U, C0, C1, C2, C3>(U uniform, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3);
delegate void UniformComponentAction<in U, C0, C1, C2, C3, C4>(U uniform, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3, ref C4 comp4);
```
:::


## `EntityComponentAction<>` and `EntityUniformComponentAction<>`
These are invokable through [`Stream<>.For`](Stream.For.md). In addition to the Components and optional Uniform, they also receive the Entity that can be used to interact structurally with an Entity right then and there.

::: code-group
```cs [basic]
delegate void EntityComponentAction<C0>(Entity entity, ref C0 comp0);
delegate void EntityComponentAction<C0, C1>(Entity entity, ref C0 comp0, ref C1 comp1);
delegate void EntityComponentAction<C0, C1, C2>(Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2);
delegate void EntityComponentAction<C0, C1, C2, C3>(Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3); 
delegate void EntityComponentAction<C0, C1, C2, C3, C4>(Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3, ref C4 comp4);
```

```cs [with uniform]
delegate void UniformEntityComponentAction<in U, C0>(U uniform, Entity entity, ref C0 comp0);
delegate void UniformEntityComponentAction<in U, C0, C1>(U uniform, Entity entity, ref C0 comp0, ref C1 comp1);
delegate void UniformEntityComponentAction<in U, C0, C1, C2>(U uniform, Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2);
delegate void UniformEntityComponentAction<in U, C0, C1, C2, C3>(U uniform, Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3);
delegate void UniformEntityComponentAction<in U, C0, C1, C2, C3, C4>(U uniform, Entity entity, ref C0 comp0, ref C1 comp1, ref C2 comp2, ref C3 comp3, ref C4 comp4);
```
:::


## `MemoryAction<>` and `MemoryUniformAction<>`
These are invoked by [`Stream<>.Raw`](Stream.Raw.md).

::: code-group
```cs [basic]
delegate void MemoryAction<C0>(Memory<C0> comp0);
delegate void MemoryAction<C0, C1>(Memory<C0> comp0, Memory<C1> comp1);  
delegate void MemoryAction<C0, C1, C2>(Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2);
delegate void MemoryAction<C0, C1, C2, C3>(Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2, Memory<C3> comp3);
delegate void MemoryAction<C0, C1, C2, C3, C4>(Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2, Memory<C3> comp3, Memory<C4> comp4);
```

```cs [with uniform]
delegate void MemoryUniformAction<in U, C0>(U uniform, Memory<C0> comp0);
delegate void MemoryUniformAction<in U, C0, C1>(U uniform, Memory<C0> comp0, Memory<C1> comp1);
delegate void MemoryUniformAction<in U, C0, C1, C2>(U uniform, Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2); 
delegate void MemoryUniformAction<in U, C0, C1, C2, C3>(U uniform, Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2, Memory<C3> comp3);
delegate void MemoryUniformAction<in U, C0, C1, C2, C3, C4>(U uniform, Memory<C0> comp0, Memory<C1> comp1, Memory<C2> comp2, Memory<C3> comp3, Memory<C4> comp4);
```
:::

## My name is `in`-igo Montoya

::: warning *"You keep using that keyword. I do not think it means what you think it means."*
The `in` keyword in the `Uniform*` delegates' type parmeters denotes [contravariance](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/covariance-contravariance/). This simply means that the `U` parameter can be a broader type than the one you pass in.

It does not mean `readonly` or `in` as in C# method arguments, but uniforms are still always passed by value. If that value is a reference type, you can of course do [fun stuff](/cookbook/staples/Numbering.md) with it... 🦊

Components are always passed by reference because they  reside in a special storage object that enables this.
:::

Attentive readers will notice that reference type Uniforms passed to a `Job` should, of course, be chosen carefully to be thread safe if they are to be mutated from multiple threads. Anything in `System.Collections.Concurrent` is a faithful friend here.

## Uniforms

Uniforms are a way to pass data to your `For`, `Job`, and even `Raw` delegates. They are passed by value, and can be of any type. They are neat to feed data to your delegates that is not part of the component data, but relevant to the operation you are performing.

Typical examples of uniforms are the current dt (delta time) or positions or forces, e.g. for explosions or swarming behaviour, or even a list of UI objects that were clicked.


## Varyings
Later versions of **fenn**ecs may introduce the concept of "varyings". If you need this feature, please don't hesitate to reach out with some ideas and use cases!


### Varyings at Home
Until then, you can use a mutable reference type object as a uniform, and pass it to your `For` or `Job` delegate. This way, you can mutate the object from the delegate, and each subsequent calls will see these changes. 

### Remember: Thread fast, thread hard, and thread safe!

Where many threads live, there also be dragons.

:neofox_hug_dragn_heart: 