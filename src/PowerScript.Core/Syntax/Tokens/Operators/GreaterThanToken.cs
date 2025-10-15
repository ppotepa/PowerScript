using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Operators;

public class GreaterThanToken : Token
{
    public GreaterThanToken()
    {
    }

    public GreaterThanToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override Type[] Expectations => [];

    public override string ToString()
    {
        return "GreaterThanToken(>)";
    }
}