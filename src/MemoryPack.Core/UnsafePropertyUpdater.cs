using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace MemoryPack;

public static class UnsafePropertyUpdater
{
    private static readonly ConcurrentDictionary<(Type Type, string FieldName), Delegate> FieldGetterCache =
        new ConcurrentDictionary<(Type, string), Delegate>();

    public static Func<T, IntPtr> GetOrCreateFieldGetter<T>(string backingFieldName)
    {
        var key = (typeof(T), backingFieldName);
        if (FieldGetterCache.TryGetValue(key, out Delegate? cached))
        {
            return (Func<T, IntPtr>)cached;
        }
        else
        {
            Func<T, IntPtr> getter = CreateFieldGetter<T>(backingFieldName);
            FieldGetterCache.TryAdd(key, getter);
            return getter;
        }
    }

    public static Func<T, IntPtr> CreateFieldGetter<T>(string backingFieldName)
    {
        FieldInfo? field = typeof(T).GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
        {
            throw new InvalidOperationException($"Field '{backingFieldName}' not found on type {typeof(T)}.");
        }

        var dm = new DynamicMethod(
            "GetFieldAddress",
            typeof(IntPtr),
            new Type[] { typeof(T) },
            typeof(T),
            true);

        ILGenerator il = dm.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldflda, field);
        il.Emit(OpCodes.Conv_I);
        il.Emit(OpCodes.Ret);

        return (Func<T, IntPtr>)dm.CreateDelegate(typeof(Func<T, IntPtr>));
    }

    public static unsafe void SetInitOnlyProperty<T, TValue>(T instance, string propertyName, TValue newValue)
        where T : class
    {
        string backingFieldName = $"<{propertyName}>k__BackingField";
        Func<T, IntPtr> getter = GetOrCreateFieldGetter<T>(backingFieldName);
        IntPtr fieldPtr = getter(instance);

        Unsafe.WriteUnaligned(fieldPtr.ToPointer(), newValue);
    }
}
