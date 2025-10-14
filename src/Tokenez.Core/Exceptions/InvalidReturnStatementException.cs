using Tokenez.Core.Syntax.Tokens.Base;

namespace Tokenez.Core.Exceptions
{
    /// <summary>
    ///     Thrown when a RETURN statement appears outside a function scope.
    ///     RETURN can only be used within function bodies, not in root/global scope.
    /// </summary>
    public class InvalidReturnStatementException(Token token)
        : Exception("RETURN statement can only appear inside a function")
    {
        /// <summary>The RETURN token that was used invalidly</summary>
        public Token Token { get; } = token;
    }
}