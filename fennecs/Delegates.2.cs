using fennecs.storage;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace fennecs;


public delegate void ComponentActionR<C0>(R<C0> comp0) where C0 : notnull;
public delegate void ComponentActionW<C0>(RW<C0> comp0) where C0 : notnull;

public delegate void ComponentActionER<C0>(EntityRef entity, R<C0> comp0) where C0 : notnull;
public delegate void ComponentActionEW<C0>(EntityRef entity, RW<C0> comp0) where C0 : notnull;

