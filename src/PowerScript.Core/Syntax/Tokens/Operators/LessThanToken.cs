using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Operators;

public class LessThanToken : Token
{
    public LessThanToken()
    {
    }

    public LessThanToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override Type[] Expectations => [];

    public override string ToString()
    {
        return "LessThanToken(<)";
    }
}