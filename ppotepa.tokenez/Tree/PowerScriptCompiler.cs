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
    public class PowerScriptCompiler : IPowerScriptCompiler
    {
        private readonly TokenTree _tree;
        private readonly Dictionary<string, object> _variables = [];

        public PowerScriptCompiler(TokenTree tree)
        {
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
        }

        /// <summary>
        ///     Executes the provided scope and returns the result.
        /// </summary>
        public object? Execute(Scope scope)
        {
            object? result = null;

            // Execute all statements in the scope
            foreach (Statement stmt in scope.Statements)
            {
                try
                {
                    ExecuteStatement(stmt);

                    // If the statement returns a value, capture it
                    if (stmt is ReturnStatement returnStmt && returnStmt.ReturnValue != null)
                    {
                        result = EvaluateExpressionValue(returnStmt.ReturnValue);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.Logger.Error($"✗ Execution failed: {ex.Message}");
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        ///     Registers a function in the root scope.
        /// </summary>
        public void RegisterFunction(string functionName, FunctionDeclaration declaration)
        {
            if (string.IsNullOrWhiteSpace(functionName))
                throw new ArgumentException("Function name cannot be null or empty.", nameof(functionName));

            if (declaration == null)
                throw new ArgumentNullException(nameof(declaration));

            _tree.RootScope!.Decarations[functionName] = declaration;
        }

        /// <summary>
        ///     Checks if a function is registered in the root scope.
        /// </summary>
        public bool IsFunctionRegistered(string functionName)
        {
            if (string.IsNullOrWhiteSpace(functionName))
                return false;

            return _tree.RootScope!.Decarations.ContainsKey(functionName) &&
                   _tree.RootScope.Decarations[functionName] is FunctionDeclaration;
        }

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
                _ = decl.Scope;

                LoggerService.Logger.Warning($"📦 Compiling function: {decl.Identifier.RawToken.Text}");

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
            if (_tree.RootScope.Statements.Count > 0)
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
                throw new InvalidOperationException($"Array '{arrayName}' not found");
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
                    throw new InvalidOperationException($"Index {index} out of range for array {arrayName} (size: {list.Count})");
                }

                list[index] = value;
            }
            else if (arrayObj is double[] doubleArray)
            {
                if (index < 0 || index >= doubleArray.Length)
                {
                    throw new InvalidOperationException($"Index {index} out of range for array {arrayName} (size: {doubleArray.Length})");
                }

                doubleArray[index] = ConvertToNumber(value);
            }
            else
            {
                throw new InvalidOperationException($"Cannot assign to index of non-array variable '{arrayName}'");
            }

            LoggerService.Logger.Debug($"[EXEC] {arrayName}[{index}] = {value}");
        }

        /// <summary>
        ///     Evaluates a template string by replacing @variable references with their runtime values.
        ///     Example: `Hello @name` where name = "World" -> "Hello World"
        /// </summary>
        private string EvaluateTemplateString(TemplateStringExpression templateExpr)
        {
            System.Text.StringBuilder result = new();

            foreach (Tokens.Values.TemplatePart? part in templateExpr.Template.Parts)
            {
                if (part.IsLiteral)
                {
                    // Just append the literal text
                    result.Append(part.Text);
                }
                else
                {
                    // Look up the variable value
                    string varName = part.Text.ToUpperInvariant();
                    if (_variables.TryGetValue(varName, out object? value))
                    {
                        result.Append(value.ToString());
                    }
                    else
                    {
                        // Variable not found - leave the @var syntax or show error
                        result.Append($"@{part.Text}");
                        LoggerService.Logger.Warning($"Variable '{varName}' not found in template string");
                    }
                }
            }

            return result.ToString();
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
                    _ => throw new InvalidOperationException($"Unknown comparison operator: {op}")
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
                    return leftResult && EvaluateCondition(logicalExpr.Right);
                }

                if (op == "OR")
                {
                    // Short-circuit evaluation for OR
                    return leftResult || EvaluateCondition(logicalExpr.Right);
                }

                throw new InvalidOperationException($"Unknown logical operator: {op}");
            }

            throw new InvalidOperationException($"Cannot evaluate condition of type: {condition.GetType().Name}");
        }

        /// <summary>
        ///     Evaluates an expression to get its runtime value.
        /// </summary>
        private object EvaluateExpressionValue(Expression expr)
        {
            return expr switch
            {
                ArrayLiteralExpression arrayLiteralExpr => EvaluateArrayLiteral(arrayLiteralExpr),
                ArrayCreationExpression arrayCreationExpr => EvaluateArrayCreation(arrayCreationExpr),
                TreeStringLiteralExpression stringLiteralExpr => EvaluateStringLiteral(stringLiteralExpr),
                LiteralExpression literalExpr => EvaluateLiteral(literalExpr),
                IndexExpression indexExpr => EvaluateIndexExpression(indexExpr),
                IdentifierExpression identifierExpr => EvaluateIdentifier(identifierExpr),
                BinaryExpression binaryExpr => EvaluateBinaryExpression(binaryExpr),
                _ => throw new InvalidOperationException($"Cannot evaluate expression of type: {expr.GetType().Name}")
            };
        }

        /// <summary>
        ///     Evaluates an array literal expression.
        /// </summary>
        private List<object> EvaluateArrayLiteral(ArrayLiteralExpression arrayLiteralExpr)
        {
            List<object> array = [];

            foreach (TreeExpression elementExpr in arrayLiteralExpr.Elements)
            {
                object elementValue = EvaluateExpressionValue(elementExpr);
                array.Add(elementValue);
            }

            LoggerService.Logger.Debug($"[EXEC] Created array literal with {array.Count} elements");
            return array;
        }

        /// <summary>
        ///     Evaluates an array creation expression with specified size.
        /// </summary>
        private static List<object> EvaluateArrayCreation(ArrayCreationExpression arrayCreationExpr)
        {
            string sizeText = arrayCreationExpr.SizeToken.RawToken?.Text ?? "0";
            int size = int.Parse(sizeText);

            List<object> array = new(size);
            for (int i = 0; i < size; i++)
            {
                array.Add(0.0);
            }

            return array;
        }

        /// <summary>
        ///     Evaluates a string literal expression by removing quotes.
        /// </summary>
        private static string EvaluateStringLiteral(TreeStringLiteralExpression stringLiteralExpr)
        {
            string text = stringLiteralExpr.Value.RawToken?.Text ?? "";
            return text.Trim('"');
        }

        /// <summary>
        ///     Evaluates a literal expression (number or text).
        /// </summary>
        private static object EvaluateLiteral(LiteralExpression literalExpr)
        {
            string text = literalExpr.Value.RawToken?.Text ?? "";

            // Try parsing as integer first
            if (int.TryParse(text, out int intValue))
            {
                return intValue;
            }

            // Try parsing as double
            if (double.TryParse(text, out double doubleValue))
            {
                return doubleValue;
            }

            // Fall back to string
            return text;
        }

        /// <summary>
        ///     Evaluates an array index access expression.
        /// </summary>
        private object EvaluateIndexExpression(IndexExpression indexExpr)
        {
            string? arrayName = indexExpr.ArrayIdentifier.RawToken?.Text?.ToUpperInvariant();

            if (string.IsNullOrEmpty(arrayName) || !_variables.TryGetValue(arrayName, out object? arrayObj))
            {
                throw new InvalidOperationException($"Array not found: {arrayName}");
            }

            object indexValue = EvaluateExpressionValue(indexExpr.Index);
            int index = Convert.ToInt32(ConvertToNumber(indexValue));

            return arrayObj switch
            {
                List<object> list => GetListElement(list, index, arrayName),
                double[] doubleArray => GetArrayElement(doubleArray, index, arrayName),
                object[] objArray => GetArrayElement(objArray, index, arrayName),
                _ => throw new InvalidOperationException($"Variable {arrayName} is not an array (type: {arrayObj.GetType().Name})")
            };
        }

        /// <summary>
        ///     Gets an element from a List with bounds checking.
        /// </summary>
        private static object GetListElement(List<object> list, int index, string arrayName)
        {
            return index < 0 || index >= list.Count
                ? throw new InvalidOperationException($"Index {index} out of range for array {arrayName} (size: {list.Count})")
                : list[index];
        }

        /// <summary>
        ///     Gets an element from an array with bounds checking.
        /// </summary>
        private static object GetArrayElement(Array array, int index, string arrayName)
        {
            return index < 0 || index >= array.Length
                ? throw new InvalidOperationException($"Index {index} out of range for array {arrayName} (size: {array.Length})")
                : array.GetValue(index)!;
        }

        /// <summary>
        ///     Evaluates an identifier by looking up its value.
        /// </summary>
        private object EvaluateIdentifier(IdentifierExpression identifierExpr)
        {
            string? varName = identifierExpr.Identifier.RawToken?.Text?.ToUpperInvariant();

            return string.IsNullOrEmpty(varName) || !_variables.TryGetValue(varName, out object? value)
                ? throw new InvalidOperationException($"Variable not found: {varName}")
                : value;
        }

        /// <summary>
        ///     Evaluates a binary expression (e.g., a + b, x * y).
        /// </summary>
        private double EvaluateBinaryExpression(BinaryExpression expr)
        {
            double leftNum = ConvertToNumber(EvaluateExpressionValue(expr.Left));
            double rightNum = ConvertToNumber(EvaluateExpressionValue(expr.Right));

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
                if (Math.Abs(rightNum) < double.Epsilon)
                {
                    throw new InvalidOperationException("Division by zero");
                }

                double result = leftNum / rightNum;
                LoggerService.Logger.Debug($"[EXEC] {leftNum} / {rightNum} = {result}");
                return result;
            }

            throw new InvalidOperationException($"Unknown binary operator: {expr.Operator.GetType().Name}");
        }

        /// <summary>
        ///     Converts a value to a number (double).
        /// </summary>
        private static double ConvertToNumber(object value)
        {
            return value switch
            {
                int intValue => intValue,
                double doubleValue => doubleValue,
                string stringValue when double.TryParse(stringValue, out double parsed) => parsed,
                _ when double.TryParse(value.ToString(), out double parsed) => parsed,
                _ => throw new InvalidOperationException($"Cannot convert '{value}' to number")
            };
        }

        /// <summary>
        ///     Compares two values and returns -1, 0, or 1.
        /// </summary>
        private static int CompareValues(object left, object right)
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
                throw new InvalidOperationException($"Function not found: {funcCallStmt.FunctionName}");
            }

            if (declaration is not FunctionDeclaration functionDecl)
            {
                throw new InvalidOperationException($"{funcCallStmt.FunctionName} is not a function");
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
                    throw new InvalidOperationException($"CYCLE count must be a number, got: {countValue}");
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
                // Collection-based loops are not yet implemented
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
                    throw new InvalidOperationException($"Invalid NET method path: {netStmt.FullMethodPath}");
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
                    throw new InvalidOperationException($"Type not found: {typeName}");
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
                    MethodInfo[] methods = [.. type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                        .Where(m => m.Name == methodName)];

                    // Find method with matching parameter count
                    method = methods.FirstOrDefault(m => m.GetParameters().Length == args.Length);

                    if (method == null)
                    {
                        throw new InvalidOperationException(
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
                    throw new InvalidOperationException($"Instance methods are not yet supported. Method {methodName} must be static.");
                }
            }
            catch (Exception ex)
            {
                LoggerService.Logger.Error($"✗ NET method call failed: {ex.Message}");
            }
        }

        /// <summary>
        ///     Evaluates an expression to get its runtime value for NET method calls.
        ///     Supports string literals, numeric literals, and identifiers.
        /// </summary>
        private static object? EvaluateExpression(TreeExpression expr)
        {
            return expr switch
            {
                TreeStringLiteralExpression stringExpr => EvaluateStringLiteralForNetCall(stringExpr),
                TreeLiteralExpression literalExpr => EvaluateLiteralForNetCall(literalExpr),
                TreeIdentifierExpression identifierExpr => EvaluateIdentifierForNetCall(identifierExpr),
                TreeBinaryExpression => throw new InvalidOperationException("Binary expressions in NET calls not yet supported"),
                _ => throw new InvalidOperationException($"Unsupported expression type: {expr.GetType().Name}"),
            };
        }

        /// <summary>
        ///     Evaluates a string literal for use in NET method calls.
        /// </summary>
        private static string EvaluateStringLiteralForNetCall(TreeStringLiteralExpression stringExpr)
        {
            return stringExpr.Value.RawToken.Text.Trim('"');
        }

        /// <summary>
        ///     Evaluates a literal expression for use in NET method calls.
        ///     Attempts to parse as int, then double, otherwise returns as string.
        /// </summary>
        private static object EvaluateLiteralForNetCall(TreeLiteralExpression literalExpr)
        {
            string text = literalExpr.Value.RawToken.Text;

            // Try parsing as integer
            if (int.TryParse(text, out int intValue))
            {
                return intValue;
            }

            // Try parsing as double
            if (double.TryParse(text, out double doubleValue))
            {
                return doubleValue;
            }

            // Fall back to string
            return text;
        }

        /// <summary>
        ///     Evaluates an identifier for use in NET method calls.
        ///     Note: Returns the identifier name as a string.
        ///     Full variable lookup would require access to runtime scope.
        /// </summary>
        private static string EvaluateIdentifierForNetCall(TreeIdentifierExpression identifierExpr)
        {
            return identifierExpr.Identifier.RawToken.Text;
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
            List<ParameterExpression> parameters = [.. decl.Parameters.Select(p =>
                SysExpression.Parameter(typeof(int), p.Identifier.RawToken.Text))];

            // Build the lambda expression
            if (returnStmt.ReturnValue == null)
            {
                // Void return
                LoggerService.Logger.Info($"  → Function {funcName} returns VOID");
                System.Linq.Expressions.LambdaExpression voidLambda = SysExpression.Lambda(
                    SysExpression.Empty(),
                    parameters
                );
                _ = voidLambda.Compile();
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
                    ?? throw new InvalidOperationException($"Parameter '{ident.Identifier.RawToken.Text}' not found"),

                TreeBinaryExpression binary =>
                    BuildBinaryExpression(binary, parameters),

                _ => throw new NotImplementedException($"Expression type {expr.GetType().Name} not implemented")
            };
        }

        /// <summary>
        ///     Builds a binary operation expression.
        /// </summary>
        private System.Linq.Expressions.BinaryExpression BuildBinaryExpression(TreeBinaryExpression binary, List<ParameterExpression> parameters)
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
        private static string GetExpressionDescription(TreeExpression expr)
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
        public static void ExecuteScriptFile(string filePath)
        {
            try
            {
                // Import the interpreter namespace here to avoid circular dependencies
                Type? interpreterType = Type.GetType("ppotepa.tokenez.Interpreter.PowerScriptInterpreter, ppotepa.tokenez");
                if (interpreterType == null)
                {
                    throw new InvalidOperationException("PowerScriptInterpreter type not found. Cannot execute external scripts.");
                }

                // Create an instance using the CreateNew static method
                MethodInfo? createNewMethod = interpreterType.GetMethod("CreateNew", BindingFlags.Public | BindingFlags.Static);
                object? interpreter = createNewMethod?.Invoke(null, null);

                if (interpreter == null)
                {
                    throw new InvalidOperationException("Failed to create PowerScriptInterpreter instance.");
                }

                // Call ExecuteFile method
                MethodInfo? executeFileMethod =
                    interpreterType.GetMethod("ExecuteFile", BindingFlags.Public | BindingFlags.Instance);
                _ = executeFileMethod?.Invoke(interpreter, [filePath]);
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
            List<Diagnostic> errors = [.. diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)];
            List<Diagnostic> warnings = [.. diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning)];
            List<Diagnostic> suggestions = [.. diagnostics.Where(d => d.Severity == DiagnosticSeverity.Suggestion)];
            List<Diagnostic> info = [.. diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info)];

            // Display errors first
            if (errors.Count > 0)
            {
                LoggerService.Logger.Error("ERRORS:");
                foreach (Diagnostic? error in errors)
                {
                    LoggerService.Logger.Error($"  {error}");
                }

                LoggerService.Logger.Error("");
            }

            // Then warnings
            if (warnings.Count > 0)
            {
                LoggerService.Logger.Warning("WARNINGS:");
                foreach (Diagnostic? warning in warnings)
                {
                    LoggerService.Logger.Warning($"  {warning}");
                }

                LoggerService.Logger.Warning("");
            }

            // Then suggestions
            if (suggestions.Count > 0)
            {
                LoggerService.Logger.Info("SUGGESTIONS:");
                foreach (Diagnostic? suggestion in suggestions)
                {
                    LoggerService.Logger.Info($"  {suggestion}");
                }

                LoggerService.Logger.Info("");
            }

            // Finally info (only show if verbose or if there are no other diagnostics)
            if (info.Count > 0 && errors.Count + warnings.Count + suggestions.Count == 0)
            {
                LoggerService.Logger.Info("INFO:");
                foreach (Diagnostic? infoItem in info)
                {
                    LoggerService.Logger.Info($"  {infoItem}");
                }

                LoggerService.Logger.Info("");
            }

            // Summary
            LoggerService.Logger.Info(
                $"📊 Analysis complete: {errors.Count} errors, {warnings.Count} warnings, {suggestions.Count} suggestions");
        }
    }
}