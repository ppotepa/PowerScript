using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Values;

/// <summary>
///     Token representing a string literal value.
///     Examples: "Hello", "World", "PowerScript"
/// </summary>
public class StringLiteralToken : Token, IValue
{
    public StringLiteralToken()
    {
        Value = string.Empty;
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