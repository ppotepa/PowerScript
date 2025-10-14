using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the IN keyword for collection iteration.
///     Used in CYCLE loops: CYCLE IN collection { ... }
/// </summary>
public class InKeywordToken : Token, IKeyWordToken
{
    public InKeywordToken()
    {
    }

    public InKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After IN, expect collection identifier or expression</summary>
    public override Type[] Expectations => [typeof(IdentifierToken)];

    public override string KeyWord => "IN";
}