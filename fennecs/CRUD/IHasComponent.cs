using System.Runtime.CompilerServices;

namespace fennecs.CRUD;

/// <summary>
/// Objects of this type can express the presence of components on an entity or set of entities.
/// </summary>
public interface IHasComponent
{
    #region Convenience Defaults

    /// <summary>
    /// Check for presence of one or more component of the specified type, matching the given match term.
    /// </summary>
    /// <typeparam name="C">backing Type of the component to check for</typeparam>
    /// <param name="match">
    /// <c>default</c> matches only Plain Components, i.e. Components without a secondary key.
    /// Besides Wildcards <see cref="Match.Any">Match.Any</see>, <see cref="Entity.Any">Entity.Any</see>, <see cref="Link.Any">Link.Any</see> you may also directly specify a secondary key of type <see cref="fennecs.Key">fennecs.Key</see>.
    /// <br/>Tip: Key can be an Entity, to match a specific relation.
    /// </param>
    /// <returns>true if the entity/entities has one or more matching component; otherwise, false.</returns>
    public bool Has<C>(Match match = default) where C : notnull => Has(MatchExpression.Of<C>(match));

    /// <summary>
    /// Check for presence of the Object Link component with the specified linked object.
    /// </summary>
    /// <typeparam name="L">backing and target type of the Object Link component to check for</typeparam>
    /// <param name="linkedObject">the linked object to check for, its Key (based on its type and hash coce) will be used as the secondary key.</param>
    /// <returns>true if the entity/entities has the component with the specified linked object; otherwise, false.</returns>
    public bool Has<L>(L linkedObject) where L : class => Has<L>(Key.Of(linkedObject));

    /// <summary>
    /// Check for presence of one or more component of the specified type, matching the given match term.
    /// </summary>
    /// <remarks>
    /// This overload is primarily intended for Runtime Type Identification (RTTI) use, e.g. when inspecting, reflecting, or deserializing data.
    /// </remarks>
    /// <param name="type">backing <see cref="Type">System.Type</see> of the component to check for</param>
    /// <param name="match">
    /// <c>default</c> matches only Plain Components, i.e. Components without a secondary key.
    /// Besides Wildcards <see cref="Match.Any">Match.Any</see>, <see cref="Entity.Any">Entity.Any</see>, <see cref="Link.Any">Link.Any</see> you may also directly specify a secondary key of type <see cref="fennecs.Key">fennecs.Key</see>.
    /// </param>
    /// <returns>true if the entity/entities has one or more matching component; otherwise, false.</returns>
    public bool Has(Type type, Match match) => Has(MatchExpression.Of(type, match));

    #endregion

    /// <summary>
    /// Check for presence of one or more components matching the given match expression.
    /// </summary>
    /// <remarks>
    /// <para>This method is intended for programmatic use, or in code paths that already determined all necessary expressions.</para>
    /// <br/>
    /// Preferred overloads for static typing are:
    /// <ul>
    /// <li><see cref="Has{C}(Match)"/></li>
    /// </ul>
    /// 
    /// Preferred overloads for typeless (RTTI) use are:
    /// <ul>
    /// <li><see cref="Has(Type, Match)"/></li>
    /// </ul>
    /// 
    /// </remarks>
    public bool Has(MatchExpression expression);
}