using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using System.Collections;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    /// Defines the type of scope.
    /// </summary>
    public enum ScopeType
    {
        Root,       // The root/global scope
        Function,   // A function scope (requires RETURN statement)
        Block       // A generic block scope (if/while/etc - future use)
    }

    /// <summary>
    /// Represents a lexical scope in the code.
    /// Scopes can be nested and contain declarations, statements, and references to outer scopes.
    /// </summary>
    public class Scope
    {
        public Scope _outerScope = default;

        /// <summary>Declarations (functions, variables) made in this scope</summary>
        public Dictionary<string, Declaration> Decarations = [];

        /// <summary>Statements executed in this scope</summary>
        public List<Statement> Statements { get; set; } = new();

        /// <summary>Whether this scope contains a RETURN statement (required for function scopes)</summary>
        public bool HasReturn { get; set; }

        /// <summary>The type of this scope (root, function, block)</summary>
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