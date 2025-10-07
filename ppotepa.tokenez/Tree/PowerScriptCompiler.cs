using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using TreeExpression = ppotepa.tokenez.Tree.Expressions.Expression;
using TreeBinaryExpression = ppotepa.tokenez.Tree.Expressions.BinaryExpression;
using TreeLiteralExpression = ppotepa.tokenez.Tree.Expressions.LiteralExpression;
using TreeIdentifierExpression = ppotepa.tokenez.Tree.Expressions.IdentifierExpression;
using SysExpression = System.Linq.Expressions.Expression;
using ParameterExpression = System.Linq.Expressions.ParameterExpression;

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
    }
}
