using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Operators;

public class EqualsEqualsToken : Token
{
    public EqualsEqualsToken()
    {
    }

    public EqualsEqualsToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override Type[] Expectations => [];

    public override string ToString()
    {
        return "EqualsEqualsToken(==)";
    }
}