using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the 'WITH' keyword in alternative function syntax.
///     Used in: FUNCTION name RETURNS TYPE WITH params { }
/// </summary>
public class WithKeywordToken : Token
{
    public WithKeywordToken()
    {
    }

    public WithKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override string KeyWord => "WITH";

    public override Type[] Expectations => [];
}