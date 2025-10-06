using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Keywords.Types;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Delimiters
{
    /// <summary>
    /// Token representing '(' - opening parenthesis.
    /// Used for function parameter lists.
    /// Example: "FUNCTION add(a, b)" - the '(' before parameters
    /// </summary>
    public class ParenthesisOpen : Token
    {
        public ParenthesisOpen()
        {
        }

        public ParenthesisOpen(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After '(', expect type token (for parameters) or ')' (empty list)</summary>
        public override Type[] Expectations => [typeof(ITypeToken), typeof(ParenthesisClosed)];
    }
}