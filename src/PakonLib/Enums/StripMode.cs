using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="STRIP_MODE_000"/> values.
    /// </summary>
    public readonly struct StripMode : IEquatable<StripMode>
    {
        private StripMode(STRIP_MODE_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this strip mode.
        /// </summary>
        public STRIP_MODE_000 NativeValue { get; }

        public static StripMode FullRoll { get; } = new StripMode(STRIP_MODE_000.STRIP_MODE_FULL_ROLL);

        public static StripMode FromNative(STRIP_MODE_000 value) => new StripMode(value);

        public static StripMode FromRawValue(int value) => new StripMode((STRIP_MODE_000)value);

        public bool Equals(StripMode other) => NativeValue == other.NativeValue;

        public override bool Equals(object obj) => obj is StripMode other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(StripMode left, StripMode right) => left.Equals(right);

        public static bool operator !=(StripMode left, StripMode right) => !left.Equals(right);

        public static implicit operator STRIP_MODE_000(StripMode value) => value.NativeValue;

        public static implicit operator StripMode(STRIP_MODE_000 value) => FromNative(value);
    }
}
