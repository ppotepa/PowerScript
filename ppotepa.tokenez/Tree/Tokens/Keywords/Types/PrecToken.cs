using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords.Types
{
    /// <summary>
    ///     Token representing 'PREC' - the precision/float type.
    ///     Used for floating-point numbers and decimal calculations.
    ///     Example: "VAR PREC price = 19.99" or "FUNCTION calculate()[PREC]"
    /// </summary>
    public class PrecToken : Token, IKeyWordToken, IBaseTypeToken
    {
        public PrecToken()
        {
        }

        public PrecToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ChainToken)];
    }
}