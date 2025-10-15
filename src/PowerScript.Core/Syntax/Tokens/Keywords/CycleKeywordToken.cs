using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the CYCLE keyword for loops.
///     CYCLE is PowerScript's foreach equivalent with automatic index variables.
///     Examples:
///     - CYCLE IN collection { ... }  // Uses 'a' as automatic index
///     - CYCLE IN collection AS item { ... }  // Renamed to 'item'
///     - Nested: outer uses 'a', inner uses 'b', etc.
/// </summary>
public class CycleKeywordToken : Token, IKeyWordToken
{
    public CycleKeywordToken()
    {
    }

    public CycleKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After CYCLE, expect IN keyword for collection iteration</summary>
    public override Type[] Expectations => [typeof(InKeywordToken)];

    public override string KeyWord => "CYCLE";
}