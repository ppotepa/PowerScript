using Tokenez.Common.Logging;
using Tokenez.Compiler.ControlFlow;
using Tokenez.Compiler.Core;
using Tokenez.Compiler.Integration;
using Tokenez.Compiler.Statements;
using Tokenez.Core.AST.Statements;

namespace Tokenez.Compiler.Statements;

/// <summary>
/// Main orchestrator for statement execution.
/// Single Responsibility: Statement routing and coordination
/// </summary>
public class StatementProcessor
{
    private readonly CompilerContext _context;
    private readonly PrintStatementHandler _printHandler;
    private readonly VariableStatementHandler _variableHandler;
    private readonly ReturnStatementHandler _returnHandler;
    private readonly ExecuteStatementHandler _executeHandler;
    private readonly ArrayAssignmentStatementHandler _arrayAssignmentHandler;
    private readonly IfProcessor _ifProcessor;
    private readonly CycleProcessor _cycleProcessor;
    private readonly NetMethodCallHandler _netMethodCallHandler;

    public StatementProcessor(
        CompilerContext context,
        PrintStatementHandler printHandler,
        VariableStatementHandler variableHandler,
        ReturnStatementHandler returnHandler,
        ExecuteStatementHandler executeHandler,
        ArrayAssignmentStatementHandler arrayAssignmentHandler,
        IfProcessor ifProcessor,
        CycleProcessor cycleProcessor,
        NetMethodCallHandler netMethodCallHandler)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _printHandler = printHandler ?? throw new ArgumentNullException(nameof(printHandler));
        _variableHandler = variableHandler ?? throw new ArgumentNullException(nameof(variableHandler));
        _returnHandler = returnHandler ?? throw new ArgumentNullException(nameof(returnHandler));
        _executeHandler = executeHandler ?? throw new ArgumentNullException(nameof(executeHandler));
        _arrayAssignmentHandler = arrayAssignmentHandler ?? throw new ArgumentNullException(nameof(arrayAssignmentHandler));
        _ifProcessor = ifProcessor ?? throw new ArgumentNullException(nameof(ifProcessor));
        _cycleProcessor = cycleProcessor ?? throw new ArgumentNullException(nameof(cycleProcessor));
        _netMethodCallHandler = netMethodCallHandler ?? throw new ArgumentNullException(nameof(netMethodCallHandler));
    }

    public void ExecuteStatement(Statement statement)
    {
        if (statement == null)
        {
            throw new ArgumentNullException(nameof(statement));
        }

        if (_context.HasReturned)
        {
            LoggerService.Logger.Debug("[EXEC] Skipping statement execution due to return");
            return;
        }

        LoggerService.Logger.Debug($"[EXEC] Executing statement: {statement.GetType().Name}");

        if (statement is PrintStatement printStatement)
        {
            _printHandler.ExecutePrint(printStatement);
            return;
        }

        if (statement is VariableDeclarationStatement variableStatement)
        {
            _variableHandler.ExecuteVariableDeclaration(variableStatement);
            return;
        }

        if (statement is ReturnStatement returnStatement)
        {
            _returnHandler.ExecuteReturn(returnStatement);
            return;
        }

        if (statement is ExecuteStatement executeStatement)
        {
            _executeHandler.ExecuteExternalCommand(executeStatement);
            return;
        }

        if (statement is ArrayAssignmentStatement arrayAssignmentStatement)
        {
            _arrayAssignmentHandler.ExecuteArrayAssignment(arrayAssignmentStatement);
            return;
        }

        if (statement is IfStatement ifStatement)
        {
            _ifProcessor.ExecuteIfStatement(ifStatement);
            return;
        }

        if (statement is CycleLoopStatement cycleStatement)
        {
            _cycleProcessor.ExecuteCycle(cycleStatement);
            return;
        }

        if (statement is NetMethodCallStatement netMethodCallStatement)
        {
            _netMethodCallHandler.ExecuteNetMethodCall(netMethodCallStatement);
            return;
        }

        throw new InvalidOperationException($"Unknown statement type: {statement.GetType().Name}");
    }
}
