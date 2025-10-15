using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Scoping;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

public class ElseKeywordToken : Token, IKeyWordToken
{
    public ElseKeywordToken()
    {
    }

    public ElseKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After ELSE, expect opening brace for else block</summary>
    public override Type[] Expectations => [typeof(ScopeStartToken)];

    public override string KeyWord => "ELSE";
}