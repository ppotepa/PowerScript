using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Operators;

public class GreaterThanToken : Token
{
    public GreaterThanToken()
    {
    }

    public GreaterThanToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override Type[] Expectations => [];

    public override string ToString() => "GreaterThanToken(>)";
}
