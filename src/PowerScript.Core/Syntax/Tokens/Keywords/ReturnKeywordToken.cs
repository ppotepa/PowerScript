using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Scoping;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the RETURN keyword.
///     Used to return a value from a function or return void.
///     Examples:
///     - "RETURN 42" - returns a value
///     - "RETURN x + y" - returns an expression
///     - "RETURN" - void return (no value)
/// </summary>
public class ReturnKeywordToken : Token, IKeyWordToken
{
    public ReturnKeywordToken()
    {
    }

    public ReturnKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After RETURN, expect an identifier, value, or scope end (for void return)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken), typeof(ScopeEndToken)];

    public override string KeyWord => "RETURN";
}