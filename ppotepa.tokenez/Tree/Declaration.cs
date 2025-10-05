using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    public abstract class Declaration
    {
        protected Declaration(Token identifier)
        {
            this.Identifier = identifier;
        }

        public Token Identifier { get; set; }
    }

    public class FunctionDeclaration : Declaration
    {
        public FunctionDeclaration(Token identifier) : base(identifier) { }
    }

    public class ParameterDeclaration : Declaration
    {
        public ParameterDeclaration(Token type, Token identifier) : base(identifier)
        {
            this.DeclarativeType = type;
        }

        public Token DeclarativeType { get; }
    }
}