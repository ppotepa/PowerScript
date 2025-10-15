using System.Collections;
using System.Diagnostics;
using Tokenez.Common.Logging;
using Tokenez.Compiler.Models;
using Tokenez.Core.AST;
using Tokenez.Core.AST.Expressions;
using Tokenez.Core.AST.Statements;
using Tokenez.Runtime.Core;
using Tokenez.Runtime.Interfaces;
using Tokenez.Runtime.Models;

namespace Tokenez.Runtime;

/// <summary>
/// PowerScript executor that runs compiled PowerScript artifacts.
/// This class is responsible ONLY for execution - not compilation.
/// </summary>
public class PowerScriptExecutor : IPowerScriptExecutor
{
    private readonly Core.ExecutionContext _context = new();
    private readonly List<string> _linkedLibraries = new();
    private bool _hasReturned; // Tracks if a RETURN statement was executed

    /// <summary>
    /// Gets the current execution context.
    /// </summary>
    public IExecutionContext GetExecutionContext() => _context;

    /// <summary>
    /// Links a library to be available during execution.
    /// </summary>
    public void LinkLibrary(string libraryPath)
    {
        if (string.IsNullOrWhiteSpace(libraryPath))
        {
            throw new ArgumentException("Library path cannot be null or whitespace", nameof(libraryPath));
        }

        if (!File.Exists(libraryPath))
        {
            throw new FileNotFoundException($"Library file not found: {libraryPath}");
        }

        _linkedLibraries.Add(libraryPath);
        LoggerService.Logger.Debug($"[EXECUTOR] Linked library: {libraryPath}");
    }

    /// <summary>
    /// Executes a compiled PowerScript program.
    /// </summary>
    public ExecutionResult Execute(CompilationResult compilationResult)
    {
        if (compilationResult == null)
        {
            throw new ArgumentNullException(nameof(compilationResult));
        }

        if (!compilationResult.IsSuccess)
        {
            return ExecutionResult.Failed(
                new[] { "Cannot execute failed compilation result" }.Concat(compilationResult.Errors).ToList());
        }

        var startTime = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            LoggerService.Logger.Debug("[EXECUTOR] Starting execution");

            // Register functions from compilation result
            RegisterFunctions(compilationResult);

            // Execute the compiled artifacts
            // For now, return a placeholder result - full execution logic will be implemented
            // based on the separated domains architecture
            var result = ExecuteCompiledArtifacts(compilationResult);

            stopwatch.Stop();

            var metadata = new ExecutionMetadata(
                startTime,
                stopwatch.Elapsed);

            LoggerService.Logger.Debug($"[EXECUTOR] Execution completed successfully in {stopwatch.ElapsedMilliseconds}ms");

            return ExecutionResult.Success(result, metadata);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LoggerService.Logger.Error($"[EXECUTOR] Execution failed: {ex.Message}");

            var metadata = new ExecutionMetadata(
                startTime,
                stopwatch.Elapsed);

            return ExecutionResult.Failed(ex.Message, metadata);
        }
    }

    private void RegisterFunctions(CompilationResult compilationResult)
    {
        foreach (var function in compilationResult.Functions)
        {
            _context.RegisterFunction(function.Key, function.Value);
            LoggerService.Logger.Debug($"[EXECUTOR] Registered function: {function.Key}");
        }
    }

    private object? ExecuteCompiledArtifacts(CompilationResult compilationResult)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Executing compiled artifacts with {compilationResult.Functions.Count} functions");
        Console.WriteLine($"[DEBUG] Starting execution of {compilationResult.RootScope.Statements.Count} statements");

        // Execute statements in the root scope
        return ExecuteScope(compilationResult.RootScope);
    }

    /// <summary>
    /// Executes all statements in a scope.
    /// </summary>
    private object? ExecuteScope(Scope scope)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Executing scope with {scope.Statements.Count} statements");
        Console.WriteLine($"[DEBUG] Executing scope with {scope.Statements.Count} statements");

        object? lastResult = null;

        foreach (var statement in scope.Statements)
        {
            Console.WriteLine($"[DEBUG] Executing statement: {statement.GetType().Name} - {statement.StatementType}");
            lastResult = ExecuteStatement(statement);

            // If this is a return statement, stop execution and return the value
            if (statement is ReturnStatement)
            {
                _hasReturned = true;
                break;
            }

            // If a return was executed in a nested scope (like inside an IF), stop execution
            if (_hasReturned)
            {
                break;
            }
        }

        return lastResult;
    }

    /// <summary>
    /// Executes a single statement.
    /// </summary>
    private object? ExecuteStatement(Statement statement)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Executing statement: {statement.StatementType}");

        return statement switch
        {
            PrintStatement printStmt => ExecutePrintStatement(printStmt),
            VariableDeclarationStatement varStmt => ExecuteVariableDeclaration(varStmt),
            ReturnStatement returnStmt => ExecuteReturnStatement(returnStmt),
            IfStatement ifStmt => ExecuteIfStatement(ifStmt),
            CycleLoopStatement cycleStmt => ExecuteCycleLoopStatement(cycleStmt),
            ArrayAssignmentStatement arrayAssignStmt => ExecuteArrayAssignment(arrayAssignStmt),
            _ => throw new NotSupportedException($"Statement type {statement.StatementType} is not yet supported")
        };
    }

    /// <summary>
    /// Executes a PRINT statement.
    /// </summary>
    private object? ExecutePrintStatement(PrintStatement statement)
    {
        var value = EvaluateExpression(statement.Expression);
        var output = value?.ToString() ?? "";

        Console.WriteLine(output);
        LoggerService.Logger.Debug($"[EXECUTOR] Printed: {output}");

        return null;
    }

    /// <summary>
    /// Executes a variable declaration statement (VAR or FLEX).
    /// </summary>
    private object? ExecuteVariableDeclaration(VariableDeclarationStatement statement)
    {
        var variableName = statement.Declaration.Identifier.RawToken?.Text ??
                          throw new InvalidOperationException("Variable declaration missing identifier");

        var value = EvaluateExpression(statement.InitialValue);

        _context.SetVariable(variableName, value);
        LoggerService.Logger.Debug($"[EXECUTOR] Set variable {variableName} = {value}");

        return value;
    }

    /// <summary>
    /// Executes an array assignment statement (arr[index] = value).
    /// </summary>
    private object? ExecuteArrayAssignment(ArrayAssignmentStatement statement)
    {
        // Evaluate the value to assign
        var value = EvaluateExpression(statement.ValueExpression);

        // Get the array and index
        var arrayValue = EvaluateExpression(statement.IndexExpression.ArrayExpression);
        var indexValue = EvaluateExpression(statement.IndexExpression.Index);

        if (arrayValue is not IList list)
        {
            throw new InvalidOperationException($"Cannot assign to index of non-array type: {arrayValue?.GetType().Name ?? "null"}");
        }

        if (indexValue is not int index)
        {
            throw new InvalidOperationException($"Array index must be an integer, got: {indexValue?.GetType().Name ?? "null"}");
        }

        if (index < 0 || index >= list.Count)
        {
            throw new IndexOutOfRangeException($"Array index {index} is out of range (array size: {list.Count})");
        }

        list[index] = value;
        LoggerService.Logger.Debug($"[EXECUTOR] Array assignment: [{index}] = {value}");

        return value;
    }

    /// <summary>
    /// Executes a RETURN statement.
    /// </summary>
    private object? ExecuteReturnStatement(ReturnStatement statement)
    {
        var value = statement.ReturnValue != null ? EvaluateExpression(statement.ReturnValue) : null;
        LoggerService.Logger.Debug($"[EXECUTOR] Return: {value}");
        return value;
    }

    /// <summary>
    /// Evaluates an expression and returns its value.
    /// </summary>
    private object? EvaluateExpression(Expression expression)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Evaluating expression: {expression.ExpressionType}, Type: {expression.GetType().Name}");

        return expression switch
        {
            StringLiteralExpression stringExpr => EvaluateStringLiteral(stringExpr),
            LiteralExpression literalExpr => EvaluateLiteral(literalExpr),
            IdentifierExpression identifierExpr => EvaluateIdentifier(identifierExpr),
            BinaryExpression binaryExpr => EvaluateBinaryExpression(binaryExpr),
            LogicalExpression logicalExpr => EvaluateLogicalExpression(logicalExpr),
            ArrayLiteralExpression arrayExpr => EvaluateArrayLiteral(arrayExpr),
            IndexExpression indexExpr => EvaluateIndexExpression(indexExpr),
            TemplateStringExpression templateExpr => EvaluateTemplateString(templateExpr),
            FunctionCallExpression functionCallExpr => EvaluateFunctionCall(functionCallExpr),
            _ => throw new NotSupportedException($"Expression type {expression.ExpressionType} ({expression.GetType().Name}) is not yet supported")
        };
    }

    /// <summary>
    /// Evaluates a string literal expression.
    /// </summary>
    private string EvaluateStringLiteral(StringLiteralExpression expression)
    {
        // Remove quotes from string literal
        var text = expression.Value.RawToken?.Text ?? "";
        if (text.StartsWith('"') && text.EndsWith('"'))
        {
            text = text[1..^1];
        }
        return text;
    }

    /// <summary>
    /// Evaluates a template string expression with variable interpolation.
    /// </summary>
    private string EvaluateTemplateString(TemplateStringExpression expression)
    {
        var result = new System.Text.StringBuilder();

        foreach (var part in expression.Template.Parts)
        {
            if (part.IsLiteral)
            {
                // Add literal text as-is
                result.Append(part.Text);
            }
            else
            {
                // Interpolate variable
                var varName = part.Text;
                if (_context.HasVariable(varName))
                {
                    var value = _context.GetVariable(varName);
                    result.Append(value?.ToString() ?? "");
                }
                else
                {
                    throw new InvalidOperationException($"Variable '{varName}' is not defined in template string");
                }
            }
        }

        var finalString = result.ToString();
        LoggerService.Logger.Debug($"[EXECUTOR] Template string evaluated: {finalString}");
        return finalString;
    }

    /// <summary>
    /// Evaluates a literal expression (numbers, etc.).
    /// </summary>
    private object? EvaluateLiteral(LiteralExpression expression)
    {
        var text = expression.Value.RawToken?.Text ?? "";

        // Try to parse as integer first
        if (int.TryParse(text, out var intValue))
        {
            return intValue;
        }

        // Try to parse as double
        if (double.TryParse(text, out var doubleValue))
        {
            return doubleValue;
        }

        // Default to string
        return text;
    }

    /// <summary>
    /// Evaluates an identifier expression (variable reference).
    /// </summary>
    private object? EvaluateIdentifier(IdentifierExpression expression)
    {
        var variableName = expression.Identifier.RawToken?.Text ??
                          throw new InvalidOperationException("Identifier expression missing name");

        LoggerService.Logger.Debug($"[EXECUTOR] Looking for variable: '{variableName}'");

        if (!_context.HasVariable(variableName))
        {
            throw new InvalidOperationException($"Variable '{variableName}' is not defined");
        }

        return _context.GetVariable(variableName);
    }

    /// <summary>
    /// Evaluates a function call expression.
    /// </summary>
    private object? EvaluateFunctionCall(FunctionCallExpression expression)
    {
        var functionName = expression.FunctionName.RawToken?.Text ??
                          throw new InvalidOperationException("Function call expression missing name");

        LoggerService.Logger.Debug($"[EXECUTOR] Calling function: '{functionName}'");

        // Get the function from registered functions
        var function = _context.GetFunction(functionName) as FunctionDeclaration;
        if (function == null)
        {
            throw new InvalidOperationException($"Function '{functionName}' is not defined");
        }

        // Evaluate all arguments
        var arguments = new List<object?>();
        foreach (var argExpr in expression.Arguments)
        {
            var argValue = EvaluateExpression(argExpr);
            arguments.Add(argValue);
            LoggerService.Logger.Debug($"[EXECUTOR] Argument value: {argValue}");
        }

        // Save current variables (simple scope isolation)
        var savedVariables = new Dictionary<string, object?>(_context.Variables);
        bool savedHasReturned = _hasReturned; // Save the return flag from calling scope
        object? returnValue = null;

        try
        {
            // Reset return flag for this function call
            _hasReturned = false;

            // Bind parameters to arguments
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var param = function.Parameters[i];
                var paramName = param.Identifier.RawToken?.Text ?? throw new InvalidOperationException("Parameter missing name");
                var argValue = i < arguments.Count ? arguments[i] : null;

                _context.SetVariable(paramName, argValue);
                LoggerService.Logger.Debug($"[EXECUTOR] Bound parameter '{paramName}' = {argValue}");
            }

            // Execute the function body
            returnValue = ExecuteScope(function.Scope);

            LoggerService.Logger.Debug($"[EXECUTOR] Function '{functionName}' returned: {returnValue}");
        }
        finally
        {
            // Restore previous variables (simple scope cleanup)
            foreach (var kvp in savedVariables)
            {
                _context.SetVariable(kvp.Key, kvp.Value);
            }

            // Restore the return flag from calling scope
            _hasReturned = savedHasReturned;
        }

        return returnValue;
    }

    /// <summary>
    /// Evaluates an array literal expression.
    /// </summary>
    private object? EvaluateArrayLiteral(ArrayLiteralExpression expression)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Evaluating array literal with {expression.Elements.Count} elements");

        var result = new List<object?>();
        foreach (var element in expression.Elements)
        {
            var value = EvaluateExpression(element);
            result.Add(value);
        }

        return result;
    }

    /// <summary>
    /// Evaluates an index expression (array access).
    /// </summary>
    private object? EvaluateIndexExpression(IndexExpression expression)
    {
        // Evaluate the array expression
        var arrayValue = EvaluateExpression(expression.ArrayExpression);

        // Evaluate the index
        var indexValue = EvaluateExpression(expression.Index);

        if (arrayValue is not IList list)
        {
            throw new InvalidOperationException($"Cannot index non-array type: {arrayValue?.GetType().Name ?? "null"}");
        }

        if (indexValue is not int index)
        {
            throw new InvalidOperationException($"Array index must be an integer, got: {indexValue?.GetType().Name ?? "null"}");
        }

        if (index < 0 || index >= list.Count)
        {
            throw new IndexOutOfRangeException($"Array index {index} is out of range (array size: {list.Count})");
        }

        LoggerService.Logger.Debug($"[EXECUTOR] Array access: [{index}] = {list[index]}");
        return list[index];
    }

    /// <summary>
    /// Evaluates a binary expression (arithmetic and comparison operations).
    /// </summary>
    private object? EvaluateBinaryExpression(BinaryExpression expression)
    {
        var left = EvaluateExpression(expression.Left);
        var right = EvaluateExpression(expression.Right);
        var operatorText = expression.Operator.RawToken?.Text ?? "";

        // Handle comparison operations first
        if (IsComparisonOperator(operatorText))
        {
            return EvaluateComparison(left, right, operatorText);
        }

        // Handle arithmetic operations
        if (left is int leftInt && right is int rightInt)
        {
            return operatorText switch
            {
                "+" => leftInt + rightInt,
                "-" => leftInt - rightInt,
                "*" => leftInt * rightInt,
                "/" => rightInt != 0 ? leftInt / rightInt : throw new DivideByZeroException(),
                "%" => rightInt != 0 ? leftInt % rightInt : throw new DivideByZeroException(),
                _ => throw new NotSupportedException($"Binary operator {operatorText} is not yet supported")
            };
        }

        // Handle string concatenation
        if (operatorText == "+")
        {
            return left?.ToString() + right?.ToString();
        }

        throw new NotSupportedException($"Binary operation {operatorText} between {left?.GetType()} and {right?.GetType()} is not yet supported");
    }

    /// <summary>
    /// Evaluates a logical expression (AND, OR operations).
    /// </summary>
    private object? EvaluateLogicalExpression(LogicalExpression expression)
    {
        var operatorText = expression.Operator.RawToken?.Text ?? "";

        // Handle short-circuit evaluation
        var leftResult = EvaluateExpression(expression.Left);
        bool leftIsTrue = IsConditionTrue(leftResult);

        if (operatorText == "AND")
        {
            if (!leftIsTrue) return false; // Short-circuit: false AND anything = false
            var rightResult = EvaluateExpression(expression.Right);
            return IsConditionTrue(rightResult);
        }

        if (operatorText == "OR")
        {
            if (leftIsTrue) return true; // Short-circuit: true OR anything = true
            var rightResult = EvaluateExpression(expression.Right);
            return IsConditionTrue(rightResult);
        }

        throw new NotSupportedException($"Logical operator {operatorText} is not yet supported");
    }

    /// <summary>
    /// Checks if an operator is a comparison operator.
    /// </summary>
    private bool IsComparisonOperator(string operatorText)
    {
        return operatorText is ">" or "<" or ">=" or "<=" or "==" or "!=";
    }

    /// <summary>
    /// Evaluates a comparison operation.
    /// </summary>
    private bool EvaluateComparison(object? left, object? right, string operatorText)
    {
        // Convert booleans to integers for comparison (true = 1, false = 0)
        if (left is bool leftBool)
        {
            left = leftBool ? 1 : 0;
        }
        if (right is bool rightBool)
        {
            right = rightBool ? 1 : 0;
        }

        // Handle integer comparisons
        if (left is int leftInt && right is int rightInt)
        {
            return operatorText switch
            {
                ">" => leftInt > rightInt,
                "<" => leftInt < rightInt,
                ">=" => leftInt >= rightInt,
                "<=" => leftInt <= rightInt,
                "==" => leftInt == rightInt,
                "!=" => leftInt != rightInt,
                _ => throw new NotSupportedException($"Comparison operator {operatorText} is not yet supported")
            };
        }

        // Handle string comparisons
        if (left is string leftStr && right is string rightStr)
        {
            return operatorText switch
            {
                "==" => leftStr == rightStr,
                "!=" => leftStr != rightStr,
                _ => throw new NotSupportedException($"String comparison operator {operatorText} is not yet supported")
            };
        }

        // Handle null comparisons
        return operatorText switch
        {
            "==" => Equals(left, right),
            "!=" => !Equals(left, right),
            _ => throw new NotSupportedException($"Comparison {operatorText} between {left?.GetType()} and {right?.GetType()} is not yet supported")
        };
    }

    /// <summary>
    /// Executes an IF statement with optional ELSE clause.
    /// </summary>
    private object? ExecuteIfStatement(IfStatement statement)
    {
        var conditionResult = EvaluateExpression(statement.Condition);
        bool isTrue = IsConditionTrue(conditionResult);

        LoggerService.Logger.Debug($"[EXECUTOR] IF condition evaluated to: {isTrue}");

        if (isTrue)
        {
            return ExecuteScope(statement.ThenScope);
        }
        else if (statement.ElseScope != null)
        {
            return ExecuteScope(statement.ElseScope);
        }

        return null;
    }

    /// <summary>
    /// Executes a CYCLE loop statement.
    /// </summary>
    private object? ExecuteCycleLoopStatement(CycleLoopStatement statement)
    {
        var collectionValue = EvaluateExpression(statement.CollectionExpression);
        LoggerService.Logger.Debug($"[EXECUTOR] CYCLE collection evaluated to: {collectionValue}");

        // Handle count-based loops (CYCLE 5)
        if (collectionValue is int count)
        {
            return ExecuteCountBasedLoop(statement, count);
        }

        // TODO: Handle collection-based loops (CYCLE IN collection)
        throw new NotSupportedException("Collection-based CYCLE loops are not yet implemented");
    }

    /// <summary>
    /// Executes a count-based CYCLE loop.
    /// </summary>
    private object? ExecuteCountBasedLoop(CycleLoopStatement statement, int count)
    {
        object? lastResult = null;

        for (int i = 0; i < count; i++)
        {
            // Set the loop variable value
            _context.SetVariable(statement.LoopVariableName, i);
            LoggerService.Logger.Debug($"[EXECUTOR] CYCLE iteration {i}, {statement.LoopVariableName} = {i}");

            // Execute the loop body
            if (statement.LoopBody != null)
            {
                lastResult = ExecuteScope(statement.LoopBody);
            }
        }

        return lastResult;
    }

    /// <summary>
    /// Determines if a condition value should be considered true.
    /// </summary>
    private bool IsConditionTrue(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            int i => i != 0,
            double d => d != 0.0,
            string s => !string.IsNullOrEmpty(s),
            _ => true
        };
    }
}