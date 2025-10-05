namespace ppotepa.tokenez.Tree
{
    public class Declaration
    {
        public Declaration(string identifier)
        {
            this.Identifier = identifier;
        }

        public string Identifier { get; set; }
    }

    public class FunctionDeclaration : Declaration
    {
        public FunctionDeclaration(string identifier) : base(identifier) { }       
    }
}