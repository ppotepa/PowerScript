using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Delimiters;

/// <summary>
///     Token representing '![' - opening custom syntax block.
///     Used for custom syntax transformations: ![MAX OF numbers]
/// </summary>
public class CustomSyntaxBlockOpen : Token
{
    public CustomSyntaxBlockOpen()
    {
    }

    public CustomSyntaxBlockOpen(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>
    /// After '![', expect tokens that can start a custom syntax pattern
    /// </summary>
    public override Type[] Expectations =>
    [
        typeof(IdentifierToken),
        typeof(ValueToken)
    ];
}
