using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendly wrapper around TLX <see cref="FILM_COLOR_000"/> values.
    /// </summary>
    public readonly struct FilmColor : IEquatable<FilmColor>
    {
        private FilmColor(FILM_COLOR_000 nativeValue)
        {
            NativeValue = nativeValue;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this film color selection.
        /// </summary>
        public FILM_COLOR_000 NativeValue { get; }

        public static FilmColor Negative { get; } = new FilmColor(FILM_COLOR_000.FILM_COLOR_NEGATIVE);

        public static FilmColor Positive { get; } = new FilmColor(FILM_COLOR_000.FILM_COLOR_POSITIVE);

        public static FilmColor BlackAndWhite { get; } = FromName("FILM_COLOR_BnW_NORMAL");

        public static FilmColor BlackAndWhiteC41 { get; } = FromName("FILM_COLOR_BnW_C41");

        private static FilmColor FromName(string name) => new FilmColor((FILM_COLOR_000)Enum.Parse(typeof(FILM_COLOR_000), name));

        public static FilmColor FromNative(FILM_COLOR_000 value) => new FilmColor(value);

        public static FilmColor FromRawValue(int value) => new FilmColor((FILM_COLOR_000)value);

        public bool Equals(FilmColor other) => NativeValue == other.NativeValue;

        public override bool Equals(object obj) => obj is FilmColor other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(FilmColor left, FilmColor right) => left.Equals(right);

        public static bool operator !=(FilmColor left, FilmColor right) => !left.Equals(right);

        public static implicit operator FILM_COLOR_000(FilmColor value) => value.NativeValue;

        public static implicit operator FilmColor(FILM_COLOR_000 value) => FromNative(value);
    }
}
