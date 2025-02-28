using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using fennecs.CRUD;
using fennecs.pools;
using fennecs.storage;

namespace fennecs;

/// <summary>
/// Represents a living Entity. Can be cast to Entity to get a snapshot annotated with a generation.
/// </summary>
public readonly ref partial struct Entity
{
    internal readonly record struct Id(uint Value) : IComparable<Id>
    {
        internal bool Valid => Value != default;

        /// <summary>
        /// The Index of this Entity.
        /// </summary>
        internal uint Index => Value & ~World.Mask;

        internal uint WorldIndex => Value & World.Mask >> World.Shift;

        
        private ref Meta Meta => ref World.GetEntityMeta(this);
        internal uint Gen => World.GetGeneration(this);
        internal int Row => Meta.Row;


        /// <inheritdoc />
        public int CompareTo(Id other) => Value.CompareTo(other.Value);


        /// <inheritdoc />
        public override string ToString() => new Entity(in this).ToString();
        

        /// <summary>
        /// The World this Entity lives in.
        /// </summary>
        public World World => World.Get(WorldIndex);

    }
}