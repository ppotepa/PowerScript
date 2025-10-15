using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Scoping;

namespace PowerScript.Core.Syntax.Tokens.Delimiters;

/// <summary>
///     Token representing ']' - closing square bracket.
///     Used to close function return type declarations.
///     Example: "FUNCTION add(a, b)[INT]" - the ']' after return type
/// </summary>
public class BracketClosed : Token
{
    public BracketClosed()
    {
    }

    public BracketClosed(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After ']', expect scope start token '{'</summary>
    public override Type[] Expectations => [typeof(ScopeStartToken)];
}