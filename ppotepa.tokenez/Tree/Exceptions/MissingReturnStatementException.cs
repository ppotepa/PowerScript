using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Exceptions
{
    /// <summary>
    /// Thrown when a function scope doesn't contain a RETURN statement
    /// </summary>
    public class MissingReturnStatementException : Exception
    {
        public Scope Scope { get; }

        public MissingReturnStatementException(Scope scope)
            : base($"Function scope '{scope.ScopeName}' is missing a RETURN statement")
        {
            Scope = scope;
        }
    }
}
