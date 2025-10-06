using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Exceptions
{
    /// <summary>
    /// Thrown when a RETURN statement appears outside a function scope.
    /// RETURN can only be used within function bodies, not in root/global scope.
    /// </summary>
    public class InvalidReturnStatementException : Exception
    {
        /// <summary>The RETURN token that was used invalidly</summary>
        public Token Token { get; }

        public InvalidReturnStatementException(Token token)
            : base("RETURN statement can only appear inside a function")
        {
            Token = token;
        }
    }
}
