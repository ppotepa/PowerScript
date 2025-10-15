namespace PowerScript.Core.AST;

/// <summary>
///     Defines the type of scope.
/// </summary>
public enum ScopeType
{
    Root, // The root/global scope
    Function, // A function scope (requires RETURN statement)
    Block // A generic block scope (if/while/etc - future use)
}