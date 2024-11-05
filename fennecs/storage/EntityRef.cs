namespace fennecs.storage;

internal readonly ref struct EntityRef(ref Entity val)
{
    private readonly ref Entity _value = ref val;
    public Entity read => _value;

    public static implicit operator Entity(EntityRef self) => self._value;
}