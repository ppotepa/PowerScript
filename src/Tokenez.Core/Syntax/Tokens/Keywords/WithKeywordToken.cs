using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the 'WITH' keyword.
///     Note: The alternative function syntax (FUNCTION name RETURNS TYPE WITH params) has been removed.
///     Standard syntax is: FUNCTION name(TYPE param)[RETURNTYPE]
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