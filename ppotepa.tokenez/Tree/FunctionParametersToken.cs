using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    ///     Token representing a function's parameter list.
    ///     Contains the parsed parameter declarations.
    ///     Example: In "FUNCTION add(NUMBER a, NUMBER b)", this holds declarations for 'a' and 'b'.
    /// </summary>
    internal class FunctionParametersToken : Token
    {
        public FunctionParametersToken()
        {
        }

        public FunctionParametersToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>No tokens expected after parameters (handled by parent processor)</summary>
        public override Type[] Expectations => [];

        /// <summary>List of parameter declarations in order</summary>
        public List<Declaration> Declarations { get; set; } = [];
    }
}