using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Diagnostics;
using ppotepa.tokenez.Tree.Tokens.Operators;
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
        private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();

        public PowerScriptCompiler(TokenTree tree)
        {
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
        }

        /// <summary>
        /// Compiles all functions in the root scope and executes them.
        /// Also runs diagnostic analysis to provide warnings and suggestions.
        /// </summary>
        public void CompileAndExecute()
        {
            // First, run diagnostic analysis
            RunDiagnosticAnalysis();

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
                if (printStmt.Expression is TemplateStringExpression templateExpr)
                {
                    // Evaluate template string with variable interpolation
                    var result = EvaluateTemplateString(templateExpr);
                    Console.WriteLine(result);
                }
                else if (printStmt.Expression is TreeStringLiteralExpression stringExpr)
                {
                    // Remove quotes from string literal
                    var text = stringExpr.Value.RawToken.Text.Trim('"');
                    Console.WriteLine(text);
                }
                else if (printStmt.Expression is IdentifierExpression identExpr)
                {
                    // Print variable value
                    var varName = identExpr.Identifier.RawToken?.Text?.ToUpperInvariant() ?? "";
                    if (_variables.TryGetValue(varName, out var value))
                    {
                        Console.WriteLine(value);
                    }
                    else
                    {
                        Console.WriteLine($"[WARN] Variable '{varName}' not found");
                    }
                }
                else if (printStmt.Expression is LiteralExpression litExpr)
                {
                    // Print literal value directly
                    Console.WriteLine(litExpr.Value.RawToken?.Text ?? litExpr.ToString());
                }
                else if (printStmt.Expression is IndexExpression indexExpr)
                {
                    // Evaluate array index access and print
                    var value = EvaluateExpressionValue(indexExpr);
                    Console.WriteLine(value);
                }
                else
                {
                    // Fall back to ToString() for other expression types
                    Console.WriteLine(printStmt.Expression.ToString());
                }
            }
            else if (stmt is VariableDeclarationStatement varDeclStmt)
            {
                ExecuteVariableDeclaration(varDeclStmt);
            }
            else if (stmt is ArrayAssignmentStatement arrayAssignStmt)
            {
                ExecuteArrayAssignment(arrayAssignStmt);
            }
            else if (stmt is IfStatement ifStmt)
            {
                ExecuteIfStatement(ifStmt);
            }
            else if (stmt is FunctionCallStatement funcCallStmt)
            {
                ExecuteFunctionCall(funcCallStmt);
            }
            else if (stmt is CycleLoopStatement cycleStmt)
            {
                ExecuteCycleLoop(cycleStmt);
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
        /// Executes a variable declaration (FLEX x = value).
        /// </summary>
        private void ExecuteVariableDeclaration(VariableDeclarationStatement varStmt)
        {
            var varName = varStmt.Declaration.Identifier.RawToken?.Text?.ToUpperInvariant() ?? "";
            var value = EvaluateExpressionValue(varStmt.InitialValue);

            _variables[varName] = value;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[EXEC] Variable {varName} = {value}");
            Console.ResetColor();
        }

        /// <summary>
        /// Executes an array element assignment (FLEX arr[index] = value).
        /// </summary>
        private void ExecuteArrayAssignment(ArrayAssignmentStatement arrayAssignStmt)
        {
            var arrayName = arrayAssignStmt.ArrayIdentifier.RawToken?.Text?.ToUpperInvariant() ?? "";

            if (!_variables.TryGetValue(arrayName, out var arrayObj))
            {
                throw new Exception($"Array '{arrayName}' not found");
            }

            // Evaluate the index
            var indexValue = EvaluateExpressionValue(arrayAssignStmt.IndexExpression);
            int index = Convert.ToInt32(ConvertToNumber(indexValue));

            // Evaluate the value to assign
            var value = EvaluateExpressionValue(arrayAssignStmt.ValueExpression);

            // Assign to array
            if (arrayObj is List<object> list)
            {
                if (index < 0 || index >= list.Count)
                {
                    throw new Exception($"Index {index} out of range for array {arrayName} (size: {list.Count})");
                }
                list[index] = value;
            }
            else if (arrayObj is double[] doubleArray)
            {
                if (index < 0 || index >= doubleArray.Length)
                {
                    throw new Exception($"Index {index} out of range for array {arrayName} (size: {doubleArray.Length})");
                }
                doubleArray[index] = ConvertToNumber(value);
            }
            else
            {
                throw new Exception($"Cannot assign to index of non-array variable '{arrayName}'");
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[EXEC] {arrayName}[{index}] = {value}");
            Console.ResetColor();
        }

        /// <summary>
        /// Evaluates a template string by replacing @variable references with their runtime values.
        /// Example: `Hello @name` where name = "World" -> "Hello World"
        /// </summary>
        private string EvaluateTemplateString(TemplateStringExpression templateExpr)
        {
            var result = "";

            foreach (var part in templateExpr.Template.Parts)
            {
                if (part.IsLiteral)
                {
                    // Just append the literal text
                    result += part.Text;
                }
                else
                {
                    // Look up the variable value
                    var varName = part.Text.ToUpperInvariant();
                    if (_variables.TryGetValue(varName, out var value))
                    {
                        result += value.ToString();
                    }
                    else
                    {
                        // Variable not found - leave the @var syntax or show error
                        result += $"@{part.Text}";
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[WARN] Variable '{varName}' not found in template string");
                        Console.ResetColor();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Executes an IF statement by evaluating the condition and executing the appropriate branch.
        /// </summary>
        private void ExecuteIfStatement(IfStatement ifStmt)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[EXEC] Evaluating IF condition: {ifStmt.Condition}");
            Console.ResetColor();

            // Evaluate the condition
            bool conditionResult = EvaluateCondition(ifStmt.Condition);

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[EXEC] Condition result: {conditionResult}");
            Console.ResetColor();

            if (conditionResult)
            {
                // Execute THEN branch
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[EXEC] Executing THEN branch");
                Console.ResetColor();

                foreach (var stmt in ifStmt.ThenScope.Statements)
                {
                    ExecuteStatement(stmt);
                }
            }
            else if (ifStmt.ElseScope != null)
            {
                // Execute ELSE branch
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[EXEC] Executing ELSE branch");
                Console.ResetColor();

                foreach (var stmt in ifStmt.ElseScope.Statements)
                {
                    ExecuteStatement(stmt);
                }
            }
        }

        /// <summary>
        /// Evaluates a condition expression to a boolean result.
        /// </summary>
        private bool EvaluateCondition(Expression condition)
        {
            // Handle comparison expressions
            if (condition is BinaryExpression binaryExpr)
            {
                // Get left and right values
                var left = EvaluateExpressionValue(binaryExpr.Left);
                var right = EvaluateExpressionValue(binaryExpr.Right);

                // Get operator string from token
                var op = binaryExpr.Operator.RawToken?.Text ?? "";

                // Perform comparison based on operator
                return op switch
                {
                    "<" => CompareValues(left, right) < 0,
                    ">" => CompareValues(left, right) > 0,
                    "<=" => CompareValues(left, right) <= 0,
                    ">=" => CompareValues(left, right) >= 0,
                    "==" => CompareValues(left, right) == 0,
                    "!=" => CompareValues(left, right) != 0,
                    _ => throw new Exception($"Unknown comparison operator: {op}")
                };
            }

            // Handle logical expressions (AND/OR)
            if (condition is LogicalExpression logicalExpr)
            {
                bool leftResult = EvaluateCondition(logicalExpr.Left);

                var op = logicalExpr.Operator.RawToken?.Text ?? "";

                if (op == "AND")
                {
                    // Short-circuit evaluation for AND
                    if (!leftResult) return false;
                    return EvaluateCondition(logicalExpr.Right);
                }
                else if (op == "OR")
                {
                    // Short-circuit evaluation for OR
                    if (leftResult) return true;
                    return EvaluateCondition(logicalExpr.Right);
                }

                throw new Exception($"Unknown logical operator: {op}");
            }

            throw new Exception($"Cannot evaluate condition of type: {condition.GetType().Name}");
        }

        /// <summary>
        /// Evaluates an expression to get its runtime value.
        /// </summary>
        private object EvaluateExpressionValue(Expression expr)
        {
            if (expr is ArrayLiteralExpression arrayLiteralExpr)
            {
                // Create array from literal values
                var array = new List<object>();
                foreach (var elementExpr in arrayLiteralExpr.Elements)
                {
                    var elementValue = EvaluateExpressionValue(elementExpr);
                    array.Add(elementValue);
                }
                
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[EXEC] Created array literal with {array.Count} elements");
                Console.ResetColor();
                
                return array;
            }

            if (expr is ArrayCreationExpression arrayCreationExpr)
            {
                // Create array with specified size
                var sizeText = arrayCreationExpr.SizeToken.RawToken?.Text ?? "0";
                int size = int.Parse(sizeText);

                // Create a List<object> initialized with zeros
                var array = new List<object>(size);
                for (int i = 0; i < size; i++)
                {
                    array.Add(0.0);
                }
                return array;
            }

            if (expr is TreeStringLiteralExpression stringLiteralExpr)
            {
                // String literal - remove quotes
                var text = stringLiteralExpr.Value.RawToken?.Text ?? "";
                return text.Trim('"');
            }

            if (expr is LiteralExpression literalExpr)
            {
                // Parse the literal value
                var text = literalExpr.Value.RawToken?.Text ?? "";
                if (int.TryParse(text, out int intValue))
                    return intValue;
                if (double.TryParse(text, out double doubleValue))
                    return doubleValue;
                return text;
            }

            if (expr is IndexExpression indexExpr)
            {
                // Array index access
                var arrayName = indexExpr.ArrayIdentifier.RawToken?.Text?.ToUpperInvariant();
                if (!_variables.TryGetValue(arrayName, out var arrayObj))
                {
                    throw new Exception($"Array not found: {arrayName}");
                }

                // Evaluate the index expression
                var indexValue = EvaluateExpressionValue(indexExpr.Index);
                int index = Convert.ToInt32(ConvertToNumber(indexValue));

                // Support different array types
                if (arrayObj is List<object> list)
                {
                    if (index < 0 || index >= list.Count)
                    {
                        throw new Exception($"Index {index} out of range for array {arrayName} (size: {list.Count})");
                    }
                    return list[index];
                }
                else if (arrayObj is double[] doubleArray)
                {
                    if (index < 0 || index >= doubleArray.Length)
                    {
                        throw new Exception($"Index {index} out of range for array {arrayName} (size: {doubleArray.Length})");
                    }
                    return doubleArray[index];
                }
                else if (arrayObj is object[] objArray)
                {
                    if (index < 0 || index >= objArray.Length)
                    {
                        throw new Exception($"Index {index} out of range for array {arrayName} (size: {objArray.Length})");
                    }
                    return objArray[index];
                }
                else
                {
                    throw new Exception($"Variable {arrayName} is not an array (type: {arrayObj.GetType().Name})");
                }
            }

            if (expr is IdentifierExpression identifierExpr)
            {
                // Look up variable value
                var varName = identifierExpr.Identifier.RawToken?.Text?.ToUpperInvariant();
                if (_variables.TryGetValue(varName, out var value))
                {
                    return value;
                }
                throw new Exception($"Variable not found: {varName}");
            }

            if (expr is BinaryExpression binaryExpr)
            {
                // Evaluate binary operation
                return EvaluateBinaryExpression(binaryExpr);
            }

            throw new Exception($"Cannot evaluate expression of type: {expr.GetType().Name}");
        }

        /// <summary>
        /// Evaluates a binary expression (e.g., a + b, x * y).
        /// </summary>
        private object EvaluateBinaryExpression(BinaryExpression expr)
        {
            var left = EvaluateExpressionValue(expr.Left);
            var right = EvaluateExpressionValue(expr.Right);

            // Convert to numeric values
            double leftNum = ConvertToNumber(left);
            double rightNum = ConvertToNumber(right);

            // Perform the operation based on operator type
            if (expr.Operator is PlusToken)
            {
                var result = leftNum + rightNum;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[EXEC] {leftNum} + {rightNum} = {result}");
                Console.ResetColor();
                return result;
            }
            else if (expr.Operator is MinusToken)
            {
                var result = leftNum - rightNum;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[EXEC] {leftNum} - {rightNum} = {result}");
                Console.ResetColor();
                return result;
            }
            else if (expr.Operator is MultiplyToken)
            {
                var result = leftNum * rightNum;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[EXEC] {leftNum} * {rightNum} = {result}");
                Console.ResetColor();
                return result;
            }
            else if (expr.Operator is DivideToken)
            {
                if (rightNum == 0)
                {
                    throw new Exception("Division by zero");
                }
                var result = leftNum / rightNum;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[EXEC] {leftNum} / {rightNum} = {result}");
                Console.ResetColor();
                return result;
            }
            else
            {
                throw new Exception($"Unknown binary operator: {expr.Operator.GetType().Name}");
            }
        }

        /// <summary>
        /// Converts a value to a number (double).
        /// </summary>
        private double ConvertToNumber(object value)
        {
            if (value is int intValue)
                return intValue;
            if (value is double doubleValue)
                return doubleValue;
            if (double.TryParse(value.ToString(), out double result))
                return result;

            throw new Exception($"Cannot convert '{value}' to number");
        }

        /// <summary>
        /// Compares two values and returns -1, 0, or 1.
        /// </summary>
        private int CompareValues(object left, object right)
        {
            // Try numeric comparison
            if (left is int leftInt && right is int rightInt)
                return leftInt.CompareTo(rightInt);

            if (left is double leftDouble && right is double rightDouble)
                return leftDouble.CompareTo(rightDouble);

            // Try numeric conversion
            if (double.TryParse(left.ToString(), out double leftNum) &&
                double.TryParse(right.ToString(), out double rightNum))
            {
                return leftNum.CompareTo(rightNum);
            }

            // Fall back to string comparison
            return string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
        }

        /// <summary>
        /// Executes a function call by looking up the function declaration and running its body.
        /// </summary>
        private void ExecuteFunctionCall(FunctionCallStatement funcCallStmt)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[EXEC] Calling function: {funcCallStmt.FunctionName}()");
            Console.ResetColor();

            // Find the function declaration in the root scope
            if (!_tree.RootScope.Decarations.TryGetValue(funcCallStmt.FunctionName, out var declaration))
            {
                throw new Exception($"Function not found: {funcCallStmt.FunctionName}");
            }

            if (declaration is not FunctionDeclaration functionDecl)
            {
                throw new Exception($"{funcCallStmt.FunctionName} is not a function");
            }

            // Execute all statements in the function's scope
            foreach (var stmt in functionDecl.Scope.Statements)
            {
                ExecuteStatement(stmt);
            }
        }

        /// <summary>
        /// Executes a CYCLE loop (count-based or collection-based).
        /// </summary>
        private void ExecuteCycleLoop(CycleLoopStatement cycleStmt)
        {
            if (cycleStmt.IsCountBased)
            {
                // Count-based loop: CYCLE 5 AS i { ... }
                var countValue = EvaluateExpressionValue(cycleStmt.CollectionExpression);

                if (!int.TryParse(countValue.ToString(), out int count))
                {
                    throw new Exception($"CYCLE count must be a number, got: {countValue}");
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[EXEC] Starting count-based CYCLE loop: {count} iterations, variable '{cycleStmt.LoopVariableName}'");
                Console.ResetColor();

                // Execute the loop body 'count' times
                for (int i = 0; i < count; i++)
                {
                    // Set the loop variable to the current index
                    var varName = cycleStmt.LoopVariableName.ToUpperInvariant();
                    _variables[varName] = i;

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[EXEC] CYCLE iteration {i}, {varName} = {i}");
                    Console.ResetColor();

                    // Execute all statements in the loop body
                    foreach (var stmt in cycleStmt.LoopBody.Statements)
                    {
                        ExecuteStatement(stmt);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[EXEC] CYCLE loop completed");
                Console.ResetColor();
            }
            else
            {
                // Collection-based loop: CYCLE IN items AS item { ... }
                // TODO: Implement collection-based loops
                throw new NotImplementedException("Collection-based CYCLE loops not yet implemented. Use count-based: CYCLE 5 { ... }");
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

        /// <summary>
        /// Runs diagnostic analysis and displays warnings, errors, and suggestions.
        /// </summary>
        private void RunDiagnosticAnalysis()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║           DIAGNOSTIC ANALYSIS          ║");
            Console.WriteLine("╚════════════════════════════════════════╝\n");
            Console.ResetColor();

            var analyzer = new DiagnosticAnalyzer();
            var diagnostics = analyzer.Analyze(_tree.RootScope, _tree.Tokens);

            if (diagnostics.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ No issues found - your code looks good!");
                Console.ResetColor();
                return;
            }

            // Group diagnostics by severity
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            var warnings = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ToList();
            var suggestions = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Suggestion).ToList();
            var info = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ToList();

            // Display errors first
            if (errors.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERRORS:");
                foreach (var error in errors)
                    Console.WriteLine($"  {error}");
                Console.WriteLine();
                Console.ResetColor();
            }

            // Then warnings
            if (warnings.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("WARNINGS:");
                foreach (var warning in warnings)
                    Console.WriteLine($"  {warning}");
                Console.WriteLine();
                Console.ResetColor();
            }

            // Then suggestions
            if (suggestions.Any())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("SUGGESTIONS:");
                foreach (var suggestion in suggestions)
                    Console.WriteLine($"  {suggestion}");
                Console.WriteLine();
                Console.ResetColor();
            }

            // Finally info (only show if verbose or if there are no other diagnostics)
            if (info.Any() && (errors.Count + warnings.Count + suggestions.Count == 0))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("INFO:");
                foreach (var infoItem in info)
                    Console.WriteLine($"  {infoItem}");
                Console.WriteLine();
                Console.ResetColor();
            }

            // Summary
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"📊 Analysis complete: {errors.Count} errors, {warnings.Count} warnings, {suggestions.Count} suggestions");
            Console.ResetColor();
        }
    }
}
