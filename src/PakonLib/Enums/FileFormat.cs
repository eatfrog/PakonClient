using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="FILE_FORMAT_000"/> values.
    /// </summary>
    public readonly struct FileFormat : IEquatable<FileFormat>
    {
        private FileFormat(FILE_FORMAT_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this file format.
        /// </summary>
        public FILE_FORMAT_000 NativeValue { get; }

        public static FileFormat Jpeg { get; } = new FileFormat(FILE_FORMAT_000.iFILE_FORMAT_JPG);

        public static FileFormat Bitmap { get; } = new FileFormat(FILE_FORMAT_000.iFILE_FORMAT_BMP);

        public static FileFormat Tiff { get; } = new FileFormat(FILE_FORMAT_000.iFILE_FORMAT_TIF);

        public static FileFormat Exif { get; } = new FileFormat(FILE_FORMAT_000.iFILE_FORMAT_EXIF);

        public static FileFormat FromNative(FILE_FORMAT_000 value) => new FileFormat(value);

        public static FileFormat FromRawValue(int value) => new FileFormat((FILE_FORMAT_000)value);

        public bool Equals(FileFormat other) => NativeValue == other.NativeValue;

        public override bool Equals(object obj) => obj is FileFormat other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(FileFormat left, FileFormat right) => left.Equals(right);

        public static bool operator !=(FileFormat left, FileFormat right) => !left.Equals(right);

        public static implicit operator FILE_FORMAT_000(FileFormat format) => format.NativeValue;

        public static implicit operator FileFormat(FILE_FORMAT_000 format) => FromNative(format);
    }
}
