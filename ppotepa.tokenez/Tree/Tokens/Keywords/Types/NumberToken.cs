using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords.Types
{
    /// <summary>
    ///     Token representing the 'NUMBER' type keyword.
    ///     In PowerScript, NUMBER is treated as equivalent to INT.
    ///     Used in alternative function syntax: FUNCTION name RETURNS NUMBER WITH params
    /// </summary>
    public class NumberToken : Token, ITypeToken
    {
        public NumberToken()
        {
        }

        public NumberToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override string KeyWord => "NUMBER";

        public override Type[] Expectations => [];
    }
}
