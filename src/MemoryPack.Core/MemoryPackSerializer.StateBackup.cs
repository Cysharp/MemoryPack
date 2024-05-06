using System.Runtime.CompilerServices;

namespace MemoryPack;

public static partial class MemoryPackSerializer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StateSnapshot ResetState(bool resetReaderState = true, bool resetWriterState = true)
        => new(resetReaderState, resetWriterState);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StateSnapshot ResetReaderState()
        => new(true, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StateSnapshot ResetWriterState()
        => new(false, true);

    // Nested types

    public readonly struct StateSnapshot : IDisposable
    {
        readonly bool _resetReaderState;
        readonly bool _resetWriterState;
        readonly SerializerWriterThreadStaticState? _threadStaticState;
        readonly MemoryPackWriterOptionalState? _threadStaticWriterOptionalState;
        readonly MemoryPackReaderOptionalState? _threadStaticReaderOptionalState;

        internal StateSnapshot(bool resetReaderState, bool resetWriterState)
        {
            _resetReaderState = resetReaderState;
            _resetWriterState = resetWriterState;

            if (resetReaderState)
            {
                _threadStaticReaderOptionalState = threadStaticReaderOptionalState;
                threadStaticReaderOptionalState = null;

            }

            if (resetWriterState)
            {
                _threadStaticState = threadStaticState;
                threadStaticState = null;
                _threadStaticWriterOptionalState = threadStaticWriterOptionalState;
                threadStaticWriterOptionalState = null;
            }
        }

        public void Dispose()
        {
            if (_resetReaderState)
            {
                threadStaticReaderOptionalState = _threadStaticReaderOptionalState;
            }

            if (_resetWriterState)
            {
                threadStaticState = _threadStaticState;
                threadStaticWriterOptionalState = _threadStaticWriterOptionalState;
            }
        }
    }
}
