using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Values
{
    /// <summary>
    /// Token representing a string literal value.
    /// Examples: "Hello", "World", "PowerScript"
    /// </summary>
    public class StringLiteralToken : Token, IValue
    {
        public StringLiteralToken()
        {
        }

        public StringLiteralToken(RawToken rawToken) : base(rawToken)
        {
            // Remove quotes from the string value
            Value = rawToken?.Text?.Trim('"') ?? string.Empty;
        }

        /// <summary>After string literal, context-dependent (handled by processors)</summary>
        public override Type[] Expectations => [];

        /// <summary>The string value without quotes</summary>
        public string Value { get; }
    }
}
