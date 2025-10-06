using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Exceptions
{
    /// <summary>
    /// Thrown when a function scope doesn't contain a RETURN statement.
    /// All functions in this language must explicitly return a value.
    /// This is validated when the function scope ends (at '}').
    /// </summary>
    public class MissingReturnStatementException : Exception
    {
        /// <summary>The scope that is missing the RETURN statement</summary>
        public Scope Scope { get; }

        public MissingReturnStatementException(Scope scope)
            : base($"Function scope '{scope.ScopeName}' is missing a RETURN statement")
        {
            Scope = scope;
        }
    }
}
