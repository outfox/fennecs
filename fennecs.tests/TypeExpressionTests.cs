// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#pragma warning disable CS0169 // Field is never used

namespace fennecs.tests;

public class TypeExpressionTests(ITestOutputHelper output)
{
    [Fact]
    public void To_String()
    {
        output.WriteLine(TypeExpression.Of<TypeEmpty>(Match.Plain).ToString());
        output.WriteLine(TypeExpression.Of<TypeEmpty>(Match.Any).ToString());
        output.WriteLine(TypeExpression.Of<TypeEmpty>(Match.Object).ToString());
        output.WriteLine(TypeExpression.Of<TypeEmpty>(Match.Entity).ToString());
        output.WriteLine(TypeExpression.Of<TypeEmpty>(new(new(123))).ToString());
    }


    [Fact]
    public void Id_is_Comparable()
    {
        var t1 = TypeExpression.Of<TypeEmpty>(Match.Plain);
        var t2 = TypeExpression.Of<TypeEmpty>(Match.Plain);
        Assert.Equal(t1, t2);
    }


    [Fact]
    public void Id_is_Comparable_for_BaseTypes()
    {
        var t1 = TypeExpression.Of<double>(Match.Plain);
        var t2 = TypeExpression.Of<double>(Match.Plain);
        Assert.Equal(t1, t2);
    }


    [Fact]
    public void Is_Distinct()
    {
        var t1 = TypeExpression.Of<int>(Match.Plain);
        var t2 = TypeExpression.Of<ushort>(Match.Plain);
        Assert.NotEqual(t1, t2);
    }


    [Fact]
    public void Is_Sorted_By_TypeId_Ascending()
    {
        var random = new Random(4711);
        for (var i = 0; i < 10_000; i++)
        {
            var id = random.Next();
            var deco = (TypeID)(random.Next() % TypeID.MaxValue);
            var t1 = new TypeExpression((TypeID)i, (new Entity(id, deco)));
            var t2 = new TypeExpression((TypeID)(i + 1), (new Entity(id, deco)));

            //  If this test fails, Archetypes will not be able to build immutable buckets for Wildcards.
            Assert.True(t1.CompareTo(t2) < 0);
            Assert.True(t2.CompareTo(t1) > 0);
        }
    }


    [Fact]
    public void Implicitly_decays_to_Type()
    {
        var t1 = TypeExpression.Of<TypeEmpty>(Match.Plain).Type;
        var t2 = typeof(TypeEmpty);
        Assert.Equal(t2, t1);
        Assert.Equal(t1, t2);
    }


    [Fact]
    public void Has_Equality_Operator()
    {
        var t1 = TypeExpression.Of<TypeEmpty>(Match.Plain);
        var t2 = TypeExpression.Of<TypeEmpty>(Match.Plain);
        var t3 = TypeExpression.Of<string>(Match.Plain);
        Assert.True(t1 == t2);
        Assert.False(t1 == t3);
    }


    [Fact]
    public void Has_Inequality_Operator()
    {
        var t1 = TypeExpression.Of<TypeEmpty>(Match.Plain);
        var t2 = TypeExpression.Of<int>(Match.Plain);
        var t3 = TypeExpression.Of<int>(Match.Plain);
        Assert.True(t1 != t2);
        Assert.False(t3 != t2);
    }

    [Fact]
    public void Can_Create_For_Type()
    {
        var tx1 = TypeExpression.Of(typeof(TypeEmpty), Match.Plain);
        var tx2 = TypeExpression.Of(typeof(TypeEmpty), Match.Any);
        var tx3 = TypeExpression.Of(typeof(TypeEmpty), new Entity(null!, new(123)));

        Assert.False(tx1.isRelation);
        Assert.True(tx2.isWildcard);
        Assert.True(tx3.isRelation);
    }


    [Fact]
    public void None_Matches_only_None()
    {
        var none = TypeExpression.Of<TypeEmpty>(Match.Plain);
        var any = TypeExpression.Of<TypeEmpty>(Match.Any);
        var obj = TypeExpression.Of<TypeEmpty>(Match.Object);
        var rel = TypeExpression.Of<TypeEmpty>(Match.Entity);

        var ent = TypeExpression.Of<TypeEmpty>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeEmpty>(Link.With("hello world"));

        Assert.True(none.Matches(none));
        Assert.False(none.Matches(any));
        Assert.False(none.Matches(obj));
        Assert.False(none.Matches(rel));
        Assert.False(none.Matches(ent));
        Assert.False(none.Matches(lnk));
    }


    [Fact]
    public void Any_Matches_only_All()
    {
        var any = TypeExpression.Of<TypeEmpty>(Match.Any);

        var typ = TypeExpression.Of<TypeEmpty>(Match.Plain);
        var ent = TypeExpression.Of<TypeEmpty>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeEmpty>(Link.With("hello world"));

        Assert.True(any.Matches(typ));
        Assert.True(any.Matches(ent));
        Assert.True(any.Matches(lnk));
    }


    [Fact]
    public void Object_Matches_only_Objects()
    {
        var obj = TypeExpression.Of<TypeEmpty>(Match.Object);

        var typ = TypeExpression.Of<TypeEmpty>(Match.Plain);
        var ent = TypeExpression.Of<TypeEmpty>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeEmpty>(Link.With("hello world"));

        Assert.False(obj.Matches(typ));
        Assert.False(obj.Matches(ent));
        Assert.True(obj.Matches(lnk));
    }


    [Fact]
    public void Relation_Matches_only_Relations()
    {
        var rel = TypeExpression.Of<TypeEmpty>(Match.Entity);

        var typ = TypeExpression.Of<TypeEmpty>(Match.Plain);
        var ent = TypeExpression.Of<TypeEmpty>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeEmpty>(Link.With("hello world"));

        Assert.False(rel.Matches(typ));
        Assert.True(rel.Matches(ent));
        Assert.False(rel.Matches(lnk));
    }


    [Fact]
    public void Target_Matches_all_Entity_Target_Relations()
    {
        var rel = TypeExpression.Of<TypeEmpty>(Match.Target);

        var typ = TypeExpression.Of<TypeEmpty>(Match.Plain);
        var ent = TypeExpression.Of<TypeEmpty>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeEmpty>(Link.With("hello world"));

        Assert.False(rel.Matches(typ));
        Assert.True(rel.Matches(ent));
        Assert.True(rel.Matches(lnk));
    }


    [Fact]
    public void Entity_only_matches_Entity()
    {
        var typ = TypeExpression.Of<TypeEmpty>(Match.Plain);
        var ent = TypeExpression.Of<TypeEmpty>(new Entity(null!, new(123)));
        var lnk = TypeExpression.Of<TypeEmpty>(Link.With("hello world"));

        Assert.False(ent.Matches(typ));
        Assert.True(ent.Matches(ent));
        Assert.False(ent.Matches(lnk));
    }

    [Fact]
    public void HasCorrectSize_in_Flags_B()
    {
        var flags = LanguageType.FlagsOf<TypeInt>();
        Assert.True(flags.HasFlag(TypeFlags.Unmanaged));
        Assert.Equal(Unsafe.SizeOf<TypeInt>(), (int)(flags & TypeFlags.SIMDSize));
    }

    [Fact]
    public void HasCorrectSize_in_Flags_TypeEmpty()
    {
        var flags = LanguageType.FlagsOf<TypeEmpty>();
        Assert.True(flags.HasFlag(TypeFlags.Unmanaged));
        var size = Unsafe.SizeOf<TypeEmpty>();
        var actual = (int)(flags & TypeFlags.SIMDSize);
        Assert.Equal(size, actual);
    }

    [Fact]
    public void HasCorrectSize_in_Flags_TypeIntInt()
    {
        var flags = LanguageType.FlagsOf<TypeIntInt>();
        Assert.True(flags.HasFlag(TypeFlags.Unmanaged));
        var size = Unsafe.SizeOf<TypeIntInt>();
        var actual = (int)(flags & TypeFlags.SIMDSize);
        Assert.Equal(size, actual);
    }

    [Theory]
    [InlineData(typeof(TypeEmpty), 1)] // sic, empty structs are 1 byte
    [InlineData(typeof(TypeInt), 4)]
    [InlineData(typeof(TypeIntInt), 8)]
    [InlineData(typeof(TypeDouble), 8)]
    [InlineData(typeof(TypeDoubleIntTight), 12)]
    public void HasCorrectSize_structs(Type type, int size)
    {
        var flags = LanguageType.Flags(type);
        Assert.True(flags.HasFlag(TypeFlags.Unmanaged));
        Assert.Equal(size, (int)(flags & TypeFlags.SIMDSize));
    }

    [Theory]
    [InlineData(typeof(TypeManagedClass))] // sic, empty structs are 1 byte
    [InlineData(typeof(TypeManagedRecord))]
    [InlineData(typeof(TypeManagedStruct))]
    [InlineData(typeof(string))]
    [InlineData(typeof(Thread))]
    [InlineData(typeof(HashSet<int>))]
    [InlineData(typeof(byte[]))]
    public void Recognizes_Managed_Types(Type type)
    {
        var flags = LanguageType.Flags(type);
        Assert.False(flags.HasFlag(TypeFlags.Unmanaged));
    }

    private struct TypeEmpty;

    private struct TypeInt
    {
        private int _value;
    };

    private struct TypeIntInt
    {
        private int _value;
        private int _value2;
    };

    private record struct TypeDouble
    {
        private double _value;
    }

    private record TypeManagedRecord;
    private class TypeManagedClass;

    private struct TypeManagedStruct
    {
        private int[] _data;
    }
    

    [StructLayout(LayoutKind.Explicit, Size = 12)]
    private record struct TypeDoubleIntTight(double Value, int Value2)
    {
        [FieldOffset(0)]
        public double Value = Value;

        [FieldOffset(8)]
        public int Value2 = Value2;
    }
}