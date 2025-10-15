using System;
using TLXLib;

namespace PakonLib.Enums
{
    /// <summary>
    /// Provides a friendlier wrapper around the TLX <see cref="INITIALIZE_CONTROL_000"/> enumeration.
    /// </summary>
    public readonly struct InitializationRequest : IEquatable<InitializationRequest>
    {
        private InitializationRequest(INITIALIZE_CONTROL_000 value)
        {
            NativeValue = value;
        }

        /// <summary>
        /// Gets the underlying TLX value represented by this request.
        /// </summary>
        public INITIALIZE_CONTROL_000 NativeValue { get; }

        /// <summary>
        /// Requests the default initialization routine.
        /// </summary>
        public static InitializationRequest Default { get; } = new InitializationRequest(INITIALIZE_CONTROL_000.INITIALIZE_Default);

        /// <summary>
        /// Requests initialization for the managed C# client.
        /// </summary>
        public static InitializationRequest CSharpClient { get; } = new InitializationRequest(INITIALIZE_CONTROL_000.INITIALIZE_CSharpClient);

        /// <summary>
        /// Creates an <see cref="InitializationRequest"/> from a native TLX enumeration value.
        /// </summary>
        /// <param name="value">The native enumeration value.</param>
        /// <returns>An <see cref="InitializationRequest"/> representing <paramref name="value"/>.</returns>
        public static InitializationRequest FromNative(INITIALIZE_CONTROL_000 value) => new InitializationRequest(value);

        /// <summary>
        /// Creates an <see cref="InitializationRequest"/> from the raw integer value used by TLX.
        /// </summary>
        /// <param name="value">The raw integer value.</param>
        /// <returns>An <see cref="InitializationRequest"/> representing <paramref name="value"/>.</returns>
        public static InitializationRequest FromRawValue(int value) => new InitializationRequest((INITIALIZE_CONTROL_000)value);

        public bool Equals(InitializationRequest other) => NativeValue == other.NativeValue;

        public override bool Equals(object obj) => obj is InitializationRequest other && Equals(other);

        public override int GetHashCode() => NativeValue.GetHashCode();

        public override string ToString() => NativeValue.ToString();

        public static bool operator ==(InitializationRequest left, InitializationRequest right) => left.Equals(right);

        public static bool operator !=(InitializationRequest left, InitializationRequest right) => !(left == right);
    }
}
