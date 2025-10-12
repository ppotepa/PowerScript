#nullable enable
using ppotepa.tokenez.Logging;
using System.Reflection;

namespace ppotepa.tokenez.DotNet;

/// <summary>
///     Manages .NET assembly loading and type resolution for PowerScript.
///     Supports linking namespaces via LINK statements and resolving types
///     with or without full namespace paths.
/// </summary>
public class DotNetLinker : IDotNetLinker
{
    private readonly HashSet<string> _linkedNamespaces = [];
    private readonly Dictionary<string, Assembly> _loadedAssemblies = [];
    private readonly Dictionary<string, Type?> _typeCache = [];

    /// <summary>
    ///     Gets all linked namespaces
    /// </summary>
    public IReadOnlySet<string> LinkedNamespaces => _linkedNamespaces;

    /// <summary>
    ///     Links a .NET namespace, making its types available for direct reference.
    ///     Example: After LinkNamespace("System.Collections.Generic"),
    ///     you can use "List" instead of "System.Collections.Generic.List"
    /// </summary>
    public void LinkNamespace(string namespacePath)
    {
        LoggerService.Logger.Info($"Linking namespace: {namespacePath}");

        _linkedNamespaces.Add(namespacePath);

        // Try to load the corresponding assembly
        string assemblyName = GetAssemblyNameFromNamespace(namespacePath);
        if (!_loadedAssemblies.ContainsKey(assemblyName))
        {
            try
            {
                Assembly assembly = Assembly.Load(assemblyName);
                _loadedAssemblies[assemblyName] = assembly;

                LoggerService.Logger.Success($"Loaded assembly: {assemblyName}");
            }
            catch (Exception ex)
            {
                // Try loading from AppDomain as fallback
                Assembly? existingAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == assemblyName);

                if (existingAssembly != null)
                {
                    _loadedAssemblies[assemblyName] = existingAssembly;
                    LoggerService.Logger.Success($"Found assembly in AppDomain: {assemblyName}");
                }
                else
                {
                    LoggerService.Logger.Warning($"Could not load assembly '{assemblyName}': {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    ///     Resolves a type name to a .NET Type.
    ///     Supports:
    ///     - Full paths with :: (System::Collections::Generic::List)
    ///     - Full paths with dots (System.Collections.Generic.List)
    ///     - Short names if namespace is linked (List, if System.Collections.Generic is linked)
    ///     - PowerScript basic types (INT, PREC, STRING, CHAR)
    /// </summary>
    public Type? ResolveType(string typeName)
    {
        LoggerService.Logger.Debug($"Resolving type: {typeName}");

        // Case 1: Type name includes namespace operator (System::Collections::Generic::List)
        if (typeName.Contains("::"))
        {
            string dotNotation = typeName.Replace("::", ".");
            Type? type = ResolveFullyQualifiedType(dotNotation);
            if (type != null)
            {
                return type;
            }
        }

        // Case 2: Check if it's a basic PowerScript type
        if (IsBasicPowerScriptType(typeName))
        {
            Type? type = MapPowerScriptTypeToDotNet(typeName);
            LoggerService.Logger.Debug($"Resolved PowerScript type '{typeName}' to .NET type '{type?.FullName}'");
            return type;
        }

        // Case 3: Check for direct type in linked namespaces
        foreach (string ns in _linkedNamespaces)
        {
            string fullName = $"{ns}.{typeName}";
            Type? type = ResolveFullyQualifiedType(fullName);
            if (type != null)
            {
                LoggerService.Logger.Debug($"Resolved '{typeName}' to '{type.FullName}' via linked namespace '{ns}'");
                return type;
            }
        }

        // Case 4: Try as fully qualified type name
        Type? directType = ResolveFullyQualifiedType(typeName);
        if (directType != null)
        {
            return directType;
        }

        LoggerService.Logger.Warning($"Could not resolve type: {typeName}");
        return null;
    }

    /// <summary>
    ///     Resolves a fully qualified type name (with dots)
    /// </summary>
    private Type? ResolveFullyQualifiedType(string fullTypeName)
    {
        // Check cache first
        if (_typeCache.TryGetValue(fullTypeName, out Type? cachedType))
        {
            return cachedType;
        }

        // Search in all loaded assemblies
        foreach (Assembly assembly in _loadedAssemblies.Values)
        {
            Type? type = assembly.GetType(fullTypeName, false, true); // Case insensitive
            if (type != null)
            {
                _typeCache[fullTypeName] = type;
                return type;
            }
        }

        // Try to find in default assemblies (system assemblies)
        Type? systemType = Type.GetType(fullTypeName, false, true);
        if (systemType != null)
        {
            _typeCache[fullTypeName] = systemType;
            return systemType;
        }

        // Search in all loaded assemblies in AppDomain
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? type = assembly.GetType(fullTypeName, false, true);
            if (type != null)
            {
                _typeCache[fullTypeName] = type;
                return type;
            }
        }

        return null;
    }

    /// <summary>
    ///     Gets all linked assemblies.
    /// </summary>
    public IReadOnlyCollection<Assembly> GetLinkedAssemblies()
    {
        return _loadedAssemblies.Values.ToList().AsReadOnly();
    }

    /// <summary>
    ///     Extracts the assembly name from a namespace path.
    ///     Example: "System.Collections.Generic" -> "System.Collections"
    /// </summary>
    private static string GetAssemblyNameFromNamespace(string namespacePath)
    {
        // Common .NET assembly mappings
        Dictionary<string, string> knownAssemblies = new()
        {
            { "System", "System.Runtime" },
            { "System.Collections.Generic", "System.Collections" },
            { "System.Collections", "System.Collections" },
            { "System.IO", "System.IO.FileSystem" },
            { "System.Text", "System.Runtime" },
            { "System.Linq", "System.Linq" },
            { "System.Threading", "System.Threading" },
            { "System.Net", "System.Net.Primitives" }
        };

        // Check known mappings - find the first matching namespace prefix
        KeyValuePair<string, string>? matchingAssembly = knownAssemblies
            .FirstOrDefault(kvp => namespacePath.StartsWith(kvp.Key));

        if (matchingAssembly.HasValue)
        {
            return matchingAssembly.Value.Value;
        }

        // Default: use the first two parts of the namespace
        string[] parts = namespacePath.Split('.');
        return parts.Length >= 2 ? string.Join(".", parts.Take(2)) : namespacePath;
    }

    /// <summary>
    ///     Checks if a type name is a PowerScript basic type
    /// </summary>
    private static bool IsBasicPowerScriptType(string typeName)
    {
        return typeName switch
        {
            "INT" or "Int32" => true,
            "PREC" or "PRECISION" or "Double" => true,
            "STRING" or "String" => true,
            "CHAR" or "Char" => true,
            "BOOL" or "Boolean" => true,
            _ => false
        };
    }

    /// <summary>
    ///     Maps PowerScript type names to .NET types
    /// </summary>
    private static Type? MapPowerScriptTypeToDotNet(string typeName)
    {
        return typeName switch
        {
            "INT" or "Int32" => typeof(int),
            "PREC" or "PRECISION" or "Double" => typeof(double),
            "STRING" or "String" => typeof(string),
            "CHAR" or "Char" => typeof(char),
            "BOOL" or "Boolean" => typeof(bool),
            _ => null
        };
    }
}