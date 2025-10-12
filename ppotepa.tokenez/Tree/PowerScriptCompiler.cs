#nullable enable
using ppotepa.tokenez.Logging;
using ppotepa.tokenez.Tree.Diagnostics;
using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Operators;
using System.Reflection;
using ParameterExpression = System.Linq.Expressions.ParameterExpression;
using SysExpression = System.Linq.Expressions.Expression;
using TreeBinaryExpression = ppotepa.tokenez.Tree.Expressions.BinaryExpression;
using TreeExpression = ppotepa.tokenez.Tree.Expressions.Expression;
using TreeIdentifierExpression = ppotepa.tokenez.Tree.Expressions.IdentifierExpression;
using TreeLiteralExpression = ppotepa.tokenez.Tree.Expressions.LiteralExpression;
using TreeStringLiteralExpression = ppotepa.tokenez.Tree.Expressions.StringLiteralExpression;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    ///     Compiles PowerScript token trees into executable .NET Lambda expressions.
    ///     Handles function compilation, parameter binding, and expression tree building.
    /// </summary>
    public class PowerScriptCompiler(TokenTree tree)
    {
        private readonly TokenTree _tree = tree ?? throw new ArgumentNullException(nameof(tree));
        private readonly Dictionary<string, object> _variables = [];

        /// <summary>
        ///     Compiles all functions in the root scope and executes them.
        ///     Also runs diagnostic analysis to provide warnings and suggestions.
        /// </summary>
        public void CompileAndExecute()
        {
            // First, run diagnostic analysis
            RunDiagnosticAnalysis();

            LoggerService.Logger.Info("\n╔════════════════════════════════════════╗");
            LoggerService.Logger.Info("║     COMPILING & EXECUTING FUNCTIONS    ║");
            LoggerService.Logger.Info("╚════════════════════════════════════════╝\n");

            // Iterate through all function declarations in root scope
            foreach (FunctionDeclaration decl in _tree.RootScope!.Decarations.Values.OfType<FunctionDeclaration>())
            {
                string funcName = decl.Identifier.RawToken.Text;
                Scope funcScope = decl.Scope;

                LoggerService.Logger.Warning($"📦 Compiling function: {funcName}");

                try
                {
                    CompileFunction(decl);
                }
                catch (Exception ex)
                {
                    LoggerService.Logger.Error($"  ✗ Compilation failed: {ex.Message}");
                }
            }

            // Now execute any statements in the root scope (like PRINT statements)
            if (_tree.RootScope.Statements.Any())
            {
                LoggerService.Logger.Info("\n╔════════════════════════════════════════╗");
                LoggerService.Logger.Info("║       EXECUTING ROOT STATEMENTS        ║");
                LoggerService.Logger.Info("╚════════════════════════════════════════╝\n");

                foreach (Statement stmt in _tree.RootScope.Statements)
                {
                    try
                    {
                        ExecuteStatement(stmt);
                    }
                    catch (Exception ex)
                    {
                        LoggerService.Logger.Error($"✗ Execution failed: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        ///     Executes a statement (like PRINT, NET method calls, or EXECUTE).
        /// </summary>
        private void ExecuteStatement(Statement stmt)
        {
            if (stmt is PrintStatement printStmt)
            {
                // Evaluate and print the expression
                if (printStmt.Expression is TemplateStringExpression templateExpr)
                {
                    // Evaluate template string with variable interpolation
                    string result = EvaluateTemplateString(templateExpr);
                    Console.WriteLine(result);
                }
                else if (printStmt.Expression is TreeStringLiteralExpression stringExpr)
                {
                    // Remove quotes from string literal
                    string text = stringExpr.Value.RawToken.Text.Trim('"');
                    Console.WriteLine(text);
                }
                else if (printStmt.Expression is IdentifierExpression identExpr)
                {
                    // Print variable value
                    string varName = identExpr.Identifier.RawToken?.Text?.ToUpperInvariant() ?? "";
                    if (_variables.TryGetValue(varName, out object? value))
                    {
                        Console.WriteLine(value);
                    }
                    else
                    {
                        LoggerService.Logger.Warning($"[WARN] Variable '{varName}' not found");
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
                    object value = EvaluateExpressionValue(indexExpr);
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
        ///     Executes a variable declaration (FLEX x = value).
        /// </summary>
        private void ExecuteVariableDeclaration(VariableDeclarationStatement varStmt)
        {
            string varName = varStmt.Declaration.Identifier.RawToken?.Text?.ToUpperInvariant() ?? "";
            object value = EvaluateExpressionValue(varStmt.InitialValue);

            _variables[varName] = value;

            LoggerService.Logger.Debug($"[EXEC] Variable {varName} = {value}");
        }

        /// <summary>
        ///     Executes an array element assignment (FLEX arr[index] = value).
        /// </summary>
        private void ExecuteArrayAssignment(ArrayAssignmentStatement arrayAssignStmt)
        {
            string arrayName = arrayAssignStmt.ArrayIdentifier.RawToken?.Text?.ToUpperInvariant() ?? "";

            if (!_variables.TryGetValue(arrayName, out object? arrayObj))
            {
                throw new Exception($"Array '{arrayName}' not found");
            }

            // Evaluate the index
            object indexValue = EvaluateExpressionValue(arrayAssignStmt.IndexExpression);
            int index = Convert.ToInt32(ConvertToNumber(indexValue));

            // Evaluate the value to assign
            object value = EvaluateExpressionValue(arrayAssignStmt.ValueExpression);

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

            LoggerService.Logger.Debug($"[EXEC] {arrayName}[{index}] = {value}");
        }

        /// <summary>
        ///     Evaluates a template string by replacing @variable references with their runtime values.
        ///     Example: `Hello @name` where name = "World" -> "Hello World"
        /// </summary>
        private string EvaluateTemplateString(TemplateStringExpression templateExpr)
        {
            string result = "";

            foreach (Tokens.Values.TemplatePart? part in templateExpr.Template.Parts)
            {
                if (part.IsLiteral)
                {
                    // Just append the literal text
                    result += part.Text;
                }
                else
                {
                    // Look up the variable value
                    string varName = part.Text.ToUpperInvariant();
                    if (_variables.TryGetValue(varName, out object? value))
                    {
                        result += value.ToString();
                    }
                    else
                    {
                        // Variable not found - leave the @var syntax or show error
                        result += $"@{part.Text}";
                        LoggerService.Logger.Warning($"[WARN] Variable '{varName}' not found in template string");
                        Console.ResetColor();
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Executes an IF statement by evaluating the condition and executing the appropriate branch.
        /// </summary>
        private void ExecuteIfStatement(IfStatement ifStmt)
        {
            LoggerService.Logger.Info($"[EXEC] Evaluating IF condition: {ifStmt.Condition}");

            // Evaluate the condition
            bool conditionResult = EvaluateCondition(ifStmt.Condition);

            LoggerService.Logger.Info($"[EXEC] Condition result: {conditionResult}");

            if (conditionResult)
            {
                // Execute THEN branch
                LoggerService.Logger.Info("[EXEC] Executing THEN branch");

                foreach (Statement stmt in ifStmt.ThenScope.Statements)
                {
                    ExecuteStatement(stmt);
                }
            }
            else if (ifStmt.ElseScope != null)
            {
                // Execute ELSE branch
                LoggerService.Logger.Info("[EXEC] Executing ELSE branch");

                foreach (Statement stmt in ifStmt.ElseScope.Statements)
                {
                    ExecuteStatement(stmt);
                }
            }
        }

        /// <summary>
        ///     Evaluates a condition expression to a boolean result.
        /// </summary>
        private bool EvaluateCondition(Expression condition)
        {
            // Handle comparison expressions
            if (condition is BinaryExpression binaryExpr)
            {
                // Get left and right values
                object left = EvaluateExpressionValue(binaryExpr.Left);
                object right = EvaluateExpressionValue(binaryExpr.Right);

                // Get operator string from token
                string op = binaryExpr.Operator.RawToken?.Text ?? "";

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

                string op = logicalExpr.Operator.RawToken?.Text ?? "";

                if (op == "AND")
                {
                    // Short-circuit evaluation for AND
                    return !leftResult ? false : EvaluateCondition(logicalExpr.Right);
                }

                if (op == "OR")
                {
                    // Short-circuit evaluation for OR
                    return leftResult ? true : EvaluateCondition(logicalExpr.Right);
                }

                throw new Exception($"Unknown logical operator: {op}");
            }

            throw new Exception($"Cannot evaluate condition of type: {condition.GetType().Name}");
        }

        /// <summary>
        ///     Evaluates an expression to get its runtime value.
        /// </summary>
        private object EvaluateExpressionValue(Expression expr)
        {
            if (expr is ArrayLiteralExpression arrayLiteralExpr)
            {
                // Create array from literal values
                List<object> array = [];
                foreach (TreeExpression? elementExpr in arrayLiteralExpr.Elements)
                {
                    object elementValue = EvaluateExpressionValue(elementExpr);
                    array.Add(elementValue);
                }

                LoggerService.Logger.Debug($"[EXEC] Created array literal with {array.Count} elements");

                return array;
            }

            if (expr is ArrayCreationExpression arrayCreationExpr)
            {
                // Create array with specified size
                string sizeText = arrayCreationExpr.SizeToken.RawToken?.Text ?? "0";
                int size = int.Parse(sizeText);

                // Create a List<object> initialized with zeros
                List<object> array = new(size);
                for (int i = 0; i < size; i++)
                {
                    array.Add(0.0);
                }

                return array;
            }

            if (expr is TreeStringLiteralExpression stringLiteralExpr)
            {
                // String literal - remove quotes
                string text = stringLiteralExpr.Value.RawToken?.Text ?? "";
                return text.Trim('"');
            }

            if (expr is LiteralExpression literalExpr)
            {
                // Parse the literal value
                string text = literalExpr.Value.RawToken?.Text ?? "";
                return int.TryParse(text, out int intValue) ? intValue :
                    double.TryParse(text, out double doubleValue) ? doubleValue : text;
            }

            if (expr is IndexExpression indexExpr)
            {
                // Array index access
                string? arrayName = indexExpr.ArrayIdentifier.RawToken?.Text?.ToUpperInvariant();
                if (string.IsNullOrEmpty(arrayName) || !_variables.TryGetValue(arrayName, out object? arrayObj))
                {
                    throw new Exception($"Array not found: {arrayName}");
                }

                // Evaluate the index expression
                object indexValue = EvaluateExpressionValue(indexExpr.Index);
                int index = Convert.ToInt32(ConvertToNumber(indexValue));

                // Support different array types
                return arrayObj is List<object> list
                    ? index < 0 || index >= list.Count
                        ? throw new Exception($"Index {index} out of range for array {arrayName} (size: {list.Count})")
                        : list[index]
                    : arrayObj is double[] doubleArray
                    ? index < 0 || index >= doubleArray.Length
                        ? throw new Exception(
                            $"Index {index} out of range for array {arrayName} (size: {doubleArray.Length})")
                        : (object)doubleArray[index]
                    : arrayObj is object[] objArray
                        ? index < 0 || index >= objArray.Length
                            ? throw new Exception(
                                $"Index {index} out of range for array {arrayName} (size: {objArray.Length})")
                            : objArray[index]
                        : throw new Exception($"Variable {arrayName} is not an array (type: {arrayObj.GetType().Name})");
            }

            if (expr is IdentifierExpression identifierExpr)
            {
                // Look up variable value
                string? varName = identifierExpr.Identifier.RawToken?.Text?.ToUpperInvariant();
                if (string.IsNullOrEmpty(varName) || !_variables.TryGetValue(varName, out object? value))
                {
                    throw new Exception($"Variable not found: {varName}");
                }
                return value;
            }

            if (expr is BinaryExpression binaryExpr)
            {
                // Evaluate binary operation
                return EvaluateBinaryExpression(binaryExpr);
            }

            throw new Exception($"Cannot evaluate expression of type: {expr.GetType().Name}");
        }

        /// <summary>
        ///     Evaluates a binary expression (e.g., a + b, x * y).
        /// </summary>
        private object EvaluateBinaryExpression(BinaryExpression expr)
        {
            object left = EvaluateExpressionValue(expr.Left);
            object right = EvaluateExpressionValue(expr.Right);

            // Convert to numeric values
            double leftNum = ConvertToNumber(left);
            double rightNum = ConvertToNumber(right);

            // Perform the operation based on operator type
            if (expr.Operator is PlusToken)
            {
                double result = leftNum + rightNum;
                LoggerService.Logger.Debug($"[EXEC] {leftNum} + {rightNum} = {result}");
                return result;
            }

            if (expr.Operator is MinusToken)
            {
                double result = leftNum - rightNum;
                LoggerService.Logger.Debug($"[EXEC] {leftNum} - {rightNum} = {result}");
                return result;
            }

            if (expr.Operator is MultiplyToken)
            {
                double result = leftNum * rightNum;
                LoggerService.Logger.Debug($"[EXEC] {leftNum} * {rightNum} = {result}");
                return result;
            }

            if (expr.Operator is DivideToken)
            {
                if (rightNum == 0)
                {
                    throw new Exception("Division by zero");
                }

                double result = leftNum / rightNum;
                LoggerService.Logger.Debug($"[EXEC] {leftNum} / {rightNum} = {result}");
                return result;
            }

            throw new Exception($"Unknown binary operator: {expr.Operator.GetType().Name}");
        }

        /// <summary>
        ///     Converts a value to a number (double).
        /// </summary>
        private double ConvertToNumber(object value)
        {
            return value is int intValue
                ? intValue
                : value is double doubleValue
                    ? doubleValue
                    : double.TryParse(value.ToString(), out double result)
                        ? result
                        : throw new Exception($"Cannot convert '{value}' to number");
        }

        /// <summary>
        ///     Compares two values and returns -1, 0, or 1.
        /// </summary>
        private int CompareValues(object left, object right)
        {
            // Try numeric comparison
            if (left is int leftInt && right is int rightInt)
            {
                return leftInt.CompareTo(rightInt);
            }

            if (left is double leftDouble && right is double rightDouble)
            {
                return leftDouble.CompareTo(rightDouble);
            }

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
        ///     Executes a function call by looking up the function declaration and running its body.
        /// </summary>
        private void ExecuteFunctionCall(FunctionCallStatement funcCallStmt)
        {
            LoggerService.Logger.Info($"[EXEC] Calling function: {funcCallStmt.FunctionName}()");

            // Find the function declaration in the root scope
            if (!_tree.RootScope!.Decarations.TryGetValue(funcCallStmt.FunctionName, out Declaration? declaration))
            {
                throw new Exception($"Function not found: {funcCallStmt.FunctionName}");
            }

            if (declaration is not FunctionDeclaration functionDecl)
            {
                throw new Exception($"{funcCallStmt.FunctionName} is not a function");
            }

            // Execute all statements in the function's scope
            foreach (Statement stmt in functionDecl.Scope.Statements)
            {
                ExecuteStatement(stmt);
            }
        }

        /// <summary>
        ///     Executes a CYCLE loop (count-based or collection-based).
        /// </summary>
        private void ExecuteCycleLoop(CycleLoopStatement cycleStmt)
        {
            if (cycleStmt.IsCountBased)
            {
                // Count-based loop: CYCLE 5 AS i { ... }
                object countValue = EvaluateExpressionValue(cycleStmt.CollectionExpression);

                if (!int.TryParse(countValue.ToString(), out int count))
                {
                    throw new Exception($"CYCLE count must be a number, got: {countValue}");
                }

                LoggerService.Logger.Info(
                    $"[EXEC] Starting count-based CYCLE loop: {count} iterations, variable '{cycleStmt.LoopVariableName}'");

                // Execute the loop body 'count' times
                for (int i = 0; i < count; i++)
                {
                    // Set the loop variable to the current index
                    string varName = cycleStmt.LoopVariableName.ToUpperInvariant();
                    _variables[varName] = i;

                    LoggerService.Logger.Debug($"[EXEC] CYCLE iteration {i}, {varName} = {i}");

                    // Execute all statements in the loop body
                    if (cycleStmt.LoopBody != null)
                    {
                        foreach (Statement stmt in cycleStmt.LoopBody.Statements)
                        {
                            ExecuteStatement(stmt);
                        }
                    }
                }

                LoggerService.Logger.Info("[EXEC] CYCLE loop completed");
            }
            else
            {
                // Collection-based loop: CYCLE IN items AS item { ... }
                // TODO: Implement collection-based loops
                throw new NotImplementedException(
                    "Collection-based CYCLE loops not yet implemented. Use count-based: CYCLE 5 { ... }");
            }
        }

        /// <summary>
        ///     Executes a NET:: method call using reflection.
        /// </summary>
        private void ExecuteNetMethodCall(NetMethodCallStatement netStmt)
        {
            try
            {
                // Parse the method path: System.Console.WriteLine or Console.WriteLine
                string[] parts = netStmt.FullMethodPath.Split('.');
                if (parts.Length < 2)
                {
                    throw new Exception($"Invalid NET method path: {netStmt.FullMethodPath}");
                }

                // The last part is the method name
                string methodName = parts[^1];

                // Everything before is the type name
                string typeName = string.Join(".", parts[..^1]);

                // Try to resolve the type using DotNetLinker (which knows about linked namespaces)
                Type? type = _tree.DotNetLinker.ResolveType(typeName);

                // Fallback: try direct resolution if DotNetLinker didn't find it
                if (type == null)
                {
                    // Try in System assemblies (with proper assembly name)
                    type = Type.GetType($"{typeName}, System.Console");
                }

                if (type == null)
                {
                    type = Type.GetType(typeName);
                }

                // If still not found, search in all loaded assemblies
                if (type == null)
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(typeName);
                        if (type != null)
                        {
                            break;
                        }
                    }
                }

                if (type == null)
                {
                    throw new Exception($"Type not found: {typeName}");
                }

                // Evaluate arguments and determine their types
                object?[] args = new object?[netStmt.Arguments.Count];
                Type[] argTypes = new Type[netStmt.Arguments.Count];

                for (int i = 0; i < netStmt.Arguments.Count; i++)
                {
                    args[i] = EvaluateExpression(netStmt.Arguments[i]);
                    argTypes[i] = args[i]?.GetType() ?? typeof(object);
                }

                // Find the method with matching parameter types
                MethodInfo? method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance,
                    null, argTypes, null);

                if (method == null)
                {
                    // Try without exact parameter matching - get all methods and find best match
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                        .Where(m => m.Name == methodName)
                        .ToArray();

                    // Find method with matching parameter count
                    method = methods.FirstOrDefault(m => m.GetParameters().Length == args.Length);

                    if (method == null)
                    {
                        throw new Exception(
                            $"Method not found: {methodName} on type {typeName} with {args.Length} parameter(s)");
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
                LoggerService.Logger.Error($"✗ NET method call failed: {ex.Message}");
            }
        }

        /// <summary>
        ///     Evaluates an expression to get its runtime value.
        /// </summary>
        private object? EvaluateExpression(TreeExpression expr)
        {
            if (expr is TreeStringLiteralExpression stringExpr)
            {
                // Remove quotes from string literal
                return stringExpr.Value.RawToken.Text.Trim('"');
            }

            if (expr is TreeLiteralExpression literalExpr)
            {
                string text = literalExpr.Value.RawToken.Text;

                // Try to parse as number
                return int.TryParse(text, out int intValue) ? intValue :
                    double.TryParse(text, out double doubleValue) ? doubleValue : text;
            }

            if (expr is TreeIdentifierExpression identifierExpr)
            {
                // TODO: Look up identifier value in scope
                return identifierExpr.Identifier.RawToken.Text;
            }

            if (expr is TreeBinaryExpression binaryExpr)
            {
                // TODO: Evaluate binary expressions
                throw new Exception("Binary expressions in NET calls not yet supported");
            }

            throw new Exception($"Unsupported expression type: {expr.GetType().Name}");
        }

        /// <summary>
        ///     Compiles a single function declaration.
        /// </summary>
        private void CompileFunction(FunctionDeclaration decl)
        {
            string funcName = decl.Identifier.RawToken.Text;
            Scope funcScope = decl.Scope;

            // Get the return statement
            ReturnStatement? returnStmt = funcScope.Statements.OfType<ReturnStatement>().FirstOrDefault();
            if (returnStmt == null)
            {
                LoggerService.Logger.Warning($"  ⚠ No return statement found in {funcName}");
                return;
            }

            // Build parameter expressions
            List<ParameterExpression> parameters = decl.Parameters.Select(p =>
                SysExpression.Parameter(typeof(int), p.Identifier.RawToken.Text)
            ).ToList();

            // Build the lambda expression
            if (returnStmt.ReturnValue == null)
            {
                // Void return
                LoggerService.Logger.Info($"  → Function {funcName} returns VOID");
                System.Linq.Expressions.LambdaExpression voidLambda = SysExpression.Lambda(
                    SysExpression.Empty(),
                    parameters
                );
                Delegate compiledVoid = voidLambda.Compile();
                LoggerService.Logger.Success("  ✓ Compiled as void function");
            }
            else
            {
                // Value return - compile and execute
                SysExpression bodyExpression = BuildExpression(returnStmt.ReturnValue, parameters);

                if (parameters.Count == 0)
                {
                    // Parameterless function - can execute immediately
                    System.Linq.Expressions.Expression<Func<int>> lambda = SysExpression.Lambda<Func<int>>(bodyExpression);
                    Func<int> compiled = lambda.Compile();

                    LoggerService.Logger.Info($"  → Lambda: () => {GetExpressionDescription(returnStmt.ReturnValue)}");

                    // Execute the function
                    int result = compiled();
                    LoggerService.Logger.Success($"  ✓ Executed: {funcName}() = {result}");
                }
                else if (parameters.Count == 2)
                {
                    // Two-parameter function
                    System.Linq.Expressions.Expression<Func<int, int, int>> lambda = SysExpression.Lambda<Func<int, int, int>>(bodyExpression, parameters);
                    Func<int, int, int> compiled = lambda.Compile();

                    LoggerService.Logger.Info(
                        $"  → Lambda: ({string.Join(", ", parameters.Select(p => p.Name))}) => {GetExpressionDescription(returnStmt.ReturnValue)}");

                    // Example execution with test values
                    int result = compiled(10, 5);
                    LoggerService.Logger.Success($"  ✓ Compiled and tested: {funcName}(10, 5) = {result}");
                }
                else
                {
                    LoggerService.Logger.Info(
                        $"  → Lambda: ({string.Join(", ", parameters.Select(p => p.Name))}) => {GetExpressionDescription(returnStmt.ReturnValue)}");
                    LoggerService.Logger.Success(
                        $"  ✓ Compiled successfully (execution requires {parameters.Count} parameters)");
                }
            }
        }

        /// <summary>
        ///     Builds a .NET Expression from a PowerScript expression.
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
        ///     Builds a binary operation expression.
        /// </summary>
        private SysExpression BuildBinaryExpression(TreeBinaryExpression binary, List<ParameterExpression> parameters)
        {
            SysExpression left = BuildExpression(binary.Left, parameters);
            SysExpression right = BuildExpression(binary.Right, parameters);

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
        ///     Gets a human-readable description of an expression.
        /// </summary>
        private string GetExpressionDescription(TreeExpression expr)
        {
            return expr switch
            {
                TreeLiteralExpression literal => literal.Value.RawToken.Text,
                TreeIdentifierExpression ident => ident.Identifier.RawToken.Text,
                TreeBinaryExpression binary =>
                    $"{GetExpressionDescription(binary.Left)} {binary.Operator.RawToken.Text} {GetExpressionDescription(binary.Right)}",
                _ => expr.ExpressionType
            };
        }

        /// <summary>
        ///     Executes an external PowerScript file using the interpreter.
        /// </summary>
        private void ExecuteScriptFile(string filePath)
        {
            try
            {
                // Import the interpreter namespace here to avoid circular dependencies
                Type? interpreterType = Type.GetType("ppotepa.tokenez.Interpreter.PowerScriptInterpreter, ppotepa.tokenez");
                if (interpreterType == null)
                {
                    throw new Exception("PowerScriptInterpreter type not found. Cannot execute external scripts.");
                }

                // Create an instance using the CreateNew static method
                MethodInfo? createNewMethod = interpreterType.GetMethod("CreateNew", BindingFlags.Public | BindingFlags.Static);
                object? interpreter = createNewMethod?.Invoke(null, null);

                if (interpreter == null)
                {
                    throw new Exception("Failed to create PowerScriptInterpreter instance.");
                }

                // Call ExecuteFile method
                MethodInfo? executeFileMethod =
                    interpreterType.GetMethod("ExecuteFile", BindingFlags.Public | BindingFlags.Instance);
                executeFileMethod?.Invoke(interpreter, new object[] { filePath });
            }
            catch (Exception ex)
            {
                LoggerService.Logger.Error($"✗ EXECUTE failed: {ex.Message}");
            }
        }

        /// <summary>
        ///     Runs diagnostic analysis and displays warnings, errors, and suggestions.
        /// </summary>
        private void RunDiagnosticAnalysis()
        {
            LoggerService.Logger.Info("\n╔════════════════════════════════════════╗");
            LoggerService.Logger.Info("║           DIAGNOSTIC ANALYSIS          ║");
            LoggerService.Logger.Info("╚════════════════════════════════════════╝\n");

            DiagnosticAnalyzer analyzer = new();
            List<Diagnostic> diagnostics = analyzer.Analyze(_tree.RootScope!, _tree.Tokens!);

            if (diagnostics.Count == 0)
            {
                LoggerService.Logger.Success("✅ No issues found - your code looks good!");
                return;
            }

            // Group diagnostics by severity
            List<Diagnostic> errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            List<Diagnostic> warnings = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ToList();
            List<Diagnostic> suggestions = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Suggestion).ToList();
            List<Diagnostic> info = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info).ToList();

            // Display errors first
            if (errors.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERRORS:");
                foreach (Diagnostic? error in errors)
                {
                    Console.WriteLine($"  {error}");
                }

                Console.WriteLine();
                Console.ResetColor();
            }

            // Then warnings
            if (warnings.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("WARNINGS:");
                foreach (Diagnostic? warning in warnings)
                {
                    Console.WriteLine($"  {warning}");
                }

                Console.WriteLine();
                Console.ResetColor();
            }

            // Then suggestions
            if (suggestions.Any())
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("SUGGESTIONS:");
                foreach (Diagnostic? suggestion in suggestions)
                {
                    Console.WriteLine($"  {suggestion}");
                }

                Console.WriteLine();
                Console.ResetColor();
            }

            // Finally info (only show if verbose or if there are no other diagnostics)
            if (info.Any() && errors.Count + warnings.Count + suggestions.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("INFO:");
                foreach (Diagnostic? infoItem in info)
                {
                    Console.WriteLine($"  {infoItem}");
                }

                Console.WriteLine();
                Console.ResetColor();
            }

            // Summary
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"📊 Analysis complete: {errors.Count} errors, {warnings.Count} warnings, {suggestions.Count} suggestions");
            Console.ResetColor();
        }
    }
}