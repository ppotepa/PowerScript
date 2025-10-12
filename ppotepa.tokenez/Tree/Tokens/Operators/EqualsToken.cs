using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Operators
{
    /// <summary>
    ///     Token representing the assignment operator (=).
    ///     Used in variable declarations and assignments.
    ///     Example: "VAR x = 10" or "x = 20"
    /// </summary>
    public class EqualsToken : Token
    {
        public EqualsToken()
        {
        }

        public EqualsToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After =, expect a value, identifier, or expression</summary>
        public override Type[] Expectations =>
        [
            typeof(ValueToken),
            typeof(IdentifierToken),
            typeof(StringLiteralToken)
        ];
    }
}