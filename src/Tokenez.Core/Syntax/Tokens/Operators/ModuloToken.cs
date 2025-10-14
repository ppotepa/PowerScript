using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Delimiters;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Keywords;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Scoping;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Core.Syntax.Tokens.Operators
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
