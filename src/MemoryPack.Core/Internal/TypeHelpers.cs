namespace MemoryPack.Internal;

// TODO: use or remove this?

internal static class TypeHelpers
{
    public static bool IsReferenceOrNullable<T>()
    {
        return Cache<T>.IsReferenceOrNullable;
    }

    static class Cache<T>
    {
        public static bool IsReferenceOrNullable;

        static Cache()
        {
            var type = typeof(T);
            IsReferenceOrNullable = !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }
    }
}
