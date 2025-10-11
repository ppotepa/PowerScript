using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Keywords.Types;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Delimiters
{
    /// <summary>
    /// Token representing '[' - opening square bracket.
    /// Used for function return type declarations.
    /// Example: "FUNCTION add(a, b)[INT]" - the '[' before return type
    /// </summary>
    public class BracketOpen : Token
    {
        public BracketOpen()
        {
        }

        public BracketOpen(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After '[', expect type token for return type</summary>
        public override Type[] Expectations => [typeof(ITypeToken)];
    }
}