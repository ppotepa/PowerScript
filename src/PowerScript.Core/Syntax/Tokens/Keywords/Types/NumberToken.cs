using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords.Types;

/// <summary>
///     Token representing the 'NUMBER' type keyword.
///     In PowerScript, NUMBER can hold both integers and floating-point values (INT or DOUBLE).
///     This is the most flexible numeric type - it accepts whole numbers and decimals.
///     Supports optional bit-width specification: NUMBER[8], NUMBER[16], NUMBER[32], NUMBER[64]
///     Examples: NUMBER x = 42, NUMBER pi = 3.14159, NUMBER[8] small = 100
/// </summary>
public class NumberToken : Token, ITypeToken, IBaseTypeToken
{
    public NumberToken()
    {
    }

    public NumberToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override string KeyWord => "NUMBER";

    // After NUMBER, expect identifier (variable name) or [ (for bit width)
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(BracketOpen)];
}