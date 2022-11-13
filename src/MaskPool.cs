using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HypEcs;

public static class MaskPool
{
    static readonly Stack<Mask> Stack = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mask Get()
    {
        return Stack.Count > 0 ? Stack.Pop() : new Mask();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(Mask list)
    {
        list.Clear();
        Stack.Push(list);
    }
}