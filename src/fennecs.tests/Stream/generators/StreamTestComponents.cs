namespace fennecs.tests.Stream;

// Component types used by the generated Stream test batteries (see Stream.Tests.tt).
// All expose the same shape - int Value plus a static New(int) factory - so the
// T4 templates emit identical test code for value-type, reference-type, and mixed
// component sets. Five distinct types per kind are needed so each Stream slot has
// its own component type (archetype identity, Where() overload resolution).

public record struct ValA(int Value) { public static ValA New(int v) => new(v); }
public record struct ValB(int Value) { public static ValB New(int v) => new(v); }
public record struct ValC(int Value) { public static ValC New(int v) => new(v); }
public record struct ValD(int Value) { public static ValD New(int v) => new(v); }
public record struct ValE(int Value) { public static ValE New(int v) => new(v); }

public sealed class RefA { public int Value { get; init; } public static RefA New(int v) => new() { Value = v }; }
public sealed class RefB { public int Value { get; init; } public static RefB New(int v) => new() { Value = v }; }
public sealed class RefC { public int Value { get; init; } public static RefC New(int v) => new() { Value = v }; }
public sealed class RefD { public int Value { get; init; } public static RefD New(int v) => new() { Value = v }; }
public sealed class RefE { public int Value { get; init; } public static RefE New(int v) => new() { Value = v }; }
