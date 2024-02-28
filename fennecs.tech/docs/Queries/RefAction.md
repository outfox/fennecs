# `RefAction` and `RefActionU`

The delegate signatures mirror the count and order of the Query's Stream Types.
::: code-group
```cs [plain]
delegate void RefAction<C0>(ref C0 comp0);
delegate void RefAction<C0, C1>(ref C0 comp0, ref C1 comp1);
delegate void RefAction<C0, C1, C2>(ref C0 comp0, ref C1 comp1, ref C2 comp2);
delegate void RefAction<C0, C1, C2, C3>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3);
delegate void RefAction<C0, C1, C2, C3, C4>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4);
```

```cs [with uniform]
delegate void RefActionU<C0, in U>(ref C0 comp0, U uniform);
delegate void RefActionU<C0, C1, in U>(ref C0 comp0, ref C1 comp1, U uniform);
delegate void RefActionU<C0, C1, C2, in U>(ref C0 comp0, ref C1 comp1, ref C2 comp2, U uniform);
delegate void RefActionU<C0, C1, C2, C3, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, U uniform);
delegate void RefActionU<C0, C1, C2, C3, C4, in U>(ref C0 c0, ref C1 c1, ref C2 c2, ref C3 c3, ref C4 c4, U uniform);
```
:::


