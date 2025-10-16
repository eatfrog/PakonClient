using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="SCANNER_TYPE_000"/> values.
    /// </summary>
    public readonly struct ScannerType : IEquatable<ScannerType>
    {
        private ScannerType(SCANNER_TYPE_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this scanner type.
        /// </summary>
        public SCANNER_TYPE_000 NativeValue { get; }

        public static ScannerType Unknown { get; } = new ScannerType(SCANNER_TYPE_000.SCANNER_TYPE_UNKNOWN);

        public static ScannerType F135 { get; } = new ScannerType(SCANNER_TYPE_000.SCANNER_TYPE_F_135);

        public static ScannerType F135Plus { get; } = new ScannerType(SCANNER_TYPE_000.SCANNER_TYPE_F_135_PLUS);

        public static ScannerType F235 { get; } = new ScannerType(SCANNER_TYPE_000.SCANNER_TYPE_F_235);

        public static ScannerType F235C { get; } = new ScannerType(SCANNER_TYPE_000.SCANNER_TYPE_F_235C);

        public static ScannerType F335 { get; } = new ScannerType(SCANNER_TYPE_000.SCANNER_TYPE_F_335);

        public static ScannerType F335C { get; } = new ScannerType(SCANNER_TYPE_000.SCANNER_TYPE_F_335C);

        public static ScannerType FromNative(SCANNER_TYPE_000 value) => new ScannerType(value);

        public static ScannerType FromRawValue(int value) => new ScannerType((SCANNER_TYPE_000)value);

        public bool Equals(ScannerType other) => NativeValue == other.NativeValue;

        public override bool Equals(object obj) => obj is ScannerType other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(ScannerType left, ScannerType right) => left.Equals(right);

        public static bool operator !=(ScannerType left, ScannerType right) => !left.Equals(right);

        public static implicit operator SCANNER_TYPE_000(ScannerType type) => type.NativeValue;

        public static implicit operator ScannerType(SCANNER_TYPE_000 type) => FromNative(type);
    }
}
