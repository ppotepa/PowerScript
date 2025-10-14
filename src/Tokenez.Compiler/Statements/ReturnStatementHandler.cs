using Tokenez.Common.Logging;
using Tokenez.Compiler.Core;
using Tokenez.Core.AST.Statements;

namespace Tokenez.Compiler.Statements;

/// <summary>
/// Handles RETURN statement execution.
/// Single Responsibility: Return statement processing
/// </summary>
public class ReturnStatementHandler
{
    private readonly CompilerContext _context;
    private readonly Func<object, object> _evaluateExpression;

    public ReturnStatementHandler(CompilerContext context, Func<object, object> evaluateExpression)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public void ExecuteReturn(ReturnStatement returnStatement)
    {
        if (returnStatement == null)
        {
            throw new ArgumentNullException(nameof(returnStatement));
        }

        object returnValue = _evaluateExpression(returnStatement.Expression);

        _context.LastReturnValue = returnValue;
        _context.HasReturned = true;

        LoggerService.Logger.Debug($"[EXEC] RETURN: {returnValue}");
    }
}
