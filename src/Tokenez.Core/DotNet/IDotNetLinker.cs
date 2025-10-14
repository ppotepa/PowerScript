using System.Reflection;

namespace Tokenez.Core.DotNet;

/// <summary>
///     Interface for linking .NET assemblies and resolving types.
/// </summary>
public interface IDotNetLinker
{
    /// <summary>
    ///     Links a .NET namespace by loading its assembly.
    /// </summary>
    /// <param name="namespaceName">The namespace to link (e.g., "System.IO")</param>
    void LinkNamespace(string namespaceName);

    /// <summary>
    ///     Resolves a type name to a .NET Type, using linked namespaces.
    /// </summary>
    /// <param name="typeName">The type name to resolve</param>
    /// <returns>The resolved Type, or null if not found</returns>
    Type? ResolveType(string typeName);

    /// <summary>
    ///     Gets all linked assemblies.
    /// </summary>
    /// <returns>Collection of linked assemblies</returns>
    IReadOnlyCollection<Assembly> GetLinkedAssemblies();
}