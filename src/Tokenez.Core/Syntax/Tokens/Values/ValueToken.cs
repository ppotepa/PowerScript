using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Values;

/// <summary>
///     Token representing a literal value (number, string, etc.).
///     Examples: 42, "hello", 3.14
/// </summary>
public class ValueToken : Token
{
    public ValueToken()
    {
    }

    public ValueToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After value, context-dependent (handled by processors)</summary>
    public override Type[] Expectations => [];
}