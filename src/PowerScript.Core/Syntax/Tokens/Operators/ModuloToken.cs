using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Operators;

/// <summary>
///     Token representing the modulo operator (%).
///     Example: a % b
/// </summary>
public class ModuloToken : Token
{
    public ModuloToken()
    {
    }

    public ModuloToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After modulo, expect: identifier, number, or expression</summary>
    public override Type[] Expectations =>
    [
        typeof(IdentifierToken),
        typeof(ValueToken),
        typeof(ParenthesisOpen)
    ];
}