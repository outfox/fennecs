using fennecs.reflection;

namespace fennecs.tests;

public class ReflectionExtensionsTests
{
    private class Base
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        public float value = 1;
    }

    private class Derived1 : Base
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        public int additionalValue = 2; 
    }
    
    private class Derived2 : Base
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        public int additionalValue = 2; 
    }
    
    private class Derived3 : Derived1
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        public int additionalValue = 2; 
    }
    
    [Fact]
    private void CanAddVirtual_string()
    {
        using var world = new World();
        var entity = world.Spawn();
        
        entity.AddVirtual("string");
        
        Assert.True(entity.Has<string>());
    }

    [Fact]
    private void CanAddVirtual_Derived()
    {
        using var world = new World();
        var entity = world.Spawn();

        Base baseInstance = new Derived1();
        entity.AddVirtual(baseInstance);
        
        Assert.True(entity.Has<Derived1>());
        
        ref var derived = ref entity.Ref<Derived1>();
        Assert.Equal(1, derived.value);
        Assert.Equal(2, derived.additionalValue);
    }
    
    [Fact]
    private void CanGetVirtual_MultipleInheritance()
    {
        using var world = new World();
        var entity = world.Spawn().Add("noise").AddVirtual(123);
        
        entity.AddVirtual(new Base());
        entity.AddVirtual(new Derived1());
        entity.AddVirtual(new Derived2());
        entity.AddVirtual(new Derived3());

        var baseComponents = entity.GetVirtual<Base>();
        var derivedComponents1 = entity.GetVirtual<Derived1>();
        var derivedComponents2 = entity.GetVirtual<Derived2>();
        var derivedComponents3 = entity.GetVirtual<Derived3>();

        Assert.Equal(4, baseComponents.Length);
        Assert.Equal(2, derivedComponents1.Length);
        Assert.Single(derivedComponents2);
        Assert.Single(derivedComponents3);
        
        Assert.True(entity.Has<Derived1>());
        Assert.True(entity.Has<Derived2>());
        Assert.True(entity.Has<Derived3>());
    }
    
    [Fact]
    private void CanGetVirtual()
    {
        using var world = new World();
        var entity = world.Spawn();

        Base baseInstance = new Derived1();
        entity.AddVirtual(baseInstance);

        var baseComponents = entity.GetVirtual<Base>();
        var derivedComponents = entity.GetVirtual<Derived1>();
        
        Assert.NotNull(baseComponents);
        Assert.NotNull(derivedComponents);
        Assert.Single(baseComponents);
        Assert.Single(derivedComponents);
    }
    
    [Fact]
    private void CanGetVirtual_Empty()
    {
        using var world = new World();
        var entity = world.Spawn();

        var baseInstance = new Base();
        entity.AddVirtual(baseInstance);

        Assert.True(entity.Has<Base>());
        Assert.False(entity.Has<Derived1>());
        
        Assert.Single(entity.GetVirtual<Base>());
        Assert.Empty(entity.GetVirtual<Derived1>());
    }

    [Fact]
    private void AddVirtual_FailsOnMultipleAdd()
    {
        using var world = new World();
        var entity = world.Spawn();
        var baseInstance = new Derived1();
        entity.AddVirtual(baseInstance);
        Assert.Throws<ArgumentException>(() => entity.AddVirtual(new Derived1()));
    }
}
