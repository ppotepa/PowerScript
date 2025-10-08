using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using TreeExpression = ppotepa.tokenez.Tree.Expressions.Expression;
using TreeBinaryExpression = ppotepa.tokenez.Tree.Expressions.BinaryExpression;
using TreeLiteralExpression = ppotepa.tokenez.Tree.Expressions.LiteralExpression;
using TreeIdentifierExpression = ppotepa.tokenez.Tree.Expressions.IdentifierExpression;
using TreeStringLiteralExpression = ppotepa.tokenez.Tree.Expressions.StringLiteralExpression;
using SysExpression = System.Linq.Expressions.Expression;
using ParameterExpression = System.Linq.Expressions.ParameterExpression;
using System.Reflection;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    /// Compiles PowerScript token trees into executable .NET Lambda expressions.
    /// Handles function compilation, parameter binding, and expression tree building.
    /// </summary>
    public class PowerScriptCompiler
    {
        private readonly TokenTree _tree;

        public PowerScriptCompiler(TokenTree tree)
        {
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
        }

        /// <summary>
        /// Compiles all functions in the root scope and executes them.
        /// </summary>
        public void CompileAndExecute()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║     COMPILING & EXECUTING FUNCTIONS    ║");
            Console.WriteLine("╚════════════════════════════════════════╝\n");
            Console.ResetColor();

            // Iterate through all function declarations in root scope
            foreach (var decl in _tree.RootScope.Decarations.Values.OfType<FunctionDeclaration>())
            {
                var funcName = decl.Identifier.RawToken.Text;
                var funcScope = decl.Scope;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"📦 Compiling function: {funcName}");
                Console.ResetColor();

                try
                {
                    CompileFunction(decl);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ Compilation failed: {ex.Message}");
                    Console.ResetColor();
                }
            }

            // Now execute any statements in the root scope (like PRINT statements)
            if (_tree.RootScope.Statements.Any())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n╔════════════════════════════════════════╗");
                Console.WriteLine("║       EXECUTING ROOT STATEMENTS        ║");
                Console.WriteLine("╚════════════════════════════════════════╝\n");
                Console.ResetColor();

                foreach (var stmt in _tree.RootScope.Statements)
                {
                    try
                    {
                        ExecuteStatement(stmt);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"✗ Execution failed: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
        }

        /// <summary>
        /// Executes a statement (like PRINT, NET method calls, or EXECUTE).
        /// </summary>
        private void ExecuteStatement(Statement stmt)
        {
            if (stmt is PrintStatement printStmt)
            {
                // Evaluate and print the expression
                if (printStmt.Expression is TreeStringLiteralExpression stringExpr)
                {
                    // Remove quotes from string literal
                    var text = stringExpr.Value.RawToken.Text.Trim('"');
                    Console.WriteLine(text);
                }
                else
                {
                    // TODO: Handle other expression types (function calls, variables, etc.)
                    Console.WriteLine(printStmt.Expression.ToString());
                }
            }
            else if (stmt is NetMethodCallStatement netStmt)
            {
                ExecuteNetMethodCall(netStmt);
            }
            else if (stmt is ExecuteStatement execStmt)
            {
                ExecuteScriptFile(execStmt.FilePath);
            }
        }

        /// <summary>
        /// Executes a NET:: method call using reflection.
        /// </summary>
        private void ExecuteNetMethodCall(NetMethodCallStatement netStmt)
        {
            try
            {
                // Parse the method path: System.Console.WriteLine
                var parts = netStmt.FullMethodPath.Split('.');
                if (parts.Length < 2)
                {
                    throw new Exception($"Invalid NET method path: {netStmt.FullMethodPath}");
                }

                // The last part is the method name
                var methodName = parts[^1];

                // Everything before is the type name
                var typeName = string.Join(".", parts[..^1]);

                // Try to find the type
                Type? type = null;

                // First, try in System assemblies (with proper assembly name)
                type = Type.GetType($"{typeName}, System.Console");

                if (type == null)
                {
                    type = Type.GetType(typeName);
                }

                // If not found, search in all loaded assemblies
                if (type == null)
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(typeName);
                        if (type != null) break;
                    }
                }

                if (type == null)
                {
                    throw new Exception($"Type not found: {typeName}");
                }

                // Evaluate arguments and determine their types
                var args = new object?[netStmt.Arguments.Count];
                var argTypes = new Type[netStmt.Arguments.Count];

                for (int i = 0; i < netStmt.Arguments.Count; i++)
                {
                    args[i] = EvaluateExpression(netStmt.Arguments[i]);
                    argTypes[i] = args[i]?.GetType() ?? typeof(object);
                }

                // Find the method with matching parameter types
                var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, argTypes, null);

                if (method == null)
                {
                    // Try without exact parameter matching - get all methods and find best match
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                                     .Where(m => m.Name == methodName)
                                     .ToArray();

                    // Find method with matching parameter count
                    method = methods.FirstOrDefault(m => m.GetParameters().Length == args.Length);

                    if (method == null)
                    {
                        throw new Exception($"Method not found: {methodName} on type {typeName} with {args.Length} parameter(s)");
                    }
                }

                // Invoke the method
                if (method.IsStatic)
                {
                    method.Invoke(null, args);
                }
                else
                {
                    // For instance methods, we need an instance
                    // For now, we'll only support static methods
                    throw new Exception($"Instance methods are not yet supported. Method {methodName} must be static.");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ NET method call failed: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Evaluates an expression to get its runtime value.
        /// </summary>
        private object? EvaluateExpression(TreeExpression expr)
        {
            if (expr is TreeStringLiteralExpression stringExpr)
            {
                // Remove quotes from string literal
                return stringExpr.Value.RawToken.Text.Trim('"');
            }
            else if (expr is TreeLiteralExpression literalExpr)
            {
                var text = literalExpr.Value.RawToken.Text;

                // Try to parse as number
                if (int.TryParse(text, out int intValue))
                    return intValue;
                if (double.TryParse(text, out double doubleValue))
                    return doubleValue;

                return text;
            }
            else if (expr is TreeIdentifierExpression identifierExpr)
            {
                // TODO: Look up identifier value in scope
                return identifierExpr.Identifier.RawToken.Text;
            }
            else if (expr is TreeBinaryExpression binaryExpr)
            {
                // TODO: Evaluate binary expressions
                throw new Exception("Binary expressions in NET calls not yet supported");
            }

            throw new Exception($"Unsupported expression type: {expr.GetType().Name}");
        }

        /// <summary>
        /// Compiles a single function declaration.
        /// </summary>
        private void CompileFunction(FunctionDeclaration decl)
        {
            var funcName = decl.Identifier.RawToken.Text;
            var funcScope = decl.Scope;

            // Get the return statement
            var returnStmt = funcScope.Statements.OfType<ReturnStatement>().FirstOrDefault();
            if (returnStmt == null)
            {
                Console.WriteLine($"  ⚠ No return statement found in {funcName}");
                return;
            }

            // Build parameter expressions
            var parameters = decl.Parameters.Select(p =>
                SysExpression.Parameter(typeof(int), p.Identifier.RawToken.Text)
            ).ToList();

            // Build the lambda expression
            if (returnStmt.ReturnValue == null)
            {
                // Void return
                Console.WriteLine($"  → Function {funcName} returns VOID");
                var voidLambda = SysExpression.Lambda(
                    SysExpression.Empty(),
                    parameters
                );
                var compiledVoid = voidLambda.Compile();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  ✓ Compiled as void function");
                Console.ResetColor();
            }
            else
            {
                // Value return - compile and execute
                var bodyExpression = BuildExpression(returnStmt.ReturnValue, parameters);

                if (parameters.Count == 0)
                {
                    // Parameterless function - can execute immediately
                    var lambda = SysExpression.Lambda<Func<int>>(bodyExpression);
                    var compiled = lambda.Compile();

                    Console.WriteLine($"  → Lambda: () => {GetExpressionDescription(returnStmt.ReturnValue)}");

                    // Execute the function
                    var result = compiled();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ Executed: {funcName}() = {result}");
                    Console.ResetColor();
                }
                else if (parameters.Count == 2)
                {
                    // Two-parameter function
                    var lambda = SysExpression.Lambda<Func<int, int, int>>(bodyExpression, parameters);
                    var compiled = lambda.Compile();

                    Console.WriteLine($"  → Lambda: ({string.Join(", ", parameters.Select(p => p.Name))}) => {GetExpressionDescription(returnStmt.ReturnValue)}");

                    // Example execution with test values
                    var result = compiled(10, 5);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ Compiled and tested: {funcName}(10, 5) = {result}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  → Lambda: ({string.Join(", ", parameters.Select(p => p.Name))}) => {GetExpressionDescription(returnStmt.ReturnValue)}");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✓ Compiled successfully (execution requires {parameters.Count} parameters)");
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Builds a .NET Expression from a PowerScript expression.
        /// </summary>
        private SysExpression BuildExpression(TreeExpression expr, List<ParameterExpression> parameters)
        {
            return expr switch
            {
                TreeLiteralExpression literal =>
                    SysExpression.Constant(int.Parse(literal.Value.RawToken.Text)),

                TreeIdentifierExpression ident =>
                    parameters.FirstOrDefault(p => p.Name == ident.Identifier.RawToken.Text)
                    ?? throw new Exception($"Parameter '{ident.Identifier.RawToken.Text}' not found"),

                TreeBinaryExpression binary =>
                    BuildBinaryExpression(binary, parameters),

                _ => throw new NotImplementedException($"Expression type {expr.GetType().Name} not implemented")
            };
        }

        /// <summary>
        /// Builds a binary operation expression.
        /// </summary>
        private SysExpression BuildBinaryExpression(TreeBinaryExpression binary, List<ParameterExpression> parameters)
        {
            var left = BuildExpression(binary.Left, parameters);
            var right = BuildExpression(binary.Right, parameters);

            return binary.Operator.RawToken.Text switch
            {
                "+" => SysExpression.Add(left, right),
                "-" => SysExpression.Subtract(left, right),
                "*" => SysExpression.Multiply(left, right),
                "/" => SysExpression.Divide(left, right),
                _ => throw new NotImplementedException($"Operator '{binary.Operator.RawToken.Text}' not implemented")
            };
        }

        /// <summary>
        /// Gets a human-readable description of an expression.
        /// </summary>
        private string GetExpressionDescription(TreeExpression expr)
        {
            return expr switch
            {
                TreeLiteralExpression literal => literal.Value.RawToken.Text,
                TreeIdentifierExpression ident => ident.Identifier.RawToken.Text,
                TreeBinaryExpression binary => $"{GetExpressionDescription(binary.Left)} {binary.Operator.RawToken.Text} {GetExpressionDescription(binary.Right)}",
                _ => expr.ExpressionType.ToString()
            };
        }

        /// <summary>
        /// Executes an external PowerScript file using the interpreter.
        /// </summary>
        private void ExecuteScriptFile(string filePath)
        {
            try
            {
                // Import the interpreter namespace here to avoid circular dependencies
                var interpreterType = Type.GetType("ppotepa.tokenez.Interpreter.PowerScriptInterpreter, ppotepa.tokenez");
                if (interpreterType == null)
                {
                    throw new Exception("PowerScriptInterpreter type not found. Cannot execute external scripts.");
                }

                // Create an instance using the CreateNew static method
                var createNewMethod = interpreterType.GetMethod("CreateNew", BindingFlags.Public | BindingFlags.Static);
                var interpreter = createNewMethod?.Invoke(null, null);

                if (interpreter == null)
                {
                    throw new Exception("Failed to create PowerScriptInterpreter instance.");
                }

                // Call ExecuteFile method
                var executeFileMethod = interpreterType.GetMethod("ExecuteFile", BindingFlags.Public | BindingFlags.Instance);
                executeFileMethod?.Invoke(interpreter, new object[] { filePath });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ EXECUTE failed: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
