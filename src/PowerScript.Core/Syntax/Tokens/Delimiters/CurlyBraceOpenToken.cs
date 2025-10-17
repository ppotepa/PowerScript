using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Delimiters;

/// <summary>
/// Represents the opening curly brace '{' used for object literals.
/// Example: {name = "John", age = 30}
/// </summary>
public class CurlyBraceOpenToken : Token
{
    public CurlyBraceOpenToken()
    {
    }

    public CurlyBraceOpenToken(RawToken raw) : base(raw)
    {
    }

    /// <summary>After '{', expect property name or closing brace for empty object</summary>
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(CurlyBraceCloseToken)];
}
