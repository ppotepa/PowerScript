using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Core.Syntax.Tokens.Keywords;

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