using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack;

public ref partial struct DoNothingMemoryPackWriter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1>(scoped in T1 value1)
        where T1 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1>(byte propertyCount, scoped in T1 value1)
        where T1 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2>(scoped in T1 value1, scoped in T2 value2)
        where T1 : unmanaged
        where T2 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2>(byte propertyCount, scoped in T1 value1, scoped in T2 value2)
        where T1 : unmanaged
        where T2 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4, T5>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4, T5, T6>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4, T5, T6, T7>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13, scoped in T14 value14)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
        where T14 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>() + Unsafe.SizeOf<T14>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13, scoped in T14 value14)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
        where T14 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>() + Unsafe.SizeOf<T14>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13, scoped in T14 value14, scoped in T15 value15)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
        where T14 : unmanaged
        where T15 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>() + Unsafe.SizeOf<T14>() + Unsafe.SizeOf<T15>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13, scoped in T14 value14, scoped in T15 value15)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
        where T14 : unmanaged
        where T15 : unmanaged
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>() + Unsafe.SizeOf<T14>() + Unsafe.SizeOf<T15>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1>(scoped in T1 value1)
    {
        var size = Unsafe.SizeOf<T1>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1>(byte propertyCount, scoped in T1 value1)
    {
        var size = Unsafe.SizeOf<T1>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2>(scoped in T1 value1, scoped in T2 value2)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2>(byte propertyCount, scoped in T1 value1, scoped in T2 value2)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4, T5>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4, T5, T6>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4, T5, T6, T7>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13, scoped in T14 value14)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>() + Unsafe.SizeOf<T14>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13, scoped in T14 value14)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>() + Unsafe.SizeOf<T14>() + 1;
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanaged<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13, scoped in T14 value14, scoped in T15 value15)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>() + Unsafe.SizeOf<T14>() + Unsafe.SizeOf<T15>();
        Advance(size);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DangerousWriteUnmanagedWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(byte propertyCount, scoped in T1 value1, scoped in T2 value2, scoped in T3 value3, scoped in T4 value4, scoped in T5 value5, scoped in T6 value6, scoped in T7 value7, scoped in T8 value8, scoped in T9 value9, scoped in T10 value10, scoped in T11 value11, scoped in T12 value12, scoped in T13 value13, scoped in T14 value14, scoped in T15 value15)
    {
        var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + Unsafe.SizeOf<T5>() + Unsafe.SizeOf<T6>() + Unsafe.SizeOf<T7>() + Unsafe.SizeOf<T8>() + Unsafe.SizeOf<T9>() + Unsafe.SizeOf<T10>() + Unsafe.SizeOf<T11>() + Unsafe.SizeOf<T12>() + Unsafe.SizeOf<T13>() + Unsafe.SizeOf<T14>() + Unsafe.SizeOf<T15>() + 1;
        Advance(size);
    }

}
