using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Operators;

public class CommaSeparatorToken : Token
{
    public CommaSeparatorToken()
    {
    }

    public CommaSeparatorToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];
}