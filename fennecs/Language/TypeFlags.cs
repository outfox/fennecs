namespace fennecs.Language;

[Flags]
internal enum TypeFlags : ushort
{ 
    SIMDSize  = 0x1fff, // bottom 12 bits.
    Unmanaged = 0x8000, // top bit.
}