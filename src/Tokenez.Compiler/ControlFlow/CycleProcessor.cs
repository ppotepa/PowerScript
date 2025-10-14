using Tokenez.Common.Logging;
using Tokenez.Compiler.Core;

namespace Tokenez.Compiler.ControlFlow;

/// <summary>
/// Executes loop (CYCLE) statements.
/// Single Responsibility: Loop processing
/// </summary>
public class CycleProcessor
{
    private readonly CompilerContext _context;
    private readonly Func<object, bool> _evaluateCondition;
    private readonly Action<object> _executeScope;
    private const int MaxIterations = 100000;

    public CycleProcessor(
        CompilerContext context,
        Func<object, bool> evaluateCondition,
        Action<object> executeScope)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _evaluateCondition = evaluateCondition ?? throw new ArgumentNullException(nameof(evaluateCondition));
        _executeScope = executeScope ?? throw new ArgumentNullException(nameof(executeScope));
    }

    public void ExecuteCycle(CycleStatement cycleStatement)
    {
        if (cycleStatement == null)
        {
            throw new ArgumentNullException(nameof(cycleStatement));
        }

        LoggerService.Logger.Debug("[EXEC] Starting CYCLE loop");

        int iterationCount = 0;

        while (true)
        {
            ValidateIterationCount(iterationCount);

            bool conditionResult = _evaluateCondition(cycleStatement.Condition);

            if (!conditionResult)
            {
                LoggerService.Logger.Debug($"[EXEC] CYCLE condition false after {iterationCount} iterations");
                break;
            }

            LoggerService.Logger.Debug($"[EXEC] CYCLE iteration {iterationCount + 1}");

            _executeScope(cycleStatement.BodyScope);

            if (_context.HasReturned)
            {
                LoggerService.Logger.Debug($"[EXEC] CYCLE terminated by RETURN after {iterationCount + 1} iterations");
                break;
            }

            iterationCount++;
        }

        LoggerService.Logger.Debug($"[EXEC] CYCLE completed with {iterationCount} iterations");
    }

    private static void ValidateIterationCount(int iterationCount)
    {
        if (iterationCount >= MaxIterations)
        {
            throw new InvalidOperationException(
                $"Infinite loop detected: CYCLE exceeded maximum iteration count of {MaxIterations}");
        }
    }
}
