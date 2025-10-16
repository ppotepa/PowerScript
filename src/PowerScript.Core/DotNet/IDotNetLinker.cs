using PowerScript.Core.AST.Expressions;

namespace PowerScript.Core.DotNet;

/// <summary>
/// Interface for linking .NET member access in PowerScript
/// </summary>
public interface IDotNetLinker
{
    /// <summary>
    /// Registers a .NET member access expression for delayed resolution
    /// </summary>
    /// <param name="expression">The NetMemberAccessExpression to register</param>
    void RegisterMemberAccess(NetMemberAccessExpression expression);
}
