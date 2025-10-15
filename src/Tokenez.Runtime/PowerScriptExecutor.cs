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
    /// Evaluates a binary expression (arithmetic operations).
    /// </summary>
    private object? EvaluateBinaryExpression(BinaryExpression expression)
    {
        var left = EvaluateExpression(expression.Left);
        var right = EvaluateExpression(expression.Right);
        var operatorText = expression.Operator.RawToken?.Text ?? "";

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
}