using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Base
{
    /// <summary>
    ///     Root token representing the start of the token stream.
    ///     This is the first token in the linked list.
    ///     Only expects FUNCTION tokens at the root level.
    /// </summary>
    internal class TokenRoot : Token
    {
        public TokenRoot()
        {
        }

        public TokenRoot(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>At root level, only FUNCTION declarations are allowed</summary>
        public override Type[] Expectations => [typeof(FunctionToken)];
    }
}