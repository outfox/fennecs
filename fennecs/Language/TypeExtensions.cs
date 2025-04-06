using System.Reflection;

namespace fennecs.Language;

internal static class TypeExtensions
{
    public static bool IsUnmanaged(this System.Type t)
    {
        if (t.IsPrimitive || t.IsPointer || t.IsEnum) return true;
        
        if (!t.IsValueType || t.IsGenericType || t.IsByRef || t.IsByRefLike) return false;

        var fields = t.GetFields(BindingFlags.Public
                                 | BindingFlags.NonPublic
                                 | BindingFlags.Instance);
        
        // Recursively check all fields.
        return fields.All(x => x.FieldType.IsUnmanaged());
    }
}