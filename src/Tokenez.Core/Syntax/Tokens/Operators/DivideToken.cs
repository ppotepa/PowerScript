using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Core.Syntax.Tokens.Operators;

/// <summary>
///     Token representing the '/' division operator.
///     Used in binary expressions: a / b
/// </summary>
public class DivideToken : Token
{
    public DivideToken()
    {
    }

    public DivideToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After '/', expect an identifier or value (right operand)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];
}