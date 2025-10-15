using Tokenez.Common.Logging;
using Tokenez.Compiler.ControlFlow;
using Tokenez.Compiler.Core;
using Tokenez.Compiler.Core.Variables;
using Tokenez.Compiler.Expressions;
using Tokenez.Compiler.Functions;
using Tokenez.Compiler.Integration;
using Tokenez.Compiler.Interfaces;
using Tokenez.Compiler.Statements;
using Tokenez.Core.AST;
using Tokenez.Core.AST.Expressions;
using Tokenez.Core.AST.Statements;
using Tokenez.Parser.Lexer;

namespace Tokenez.Compiler;

/// <summary>
/// Facade for PowerScript compilation and execution.
/// Orchestrates all compilation components following clean architecture.
/// Single Responsibility: Coordination and lifecycle management
/// </summary>
public class PowerScriptCompiler : IPowerScriptCompiler
{
    private readonly TokenTree _tree;
    private readonly CompilerContext _context;
    private readonly VariableRegistry _variableRegistry;
    private readonly FunctionRegistry _functionRegistry;
    private readonly FunctionCompiler _functionCompiler;
    private readonly FunctionCaller _functionCaller;
    private readonly ExpressionEvaluator _expressionEvaluator;
    private readonly ConditionEvaluator _conditionEvaluator;
    private readonly StatementProcessor _statementProcessor;
    private readonly ScopeProcessor _scopeProcessor;

    public PowerScriptCompiler(TokenTree tree)
    {
        _tree = tree ?? throw new ArgumentNullException(nameof(tree));

        if (tree.RootScope == null)
        {
            throw new ArgumentException("Token tree must have a root scope", nameof(tree));
        }

        // Initialize core components
        _context = new CompilerContext(tree.RootScope);
        _variableRegistry = new VariableRegistry(_context.Variables);
        _functionRegistry = new FunctionRegistry();

        // Initialize function components
        _functionCompiler = new FunctionCompiler(_context, ExecuteScope);

        // Initialize expression evaluator (with function call callback)
        _expressionEvaluator = new ExpressionEvaluator(_variableRegistry, EvaluateFunctionCall);
        _functionCaller = new FunctionCaller(_context, _functionRegistry, _functionCompiler, obj => _expressionEvaluator.Evaluate((Expression)obj));

        // Initialize condition evaluator
        _conditionEvaluator = new ConditionEvaluator(obj => _expressionEvaluator.Evaluate((Expression)obj));

        // Initialize control flow processors
        var ifProcessor = new IfProcessor(_context, _conditionEvaluator.EvaluateCondition, ExecuteScope);
        var cycleProcessor = new CycleProcessor(_context, _variableRegistry, obj => _expressionEvaluator.Evaluate((Expression)obj), ExecuteScope);

        // Initialize statement handlers
        var printHandler = new PrintStatementHandler(obj => _expressionEvaluator.Evaluate((Expression)obj));
        var variableHandler = new VariableStatementHandler(_variableRegistry, obj => _expressionEvaluator.Evaluate((Expression)obj));
        var returnHandler = new ReturnStatementHandler(_context, obj => _expressionEvaluator.Evaluate((Expression)obj));
        var executeHandler = new ExecuteStatementHandler();
        var arrayAssignmentHandler = new ArrayAssignmentStatementHandler(_variableRegistry, obj => _expressionEvaluator.Evaluate((Expression)obj));
        var netMethodCallHandler = new NetMethodCallHandler(obj => _expressionEvaluator.Evaluate((Expression)obj));

        // Initialize statement processor
        _statementProcessor = new StatementProcessor(
            _context,
            printHandler,
            variableHandler,
            returnHandler,
            executeHandler,
            arrayAssignmentHandler,
            ifProcessor,
            cycleProcessor,
            netMethodCallHandler);

        // Initialize scope processor
        _scopeProcessor = new ScopeProcessor();
    }

    /// <summary>
    /// Executes the provided scope and returns the result.
    /// </summary>
    public object? Execute(Scope scope)
    {
        if (scope == null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        LoggerService.Logger.Debug("[COMPILER] Starting scope execution");

        RegisterFunctionsInScope(scope);

        object? result = ExecuteScope(scope);

        LoggerService.Logger.Debug($"[COMPILER] Scope execution completed. Result: {result}");

        return result;
    }

    /// <summary>
    /// Compiles and executes the root scope.
    /// This method is provided for backward compatibility.
    /// </summary>
    public object? CompileAndExecute()
    {
        if (_tree.RootScope == null)
        {
            throw new InvalidOperationException("Cannot execute: token tree has no root scope");
        }

        return Execute(_tree.RootScope);
    }

    /// <summary>
    /// Registers a function in the compiler's function table.
    /// </summary>
    public void RegisterFunction(string functionName, FunctionDeclaration declaration)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            throw new ArgumentException("Function name cannot be null or whitespace", nameof(functionName));
        }

        if (declaration == null)
        {
            throw new ArgumentNullException(nameof(declaration));
        }

        _functionRegistry.RegisterFunction(declaration);
        LoggerService.Logger.Debug($"[COMPILER] Registered function: {functionName}");
    }

    /// <summary>
    /// Checks if a function is registered.
    /// </summary>
    public bool IsFunctionRegistered(string functionName)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            return false;
        }

        return _functionRegistry.IsFunctionDeclared(functionName);
    }

    private void RegisterFunctionsInScope(Scope scope)
    {
        if (scope.Decarations == null)
        {
            return;
        }

        foreach (var declaration in scope.Decarations.Values)
        {
            if (declaration is FunctionDeclaration funcDecl)
            {
                _functionRegistry.RegisterFunction(funcDecl);
            }
        }
    }

    private object? ExecuteScope(Scope scope)
    {
        return _scopeProcessor.ExecuteScope(scope, _context, ExecuteStatement);
    }

    private void ExecuteScope(object scopeObj)
    {
        if (scopeObj is Scope scope)
        {
            ExecuteScope(scope);
        }
    }

    private void ExecuteStatement(Statement statement)
    {
        _statementProcessor.ExecuteStatement(statement);
    }

    private object EvaluateFunctionCall(FunctionCallExpression functionCall)
    {
        return _functionCaller.CallFunction(functionCall);
    }
}
