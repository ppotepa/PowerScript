using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the IF keyword for conditional statements.
///     SQL-style syntax: IF condition { ... } or IF condition { ... } ELSE { ... }
/// </summary>
public class IfKeywordToken : Token, IKeyWordToken
{
    public IfKeywordToken()
    {
    }

    public IfKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After IF, expect an identifier or value to start condition</summary>
    public override Type[] Expectations =>
    [
        typeof(IdentifierToken),
        typeof(ValueToken),
        typeof(StringLiteralToken)
    ];

    public override string KeyWord => "IF";
}