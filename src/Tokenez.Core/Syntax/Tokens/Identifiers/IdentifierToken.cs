using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Identifiers;

/// <summary>
///     Token representing an identifier (variable name, parameter name, function name).
///     Examples: "add", "x", "count", "myVariable"
/// </summary>
public class IdentifierToken : Token, IValue
{
    public IdentifierToken()
    {
        Value = string.Empty;
    }

    public IdentifierToken(RawToken rawToken) : base(rawToken)
    {
        Value = rawToken.Text;
    }

    /// <summary>After identifier, context-dependent (handled by processors)</summary>
    public override Type[] Expectations => [];

    /// <summary>The identifier name</summary>
    public string Value { get; }
}