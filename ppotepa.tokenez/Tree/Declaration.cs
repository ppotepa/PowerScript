#nullable enable
using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    /// Base class for all declarations (functions, parameters, variables).
    /// Declarations define new named entities that can be referenced later.
    /// </summary>
    public abstract class Declaration
    {
        protected Declaration(Token identifier)
        {
            this.Identifier = identifier;
        }

        /// <summary>The identifier token containing the name of this declaration</summary>
        public Token Identifier { get; set; }
    }

    /// <summary>
    /// Represents a function declaration.
    /// Example: "FUNCTION add(a, b) { ... }" or "FUNCTION multiply(INT A, INT B)[INT] { ... }"
    /// </summary>
    public class FunctionDeclaration : Declaration
    {
        public FunctionDeclaration(Token identifier) : base(identifier) { }

        /// <summary>List of parameter declarations for this function</summary>
        public List<Declaration> Parameters { get; set; } = new();

        /// <summary>The scope (body) of this function</summary>
        public Scope Scope { get; set; } = default!;

        /// <summary>The return type token (e.g., INT), or null for void functions</summary>
        public Token? ReturnType { get; set; }
    }

    /// <summary>
    /// Represents a parameter declaration in a function signature.
    /// Example: In "FUNCTION add(NUMBER a, NUMBER b)", "NUMBER a" is a ParameterDeclaration.
    /// </summary>
    public class ParameterDeclaration : Declaration
    {
        public ParameterDeclaration(Token type, Token identifier) : base(identifier)
        {
            this.DeclarativeType = type;
        }

        /// <summary>The type token (e.g., NUMBER, STRING)</summary>
        public Token DeclarativeType { get; }
    }

    /// <summary>
    /// Represents a variable declaration.
    /// Example: "VAR x = 10" or "VAR INT x = 10"
    /// </summary>
    public class VariableDeclaration : Declaration
    {
        public VariableDeclaration(Token identifier) : base(identifier) { }

        public VariableDeclaration(Token type, Token identifier) : base(identifier)
        {
            this.DeclarativeType = type;
        }

        /// <summary>The type token (e.g., INT), or null for inferred types</summary>
        public Token? DeclarativeType { get; }

        /// <summary>The initial value expression assigned to this variable</summary>
        public Expression? InitialValue { get; set; }
    }
}
