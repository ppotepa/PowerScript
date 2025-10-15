using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Operators;

/// <summary>
///     Token representing the '+' addition operator.
///     Used in binary expressions: a + b
/// </summary>
public class PlusToken : Token
{
    public PlusToken()
    {
    }

    public PlusToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After '+', expect an identifier or value (right operand)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];
}