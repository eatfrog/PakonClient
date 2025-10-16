using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="SCAN_CONTROL_000"/> flag values.
    /// </summary>
    public readonly struct ScanControl : IEquatable<ScanControl>
    {
        private ScanControl(SCAN_CONTROL_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this scan control flag combination.
        /// </summary>
        public SCAN_CONTROL_000 NativeValue { get; }

        public static ScanControl None { get; } = new ScanControl(SCAN_CONTROL_000.SCAN_None);

        public static ScanControl AggressiveFraming { get; } = FromName("SCAN_AggressiveFraming");

        public static ScanControl UseScratchRemoval { get; } = FromName("SCAN_UseScratchRemoval");

        public static ScanControl HasFilmDrag { get; } = FromName("SCAN_HasFilmDrag");

        public static ScanControl ReadDx { get; } = FromName("SCAN_Read_DX");

        public static ScanControl RftSenseSplice { get; } = FromName("SCAN_RFT_SenseSplice");

        public static ScanControl Use24mmExternalFileMof { get; } = FromName("SCAN_Use24mmExternalFileMOF");

        public static ScanControl Use24mmAutoLoader { get; } = FromName("SCAN_Use24mmAutoLoader");

        public static ScanControl Use24mmAutoLoaderMof { get; } = FromName("SCAN_Use24mmAutoLoaderMOF");

        public static ScanControl LampWarmUp { get; } = FromName("SCAN_LampWarmUp");

        public static ScanControl PreScan { get; } = FromName("SCAN_PreScan");

        private static ScanControl FromName(string name) => new ScanControl((SCAN_CONTROL_000)Enum.Parse(typeof(SCAN_CONTROL_000), name));

        public static ScanControl FromNative(SCAN_CONTROL_000 value) => new ScanControl(value);

        public static ScanControl FromRawValue(int value) => new ScanControl((SCAN_CONTROL_000)value);

        public bool Equals(ScanControl other) => NativeValue.Equals(other.NativeValue);

        public override bool Equals(object obj) => obj is ScanControl other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(ScanControl left, ScanControl right) => left.Equals(right);

        public static bool operator !=(ScanControl left, ScanControl right) => !left.Equals(right);

        public static ScanControl operator |(ScanControl left, ScanControl right) => new ScanControl(left.NativeValue | right.NativeValue);

        public static ScanControl operator &(ScanControl left, ScanControl right) => new ScanControl(left.NativeValue & right.NativeValue);

        public static ScanControl operator ~(ScanControl value) => new ScanControl(~value.NativeValue);

        public bool HasFlag(ScanControl flag) => (NativeValue & flag.NativeValue) == flag.NativeValue;

        public static implicit operator SCAN_CONTROL_000(ScanControl value) => value.NativeValue;

        public static implicit operator ScanControl(SCAN_CONTROL_000 value) => FromNative(value);
    }
}
