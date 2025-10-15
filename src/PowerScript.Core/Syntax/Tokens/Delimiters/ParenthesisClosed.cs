using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Scoping;

namespace PowerScript.Core.Syntax.Tokens.Delimiters;

/// <summary>
///     Token representing ')' - closing parenthesis.
///     Closes function parameter lists.
///     Example: "FUNCTION add(a, b)" - the ')' after parameters
/// </summary>
public class ParenthesisClosed : Token
{
    public ParenthesisClosed()
    {
    }

    public ParenthesisClosed(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After ')', expect '{' to start function body or '[' for return type</summary>
    public override Type[] Expectations => [typeof(ScopeStartToken), typeof(BracketOpen)];
}