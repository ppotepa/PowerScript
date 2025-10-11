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

        /// <summary>For function scopes, reference back to the function declaration</summary>
        public FunctionDeclaration FunctionDeclaration { get; set; }

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

        /// <summary>
        /// Visualizes the scope tree structure with indentation.
        /// Shows scope name, type, declarations, statements, and nested scopes.
        /// </summary>
        public void Visualize(int depth = 0)
        {
            string indent = new string(' ', depth * 2);

            // Scope header
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{indent}├─ Scope: {ScopeName} ({Type})");
            Console.ResetColor();

            // Declarations
            if (Decarations.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{indent}│  Declarations:");
                foreach (var decl in Decarations)
                {
                    Console.WriteLine($"{indent}│    • {decl.Key} ({decl.Value.GetType().Name})");

                    // If it's a function declaration, visualize its scope
                    if (decl.Value is FunctionDeclaration funcDecl && funcDecl.Scope != null)
                    {
                        funcDecl.Scope.Visualize(depth + 2);
                    }
                }
                Console.ResetColor();
            }

            // Statements
            if (Statements.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{indent}│  Statements:");
                foreach (var stmt in Statements)
                {
                    Console.WriteLine($"{indent}│    • {stmt.StatementType}");
                    if (stmt is ReturnStatement retStmt)
                    {
                        if (retStmt.ReturnValue != null)
                        {
                            Console.WriteLine($"{indent}│      Returns: {retStmt.ReturnValue.ExpressionType}");
                        }
                        else
                        {
                            Console.WriteLine($"{indent}│      Returns: void");
                        }
                    }
                    // TODO: Re-implement PRINT statement
                    // else if (stmt is PrintStatement printStmt)
                    // {
                    //     if (printStmt.PrintValue != null)
                    //     {
                    //         Console.WriteLine($"{indent}│      Prints: {printStmt.PrintValue.ExpressionType}");
                    //     }
                    // }
                }
                Console.ResetColor();
            }

            // Return status
            if (Type == ScopeType.Function)
            {
                Console.ForegroundColor = HasReturn ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"{indent}│  HasReturn: {HasReturn}");
                Console.ResetColor();
            }

            // Inner scope (if exists)
            if (InnerScope != null)
            {
                InnerScope.Visualize(depth + 1);
            }
        }
    }
}