using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Identifiers
{
    /// <summary>
    ///     Token representing an identifier (variable name, parameter name, function name).
    ///     Examples: "add", "x", "count", "myVariable"
    /// </summary>
    public class IdentifierToken : Token, IValue
    {
        public IdentifierToken()
        {
            Value = string.Empty;
        }

        public IdentifierToken(RawToken rawToken) : base(rawToken)
        {
            Value = rawToken.Text;
        }

        /// <summary>After identifier, context-dependent (handled by processors)</summary>
        public override Type[] Expectations => [];

        /// <summary>The identifier name</summary>
        public string Value { get; }
    }
}