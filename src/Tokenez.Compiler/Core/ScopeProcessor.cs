using Tokenez.Core.AST;

namespace Tokenez.Compiler.Core;

/// <summary>
/// Executes scope statements sequentially.
/// Single Responsibility: Scope execution orchestration
/// </summary>
public class ScopeProcessor
{
    public object? ExecuteScope(Scope scope, CompilerContext context, Action<Statement> statementExecutor)
    {
        if (scope == null)
        {
            throw new ArgumentNullException(nameof(scope));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (statementExecutor == null)
        {
            throw new ArgumentNullException(nameof(statementExecutor));
        }

        object? result = null;

        foreach (Statement statement in scope.Statements)
        {
            if (context.HasReturned)
            {
                break;
            }

            statementExecutor(statement);

            if (context.HasReturned)
            {
                result = context.LastReturnValue;
                break;
            }
        }

        return result;
    }
}
