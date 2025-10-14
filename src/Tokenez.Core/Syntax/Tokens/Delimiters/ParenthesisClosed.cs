using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Scoping;

namespace Tokenez.Core.Syntax.Tokens.Delimiters;

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