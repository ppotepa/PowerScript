using Tokenez.Common.Logging;
using Tokenez.Core.AST.Statements;

namespace Tokenez.Compiler.Statements;

public class ExecuteStatementHandler
{
    public void ExecuteExternalCommand(ExecuteStatement executeStatement)
    {
        if (executeStatement == null) throw new ArgumentNullException(nameof(executeStatement));
        
        string filePath = executeStatement.FilePath;
        LoggerService.Logger.Debug($"[EXEC] EXECUTE: {filePath}");
        
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException($"Script file not found: {filePath}");
        }
        
        LoggerService.Logger.Warning("EXECUTE statement support not yet fully implemented");
    }
}
