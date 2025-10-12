namespace ppotepa.tokenez.Tree.Builders;

/// <summary>
///     Handles logging for the tree building process with depth-based indentation
/// </summary>
internal static class BuilderLogger
{
    public static void LogScopeStart(string scopeName, int depth)
    {
        string indent = GetIndent(depth + 1);
        Console.WriteLine($"{indent}CreateScope: {scopeName} (depth={depth})");
    }

    public static void LogScopeComplete(string scopeName, int depth)
    {
        string indent = GetIndent(depth + 1);
        Console.WriteLine($"{indent}Scope complete: {scopeName}");
    }

    public static void LogProcessing(string tokenType, string tokenText, int depth)
    {
        string indent = GetIndent(depth + 1);
        Console.WriteLine($"{indent}\tProcessing: {tokenType} '{tokenText}'");
    }

    public static void LogFunctionFound(int depth)
    {
        string indent = GetIndent(depth + 1);
        Console.WriteLine($"{indent}\t\tFound Function");
    }

    public static void LogFunctionName(string name, int depth)
    {
        string indent = GetIndent(depth + 1);
        Console.WriteLine($"{indent}\t\t\tFunction name: {name}");
    }

    public static void LogParametersStarting(int depth)
    {
        string indent = GetIndent(depth + 1);
        Console.WriteLine($"{indent}\t\t\tParameters starting");
    }

    private static string GetIndent(int depth)
    {
        return new string('\u0009', depth);
    }
}