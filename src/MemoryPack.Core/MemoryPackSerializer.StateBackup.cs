using System.Runtime.CompilerServices;

namespace MemoryPack;

public static partial class MemoryPackSerializer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StateBackup BackupState() => new(true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SerializerStateBackup BackupSerializerState() => new(true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DeserializerStateBackup BackupDeserializerState() => new(true);

    // Nested types

    public readonly struct StateBackup : IDisposable
    {
        readonly SerializerWriterThreadStaticState? _threadStaticState;
        readonly MemoryPackWriterOptionalState? _threadStaticWriterOptionalState;
        readonly MemoryPackReaderOptionalState? _threadStaticReaderOptionalState;
        readonly bool _isValid;

        internal StateBackup(bool isValid)
        {
            _threadStaticState = threadStaticState;
            _threadStaticWriterOptionalState = threadStaticWriterOptionalState;
            _threadStaticReaderOptionalState = threadStaticReaderOptionalState;
            _isValid = isValid;
        }

        public void Dispose()
        {
            if (!_isValid)
                return;

            threadStaticState = _threadStaticState;
            threadStaticWriterOptionalState = _threadStaticWriterOptionalState;
            threadStaticReaderOptionalState = _threadStaticReaderOptionalState;
        }
    }

    public readonly struct SerializerStateBackup : IDisposable
    {
        readonly SerializerWriterThreadStaticState? _threadStaticState;
        readonly MemoryPackWriterOptionalState? _threadStaticWriterOptionalState;
        readonly bool _isValid;

        internal SerializerStateBackup(bool isValid)
        {
            _threadStaticState = threadStaticState;
            _threadStaticWriterOptionalState = threadStaticWriterOptionalState;
            _isValid = isValid;
        }

        public void Dispose()
        {
            if (!_isValid)
                return;

            threadStaticState = _threadStaticState;
            threadStaticWriterOptionalState = _threadStaticWriterOptionalState;
        }
    }

    public readonly struct DeserializerStateBackup : IDisposable
    {
        readonly MemoryPackReaderOptionalState? _threadStaticReaderOptionalState;
        readonly bool _isValid;

        internal DeserializerStateBackup(bool isValid)
        {
            _threadStaticReaderOptionalState = threadStaticReaderOptionalState;
            _isValid = isValid;
        }

        public void Dispose()
        {
            if (!_isValid)
                return;

            threadStaticReaderOptionalState = _threadStaticReaderOptionalState;
        }
    }
}
