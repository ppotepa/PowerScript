using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Core.Syntax.Tokens.Keywords
{
    /// <summary>
    ///     Token representing the AND logical operator (SQL-style).
    ///     Used in conditional expressions: IF a > b AND c < d { ... }
    /// </summary>
    public class AndKeywordToken : Token, IKeyWordToken
    {
        public AndKeywordToken()
        {
        }

        public AndKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After AND, expect an identifier or value</summary>
        public override Type[] Expectations =>
        [
            typeof(IdentifierToken),
            typeof(ValueToken),
            typeof(StringLiteralToken)
        ];

        public override string KeyWord => "AND";
    }
}