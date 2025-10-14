using Tokenez.Common.Logging;
using Tokenez.Core.AST.Statements;

namespace Tokenez.Compiler.Statements;

/// <summary>
/// Handles EXECUTE (external command) statement execution.
/// Single Responsibility: External command execution
/// </summary>
public class ExecuteStatementHandler
{
    private readonly Func<object, object> _evaluateExpression;

    public ExecuteStatementHandler(Func<object, object> evaluateExpression)
    {
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public void ExecuteExternalCommand(ExecuteStatement executeStatement)
    {
        if (executeStatement == null)
        {
            throw new ArgumentNullException(nameof(executeStatement));
        }

        string command = EvaluateCommandExpression(executeStatement.CommandExpression);

        LoggerService.Logger.Debug($"[EXEC] EXECUTE: {command}");

        ExecuteCommandInShell(command);
    }

    private string EvaluateCommandExpression(object commandExpression)
    {
        object result = _evaluateExpression(commandExpression);

        return result == null
            ? throw new InvalidOperationException("EXECUTE command evaluated to null")
            : result is string commandString
            ? commandString
            : result.ToString() ?? throw new InvalidOperationException("EXECUTE command cannot be converted to string");
    }

    private static void ExecuteCommandInShell(string command)
    {
        try
        {
            System.Diagnostics.Process process = new()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = GetShellExecutable(),
                    Arguments = GetShellArguments(command),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.Write(output);
            }

            if (!string.IsNullOrWhiteSpace(errors))
            {
                Console.Error.Write(errors);
            }
        }
        catch (Exception ex)
        {
            LoggerService.Logger.Error($"[EXEC] Failed to execute command: {command}. Error: {ex.Message}");
            throw new InvalidOperationException($"Failed to execute command: {command}", ex);
        }
    }

    private static string GetShellExecutable()
    {
        return OperatingSystem.IsWindows()
            ? "cmd.exe"
            : OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()
            ? "/bin/bash"
            : throw new PlatformNotSupportedException("Unsupported operating system for EXECUTE command");
    }

    private static string GetShellArguments(string command)
    {
        return OperatingSystem.IsWindows()
            ? $"/c {command}"
            : OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()
            ? $"-c \"{command}\""
            : throw new PlatformNotSupportedException("Unsupported operating system for EXECUTE command");
    }
}
