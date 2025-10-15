using Tokenez.Common.Logging;
using Tokenez.Compiler.Core;
using Tokenez.Compiler.Core.Variables;
using Tokenez.Core.AST.Statements;

namespace Tokenez.Compiler.ControlFlow;

public class CycleProcessor
{
    private readonly CompilerContext _context;
    private readonly VariableRegistry _variableRegistry;
    private readonly Func<object, object> _evaluateExpression;
    private readonly Action<object> _executeScope;
    private const int MaxIterations = 100000;

    public CycleProcessor(
        CompilerContext context,
        VariableRegistry variableRegistry,
        Func<object, object> evaluateExpression,
        Action<object> executeScope)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _variableRegistry = variableRegistry ?? throw new ArgumentNullException(nameof(variableRegistry));
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
        _executeScope = executeScope ?? throw new ArgumentNullException(nameof(executeScope));
    }

    public void ExecuteCycle(CycleLoopStatement cycleStatement)
    {
        if (cycleStatement == null) throw new ArgumentNullException(nameof(cycleStatement));
        if (cycleStatement.LoopBody == null) return;

        string loopVarName = cycleStatement.LoopVariableName;

        LoggerService.Logger.Debug($"[CYCLE] LoopVariableName from parser: '{loopVarName}'");

        // Ensure we have a valid loop variable name
        if (string.IsNullOrWhiteSpace(loopVarName))
        {
            loopVarName = "A"; // Default loop variable name
            LoggerService.Logger.Debug($"[CYCLE] Using default loop variable name: '{loopVarName}'");
        }

        if (cycleStatement.IsCountBased)
        {
            object countValue = _evaluateExpression(cycleStatement.CollectionExpression);
            int count = Convert.ToInt32(countValue);
            for (int i = 0; i < count; i++)
            {
                _variableRegistry.DeclareOrUpdateVariable(loopVarName, i);
                _executeScope(cycleStatement.LoopBody);
                if (_context.HasReturned) return;
            }
        }
        else
        {
            object collectionValue = _evaluateExpression(cycleStatement.CollectionExpression);
            if (collectionValue is System.Collections.IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    _variableRegistry.DeclareOrUpdateVariable(loopVarName, item);
                    _executeScope(cycleStatement.LoopBody);
                    if (_context.HasReturned) return;
                }
            }
        }
    }
}
