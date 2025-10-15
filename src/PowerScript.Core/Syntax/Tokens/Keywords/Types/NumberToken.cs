using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords.Types;

/// <summary>
///     Token representing the 'NUMBER' type keyword.
///     In PowerScript, NUMBER is treated as equivalent to INT.
///     Note: The alternative function syntax (FUNCTION name RETURNS NUMBER WITH params) has been removed.
/// </summary>
public class NumberToken : Token, ITypeToken
{
    public NumberToken()
    {
    }

    public NumberToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override string KeyWord => "NUMBER";

    public override Type[] Expectations => [];
}