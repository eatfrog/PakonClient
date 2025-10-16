using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="INDEX_000"/> values used when addressing pictures.
    /// </summary>
    public readonly struct PictureIndex : IEquatable<PictureIndex>
    {
        private PictureIndex(INDEX_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this index.
        /// </summary>
        public INDEX_000 NativeValue { get; }

        public static PictureIndex All { get; } = new PictureIndex(INDEX_000.INDEX_All);

        public static PictureIndex AllSelected { get; } = new PictureIndex(INDEX_000.INDEX_AllSelected);

        public static PictureIndex Current { get; } = new PictureIndex(INDEX_000.INDEX_Current);

        public static PictureIndex First { get; } = new PictureIndex(INDEX_000.INDEX_First);

        public static PictureIndex InsertPictureAtEnd { get; } = new PictureIndex(INDEX_000.INDEX_InsertPictureAtEnd);

        public static PictureIndex FromNative(INDEX_000 value) => new PictureIndex(value);

        public static PictureIndex FromRawValue(int value) => new PictureIndex((INDEX_000)value);

        public bool Equals(PictureIndex other) => NativeValue == other.NativeValue;

        public override bool Equals(object obj) => obj is PictureIndex other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(PictureIndex left, PictureIndex right) => left.Equals(right);

        public static bool operator !=(PictureIndex left, PictureIndex right) => !left.Equals(right);

        public static implicit operator INDEX_000(PictureIndex index) => index.NativeValue;

        public static implicit operator PictureIndex(INDEX_000 index) => FromNative(index);
    }
}
