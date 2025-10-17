using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Scoping;

/// <summary>
///     Token representing '{' - the start of a scope/block.
///     Used to begin function bodies and other code blocks.
/// </summary>
public class ScopeStartToken : Token
{
    public ScopeStartToken()
    {
    }

    public ScopeStartToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After '{', expect RETURN, FLEX, or nested FUNCTION declarations (PRINT is now a library function)</summary>
    public override Type[] Expectations =>
    [
        typeof(ReturnKeywordToken),
        typeof(FlexKeywordToken),
        typeof(FunctionToken),
        typeof(IfKeywordToken),
        typeof(CycleKeywordToken)
    ];

    public override string KeyWord => "{";
}