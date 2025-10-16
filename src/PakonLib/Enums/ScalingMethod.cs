using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="SCALING_METHOD_000"/> values.
    /// </summary>
    public readonly struct ScalingMethod : IEquatable<ScalingMethod>
    {
        private ScalingMethod(SCALING_METHOD_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this scaling method.
        /// </summary>
        public SCALING_METHOD_000 NativeValue { get; }

        /// <summary>
        /// Returns the TLX bicubic scaling method.
        /// </summary>
        public static ScalingMethod Bicubic { get; } = new ScalingMethod(SCALING_METHOD_000.SCALING_METHOD_BICUBIC);

        /// <summary>
        /// Creates a <see cref="ScalingMethod"/> from an existing TLX value.
        /// </summary>
        public static ScalingMethod FromNative(SCALING_METHOD_000 value) => new ScalingMethod(value);

        /// <summary>
        /// Creates a <see cref="ScalingMethod"/> from the raw TLX integer value.
        /// </summary>
        public static ScalingMethod FromRawValue(int value) => new ScalingMethod((SCALING_METHOD_000)value);

        public bool Equals(ScalingMethod other) => NativeValue == other.NativeValue;

        public override bool Equals(object obj) => obj is ScalingMethod other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(ScalingMethod left, ScalingMethod right) => left.Equals(right);

        public static bool operator !=(ScalingMethod left, ScalingMethod right) => !left.Equals(right);

        /// <summary>
        /// Performs an implicit conversion to the underlying TLX enumeration.
        /// </summary>
        public static implicit operator SCALING_METHOD_000(ScalingMethod method) => method.NativeValue;

        /// <summary>
        /// Performs an implicit conversion from the underlying TLX enumeration.
        /// </summary>
        public static implicit operator ScalingMethod(SCALING_METHOD_000 method) => FromNative(method);
    }
}
