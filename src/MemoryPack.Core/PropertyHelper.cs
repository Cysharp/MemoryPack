using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace MemoryPack;

public static class PropertyHelper
{
    private static readonly ConcurrentDictionary<(Type Type, string FieldName), Delegate> _cache = new();

    public static void SetInitOnlyProperty<T, TValue>(T instance, string propertyName, TValue newValue)
    {
        Type typeOfT = typeof(T);
        var key = (typeOfT, propertyName);
        if (!_cache.TryGetValue(key, out Delegate? setter))
        {
            var prop = typeOfT.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop is null)
            {
                throw new ArgumentException($"Property \"{propertyName}\" not found in type \"{typeOfT}\"");
            }

            var setMethod = prop.GetSetMethod(true);
            if (setMethod is null)
            {
                throw new ArgumentException($"Property \"{propertyName}\"  does not have a setter.");
            }

            var instanceParameter = Expression.Parameter(typeOfT, "instance");
            var valueParameter = Expression.Parameter(typeof(TValue), "value");
            var callExpr = Expression.Call(instanceParameter, setMethod, valueParameter);
            var lambda = Expression.Lambda<Action<T, TValue>>(callExpr, instanceParameter, valueParameter);
            setter = lambda.Compile();
            _cache.TryAdd(key, setter);
        }

        ((Action<T, TValue>)setter).Invoke(instance, newValue);
    }
}
