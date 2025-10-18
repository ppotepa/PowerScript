using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Operators;

namespace PowerScript.Core.Syntax.Tokens.Delimiters;

/// <summary>
///     Token representing ']' - closing custom syntax block.
///     Closes custom syntax transformations started with ![
/// </summary>
public class CustomSyntaxBlockClose : Token
{
    public CustomSyntaxBlockClose()
    {
    }

    public CustomSyntaxBlockClose(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>
    /// After ']' closing custom syntax block, expect operators or delimiters
    /// </summary>
    public override Type[] Expectations =>
    [
        typeof(EqualsToken),
        typeof(PlusToken),
        typeof(CommaToken)
    ];
}
