using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using System.Collections;

namespace ppotepa.tokenez.Tree
{
    public enum ScopeType
    {
        Root,
        Function,
        Block
    }

    public class Scope
    {
        public Scope _outerScope = default;
        public Dictionary<string, Declaration> Decarations = [];
        public List<Statement> Statements { get; set; } = new();
        public bool HasReturn { get; set; }
        public ScopeType Type { get; set; } = ScopeType.Block;

        public Scope(string scopeName)
        {
            this.ScopeName = scopeName;
        }

        public Scope() { }
        public IEnumerator Enumerator { get; set; }

        public Scope InnerScope { get; set; }

        public string Name { get; }

        public Scope OuterScope
        {
            get => _outerScope;
            set => _outerScope = value;
        }

        public string ScopeName { get; set; }
        public Token Token { get; set; }

        public override string ToString()
        {
            if (OuterScope is null)
            {
                return ScopeName;
            }
            else
            {
                string result = string.Empty;
                Scope current = this;
                do
                {
                    result = $"{result}.{ScopeName}";
                }
                while (current.OuterScope is null);
                return result;
            }
        }
    }
}