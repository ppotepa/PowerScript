using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Operators
{
    /// <summary>
    /// Token representing the '/' division operator.
    /// Used in binary expressions: a / b
    /// </summary>
    public class DivideToken : Token
    {
        public DivideToken()
        {
        }

        public DivideToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After '/', expect an identifier or value (right operand)</summary>
        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];
    }
}
