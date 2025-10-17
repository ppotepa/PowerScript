using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Delimiters;

/// <summary>
/// Represents the closing curly brace '}' used for object literals.
/// Example: {name = "John", age = 30}
/// </summary>
public class CurlyBraceCloseToken : Token
{
    public CurlyBraceCloseToken()
    {
    }

    public CurlyBraceCloseToken(RawToken raw) : base(raw)
    {
    }

    /// <summary>After '}', could be 'as' for type annotation or other tokens</summary>
    public override Type[] Expectations => [typeof(AsKeywordToken), typeof(CommaToken)];
}
