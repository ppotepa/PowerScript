using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Identifiers;

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