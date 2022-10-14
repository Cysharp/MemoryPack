// See https://aka.ms/new-console-template for more information
using MemoryPack;
using System.Buffers;
using System.Runtime.CompilerServices;

Console.WriteLine("Hello, World!");

// TODO: add this.
#pragma warning disable CS9074 // The 'scoped' modifier of parameter doesn't match overridden or implemented member.

//[MemoryPackable]
public partial class HelloMemoryPackable
{
    public int MyProperty { get; set; }
}


[MemoryPackable]
public partial class HelloMemoryPackable2
{
    public HelloMemoryPackable? MyProperty { get; set; }
    // public TypeAccessException My3Property { get; set; }
}


partial class HelloMemoryPackable : IMemoryPackable<HelloMemoryPackable>
{
    static HelloMemoryPackable()
    {
        // MemoryPackFormatterProvider.Register<HelloMemoryPackable>();
        // TODO: call directly.
        RegisterFormatter();
    }

    public static void RegisterFormatter()
    {
        if (!MemoryPackFormatterProvider.IsRegistered<HelloMemoryPackable>())
        {
            // TODO: add self formatter.
            MemoryPackFormatterProvider.Register(new HelloMemoryPackableFormatter());
        }

    }

    public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref HelloMemoryPackable? value)
        // TODO: additional constraint
        where TBufferWriter : class, IBufferWriter<byte>
    {

        if (value == null)
        {
            writer.WriteNullObjectHeader();
            goto END;
        }

        writer.WriteUnmanagedWithObjectHeader(1, value.MyProperty);

    END:

        return;
    }

    public static void Deserialize(ref MemoryPackReader reader, ref HelloMemoryPackable? value)
    {

        if (!reader.TryReadObjectHeader(out var count))
        {
            value = default!;
            goto END;
        }

        int __MyProperty;

        if (count == 1)
        {
            if (value == null)
            {
                reader.ReadUnmanaged(out __MyProperty);


                goto NEW;
            }
            else
            {
                __MyProperty = value.MyProperty;

                reader.ReadUnmanaged(out __MyProperty);

                goto SET;
            }

        }
        else if (count > 1)
        {
            MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);
            goto END;
        }
        else
        {
            if (value == null)
            {
                __MyProperty = default!;
            }
            else
            {
                __MyProperty = value.MyProperty;
            }


            if (count == 0) goto SKIP_READ;
            reader.ReadUnmanaged(out __MyProperty); if (count == 1) goto SKIP_READ;

            SKIP_READ:
            if (value == null)
            {
                goto NEW;
            }
            else
            {
                goto SET;
            }

        }

    SET:

        value.MyProperty = __MyProperty;
        goto END;

    NEW:
        value = new HelloMemoryPackable()
        {
            MyProperty = __MyProperty
        };
    END:

        return;
    }

    // Add additional formatter
    sealed class HelloMemoryPackableFormatter : MemoryPackFormatter<HelloMemoryPackable>
    {
        // TODO: check langversion? 11 or above, use scoped
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer,  ref HelloMemoryPackable? value)
        {
            HelloMemoryPackable.Serialize(ref writer, ref value);
        }

        public override void Deserialize(ref MemoryPackReader reader, ref HelloMemoryPackable? value)
        {
            HelloMemoryPackable.Deserialize(ref reader, ref value);
        }
    }
}


//partial class HelloMemoryPackable2 : IMemoryPackable<HelloMemoryPackable2>
//{
//    static HelloMemoryPackable2()
//    {
//        MemoryPackFormatterProvider.Register<HelloMemoryPackable2>();
//    }

//    public static void RegisterFormatter()
//    {
//        if (!MemoryPackFormatterProvider.IsRegistered<HelloMemoryPackable2>())
//        {
//            MemoryPackFormatterProvider.Register(new MemoryPack.Formatters.MemoryPackableFormatter<HelloMemoryPackable2>());
//        }

//    }

//    public static void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref HelloMemoryPackable2? value)
//    {

//        if (value == null)
//        {
//            writer.WriteNullObjectHeader();
//            goto END;
//        }

//        writer.WriteObjectHeader(1);
//        writer.WritePackable(value.MyProperty);

//    END:

//        return;
//    }

//    public static void Deserialize(ref MemoryPackReader reader, ref HelloMemoryPackable2? value)
//    {

//        if (!reader.TryReadObjectHeader(out var count))
//        {
//            value = default!;
//            goto END;
//        }

//        global::HelloMemoryPackable __MyProperty;

//        if (count == 1)
//        {
//            if (value == null)
//            {
//                __MyProperty = reader.ReadPackable<global::HelloMemoryPackable>();


//                goto NEW;
//            }
//            else
//            {
//                __MyProperty = value.MyProperty;

//                reader.ReadPackable(ref __MyProperty);

//                goto SET;
//            }

//        }
//        else if (count > 1)
//        {
//            MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);
//            goto END;
//        }
//        else
//        {
//            if (value == null)
//            {
//                __MyProperty = default!;
//            }
//            else
//            {
//                __MyProperty = value.MyProperty;
//            }


//            if (count == 0) goto SKIP_READ;
//            reader.ReadPackable(ref __MyProperty); if (count == 1) goto SKIP_READ;

//            SKIP_READ:
//            if (value == null)
//            {
//                goto NEW;
//            }
//            else
//            {
//                goto SET;
//            }

//        }

//    SET:

//        value.MyProperty = __MyProperty;
//        goto END;

//    NEW:
//        value = new HelloMemoryPackable2()
//        {
//            MyProperty = __MyProperty
//        };
//    END:

//        return;
//    }
//}
