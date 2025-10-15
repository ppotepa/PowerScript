using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords.Types;

public class FunctionTypeToken : Token
{
    public FunctionTypeToken()
    {
    }

    public FunctionTypeToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override Type[] Expectations => [typeof(IdentifierToken)];
}