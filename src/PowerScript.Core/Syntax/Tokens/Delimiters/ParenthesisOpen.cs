using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Keywords.Types;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Delimiters;

/// <summary>
///     Token representing '(' - opening parenthesis.
///     Used for function parameter lists.
///     Example: "FUNCTION add(a, b)" - the '(' before parameters
/// </summary>
public class ParenthesisOpen : Token
{
    public ParenthesisOpen()
    {
    }

    public ParenthesisOpen(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After '(', expect type token (for parameters) or ')' (empty list)</summary>
    public override Type[] Expectations => [typeof(ITypeToken), typeof(ParenthesisClosed)];
}