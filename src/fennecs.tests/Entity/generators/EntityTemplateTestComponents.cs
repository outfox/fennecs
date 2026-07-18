// SPDX-License-Identifier: MIT

// Component types for the generated EntityTemplate test batteries (EntityTemplate.Tests.tt).
// TplA..TplF serve as the required (Needs) components, one per possible arity slot.
// The Baked* types are used for template-baked (Add) components and never overlap the required set.

namespace fennecs.tests.Templates;

public record struct TplA(int Value) { public static TplA New(int v) => new(v); }
public record struct TplB(int Value) { public static TplB New(int v) => new(v); }
public record struct TplC(int Value) { public static TplC New(int v) => new(v); }
public record struct TplD(int Value) { public static TplD New(int v) => new(v); }
public record struct TplE(int Value) { public static TplE New(int v) => new(v); }
public record struct TplF(int Value) { public static TplF New(int v) => new(v); }

public record struct BakedVal(int Value) { public static BakedVal New(int v) => new(v); }
public record struct BakedTag;
public record struct BakedRel(int Value) { public static BakedRel New(int v) => new(v); }
public sealed class BakedLink { public int Value { get; init; } public static BakedLink New(int v) => new() { Value = v }; }
