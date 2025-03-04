using fennecs.CRUD;

namespace fennecs;

/// <summary>
/// An object in a fennecs.<see cref="fennecs.World"/>. Entities can have any number of Components.
/// This struct represents a snapshot of the Entity to store on the heap. (e.g. in a field or collection)
/// </summary>
/// <remarks>
/// Entities represented by this struct are Snapshots, therefore not guaranteed to be <see cref="Alive"/>,
/// and should be checked before use. (This is done automatically by all public methods
/// on this struct, and where they can they return a <see cref="EntityRef"/> for the remainder of the scope.)
/// </remarks>
public readonly struct Entity : IEntity, IComparable<Entity>, IComparable<EntityRef>
{
    internal Entity(Id id, uint generation)
    {
        _id = id;
        _generation = generation;
    }

        private readonly Id _id;
        private readonly uint _generation;
        
        internal uint Index => _id.Index;

        private ref Meta Meta => ref _id.Meta;

        /// <summary>
        /// Is this Entity alive in its World?
        /// </summary>
        public bool Alive => _id.Generation == _generation;

        /// <summary>
        /// <c>null</c> equivalent for Entity.
        /// </summary>
        public static readonly Snapshot None = default;

        /// <summary>
        /// The World this Entity belongs to.
        /// </summary>
        public World World => _id.World;

        /// <summary>
        /// The Archetype this Entity belongs to.
        /// </summary>
        public Archetype Archetype => Meta.Archetype;


        #region IComparable/IEquatable Implementation

        /// <inheritdoc cref="IEquatable{T}"/>
        public bool Equals(Entity other) => _id == other._id && _generation == other._generation;

        /// <inheritdoc cref="IComparable{T}"/>
        public int CompareTo(Entity other)
        {
            var id = this._id.CompareTo(other._id);
            return id == 0 ? _generation.CompareTo(other._generation) : id;
        }


        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(_id, _generation);

        #endregion

        /*

        #region Constructors / Creators

        /// <summary>
        /// Create a new Entity.
        /// </summary>
        internal Snapshot(Entity.Id Identity, uint generation)
        {
            this.Identity = Identity;
            _generation = generation;
        }

        /// <summary>
        /// Implicitly convert a Key to an Entity.
        /// </summary>
        /// <throws><see cref="ArgumentException"/> if the Key is not an Entity.</throws>
        /// <throws><see cref="InvalidOperationException"/> if the Entity is not alive.</throws>
        /// <throws><see cref="NullReferenceException"/> if the decoded Entity has an invalid World.</throws>
        /// <returns>the entity, if the Key is a living Entity</returns>
        public static implicit operator Snapshot(Key key) => new(key);


        /// <summary>
        /// Construct an Entity from a Key.
        /// </summary>
        /// <remarks>
        /// The entity may technically not be alive, but relation keys are usually guaranteed to be alive if the world hasn't changed since it was obtained.
        /// </remarks>
        /// <throws><see cref="ArgumentException"/> if the Key is not an Entity relation key.</throws>
        /// <throws><see cref="NullReferenceException"/> if the decoded World is invalid.</throws>
        public Snapshot(Key key)
        {
            if (!key.IsIdentity) throw new ArgumentException("Key must be an Entity.");
            Identity = key.Id;
        }

        /// <summary>
        /// The Key of this Entity (for use in relations).
        /// </summary>
        internal Key Key => new(this);

        #endregion

        /// <inheritdoc />
        public IReadOnlyList<Component> Components => World.GetComponents(this);

        /// <summary>
        /// <para><b>Wildcard match expression for Entity iteration.</b><br/>This matches only <b>Entity-Entity</b> Relations of the given Stream Type.
        /// </para>
        /// <para>This expression is free when applied to a Filter expression, see <see cref="Query"/>.
        /// </para>
        /// <para>Applying this to a Query's Stream Type can result in multiple iterations over entities if they match multiple component types. This is due to the wildcard's nature of matching all components.</para>
        /// </summary>
        /// <inheritdoc cref="Any"/>
        public static readonly Match Any = Match.Entity;


        /// <inheritdoc />
        public override string ToString()
        {
            var gen = Alive ? $"g{_generation:D5}" : "(DEAD)";
            return $"E{Identity}{gen}";
        }

        /// <inheritdoc />
        public Snapshot Add<C>(C component, Key key = default, [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) where C : notnull
        {
            World.AddComponent(this, TypeExpression.Of<C>(key), component, callerFile, callerLine);
            return this;
        }

        /// <inheritdoc />
        public Snapshot Add(object component, Key key = default, [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
        {
            World.AddComponent(this, TypeExpression.Of(component.GetType(), key), component, callerFile, callerLine);
            return this;
        }

        /// <inheritdoc />
        public Snapshot Add<C>(Key key = default, [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) where C : notnull, new() => Add(new C(), key);

        /// <inheritdoc />
        public Snapshot Link<L>(L link, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
            where L : class
        {
            if (typeof(L) == typeof(object))
            {
                World.AddComponent(this, TypeExpression.Of(link.GetType(), Key.Of(link)), link);
                return this;
            }

            World.AddComponent(this, TypeExpression.Of<L>(Key.Of(link)), link);
            return this;
        }

        /// <inheritdoc />
        public Snapshot Remove<C>(Match match = default, [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0) where C : notnull => Remove(MatchExpression.Of<C>(match));

        /// <inheritdoc />
        public Snapshot Remove(MatchExpression expression, [CallerFilePath] string callerFile = "",
            [CallerLineNumber] int callerLine = 0)
        {
            World.RemoveComponent(this, expression);
            return this;
        }

        /// <inheritdoc />
        public bool Has<C>(Match match = default) where C : notnull => World.HasComponent<C>(this, match);

        /// <inheritdoc />
        public bool Has(Type type, Match match = default) => World.HasComponent(this, type, match);

        /// <inheritdoc />
        public void Despawn() => World.Despawn(this);


        /// <summary>
        /// Returns a <c>ref readonly</c> to a component of the given type, matching the given Key.
        /// </summary>
        public ref readonly C Get<C>(Key key = default) where C : notnull => ref World.GetComponent<C>(this, key);

        /// <summary>
        /// Gets the component from the Entity (boxed)
        /// </summary>
        public object Get(Type type, Key key = default)
        {
            if (World.TryGetComponent(this, TypeExpression.Of(type, key), out var component)) return component;
            throw new InvalidOperationException($"Entity {this} does not have a component of {type} for key {key}");
        }


        /// <summary>
        /// Returns a <c>ref readonly</c> to a component of the given type, matching the given Key.
        /// </summary>
        public bool TryGet(Type type, Key key, [MaybeNullWhen(false)] out object component)
        {
            return World.TryGetComponent(this, TypeExpression.Of(type, key), out component);
        }

        /// <summary>
        /// Returns a <c>ref readonly</c> to a component of the given type, matching the given Key.
        /// </summary>
        public bool TryGet(Type type, [MaybeNullWhen(false)] out object component)
        {
            return World.TryGetComponent(this, TypeExpression.Of(type), out component);
        }

        /// <summary>
        /// Returns a List of all component type expressions accompanied by their values matching the provided term and backing type <see cref="T"/>.
        /// </summary>
        /// <remarks>
        /// The values are copies, but if the components are reference types, these values will reference the same objects.
        /// <see cref="PooledList{T}"/> should be Disposed if possible, either by declaring them in a using statement, or by calling their <see cref="IDisposable.Dispose"/> method.
        /// </remarks>
        /// <returns><c>PooledList&lt;(TypeExpression type, T value)&gt;</c></returns>
        public PooledList<(TypeExpression expression, T value)> GetAll<T>(Match match) where T : notnull
        {
            using var storages = Archetype.Match<T>(match);
            var list = PooledList<(TypeExpression type, T value)>.Rent();
            var row = Row;
            list.AddRange(storages.Select(storage => (storage.Expression, storage[row])));
            return list;
        }

        /// <summary>
        /// Returns a <c>ref</c> to a component of the given type, matching the given Key.
        /// </summary>
        public ref C Write<C>(Key key = default) where C : notnull => ref Archetype.GetStorage<C>(key)[Row];

        /// <summary>
        /// Returns a <c>ref readonly</c> to a component of the given type, matching the given Key.
        /// </summary>
        public ref readonly C Read<C>(Key key = default) where C : notnull => ref Archetype.GetStorage<C>(key)[Row];

        /// <summary>
        /// Sets all components of the given backing type on the Entity, matching the given Match term.
        /// Doesn't add the component.
        /// </summary>
        /// <remarks>
        /// This (as all functions taking a Match term) supports wildcards.
        /// </remarks>
        public Snapshot Set(object value, Match match = default)
        {
            using var storages = Archetype.Match(value.GetType(), match);
            foreach (var storage in storages) storage.Store(Row, value);
            return this;
        }

        /// <summary>
        /// Sets all components of the given backing type on the Entity, matching the given Match term.
        /// Doesn't add the component.
        /// </summary>
        /// <remarks>
        /// This (as all functions taking a Match term) supports wildcards.
        /// </remarks>
        public Snapshot Set<C>(in C value, Match match = default) where C : notnull
        {
            using var storages = Archetype.Match<C>(match);
            foreach (var storage in storages) storage.Store(Row, value);
            return this;
        }

        /// <summary>
        /// Returns a reference to a component of the given type, matching the given Key.
        /// </summary>
        /// <remarks>
        /// Only use this if you need to work with the component directly, otherwise it is recommended to use <see cref="Snapshot.Get{C}(fennecs.Key)"/> and <see cref="Set{C}"/>.
        /// </remarks>
        public RWImmediate<C> Ref<C>(Key key = default) where C : notnull =>
            new(ref World.GetComponent<C>(this, key), this, key);

        /// <inheritdoc />
        public bool Has(MatchExpression expression) => World.HasComponent(this, expression);

        /// <inheritdoc />
        public bool Has<L>(L link) where L : class => World.HasComponent(this, MatchExpression.Of<L>(link.Key()));

        /// <summary>Truthy if the Entity is alive.</summary>
        public static implicit operator bool(Snapshot entity) => entity.Alive;
        */
        
        #region IEntity

        /// <inheritdoc />
        public Entity Add<C>(C component, Key key = default, string callerFile = "", int callerLine = 0) where C : notnull => entity.Add(component, key, callerFile, callerLine);

        /// <inheritdoc />
        public Entity Add<C>(Key key = default, string callerFile = "", int callerLine = 0) where C : notnull, new() => entity.Add<C>(key, callerFile, callerLine);

        /// <inheritdoc />
        public Entity Remove<C>(Match match = default, string callerFile = "", int callerLine = 0) where C : notnull => entity.Remove<C>(match, callerFile, callerLine);

        /// <inheritdoc />
        public Entity Remove(MatchExpression expression, string callerFile = "", int callerLine = 0) => entity.Remove(expression, callerFile, callerLine);

        /// <inheritdoc />
        public Entity Link<L>(L link, string callerFile = "", int callerLine = 0) where L : class => entity.Link(link, callerFile, callerLine);

        /// <inheritdoc />
        public bool Has<C>(Match match = default) where C : notnull => entity.Has<C>(match);

        /// <inheritdoc />
        public bool Has<L>(L link) where L : class => entity.Has(link);

        /// <inheritdoc />
        public bool Has(Type type, Match match) => entity.Has(type, match);

        /// <inheritdoc />
        public bool Has(MatchExpression expression) => entity.Has(expression);

        /// <inheritdoc />
        public void Despawn() => entity.Despawn();

        /// <inheritdoc />
        public IReadOnlyList<Component> Components => entity.Components;

        #endregion
        
}