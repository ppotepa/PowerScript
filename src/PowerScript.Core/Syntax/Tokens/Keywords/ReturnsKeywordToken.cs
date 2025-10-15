using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the 'RETURNS' keyword.
///     Note: The alternative function syntax (FUNCTION name RETURNS TYPE WITH params) has been removed.
///     Standard syntax is: FUNCTION name(TYPE param)[RETURNTYPE]
/// </summary>
public class ReturnsKeywordToken : Token
{
    public ReturnsKeywordToken()
    {
    }

    public ReturnsKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override string KeyWord => "RETURNS";

    public override Type[] Expectations => [];
}