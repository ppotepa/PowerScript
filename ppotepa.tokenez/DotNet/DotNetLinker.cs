using System.Reflection;

namespace ppotepa.tokenez.DotNet
{
    /// <summary>
    /// Manages .NET assembly loading and type resolution for PowerScript.
    /// Supports linking namespaces via LINK statements and resolving types
    /// with or without full namespace paths.
    /// </summary>
    public class DotNetLinker
    {
        private HashSet<string> _linkedNamespaces = new();
        private Dictionary<string, Assembly> _loadedAssemblies = new();
        private Dictionary<string, Type> _typeCache = new();

        /// <summary>
        /// Links a .NET namespace, making its types available for direct reference.
        /// Example: After LinkNamespace("System.Collections.Generic"), 
        /// you can use "List" instead of "System.Collections.Generic.List"
        /// </summary>
        public void LinkNamespace(string namespacePath)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[DotNetLinker] Linking namespace: {namespacePath}");
            Console.ResetColor();

            _linkedNamespaces.Add(namespacePath);

            // Try to load the corresponding assembly
            string assemblyName = GetAssemblyNameFromNamespace(namespacePath);
            if (!_loadedAssemblies.ContainsKey(assemblyName))
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    _loadedAssemblies[assemblyName] = assembly;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[DotNetLinker] Loaded assembly: {assemblyName}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    // Try loading from AppDomain as fallback
                    var existingAssembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == assemblyName);

                    if (existingAssembly != null)
                    {
                        _loadedAssemblies[assemblyName] = existingAssembly;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[DotNetLinker] Found assembly in AppDomain: {assemblyName}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[DotNetLinker] Warning: Could not load assembly '{assemblyName}': {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
        }

        /// <summary>
        /// Resolves a type name to a .NET Type.
        /// Supports:
        /// - Full paths with :: (System::Collections::Generic::List)
        /// - Full paths with dots (System.Collections.Generic.List)
        /// - Short names if namespace is linked (List, if System.Collections.Generic is linked)
        /// - PowerScript basic types (INT, PREC, STRING, CHAR)
        /// </summary>
        public Type? ResolveType(string typeName)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DotNetLinker] Resolving type: {typeName}");
            Console.ResetColor();

            // Case 1: Type name includes namespace operator (System::Collections::Generic::List)
            if (typeName.Contains("::"))
            {
                string dotNotation = typeName.Replace("::", ".");
                var type = ResolveFullyQualifiedType(dotNotation);
                if (type != null) return type;
            }

            // Case 2: Check if it's a basic PowerScript type
            if (IsBasicPowerScriptType(typeName))
            {
                var type = MapPowerScriptTypeToDotNet(typeName);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[DotNetLinker] Resolved PowerScript type '{typeName}' to .NET type '{type?.FullName}'");
                Console.ResetColor();
                return type;
            }

            // Case 3: Check for direct type in linked namespaces
            foreach (var ns in _linkedNamespaces)
            {
                string fullName = $"{ns}.{typeName}";
                var type = ResolveFullyQualifiedType(fullName);
                if (type != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[DotNetLinker] Resolved '{typeName}' to '{type.FullName}' via linked namespace '{ns}'");
                    Console.ResetColor();
                    return type;
                }
            }

            // Case 4: Try as fully qualified type name
            var directType = ResolveFullyQualifiedType(typeName);
            if (directType != null) return directType;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[DotNetLinker] Could not resolve type: {typeName}");
            Console.ResetColor();
            return null;
        }

        /// <summary>
        /// Gets all linked namespaces
        /// </summary>
        public IReadOnlySet<string> LinkedNamespaces => _linkedNamespaces;

        /// <summary>
        /// Resolves a fully qualified type name (with dots)
        /// </summary>
        private Type? ResolveFullyQualifiedType(string fullTypeName)
        {
            // Check cache first
            if (_typeCache.TryGetValue(fullTypeName, out var cachedType))
                return cachedType;

            // Search in all loaded assemblies
            foreach (var assembly in _loadedAssemblies.Values)
            {
                var type = assembly.GetType(fullTypeName, false, true); // Case insensitive
                if (type != null)
                {
                    _typeCache[fullTypeName] = type;
                    return type;
                }
            }

            // Try to find in default assemblies (system assemblies)
            var systemType = Type.GetType(fullTypeName, false, true);
            if (systemType != null)
            {
                _typeCache[fullTypeName] = systemType;
                return systemType;
            }

            // Search in all loaded assemblies in AppDomain
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullTypeName, false, true);
                if (type != null)
                {
                    _typeCache[fullTypeName] = type;
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// Extracts the assembly name from a namespace path.
        /// Example: "System.Collections.Generic" -> "System.Collections"
        /// </summary>
        private string GetAssemblyNameFromNamespace(string namespacePath)
        {
            // Common .NET assembly mappings
            var knownAssemblies = new Dictionary<string, string>
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

            // Check known mappings
            foreach (var kvp in knownAssemblies)
            {
                if (namespacePath.StartsWith(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            // Default: use the first two parts of the namespace
            var parts = namespacePath.Split('.');
            if (parts.Length >= 2)
            {
                return string.Join(".", parts.Take(2));
            }

            return namespacePath;
        }

        /// <summary>
        /// Checks if a type name is a PowerScript basic type
        /// </summary>
        private bool IsBasicPowerScriptType(string typeName)
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
        /// Maps PowerScript type names to .NET types
        /// </summary>
        private Type? MapPowerScriptTypeToDotNet(string typeName)
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
}
