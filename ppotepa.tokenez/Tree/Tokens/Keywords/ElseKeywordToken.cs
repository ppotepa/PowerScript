using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Scoping;

namespace ppotepa.tokenez.Tree.Tokens.Keywords;

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
