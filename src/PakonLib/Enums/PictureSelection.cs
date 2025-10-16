using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="S_OR_H_000"/> values.
    /// </summary>
    public readonly struct PictureSelection : IEquatable<PictureSelection>
    {
        private PictureSelection(S_OR_H_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this selection state.
        /// </summary>
        public S_OR_H_000 NativeValue { get; }

        public static PictureSelection None { get; } = new PictureSelection(S_OR_H_000.S_OR_H_NONE);

        public static PictureSelection Selected { get; } = new PictureSelection(S_OR_H_000.S_OR_H_SELECTED);

        public static PictureSelection Hidden { get; } = new PictureSelection(S_OR_H_000.S_OR_H_HIDDEN);

        public static PictureSelection FromNative(S_OR_H_000 value) => new PictureSelection(value);

        public static PictureSelection FromRawValue(int value) => new PictureSelection((S_OR_H_000)value);

        public bool Equals(PictureSelection other) => NativeValue == other.NativeValue;

        public override bool Equals(object obj) => obj is PictureSelection other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(PictureSelection left, PictureSelection right) => left.Equals(right);

        public static bool operator !=(PictureSelection left, PictureSelection right) => !left.Equals(right);

        public static implicit operator S_OR_H_000(PictureSelection selection) => selection.NativeValue;

        public static implicit operator PictureSelection(S_OR_H_000 selection) => FromNative(selection);
    }
}
