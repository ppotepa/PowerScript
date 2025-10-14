using Tokenez.Core.AST;

namespace Tokenez.Compiler.Core;

/// <summary>
/// Shared context for all compiler components during execution.
/// Single Responsibility: Maintains execution state across compiler components
/// </summary>
public class CompilerContext
{
    private readonly Dictionary<string, object> _variables = [];
    private readonly List<string> _callStack = [];
    private int _recursionDepth;

    public CompilerContext(Scope rootScope)
    {
        RootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
    }

    public Scope RootScope { get; }

    public object? LastReturnValue { get; set; }

    public bool HasReturned { get; set; }

    public int RecursionDepth
    {
        get => _recursionDepth;
        set => _recursionDepth = value;
    }

    public int MaxRecursionDepth { get; init; } = 1000;

    public Dictionary<string, object> Variables => _variables;

    public List<string> CallStack => _callStack;

    public void IncrementRecursion(string functionName)
    {
        _recursionDepth++;
        _callStack.Add(functionName);

        if (_recursionDepth >= MaxRecursionDepth)
        {
            throw new InvalidOperationException($"Maximum recursion depth ({MaxRecursionDepth}) exceeded");
        }
    }

    public void DecrementRecursion()
    {
        _recursionDepth--;

        if (_callStack.Count > 0)
        {
            _callStack.RemoveAt(_callStack.Count - 1);
        }
    }

    public void Reset()
    {
        _variables.Clear();
        _callStack.Clear();
        _recursionDepth = 0;
        HasReturned = false;
        LastReturnValue = null;
    }
}
