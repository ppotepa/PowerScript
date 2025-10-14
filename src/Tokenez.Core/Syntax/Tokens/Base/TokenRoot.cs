using Tokenez.Core.Syntax.Tokens.Keywords;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Base
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