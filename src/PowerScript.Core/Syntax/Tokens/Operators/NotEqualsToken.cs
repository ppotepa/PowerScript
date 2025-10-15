using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Operators;

public class NotEqualsToken : Token
{
    public NotEqualsToken()
    {
    }

    public NotEqualsToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override Type[] Expectations => [];

    public override string ToString()
    {
        return "NotEqualsToken(!=)";
    }
}