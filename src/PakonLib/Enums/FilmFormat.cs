using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="FILM_FORMAT_000"/> values.
    /// </summary>
    public readonly struct FilmFormat : IEquatable<FilmFormat>
    {
        private FilmFormat(FILM_FORMAT_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this film format.
        /// </summary>
        public FILM_FORMAT_000 NativeValue { get; }

        public static FilmFormat Format24mm { get; } = new FilmFormat(FILM_FORMAT_000.FILM_FORMAT_24MM);

        public static FilmFormat Format35mm { get; } = new FilmFormat(FILM_FORMAT_000.FILM_FORMAT_35MM);

        public static FilmFormat Format24mmCartridge { get; } = FromName("FILM_FORMAT_24MM_CART");

        public static FilmFormat Format24mmCartridgeMofReader { get; } = FromName("FILM_FORMAT_24MM_CART_MOF_READER");

        public static FilmFormat Format24mmCartridgeMofFile { get; } = FromName("FILM_FORMAT_24MM_CART_MOF_FILE");

        public static FilmFormat Format24mmCartridgeMofFileOrReader { get; } = FromName("FILM_FORMAT_24MM_CART_MOF_FILE_OR_READER");

        public static FilmFormat Format24mmFile { get; } = FromName("FILM_FORMAT_24MM_FILE");

        private static FilmFormat FromName(string name) => new FilmFormat((FILM_FORMAT_000)Enum.Parse(typeof(FILM_FORMAT_000), name));

        public static FilmFormat FromNative(FILM_FORMAT_000 value) => new FilmFormat(value);

        public static FilmFormat FromRawValue(int value) => new FilmFormat((FILM_FORMAT_000)value);

        public bool Equals(FilmFormat other) => NativeValue == other.NativeValue;

        public override bool Equals(object obj) => obj is FilmFormat other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(FilmFormat left, FilmFormat right) => left.Equals(right);

        public static bool operator !=(FilmFormat left, FilmFormat right) => !left.Equals(right);

        public static implicit operator FILM_FORMAT_000(FilmFormat value) => value.NativeValue;

        public static implicit operator FilmFormat(FILM_FORMAT_000 value) => FromNative(value);
    }
}
