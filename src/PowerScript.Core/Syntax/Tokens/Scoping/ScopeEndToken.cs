using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Scoping;

/// <summary>
///     Token representing '}' - the end of a scope/block.
///     Closes function bodies and other code blocks.
///     Triggers validation that function scopes have RETURN statements.
/// </summary>
public class ScopeEndToken : Token
{
    public ScopeEndToken()
    {
    }

    public ScopeEndToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After '}', no specific expectations (handled by parent scope)</summary>
    public override Type[] Expectations => [];

    public override string KeyWord => "}";
}