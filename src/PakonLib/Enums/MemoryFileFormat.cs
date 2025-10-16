using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="FILE_FORMAT_SAVE_TO_MEMORY_000"/> values.
    /// </summary>
    public readonly struct MemoryFileFormat : IEquatable<MemoryFileFormat>
    {
        private MemoryFileFormat(FILE_FORMAT_SAVE_TO_MEMORY_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this format.
        /// </summary>
        public FILE_FORMAT_SAVE_TO_MEMORY_000 NativeValue { get; }

        public static MemoryFileFormat Dib8 { get; } = new MemoryFileFormat(FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_DIB_8);

        public static MemoryFileFormat Planar16 { get; } = new MemoryFileFormat(FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_16);

        public static MemoryFileFormat Planar8 { get; } = new MemoryFileFormat(FILE_FORMAT_SAVE_TO_MEMORY_000.iFILE_FORMAT_SAVE_TO_MEMORY_PLANAR_8);

        public static MemoryFileFormat FromNative(FILE_FORMAT_SAVE_TO_MEMORY_000 value) => new MemoryFileFormat(value);

        public static MemoryFileFormat FromRawValue(int value) => new MemoryFileFormat((FILE_FORMAT_SAVE_TO_MEMORY_000)value);

        public bool Equals(MemoryFileFormat other) => NativeValue == other.NativeValue;

        public override bool Equals(object obj) => obj is MemoryFileFormat other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(MemoryFileFormat left, MemoryFileFormat right) => left.Equals(right);

        public static bool operator !=(MemoryFileFormat left, MemoryFileFormat right) => !left.Equals(right);

        public static implicit operator FILE_FORMAT_SAVE_TO_MEMORY_000(MemoryFileFormat format) => format.NativeValue;

        public static implicit operator MemoryFileFormat(FILE_FORMAT_SAVE_TO_MEMORY_000 format) => FromNative(format);
    }
}
