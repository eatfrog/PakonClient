using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="SAVE_CONTROL_000"/> flag values.
    /// </summary>
    public readonly struct SaveControl : IEquatable<SaveControl>
    {
        private SaveControl(SAVE_CONTROL_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this flag combination.
        /// </summary>
        public SAVE_CONTROL_000 NativeValue { get; }

        public static SaveControl None { get; } = new SaveControl(0);

        public static SaveControl SizeOriginal { get; } = new SaveControl(SAVE_CONTROL_000.SAV_SizeOriginal);

        public static SaveControl SizeLimitForDisplay { get; } = FromName("SAV_SizeLimitForDisplay");

        public static SaveControl SizeLimitForSave { get; } = FromName("SAV_SizeLimitForSave");

        public static SaveControl UseCurrentRotation { get; } = FromName("SAV_UseCurrentRotation");

        public static SaveControl UseLoResBuffer { get; } = new SaveControl(SAVE_CONTROL_000.SAV_UseLoResBuffer);

        public static SaveControl UseScratchRemovalIfAvailable { get; } = FromName("SAV_UseScratchRemovalIfAvailable");

        public static SaveControl UseColorCorrection { get; } = FromName("SAV_UseColorCorrection");

        public static SaveControl UseColorSceneBalance { get; } = FromName("SAV_UseColorSceneBalance");

        public static SaveControl UseColorAdjustments { get; } = FromName("SAV_UseColorAdjustments");

        public static SaveControl FileHeader { get; } = FromName("SAV_FileHeader");

        public static SaveControl FastUpdate8BitDib { get; } = FromName("SAV_FastUpdate8BitDib");

        public static SaveControl TopDownDib { get; } = FromName("SAV_TopDownDib");

        public static SaveControl DoNotScaleUp { get; } = FromName("SAV_DoNotScaleUp");

        public static SaveControl UseColorKcdfs { get; } = FromName("SAV_UseColorKcdfs");

        private static SaveControl FromName(string name) => new SaveControl((SAVE_CONTROL_000)Enum.Parse(typeof(SAVE_CONTROL_000), name));

        public static SaveControl FromNative(SAVE_CONTROL_000 value) => new SaveControl(value);

        public static SaveControl FromRawValue(int value) => new SaveControl((SAVE_CONTROL_000)value);

        public bool Equals(SaveControl other) => NativeValue.Equals(other.NativeValue);

        public override bool Equals(object obj) => obj is SaveControl other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(SaveControl left, SaveControl right) => left.Equals(right);

        public static bool operator !=(SaveControl left, SaveControl right) => !left.Equals(right);

        public static SaveControl operator |(SaveControl left, SaveControl right) => new SaveControl(left.NativeValue | right.NativeValue);

        public static SaveControl operator &(SaveControl left, SaveControl right) => new SaveControl(left.NativeValue & right.NativeValue);

        public static SaveControl operator ~(SaveControl value) => new SaveControl(~value.NativeValue);

        public bool HasFlag(SaveControl flag) => (NativeValue & flag.NativeValue) == flag.NativeValue;

        public static implicit operator SAVE_CONTROL_000(SaveControl value) => value.NativeValue;

        public static implicit operator SaveControl(SAVE_CONTROL_000 value) => FromNative(value);
    }
}
