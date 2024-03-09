namespace fennecs;

public delegate void RefAction<C0>(ref C0 comp0);

public delegate void RefAction<C0, C1>(ref C0 comp0, ref C1 comp1);

public delegate void RefAction<C0, C1, C2>(ref C0 comp0, ref C1 comp1, ref C2 comp2);

public delegate void RefAction<C0, C1, C2, C3>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3);

public delegate void RefAction<C0, C1, C2, C3, C4>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4);

public delegate void RefActionU<C0, in U>(ref C0 comp0, U uniform);

public delegate void RefActionU<C0, C1, in U>(ref C0 comp0, ref C1 comp1, U uniform);

public delegate void RefActionU<C0, C1, C2, in U>(ref C0 comp0, ref C1 comp1, ref C2 comp2, U uniform);

public delegate void RefActionU<C0, C1, C2, C3, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, U uniform);

public delegate void RefActionU<C0, C1, C2, C3, C4, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, U uniform);

public delegate void MemoryAction<C0>(Memory<C0> c0);

public delegate void MemoryAction<C0, C1>(Memory<C0> c0, Memory<C1> c1);

public delegate void MemoryAction<C0, C1, C2>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2);

public delegate void MemoryAction<C0, C1, C2, C3>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3);

public delegate void MemoryAction<C0, C1, C2, C3, C4>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, Memory<C4> c4);

public delegate void MemoryActionU<C0, in U>(Memory<C0> c0, U uniform);

public delegate void MemoryActionU<C0, C1, in U>(Memory<C0> c0, Memory<C1> c1, U uniform);

public delegate void MemoryActionU<C0, C1, C2, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, U uniform);

public delegate void MemoryActionU<C0, C1, C2, C3, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, U uniform);

public delegate void MemoryActionU<C0, C1, C2, C3, C4, in U>(Memory<C0> c0, Memory<C1> c1, Memory<C2> c2, Memory<C3> c3, Memory<C4> c4, U uniform);