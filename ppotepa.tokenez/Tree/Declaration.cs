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
    /// Example: "FUNCTION add(a, b) { ... }"
    /// </summary>
    public class FunctionDeclaration : Declaration
    {
        public FunctionDeclaration(Token identifier) : base(identifier) { }
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
}