using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Operators;

public class LessThanToken : Token
{
    public LessThanToken()
    {
    }

    public LessThanToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override Type[] Expectations => [];

    public override string ToString() => "LessThanToken(<)";
}
