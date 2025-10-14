using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Keywords
{
    /// <summary>
    ///     Token representing the FUNCTION keyword.
    ///     Starts a function declaration.
    ///     Example: "FUNCTION add(a, b) { ... }"
    /// </summary>
    public class FunctionToken : Token, IKeyWordToken
    {
        public FunctionToken()
        {
        }

        public FunctionToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After FUNCTION, expect an identifier (function name)</summary>
        public override Type[] Expectations => [typeof(IdentifierToken)];

        public override string KeyWord => "FUNCTION";
    }
}