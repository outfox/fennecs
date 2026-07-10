// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using fennecs.CRUD;

namespace fennecs.tests;

public class IAddRemoveBoxedTests
{
    // Minimal implementor that does NOT shadow the interface's default Get(Type, Match)
    // method, so the default implementation itself executes here. (Entity overrides it
    // with its own version, which leaves the interface's fallback body uncovered.)
    private sealed class BoxedStore : IAddRemoveBoxed<BoxedStore>
    {
        private readonly Dictionary<Type, object> _components = [];

        public bool Has(Type type, Match match) => _components.ContainsKey(type);

        public bool Get([MaybeNullWhen(false)] out object value, Type type, Match match = default)
            => _components.TryGetValue(type, out value);

        public void Set(object value, Match match = default) => _components[value.GetType()] = value;

        public BoxedStore Clear(Type type, Match match = default)
        {
            _components.Remove(type);
            return this;
        }
    }

    [Fact]
    public void Default_Get_Returns_Boxed_Value_When_Present()
    {
        IAddRemoveBoxed<BoxedStore> store = new BoxedStore();
        store.Set(123);
        store.Set("hello");

        Assert.Equal(123, store.Get(typeof(int)));
        Assert.Equal("hello", store.Get(typeof(string), Match.Plain));
    }

    [Fact]
    public void Default_Get_Returns_Null_When_Missing()
    {
        IAddRemoveBoxed<BoxedStore> store = new BoxedStore();

        Assert.Null(store.Get(typeof(int)));

        store.Set(123);
        store.Clear(typeof(int));
        Assert.Null(store.Get(typeof(int)));
    }
}
