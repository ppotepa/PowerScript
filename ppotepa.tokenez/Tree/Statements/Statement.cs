using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    /// Base class for all statements in the language.
    /// Statements are executable units of code (RETURN, assignments, function calls, etc.).
    /// </summary>
    public abstract class Statement
    {
        /// <summary>The token that starts this statement</summary>
        public Token StartToken { get; set; }

        /// <summary>String identifier for the statement type (e.g., "RETURN")</summary>
        public abstract string StatementType { get; }
    }
}
