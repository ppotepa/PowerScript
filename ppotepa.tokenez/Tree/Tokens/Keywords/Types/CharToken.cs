using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords.Types
{
    /// <summary>
    /// Token representing 'CHAR' - the character type.
    /// Used for single character values.
    /// Example: "VAR CHAR letter = 'A'" or "CHAR CHAIN text" (which equals STRING)
    /// </summary>
    public class CharToken : Token, IKeyWordToken, IBaseTypeToken
    {
        public CharToken()
        {
        }

        public CharToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ChainToken)];
    }
}