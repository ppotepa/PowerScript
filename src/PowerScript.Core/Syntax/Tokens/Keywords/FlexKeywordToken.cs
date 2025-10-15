using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the FLEX keyword for dynamic variable declaration.
///     FLEX variables can change their type at runtime.
///     Example: FLEX counter = 0  // Later can be: counter = "many"
/// </summary>
public class FlexKeywordToken : Token, IKeyWordToken
{
    public FlexKeywordToken()
    {
    }

    public FlexKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After FLEX, expect a variable name (identifier)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken)];

    public override string KeyWord => "FLEX";
}