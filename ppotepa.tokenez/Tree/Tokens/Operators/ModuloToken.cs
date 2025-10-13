using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Scoping;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Operators
{
    /// <summary>
    ///     Token representing the modulo operator (%).
    ///     Example: a % b
    /// </summary>
    public class ModuloToken : Token
    {
        public ModuloToken()
        {
        }

        public ModuloToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After modulo, expect: identifier, number, or expression</summary>
        public override Type[] Expectations =>
        [
            typeof(IdentifierToken),
            typeof(ValueToken),
            typeof(ParenthesisOpen)
        ];
    }
}
