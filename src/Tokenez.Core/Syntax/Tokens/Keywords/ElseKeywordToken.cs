using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Scoping;

namespace Tokenez.Core.Syntax.Tokens.Keywords
{
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
}