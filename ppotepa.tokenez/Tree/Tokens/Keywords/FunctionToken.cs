using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
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