namespace fennecs;

public static class Match
{
    /// <summary>
    /// In Query Matching; matches ONLY Plain Components, i.e. those without a Relation Target.
    /// </summary>
    /// <remarks>
    /// Formerly known as "None"
    /// </remarks>
    public static readonly Entity Plain = default; // == 0-bit == new(0,0)

    /// <summary>
    /// In Query Matching; matches ANY target, including None:
    /// <ul>
    /// <li>(plain components)</li>
    /// <li>(entity-entity relations)</li>
    /// <li>(entity-object relations)</li>
    /// </ul>
    /// </summary>
    public static readonly Entity Any = new(-1, 0);

    /// <summary>
    ///  In Query Matching; matches ALL Relations with a Target:
    /// <ul>
    /// <li>(entity-entity relations)</li>
    /// <li>(entity-object relations)</li>
    /// </ul>
    /// </summary>
    public static readonly Entity Relation = new(-2, 0);

    /// <summary>
    /// In Query Matching; matches ONLY Entity-Entity relations.
    /// </summary>
    public static readonly Entity Entity = new(-3, 0);

    /// <summary>
    /// In Query Matching; matches ONLY Entity-Object links.
    /// </summary>
    public static readonly Entity Object = new(-4, 0);
}