using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using fennecs.CRUD;
using fennecs.pools;
using fennecs.storage;

namespace fennecs;

public readonly ref partial struct Entity
{
    /// <summary>
    /// Stored (internal use only) representation of an Entity.
    /// </summary>
    internal readonly record struct Id(uint Value) : IComparable<Id>
    {
        internal bool Valid => Value != default;

        /// <summary>
        /// The Index of this Entity.
        /// </summary>
        internal uint Index => Value & ~World.Mask;

        internal uint WorldIndex => Value & World.Mask >> World.Shift;


        internal ref Meta Meta => ref World.GetEntityMeta(this);
        internal Archetype Archetype => Meta.Archetype;
        internal int Row => Meta.Row;

        internal uint Generation => World.GetGeneration(this);


        /// <inheritdoc />
        public int CompareTo(Id other) => Value.CompareTo(other.Value);


        /// <inheritdoc />
        public override string ToString() => new Entity(in this).ToString();
        

        /// <summary>
        /// The World this Entity lives in.
        /// </summary>
        public World World => World.Get(WorldIndex);

        public Snapshot Snapshot => new(this);
    }
}