using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
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
}