using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the AS keyword for renaming loop variables.
///     Used to rename automatic index variables in CYCLE loops.
///     Example: CYCLE IN collection AS item { ... }
/// </summary>
public class AsKeywordToken : Token, IKeyWordToken
{
    public AsKeywordToken()
    {
    }

    public AsKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After AS, expect the new variable name (identifier)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken)];

    public override string KeyWord => "AS";
}