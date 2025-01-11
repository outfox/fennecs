using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace fennecs.CRUD;

/// <summary>
/// Provides structural change (Add and Remove component) operations on entities.
/// </summary>
public interface IAddRemove<out SELF>
{
    /// <summary>
    /// Add a Plain component with value of type C to the entity/entities.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<C>(C component, Key key = default) where C : notnull;

    /// <summary>
    /// Add a newable component of type C to the entity/entities, with an optional Key.
    /// </summary>
    /// <example>
    /// This will call the default parameterless constructor of the backing component type.
    /// </example>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Add<C>(Key key = default) where C : notnull, new();// => Add(new C(), key);

    /// <summary>
    /// Remove ALL components of type C  matching the Match Expression from the entity/entities.
    /// </summary>
    /// <param name="match">the match term to match the secondary key against, can be a Wildcard or a specific Key, including an Entity. <c>default</c> matches only plain components (no secondary key)</param>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<C>(Match match = default) where C : notnull;// => Remove(MatchExpression.Of<C>(match));
    
    /// <summary>
    /// Remove all components of type C from the entity/entities, matching the Match Expression.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove(MatchExpression expression);

    /// <summary>
    /// Remove all components of type C from the entity/entities, matching the Match Expression.
    /// </summary>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Remove<C>(C target) where C : class;// => Remove<C>(Key.Of(target));

    /// <summary>
    /// Add a Object Link component with an Object of type L to the entity/entities.
    /// </summary>
    /// <example>
    /// <c>implementor.Link("An Object")</c>. This differs from <c>implementor.Add("An Object")</c> in that the former will create a new Key for the Object Link, while the latter will treat it as a plain Component.
    /// <para>
    /// Remove the link by calling <c>Remove&lt;L&gt;(link)</c>, <c>Remove&lt;L&gt;(Key.Of(link))</c> or <c>Remove&lt;L&gt;(link.Key())</c>.
    /// You can remove all links on an implementor by calling <c>Remove&lt;L&gt;(Link.Any)</c> (also if you don't have the concrete object).
    /// </para>
    /// </example>
    /// <remarks>
    /// Object Links are not forcefully kept consistent by the World, so their backing component value can be changed in user code.
    /// This semantically detaches the link but the entities will still be matched due to their old key. It is recommended to replace Links by
    /// removing the old and then adding the new link instead of changing the value of the backing component (which would preserve the old key).
    /// </remarks>
    /// <returns>itself (fluent pattern)</returns>
    public SELF Link<L>(L link) where L : class => Add(link, Key.Of(link));
}

/// <summary>
/// Provides structural change (Add and Remove component) operations on entities.
/// </summary>
/// Operations may be deferred until the end of the scope or world lock.
/// For this purpose, <see cref="CallerFilePathAttribute"/> and <see cref="CallerLineNumberAttribute"/> are used to provide meaningful error messages if necessary.
// ReSharper disable ExplicitCallerInfoArgument
public interface IAddRemoveDeferrable<out SELF>
{
    /// <inheritdoc cref="IAddRemove{SELF}.Add{C}(C,Key)"/>
    /// Operation may be deferred until the last World lock is released (usually the end of the scope of a runner).
    public SELF Add<C>(C component, Key key = default, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0) where C : notnull;

    /// <inheritdoc cref="IAddRemove{SELF}.Add{C}(C,Key)"/>
    /// Operation may be deferred until the last World lock is released (usually the end of the scope of a runner).
    public SELF Add<C>(Key key = default, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0) where C : notnull, new();// => Add(new C(), key, callerFile, callerLine);

    /// <inheritdoc cref="IAddRemove{SELF}.Remove{C}(Match)"/>
    /// Operation may be deferred until the last World lock is released (usually the end of the scope of a runner).
    public SELF Remove<C>(Match match = default, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0) where C : notnull;// => Remove(MatchExpression.Of<C>(match), callerFile, callerLine);
    
    /// <inheritdoc cref="IAddRemove{SELF}.Remove{C}(Match)"/>
    /// Operation may be deferred until the last World lock is released (usually the end of the scope of a runner).
    public SELF Remove(MatchExpression expression, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0);

    /// <inheritdoc cref="IAddRemove{SELF}.Link{L}(L)"/>
    /// Operation may be deferred until the last World lock is released (usually the end of the scope of a runner).
    public SELF Link<L>(L link, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0) where L : class;// => Add(link, Key.Of(link), callerFile, callerLine);
}