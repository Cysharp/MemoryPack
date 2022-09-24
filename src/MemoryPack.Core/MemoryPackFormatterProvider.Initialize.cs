using MemoryPack.Formatters;
using System.Runtime.CompilerServices;

namespace MemoryPack;

public static partial class MemoryPackFormatterProvider
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        RegisterWellKnownTypesFormatters();
    }

    internal static Type? TryCreateTupleFormatterType(Type type)
    {
        if (type.IsGenericType && type.FullName != null && type.FullName.StartsWith("System.Tuple"))
        {
            Type? tupleFormatterType = null;
            switch (type.GenericTypeArguments.Length)
            {
                case 1:
                    tupleFormatterType = typeof(TupleFormatter<>);
                    break;
                case 2:
                    tupleFormatterType = typeof(TupleFormatter<,>);
                    break;
                case 3:
                    tupleFormatterType = typeof(TupleFormatter<,,>);
                    break;
                case 4:
                    tupleFormatterType = typeof(TupleFormatter<,,,>);
                    break;
                case 5:
                    tupleFormatterType = typeof(TupleFormatter<,,,,>);
                    break;
                case 6:
                    tupleFormatterType = typeof(TupleFormatter<,,,,,>);
                    break;
                case 7:
                    tupleFormatterType = typeof(TupleFormatter<,,,,,,>);
                    break;
                case 8:
                    tupleFormatterType = typeof(TupleFormatter<,,,,,,,>);
                    break;
                default:
                    break;
            }

            if (tupleFormatterType != null)
            {
                return tupleFormatterType.MakeGenericType(type.GenericTypeArguments);
            }
        }

        return null;
    }

    internal static Type? TryCreateValueTupleFormatterType(Type type)
    {
        if (type.IsGenericType && type.FullName != null && type.FullName.StartsWith("System.ValueTuple"))
        {
            Type? tupleFormatterType = null;
            switch (type.GenericTypeArguments.Length)
            {
                case 1:
                    tupleFormatterType = typeof(ValueTupleFormatter<>);
                    break;
                case 2:
                    tupleFormatterType = typeof(ValueTupleFormatter<,>);
                    break;
                case 3:
                    tupleFormatterType = typeof(ValueTupleFormatter<,,>);
                    break;
                case 4:
                    tupleFormatterType = typeof(ValueTupleFormatter<,,,>);
                    break;
                case 5:
                    tupleFormatterType = typeof(ValueTupleFormatter<,,,,>);
                    break;
                case 6:
                    tupleFormatterType = typeof(ValueTupleFormatter<,,,,,>);
                    break;
                case 7:
                    tupleFormatterType = typeof(ValueTupleFormatter<,,,,,,>);
                    break;
                case 8:
                    tupleFormatterType = typeof(ValueTupleFormatter<,,,,,,,>);
                    break;
                default:
                    break;
            }

            if (tupleFormatterType != null)
            {
                return tupleFormatterType.MakeGenericType(type.GenericTypeArguments);
            }
        }

        return null;
    }
}
