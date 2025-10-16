using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="RESOLUTION_000"/> values.
    /// </summary>
    public readonly struct Resolution : IEquatable<Resolution>
    {
        private Resolution(RESOLUTION_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this resolution.
        /// </summary>
        public RESOLUTION_000 NativeValue { get; }

        public static Resolution Base4 { get; } = new Resolution(RESOLUTION_000.RESOLUTION_BASE_4);

        public static Resolution Base8 { get; } = new Resolution(RESOLUTION_000.RESOLUTION_BASE_8);

        public static Resolution Base16 { get; } = new Resolution(RESOLUTION_000.RESOLUTION_BASE_16);

        public static Resolution FromNative(RESOLUTION_000 value) => new Resolution(value);

        public static Resolution FromRawValue(int value) => new Resolution((RESOLUTION_000)value);

        public bool Equals(Resolution other) => NativeValue == other.NativeValue;

        public override bool Equals(object obj) => obj is Resolution other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(Resolution left, Resolution right) => left.Equals(right);

        public static bool operator !=(Resolution left, Resolution right) => !left.Equals(right);

        public static implicit operator RESOLUTION_000(Resolution value) => value.NativeValue;

        public static implicit operator Resolution(RESOLUTION_000 value) => FromNative(value);
    }
}
