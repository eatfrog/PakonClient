using System;
using TLXLib;

namespace PakonLib
{
    public readonly struct ErrorCode : IEquatable<ErrorCode>
    {
        private readonly ERROR_CODES_000 _value;

        private ErrorCode(ERROR_CODES_000 value)
        {
            _value = value;
        }

        public int RawValue => (int)_value;

        public string Name => _value.ToString();

        public string DisplayName => FormatName(Name);

        public bool IsDefined => Enum.IsDefined(typeof(ERROR_CODES_000), _value);

        public static ErrorCode FromInterop(ERROR_CODES_000 value) => new ErrorCode(value);

        public ERROR_CODES_000 ToInterop() => _value;

        public static ErrorCode FromValue(int value) => new ErrorCode((ERROR_CODES_000)value);

        public static bool TryFromValue(int value, out ErrorCode result)
        {
            if (Enum.IsDefined(typeof(ERROR_CODES_000), value))
            {
                result = new ErrorCode((ERROR_CODES_000)value);
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryParse(string name, out ErrorCode result)
        {
            if (Enum.TryParse(name, out ERROR_CODES_000 parsed))
            {
                result = new ErrorCode(parsed);
                return true;
            }

            result = default;
            return false;
        }

        public static ErrorCode Parse(string name)
        {
            if (!TryParse(name, out var result))
            {
                throw new ArgumentException("Unknown error code name.", nameof(name));
            }

            return result;
        }

        public override string ToString() => DisplayName;

        public bool Equals(ErrorCode other) => _value == other._value;

        public override bool Equals(object obj) => obj is ErrorCode other && Equals(other);

        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(ErrorCode left, ErrorCode right) => left.Equals(right);

        public static bool operator !=(ErrorCode left, ErrorCode right) => !left.Equals(right);

        private static string FormatName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            int prefixSeparator = name.IndexOf('_');
            if (prefixSeparator >= 0 && prefixSeparator + 1 < name.Length)
            {
                name = name.Substring(prefixSeparator + 1);
            }

            return name.Replace('_', ' ');
        }
    }
}
