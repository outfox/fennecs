namespace fennecs;

public enum StorageType : ushort
{
    /// <summary>
    /// Void storage. (used for tags)
    /// </summary>
    Void = 0,

    /// <summary>
    /// Flat Data storage (most components use this)
    /// </summary>
    Data = 1,

    /// <summary>
    /// Singleton Data storage (most components use this)
    /// </summary>
    [Obsolete("Reserved for future use.", true)]
    Singleton = 2,

    /// <summary>
    /// Reserved for future use.
    /// </summary>
    [Obsolete("Reserved for future use.", true)]
    Spatial1 = 3,

    /// <summary>
    /// Reserved for future use.
    /// </summary>
    [Obsolete("Reserved for future use.", true)]
    Spatial2 = 4,

    /// <summary>
    /// Reserved for future use.
    /// </summary>
    [Obsolete("Reserved for future use.", true)]
    Spatial3 = 5,

    /// <summary>
    /// Reserved for future use.
    /// </summary>
    [Obsolete("Reserved for future use.", true)]
    Reserved6 = 6,

    /// <summary>
    /// Reserved for future use.
    /// </summary>
    [Obsolete("Reserved for future use.", true)]
    Reserved7 = 7,
    
    Mask = 0xF000,
}