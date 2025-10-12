using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    ///     Represents a function declaration.
    ///     Example: "FUNCTION add(a, b) { ... }" or "FUNCTION multiply(INT A, INT B)[INT] { ... }"
    /// </summary>
    public class FunctionDeclaration : Declaration
    {
        public FunctionDeclaration(Token identifier) : base(identifier)
        {
        }

        /// <summary>List of parameter declarations for this function</summary>
        public List<Declaration> Parameters { get; set; } = [];

        /// <summary>The scope (body) of this function</summary>
        public Scope Scope { get; set; } = default!;

        /// <summary>The return type token (e.g., INT), or null for void functions</summary>
        public Token? ReturnType { get; set; }
    }
}