using Tokenez.Common.Logging;
using Tokenez.Compiler.Core;
using Tokenez.Core.AST.Statements;

namespace Tokenez.Compiler.ControlFlow;

/// <summary>
/// Evaluates conditional expressions and executes IF/ELSE branches.
/// Single Responsibility: IF statement processing
/// </summary>
public class IfProcessor
{
    private readonly CompilerContext _context;
    private readonly Func<object, bool> _evaluateCondition;
    private readonly Action<object> _executeScope;

    public IfProcessor(
        CompilerContext context,
        Func<object, bool> evaluateCondition,
        Action<object> executeScope)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _evaluateCondition = evaluateCondition ?? throw new ArgumentNullException(nameof(evaluateCondition));
        _executeScope = executeScope ?? throw new ArgumentNullException(nameof(executeScope));
    }

    public void ExecuteIfStatement(IfStatement ifStatement)
    {
        if (ifStatement == null)
        {
            throw new ArgumentNullException(nameof(ifStatement));
        }

        bool conditionResult = _evaluateCondition(ifStatement.Condition);

        LoggerService.Logger.Debug($"[EXEC] IF condition evaluated to: {conditionResult}");

        if (conditionResult)
        {
            LoggerService.Logger.Debug("[EXEC] Executing IF branch");
            _executeScope(ifStatement.ThenScope);
            return;
        }

        if (ifStatement.ElseScope != null)
        {
            LoggerService.Logger.Debug("[EXEC] Executing ELSE branch");
            _executeScope(ifStatement.ElseScope);
        }
    }
}
