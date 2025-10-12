#nullable enable
using System.Collections;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    ///     Represents a lexical scope in the code.
    ///     Scopes can be nested and contain declarations, statements, and references to outer scopes.
    /// </summary>
    public class Scope
    {
        /// <summary>Track which variables are dynamically typed (FLEX)</summary>
        private readonly HashSet<string> _dynamicVariables = [];

        /// <summary>Track variable types for static type checking</summary>
        private readonly Dictionary<string, Type> _variableTypes = [];

        public Scope? _outerScope;

        /// <summary>Declarations (functions, variables) made in this scope</summary>
        public Dictionary<string, Declaration> Decarations = [];

        public Scope(string scopeName)
        {
            ScopeName = scopeName;
        }

        public Scope()
        {
        }

        /// <summary>Statements executed in this scope</summary>
        public List<Statement> Statements { get; set; } = [];

        /// <summary>Whether this scope contains a RETURN statement (required for function scopes)</summary>
        public bool HasReturn { get; set; }

        /// <summary>The type of this scope (root, function, block)</summary>
        public ScopeType Type { get; set; } = ScopeType.Block;

        /// <summary>For function scopes, reference back to the function declaration</summary>
        public FunctionDeclaration? FunctionDeclaration { get; set; }

        public IEnumerator? Enumerator { get; set; }

        public Scope? InnerScope { get; set; }

        public string? Name { get; }

        public Scope? OuterScope
        {
            get => _outerScope;
            set => _outerScope = value;
        }

        public string? ScopeName { get; set; }
        public Token? Token { get; set; }

        public override string ToString()
        {
            if (OuterScope is null) return ScopeName ?? "UnnamedScope";

            var result = string.Empty;
            var current = this;
            do
            {
                result = $"{result}.{ScopeName}";
            } while (current.OuterScope is null);

            return result;
        }

        /// <summary>
        ///     Visualizes the scope tree structure with indentation.
        ///     Shows scope name, type, declarations, statements, and nested scopes.
        /// </summary>
        public void Visualize(int depth = 0)
        {
            string indent = new(' ', depth * 2);

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
                        funcDecl.Scope.Visualize(depth + 2);
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
                            Console.WriteLine($"{indent}│      Returns: {retStmt.ReturnValue.ExpressionType}");
                        else
                            Console.WriteLine($"{indent}│      Returns: void");
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
            InnerScope?.Visualize(depth + 1);
        }

        /// <summary>
        ///     Registers a variable as dynamically typed (FLEX)
        /// </summary>
        public void AddDynamicVariable(string name)
        {
            _dynamicVariables.Add(name);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[Scope] Registered dynamic variable '{name}' in scope '{ScopeName}'");
            Console.ResetColor();
        }

        /// <summary>
        ///     Checks if a variable is dynamically typed
        /// </summary>
        public bool IsDynamicVariable(string name)
        {
            if (_dynamicVariables.Contains(name)) return true;

            // Check parent scope
            return _outerScope != null ? _outerScope.IsDynamicVariable(name) : false;
        }

        /// <summary>
        ///     Registers the type of a statically typed variable
        /// </summary>
        public void RegisterVariableType(string name, Type type)
        {
            _variableTypes[name] = type;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"[Scope] Registered variable '{name}' with type '{type.Name}' in scope '{ScopeName}'");
            Console.ResetColor();
        }

        /// <summary>
        ///     Gets the declared type of a variable
        /// </summary>
        public Type? GetVariableType(string name)
        {
            if (_variableTypes.TryGetValue(name, out var type)) return type;

            // Check parent scope
            return _outerScope?.GetVariableType(name);
        }

        /// <summary>
        ///     Validates type assignment for statically typed variables
        /// </summary>
        public bool ValidateTypeAssignment(string variableName, Type valueType)
        {
            // Dynamic variables can accept any type
            if (IsDynamicVariable(variableName)) return true;

            // Get the declared type
            var declaredType = GetVariableType(variableName);
            if (declaredType == null)
                // Variable not found - might be a new declaration
                return true;

            // Check type compatibility
            return declaredType.IsAssignableFrom(valueType) || valueType.IsAssignableFrom(declaredType);
        }
    }
}