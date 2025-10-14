using Tokenez.Common.Logging;
using Tokenez.Core.AST.Statements;

namespace Tokenez.Compiler.Statements;

/// <summary>
/// Handles PRINT statement execution.
/// Single Responsibility: Print statement processing
/// </summary>
public class PrintStatementHandler
{
    private readonly Func<object, object> _evaluateExpression;

    public PrintStatementHandler(Func<object, object> evaluateExpression)
    {
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public void ExecutePrint(PrintStatement printStatement)
    {
        if (printStatement == null)
        {
            throw new ArgumentNullException(nameof(printStatement));
        }

        object value = _evaluateExpression(printStatement.Expression);
        string output = ConvertToString(value);

        Console.WriteLine(output);
        LoggerService.Logger.Debug($"[EXEC] PRINT: {output}");
    }

    private static string ConvertToString(object value)
    {
        return value == null ? string.Empty : value is string stringValue ? stringValue : value.ToString() ?? string.Empty;
    }
}
