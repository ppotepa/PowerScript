using System.Collections;
using System.Diagnostics;
using PowerScript.Common.Logging;
using PowerScript.Compiler.Models;
using PowerScript.Core.AST;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Statements; // For ExpressionStatement
using PowerScript.Runtime.Interfaces;
using PowerScript.Runtime.Models;
using PowerScript.Runtime.Core;

namespace PowerScript.Runtime;

/// <summary>
/// PowerScript executor that runs compiled PowerScript artifacts.
/// This class is responsible ONLY for execution - not compilation.
/// </summary>
public class PowerScriptExecutor : IPowerScriptExecutor
{
    private readonly Core.ExecutionContext _context = new();
    private readonly List<string> _linkedLibraries = new();
    private bool _hasReturned; // Tracks if a RETURN statement was executed

    /// <summary>
    /// Gets the current execution context.
    /// </summary>
    public IExecutionContext GetExecutionContext() => _context;

    /// <summary>
    /// Links a library to be available during execution.
    /// </summary>
    public void LinkLibrary(string libraryPath)
    {
        if (string.IsNullOrWhiteSpace(libraryPath))
        {
            throw new ArgumentException("Library path cannot be null or whitespace", nameof(libraryPath));
        }

        if (!File.Exists(libraryPath))
        {
            throw new FileNotFoundException($"Library file not found: {libraryPath}");
        }

        _linkedLibraries.Add(libraryPath);
        LoggerService.Logger.Debug($"[EXECUTOR] Linked library: {libraryPath}");
    }

    /// <summary>
    /// Executes a compiled PowerScript program.
    /// </summary>
    public ExecutionResult Execute(CompilationResult compilationResult)
    {
        if (compilationResult == null)
        {
            throw new ArgumentNullException(nameof(compilationResult));
        }

        if (!compilationResult.IsSuccess)
        {
            return ExecutionResult.Failed(
                new[] { "Cannot execute failed compilation result" }.Concat(compilationResult.Errors).ToList());
        }

        var startTime = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            LoggerService.Logger.Debug("[EXECUTOR] Starting execution");

            // Register functions from compilation result
            RegisterFunctions(compilationResult);

            // Execute the compiled artifacts
            // For now, return a placeholder result - full execution logic will be implemented
            // based on the separated domains architecture
            var result = ExecuteCompiledArtifacts(compilationResult);

            stopwatch.Stop();

            var metadata = new ExecutionMetadata(
                startTime,
                stopwatch.Elapsed);

            LoggerService.Logger.Debug($"[EXECUTOR] Execution completed successfully in {stopwatch.ElapsedMilliseconds}ms");

            return ExecutionResult.Success(result, metadata);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LoggerService.Logger.Error($"[EXECUTOR] Execution failed: {ex.Message}");

            var metadata = new ExecutionMetadata(
                startTime,
                stopwatch.Elapsed);

            return ExecutionResult.Failed(ex.Message, metadata);
        }
    }

    private void RegisterFunctions(CompilationResult compilationResult)
    {
        foreach (var function in compilationResult.Functions)
        {
            _context.RegisterFunction(function.Key, function.Value);
            LoggerService.Logger.Debug($"[EXECUTOR] Registered function: {function.Key}");
        }
    }

    private object? ExecuteCompiledArtifacts(CompilationResult compilationResult)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Executing compiled artifacts with {compilationResult.Functions.Count} functions");
        LoggerService.Logger.Debug($"[DEBUG] Starting execution of {compilationResult.RootScope.Statements.Count} statements");

        // Execute statements in the root scope
        return ExecuteScope(compilationResult.RootScope);
    }

    /// <summary>
    /// Executes all statements in a scope.
    /// </summary>
    private object? ExecuteScope(Scope scope)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Executing scope with {scope.Statements.Count} statements");
        LoggerService.Logger.Debug($"[DEBUG] Executing scope with {scope.Statements.Count} statements");

        object? lastResult = null;

        foreach (var statement in scope.Statements)
        {
            LoggerService.Logger.Debug($"[DEBUG] Executing statement: {statement.GetType().Name} - {statement.StatementType}");
            lastResult = ExecuteStatement(statement);

            // If this is a return statement, stop execution and return the value
            if (statement is ReturnStatement)
            {
                _hasReturned = true;
                break;
            }

            // If a return was executed in a nested scope (like inside an IF), stop execution
            if (_hasReturned)
            {
                break;
            }
        }

        return lastResult;
    }

    /// <summary>
    /// Executes a single statement.
    /// </summary>
    private object? ExecuteStatement(Statement statement)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Executing statement: {statement.StatementType}");

        return statement switch
        {
            PrintStatement printStmt => ExecutePrintStatement(printStmt),
            VariableDeclarationStatement varStmt => ExecuteVariableDeclaration(varStmt),
            ReturnStatement returnStmt => ExecuteReturnStatement(returnStmt),
            IfStatement ifStmt => ExecuteIfStatement(ifStmt),
            CycleLoopStatement cycleStmt => ExecuteCycleLoopStatement(cycleStmt),
            ArrayAssignmentStatement arrayAssignStmt => ExecuteArrayAssignment(arrayAssignStmt),
            NetMethodCallStatement netMethodStmt => ExecuteNetMethodCall(netMethodStmt),
            LinkStatement linkStmt => ExecuteLinkStatement(linkStmt),
            FunctionCallStatement funcStmt => ExecuteFunctionCallStatement(funcStmt),
            ExpressionStatement exprStmt => ExecuteExpressionStatement(exprStmt),
            _ => throw new NotSupportedException($"Statement type {statement.StatementType} is not yet supported")
        };
    }

    /// <summary>
    /// Executes a LINK statement (imports .NET namespace or PowerScript file).
    /// </summary>
    private object? ExecuteLinkStatement(LinkStatement statement)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Executing LINK statement: {statement.Target} (IsFile={statement.IsFilePath})");

        if (statement.IsFilePath)
        {
            // File linking must be handled at the interpreter level, not executor level
            // The executor will receive an already-linked AST from the interpreter
            LoggerService.Logger.Debug($"[EXECUTOR] File path link: {statement.Target}");
        }
        else
        {
            // .NET namespace linking - currently a no-op since we use Console -> WriteLine directly
            LoggerService.Logger.Debug($"[EXECUTOR] .NET namespace link registered: {statement.Target}");
        }

        return null;
    }

    /// <summary>
    /// Executes a PRINT statement.
    /// PowerScript convention: String values are printed in uppercase.
    /// </summary>
    private object? ExecutePrintStatement(PrintStatement statement)
    {
        var value = EvaluateExpression(statement.Expression);
        LoggerService.Logger.Debug($"[EXECUTOR] Print value: '{value}' (type={value?.GetType().Name}, length={value?.ToString()?.Length})");
        var output = value?.ToString() ?? "";
        
        // PowerScript convention: uppercase string output
        if (value is string)
        {
            output = output.ToUpperInvariant();
        }

        Console.WriteLine(output);
        LoggerService.Logger.Debug($"[EXECUTOR] Printed: {output}");

        return null;
    }

    /// <summary>
    /// Executes an expression statement (typically a function call).
    /// The return value is evaluated but discarded.
    /// </summary>
    private object? ExecuteExpressionStatement(ExpressionStatement statement)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Executing expression statement");
        var value = EvaluateExpression(statement.Expression);
        LoggerService.Logger.Debug($"[EXECUTOR] Expression evaluated to: {value}");
        return value;
    }

    /// <summary>
    /// Executes a variable declaration statement (VAR or FLEX).
    /// </summary>
    private object? ExecuteVariableDeclaration(VariableDeclarationStatement statement)
    {
        var variableName = statement.Declaration.Identifier.RawToken?.Text ??
                          throw new InvalidOperationException("Variable declaration missing identifier");

        var value = EvaluateExpression(statement.InitialValue);
        LoggerService.Logger.Debug($"[EXECUTOR] Variable value before set: '{value}' (type={value?.GetType().Name}, length={value?.ToString()?.Length})");

        _context.SetVariable(variableName, value);
        LoggerService.Logger.Debug($"[EXECUTOR] Set variable {variableName} = {value}");

        return value;
    }

    /// <summary>
    /// Executes a .NET method call statement.
    /// Example: Console -> WriteLine(42) or NET.System.Console.WriteLine("Hello")
    /// </summary>
    private object? ExecuteNetMethodCall(NetMethodCallStatement statement)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Executing .NET method call: {statement.FullMethodPath}");

        // Parse the fully qualified method path (e.g., "Console.WriteLine" or "System.Console.WriteLine")
        string[] pathParts = statement.FullMethodPath.Split('.');
        if (pathParts.Length < 2)
        {
            throw new InvalidOperationException($"Invalid .NET method path: {statement.FullMethodPath}. Expected format: ClassName.MethodName or Namespace.ClassName.MethodName");
        }

        // Extract method name (last part)
        string methodName = pathParts[^1];

        // Extract type name (everything before method name)
        string typePath = string.Join(".", pathParts[..^1]);

        // Try to find the type
        Type? type = null;

        // First, try with System namespace prefix if not already present
        if (!typePath.Contains("System"))
        {
            type = Type.GetType($"System.{typePath}");
        }

        // If not found, try the path as-is
        if (type == null)
        {
            type = Type.GetType(typePath);
        }

        // Try common assemblies
        if (type == null)
        {
            var assemblies = new[] { typeof(Console).Assembly, typeof(object).Assembly };
            foreach (var assembly in assemblies)
            {
                type = assembly.GetType(typePath) ?? assembly.GetType($"System.{typePath}");
                if (type != null) break;
            }
        }

        if (type == null)
        {
            throw new TypeLoadException($"Could not load type '{typePath}'. Make sure to use fully qualified type names or use LINK System.");
        }

        LoggerService.Logger.Debug($"[EXECUTOR] Found type: {type.FullName}, looking for method: {methodName}");

        // Evaluate argument expressions to get actual values
        var argValues = statement.Arguments.Select(EvaluateExpression).ToArray();
        var argTypes = argValues.Select(v => v?.GetType() ?? typeof(object)).ToArray();

        LoggerService.Logger.Debug($"[EXECUTOR] Calling {type.Name}.{methodName} with {argValues.Length} argument(s)");

        // Try to find the method
        var method = type.GetMethod(methodName, argTypes);

        // If exact match not found, try without parameter types (will get first overload)
        if (method == null)
        {
            var methods = type.GetMethods().Where(m => m.Name == methodName && m.IsStatic && m.GetParameters().Length == argValues.Length).ToArray();
            if (methods.Length > 0)
            {
                method = methods[0]; // Use first matching overload
            }
        }

        if (method == null)
        {
            throw new MissingMethodException($"Static method '{methodName}' with {argValues.Length} parameter(s) not found on type '{type.Name}'");
        }

        // Invoke the static method (null target for static methods)
        var result = method.Invoke(null, argValues);
        LoggerService.Logger.Debug($"[EXECUTOR] Method call completed. Result: {result}");

        return result;
    }

    /// <summary>
    /// Executes an array assignment statement (arr[index] = value).
    /// </summary>
    private object? ExecuteArrayAssignment(ArrayAssignmentStatement statement)
    {
        // Evaluate the value to assign
        var value = EvaluateExpression(statement.ValueExpression);

        // Get the array and index
        var arrayValue = EvaluateExpression(statement.IndexExpression.ArrayExpression);
        var indexValue = EvaluateExpression(statement.IndexExpression.Index);

        if (arrayValue is not IList list)
        {
            throw new InvalidOperationException($"Cannot assign to index of non-array type: {arrayValue?.GetType().Name ?? "null"}");
        }

        if (indexValue is not int index)
        {
            throw new InvalidOperationException($"Array index must be an integer, got: {indexValue?.GetType().Name ?? "null"}");
        }

        if (index < 0 || index >= list.Count)
        {
            throw new IndexOutOfRangeException($"Array index {index} is out of range (array size: {list.Count})");
        }

        list[index] = value;
        LoggerService.Logger.Debug($"[EXECUTOR] Array assignment: [{index}] = {value}");

        return value;
    }

    /// <summary>
    /// Executes a RETURN statement.
    /// </summary>
    private object? ExecuteReturnStatement(ReturnStatement statement)
    {
        var value = statement.ReturnValue != null ? EvaluateExpression(statement.ReturnValue) : null;
        LoggerService.Logger.Debug($"[EXECUTOR] Return: {value}");
        return value;
    }

    /// <summary>
    /// Executes a function call statement (e.g., PRINT(value)).
    /// </summary>
    private object? ExecuteFunctionCallStatement(FunctionCallStatement statement)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Executing function call statement: {statement.FunctionName}");

        // Get the function from registered functions
        var function = _context.GetFunction(statement.FunctionName) as FunctionDeclaration;
        if (function == null)
        {
            throw new InvalidOperationException($"Function '{statement.FunctionName}' is not defined");
        }

        // Evaluate argument tokens to values
        var arguments = new List<object?>();
        foreach (var argToken in statement.Arguments)
        {
            object? argValue = argToken switch
            {
                ValueToken valToken => int.Parse(valToken.RawToken?.Text ?? "0"),
                StringLiteralToken strToken => strToken.RawToken?.Text?.Trim('"') ?? "",
                IdentifierToken idToken => _context.GetVariable(idToken.RawToken?.Text ?? ""),
                _ => throw new InvalidOperationException($"Unsupported argument token type: {argToken.GetType().Name}")
            };
            arguments.Add(argValue);
            LoggerService.Logger.Debug($"[EXECUTOR] Argument value: {argValue}");
        }

        // Save current variables (simple scope isolation)
        var savedVariables = new Dictionary<string, object?>(_context.Variables);
        bool savedHasReturned = _hasReturned;
        object? returnValue = null;

        try
        {
            // Reset return flag for this function call
            _hasReturned = false;

            // Bind parameters to arguments
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var param = function.Parameters[i];
                var paramName = param.Identifier.RawToken?.Text ?? throw new InvalidOperationException("Parameter missing name");
                var argValue = i < arguments.Count ? arguments[i] : null;

                _context.SetVariable(paramName, argValue);
                LoggerService.Logger.Debug($"[EXECUTOR] Bound parameter '{paramName}' = {argValue}");
            }

            // Execute the function body
            returnValue = ExecuteScope(function.Scope);

            LoggerService.Logger.Debug($"[EXECUTOR] Function '{statement.FunctionName}' returned: {returnValue}");
        }
        finally
        {
            // Restore previous variables (simple scope cleanup)
            foreach (var kvp in savedVariables)
            {
                _context.SetVariable(kvp.Key, kvp.Value);
            }
            // Restore return flag from calling scope
            _hasReturned = savedHasReturned;
        }

        return returnValue;
    }

    /// <summary>
    /// Evaluates an expression and returns its value.
    /// </summary>
    private object? EvaluateExpression(Expression expression)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Evaluating expression: {expression.ExpressionType}, Type: {expression.GetType().Name}");

        return expression switch
        {
            StringLiteralExpression stringExpr => EvaluateStringLiteral(stringExpr),
            LiteralExpression literalExpr => EvaluateLiteral(literalExpr),
            IdentifierExpression identifierExpr => EvaluateIdentifier(identifierExpr),
            BinaryExpression binaryExpr => EvaluateBinaryExpression(binaryExpr),
            LogicalExpression logicalExpr => EvaluateLogicalExpression(logicalExpr),
            ArrayLiteralExpression arrayExpr => EvaluateArrayLiteral(arrayExpr),
            ArrayCreationExpression arrayCreation => EvaluateArrayCreation(arrayCreation),
            IndexExpression indexExpr => EvaluateIndexExpression(indexExpr),
            TemplateStringExpression templateExpr => EvaluateTemplateString(templateExpr),
            FunctionCallExpression functionCallExpr => EvaluateFunctionCall(functionCallExpr),
            NetMemberAccessExpression netMemberExpr => EvaluateNetMemberAccess(netMemberExpr),
            ObjectLiteralExpression objectExpr => EvaluateObjectLiteral(objectExpr),
            PropertyAccessExpression propAccessExpr => EvaluatePropertyAccess(propAccessExpr),
            _ => throw new NotSupportedException($"Expression type {expression.ExpressionType} ({expression.GetType().Name}) is not yet supported")
        };
    }

    /// <summary>
    /// Evaluates a string literal expression.
    /// </summary>
    private string EvaluateStringLiteral(StringLiteralExpression expression)
    {
        // Remove quotes from string literal
        var text = expression.Value.RawToken?.OriginalText ?? "";
        LoggerService.Logger.Debug($"[EXECUTOR] StringLiteral OriginalText: '{text}' (length={text.Length})");
        if (text.StartsWith('"') && text.EndsWith('"'))
        {
            text = text[1..^1];
            LoggerService.Logger.Debug($"[EXECUTOR] After removing quotes: '{text}' (length={text.Length})");
        }
        return text;
    }

    /// <summary>
    /// Evaluates a template string expression with variable interpolation.
    /// </summary>
    private string EvaluateTemplateString(TemplateStringExpression expression)
    {
        var result = new System.Text.StringBuilder();

        foreach (var part in expression.Template.Parts)
        {
            if (part.IsLiteral)
            {
                // Add literal text as-is
                result.Append(part.Text);
            }
            else
            {
                // Interpolate variable
                var varName = part.Text;
                if (_context.HasVariable(varName))
                {
                    var value = _context.GetVariable(varName);
                    result.Append(value?.ToString() ?? "");
                }
                else
                {
                    throw new InvalidOperationException($"Variable '{varName}' is not defined in template string");
                }
            }
        }

        var finalString = result.ToString();
        LoggerService.Logger.Debug($"[EXECUTOR] Template string evaluated: {finalString}");
        return finalString;
    }

    /// <summary>
    /// Evaluates a literal expression (numbers, etc.).
    /// </summary>
    private object? EvaluateLiteral(LiteralExpression expression)
    {
        // Handle DecimalToken directly (already parsed)
        if (expression.Value is DecimalToken decimalToken)
        {
            return decimalToken.Value;
        }

        var text = expression.Value.RawToken?.Text ?? "";

        // Try to parse as integer first
        if (int.TryParse(text, out var intValue))
        {
            return intValue;
        }

        // Try to parse as double
        if (double.TryParse(text, out var doubleValue))
        {
            return doubleValue;
        }

        // Default to string
        return text;
    }

    /// <summary>
    /// Evaluates an identifier expression (variable reference).
    /// </summary>
    private object? EvaluateIdentifier(IdentifierExpression expression)
    {
        var variableName = expression.Identifier.RawToken?.Text ??
                          throw new InvalidOperationException("Identifier expression missing name");

        LoggerService.Logger.Debug($"[EXECUTOR] Looking for variable: '{variableName}'");

        // Special case: Check for .NET types for static member access
        // This supports #Char, #Console, #String, etc.
        Type? netType = ResolveCommonNetType(variableName);
        if (netType != null)
        {
            LoggerService.Logger.Debug($"[EXECUTOR] Returning .NET type '{netType.Name}' for static member access");
            return netType;
        }

        if (!_context.HasVariable(variableName))
        {
            throw new InvalidOperationException($"Variable '{variableName}' is not defined");
        }

        return _context.GetVariable(variableName);
    }

    /// <summary>
    /// Resolves common .NET type names to their Type objects using reflection.
    /// This enables static method calls like #Char.IsLetter() or #Console.WriteLine()
    /// Searches in System namespace and other commonly used namespaces.
    /// Handles case-insensitive matching by trying proper .NET type casing.
    /// </summary>
    private Type? ResolveCommonNetType(string typeName)
    {
        // Normalize the type name - PowerScript uses uppercase (CHAR, INT, STRING)
        // but .NET types use PascalCase (Char, Int32, String)
        string normalizedTypeName = NormalizeTypeName(typeName);

        // Common namespaces to search in order of priority
        string[] searchNamespaces =
        {
            "System",
            "System.Collections.Generic",
            "System.IO",
            "System.Text",
            "System.Linq",
            "System.Threading",
            "System.Diagnostics",
            "System.Net",
            "System.Data",
            "System.Xml"
        };

        // Try each namespace with normalized type name
        foreach (var ns in searchNamespaces)
        {
            string fullTypeName = $"{ns}.{normalizedTypeName}";

            // Try to get the type from all loaded assemblies
            Type? type = Type.GetType(fullTypeName);
            if (type != null)
            {
                LoggerService.Logger.Debug($"[EXECUTOR] Resolved type '{typeName}' to '{type.FullName}'");
                return type;
            }

            // If Type.GetType fails, search in all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(fullTypeName);
                if (type != null)
                {
                    LoggerService.Logger.Debug($"[EXECUTOR] Resolved type '{typeName}' to '{type.FullName}' from assembly '{assembly.GetName().Name}'");
                    return type;
                }
            }
        }

        // Also try without namespace (for types in mscorlib)
        try
        {
            Type? directType = Type.GetType(normalizedTypeName);
            if (directType != null)
            {
                LoggerService.Logger.Debug($"[EXECUTOR] Resolved type '{typeName}' directly to '{directType.FullName}'");
                return directType;
            }
        }
        catch
        {
            // Ignore errors
        }

        return null;
    }

    /// <summary>
    /// Normalizes PowerScript type names to .NET type names.
    /// PowerScript uses uppercase (CHAR, INT, STRING) but .NET uses PascalCase.
    /// </summary>
    private string NormalizeTypeName(string typeName)
    {
        // Map common PowerScript type names to .NET type names
        return typeName.ToUpperInvariant() switch
        {
            "CHAR" => "Char",
            "STRING" => "String",
            "INT" => "Int32",
            "LONG" => "Int64",
            "DOUBLE" => "Double",
            "DECIMAL" => "Decimal",
            "BOOL" => "Boolean",
            "BOOLEAN" => "Boolean",
            "DATETIME" => "DateTime",
            "CONSOLE" => "Console",
            "MATH" => "Math",
            "CONVERT" => "Convert",
            "ARRAY" => "Array",
            "ENVIRONMENT" => "Environment",
            "PATH" => "Path",
            "FILE" => "File",
            "DIRECTORY" => "Directory",
            _ => typeName // Return as-is if no mapping found (assume proper casing)
        };
    }

    /// <summary>
    /// Evaluates a function call expression.
    /// </summary>
    private object? EvaluateFunctionCall(FunctionCallExpression expression)
    {
        var functionName = expression.FunctionName.RawToken?.Text ??
                          throw new InvalidOperationException("Function call expression missing name");

        LoggerService.Logger.Debug($"[EXECUTOR] Calling function: '{functionName}'");

        // Get the function from registered functions
        var function = _context.GetFunction(functionName) as FunctionDeclaration;
        if (function == null)
        {
            throw new InvalidOperationException($"Function '{functionName}' is not defined");
        }

        // Evaluate all arguments
        var arguments = new List<object?>();
        foreach (var argExpr in expression.Arguments)
        {
            var argValue = EvaluateExpression(argExpr);
            arguments.Add(argValue);
            LoggerService.Logger.Debug($"[EXECUTOR] Argument value: {argValue}");
        }

        // Save current variables (simple scope isolation)
        var savedVariables = new Dictionary<string, object?>(_context.Variables);
        bool savedHasReturned = _hasReturned; // Save the return flag from calling scope
        object? returnValue = null;

        try
        {
            // Reset return flag for this function call
            _hasReturned = false;

            // Bind parameters to arguments
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var param = function.Parameters[i];
                var paramName = param.Identifier.RawToken?.Text ?? throw new InvalidOperationException("Parameter missing name");
                var argValue = i < arguments.Count ? arguments[i] : null;

                _context.SetVariable(paramName, argValue);
                LoggerService.Logger.Debug($"[EXECUTOR] Bound parameter '{paramName}' = {argValue}");
            }

            // Execute the function body
            returnValue = ExecuteScope(function.Scope);

            LoggerService.Logger.Debug($"[EXECUTOR] Function '{functionName}' returned: {returnValue}");
        }
        finally
        {
            // Restore previous variables (simple scope cleanup)
            foreach (var kvp in savedVariables)
            {
                _context.SetVariable(kvp.Key, kvp.Value);
            }

            // Restore the return flag from calling scope
            _hasReturned = savedHasReturned;
        }

        return returnValue;
    }

    /// <summary>
    /// Evaluates a .NET member access expression using reflection.
    /// Supports both property access (item -> Length) and method calls (item -> ToString()).
    /// </summary>
    private object? EvaluateNetMemberAccess(NetMemberAccessExpression expression)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Evaluating .NET member access: {expression.MemberName}");

        // Evaluate the target expression (left side of arrow)
        var targetValue = EvaluateExpression(expression.Target);
        if (targetValue == null)
        {
            throw new NullReferenceException($"Cannot access member '{expression.MemberName}' on null object");
        }

        // Special case: If target is a Type object, we're calling a static method/property
        if (targetValue is Type staticType)
        {
            LoggerService.Logger.Debug($"[EXECUTOR] Static member access on type: {staticType.Name}, Member: {expression.MemberName}");

            if (expression.IsMethodCall)
            {
                // Static method call: Console -> WriteLine("Hello")
                var argValues = expression.Arguments.Select(EvaluateExpression).ToArray();
                var argTypes = argValues.Select(v => v?.GetType() ?? typeof(object)).ToArray();

                LoggerService.Logger.Debug($"[EXECUTOR] Looking for static method '{expression.MemberName}' with {argValues.Length} arguments");

                // Try to find the method
                var method = staticType.GetMethod(expression.MemberName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, argTypes, null);

                // If exact match not found, try without parameter types
                if (method == null)
                {
                    var methods = staticType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                        .Where(m => m.Name == expression.MemberName && m.GetParameters().Length == argValues.Length)
                        .ToArray();
                    if (methods.Length > 0)
                    {
                        method = methods[0];
                    }
                }

                if (method == null)
                {
                    throw new MissingMethodException($"Static method '{expression.MemberName}' with {argValues.Length} parameter(s) not found on type '{staticType.Name}'");
                }

                LoggerService.Logger.Debug($"[EXECUTOR] Invoking static method '{expression.MemberName}'");

                var result = method.Invoke(null, argValues); // null target for static methods
                LoggerService.Logger.Debug($"[EXECUTOR] Method result: {result}");
                return result;
            }
            else
            {
                // Static property access
                var property = staticType.GetProperty(expression.MemberName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (property == null)
                {
                    throw new MissingMemberException($"Static property '{expression.MemberName}' not found on type '{staticType.Name}'");
                }

                var result = property.GetValue(null); // null target for static properties
                LoggerService.Logger.Debug($"[EXECUTOR] Static property value: {result}");
                return result;
            }
        }

        // Instance member access (original code)
        var targetType = targetValue.GetType();
        LoggerService.Logger.Debug($"[EXECUTOR] Target type: {targetType.Name}, Member: {expression.MemberName}, IsMethod: {expression.IsMethodCall}");

        if (expression.IsMethodCall)
        {
            // Method call: person -> Speak("Hello")
            // Evaluate arguments first
            var argValues = expression.Arguments.Select(EvaluateExpression).ToArray();
            var argTypes = argValues.Select(v => v?.GetType() ?? typeof(object)).ToArray();

            LoggerService.Logger.Debug($"[EXECUTOR] Looking for method '{expression.MemberName}' with {argValues.Length} arguments");

            // Try to find the best matching method
            var method = targetType.GetMethod(expression.MemberName, argTypes);

            // If exact match not found, try without parameter types (will get first overload)
            if (method == null)
            {
                var methods = targetType.GetMethods().Where(m => m.Name == expression.MemberName && m.GetParameters().Length == argValues.Length).ToArray();
                if (methods.Length > 0)
                {
                    method = methods[0]; // Use first matching overload
                }
            }

            if (method == null)
            {
                throw new MissingMethodException($"Method '{expression.MemberName}' with {argValues.Length} parameter(s) not found on type '{targetType.Name}'");
            }

            LoggerService.Logger.Debug($"[EXECUTOR] Invoking method '{expression.MemberName}'");

            var result = method.Invoke(targetValue, argValues);
            LoggerService.Logger.Debug($"[EXECUTOR] Method result: {result}");
            return result;
        }
        else
        {
            // Property access: str -> Length
            var property = targetType.GetProperty(expression.MemberName);
            if (property == null)
            {
                throw new MissingMemberException($"Property '{expression.MemberName}' not found on type '{targetType.Name}'");
            }

            var result = property.GetValue(targetValue);
            LoggerService.Logger.Debug($"[EXECUTOR] Property value: {result}");
            return result;
        }
    }

    /// <summary>
    /// Evaluates an array literal expression.
    /// </summary>
    private object? EvaluateArrayLiteral(ArrayLiteralExpression expression)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Evaluating array literal with {expression.Elements.Count} elements");

        var result = new List<object?>();
        foreach (var element in expression.Elements)
        {
            var value = EvaluateExpression(element);
            result.Add(value);
        }

        return result;
    }

    /// <summary>
    /// Evaluates an array creation expression (CHAIN syntax).
    /// Creates a new array with the specified size.
    /// Example: FLEX arr = CHAIN 10
    /// </summary>
    private object? EvaluateArrayCreation(ArrayCreationExpression expression)
    {
        // Evaluate the size expression (can be literal or variable)
        object? sizeValue = EvaluateExpression(expression.SizeExpression);

        if (sizeValue is not int size)
        {
            throw new InvalidOperationException($"Array size must be an integer, got: {sizeValue?.GetType().Name ?? "null"}");
        }

        if (size < 0)
        {
            throw new InvalidOperationException($"Array size cannot be negative: {size}");
        }

        LoggerService.Logger.Debug($"[EXECUTOR] Creating array with size {size}");

        // Create a list with the specified size, initialized with default values (0)
        var result = new List<object?>(size);
        for (int i = 0; i < size; i++)
        {
            result.Add(0); // Default to 0 for numeric arrays
        }

        return result;
    }

    /// <summary>
    /// Evaluates an index expression (array access).
    /// </summary>
    private object? EvaluateIndexExpression(IndexExpression expression)
    {
        // Evaluate the array expression
        var arrayValue = EvaluateExpression(expression.ArrayExpression);

        // Evaluate the index
        var indexValue = EvaluateExpression(expression.Index);

        if (arrayValue is not IList list)
        {
            throw new InvalidOperationException($"Cannot index non-array type: {arrayValue?.GetType().Name ?? "null"}");
        }

        if (indexValue is not int index)
        {
            throw new InvalidOperationException($"Array index must be an integer, got: {indexValue?.GetType().Name ?? "null"}");
        }

        if (index < 0 || index >= list.Count)
        {
            throw new IndexOutOfRangeException($"Array index {index} is out of range (array size: {list.Count})");
        }

        LoggerService.Logger.Debug($"[EXECUTOR] Array access: [{index}] = {list[index]}");
        return list[index];
    }

    /// <summary>
    /// Evaluates a binary expression (arithmetic and comparison operations).
    /// </summary>
    private object? EvaluateBinaryExpression(BinaryExpression expression)
    {
        var left = EvaluateExpression(expression.Left);
        var right = EvaluateExpression(expression.Right);
        var operatorText = expression.Operator.RawToken?.Text ?? "";

        // Handle comparison operations first
        if (IsComparisonOperator(operatorText))
        {
            return EvaluateComparison(left, right, operatorText);
        }

        // Handle arithmetic operations
        if (left is int leftInt && right is int rightInt)
        {
            return operatorText switch
            {
                "+" => leftInt + rightInt,
                "-" => leftInt - rightInt,
                "*" => leftInt * rightInt,
                "/" => rightInt != 0 ? leftInt / rightInt : throw new DivideByZeroException(),
                "%" => rightInt != 0 ? leftInt % rightInt : throw new DivideByZeroException(),
                _ => throw new NotSupportedException($"Binary operator {operatorText} is not yet supported")
            };
        }

        // Handle string concatenation
        if (operatorText == "+")
        {
            return left?.ToString() + right?.ToString();
        }

        throw new NotSupportedException($"Binary operation {operatorText} between {left?.GetType()} and {right?.GetType()} is not yet supported");
    }

    /// <summary>
    /// Evaluates a logical expression (AND, OR operations).
    /// </summary>
    private object? EvaluateLogicalExpression(LogicalExpression expression)
    {
        var operatorText = expression.Operator.RawToken?.Text ?? "";

        // Handle short-circuit evaluation
        var leftResult = EvaluateExpression(expression.Left);
        bool leftIsTrue = IsConditionTrue(leftResult);

        if (operatorText == "AND")
        {
            if (!leftIsTrue) return false; // Short-circuit: false AND anything = false
            var rightResult = EvaluateExpression(expression.Right);
            return IsConditionTrue(rightResult);
        }

        if (operatorText == "OR")
        {
            if (leftIsTrue) return true; // Short-circuit: true OR anything = true
            var rightResult = EvaluateExpression(expression.Right);
            return IsConditionTrue(rightResult);
        }

        throw new NotSupportedException($"Logical operator {operatorText} is not yet supported");
    }

    /// <summary>
    /// Checks if an operator is a comparison operator.
    /// </summary>
    private bool IsComparisonOperator(string operatorText)
    {
        return operatorText is ">" or "<" or ">=" or "<=" or "==" or "!=";
    }

    /// <summary>
    /// Evaluates a comparison operation.
    /// </summary>
    private bool EvaluateComparison(object? left, object? right, string operatorText)
    {
        // Convert booleans to integers for comparison (true = 1, false = 0)
        if (left is bool leftBool)
        {
            left = leftBool ? 1 : 0;
        }
        if (right is bool rightBool)
        {
            right = rightBool ? 1 : 0;
        }

        // Handle integer comparisons
        if (left is int leftInt && right is int rightInt)
        {
            return operatorText switch
            {
                ">" => leftInt > rightInt,
                "<" => leftInt < rightInt,
                ">=" => leftInt >= rightInt,
                "<=" => leftInt <= rightInt,
                "==" => leftInt == rightInt,
                "!=" => leftInt != rightInt,
                _ => throw new NotSupportedException($"Comparison operator {operatorText} is not yet supported")
            };
        }

        // Handle string comparisons
        if (left is string leftStr && right is string rightStr)
        {
            return operatorText switch
            {
                "==" => leftStr == rightStr,
                "!=" => leftStr != rightStr,
                _ => throw new NotSupportedException($"String comparison operator {operatorText} is not yet supported")
            };
        }

        // Handle null comparisons
        return operatorText switch
        {
            "==" => Equals(left, right),
            "!=" => !Equals(left, right),
            _ => throw new NotSupportedException($"Comparison {operatorText} between {left?.GetType()} and {right?.GetType()} is not yet supported")
        };
    }

    /// <summary>
    /// Evaluates an object literal expression.
    /// Creates a PowerScriptObject with the specified properties.
    /// Example: {name = "John", age = 30} or {x = 1} as Point!
    /// </summary>
    private object EvaluateObjectLiteral(ObjectLiteralExpression expression)
    {
        LoggerService.Logger.Debug($"[EXECUTOR] Evaluating object literal with {expression.Properties.Count} properties");

        var properties = new Dictionary<string, object?>();

        foreach (var prop in expression.Properties)
        {
            var value = EvaluateExpression(prop.Value);
            properties[prop.Key] = value;
            LoggerService.Logger.Debug($"[EXECUTOR] Object property: {prop.Key} = {value}");
        }

        var obj = new Models.PowerScriptObject(properties, expression.TypeName, expression.IsStrict);
        
        if (expression.TypeName != null)
        {
            LoggerService.Logger.Debug($"[EXECUTOR] Created object of type '{expression.TypeName}'{(expression.IsStrict ? " (strict)" : "")}");
        }

        return obj;
    }

    /// <summary>
    /// Evaluates a property access expression.
    /// Gets the value of a property from a PowerScriptObject.
    /// Example: person.name or obj.value
    /// </summary>
    private object? EvaluatePropertyAccess(PropertyAccessExpression expression)
    {
        var target = EvaluateExpression(expression.Target);
        
        LoggerService.Logger.Debug($"[EXECUTOR] Accessing property '{expression.PropertyName}' on target type {target?.GetType().Name}");

        if (target is Models.PowerScriptObject obj)
        {
            var value = obj.GetProperty(expression.PropertyName);
            LoggerService.Logger.Debug($"[EXECUTOR] Property value: {value}");
            return value;
        }

        throw new InvalidOperationException($"Cannot access property '{expression.PropertyName}' on non-object value of type {target?.GetType().Name}");
    }

    /// <summary>
    /// Executes an IF statement with optional ELSE clause.
    /// </summary>
    private object? ExecuteIfStatement(IfStatement statement)
    {
        var conditionResult = EvaluateExpression(statement.Condition);
        bool isTrue = IsConditionTrue(conditionResult);

        LoggerService.Logger.Debug($"[EXECUTOR] IF condition evaluated to: {isTrue}");

        if (isTrue)
        {
            return ExecuteScope(statement.ThenScope);
        }
        else if (statement.ElseScope != null)
        {
            return ExecuteScope(statement.ElseScope);
        }

        return null;
    }

    /// <summary>
    /// Executes a CYCLE loop statement.
    /// </summary>
    private object? ExecuteCycleLoopStatement(CycleLoopStatement statement)
    {
        // Handle WHILE loops (CYCLE WHILE condition)
        if (statement.IsWhileLoop)
        {
            return ExecuteWhileLoop(statement);
        }

        // Handle range loops (CYCLE start TO end)
        if (statement.IsRangeLoop)
        {
            return ExecuteRangeLoop(statement);
        }

        var collectionValue = EvaluateExpression(statement.CollectionExpression);
        LoggerService.Logger.Debug($"[EXECUTOR] CYCLE collection evaluated to: {collectionValue}");

        // Handle count-based loops (CYCLE 5)
        if (collectionValue is int count)
        {
            return ExecuteCountBasedLoop(statement, count);
        }

        // Handle collection-based loops (CYCLE collection, CYCLE ELEMENTS OF collection)
        if (collectionValue is System.Collections.IEnumerable enumerable)
        {
            return ExecuteCollectionLoop(statement, enumerable);
        }

        throw new NotSupportedException($"CYCLE loops with type {collectionValue?.GetType().Name ?? "null"} are not yet supported");
    }

    /// <summary>
    /// Executes a while loop (CYCLE WHILE condition).
    /// </summary>
    private object? ExecuteWhileLoop(CycleLoopStatement statement)
    {
        object? lastResult = null;
        int iteration = 0;

        while (true)
        {
            // Evaluate the condition
            var conditionValue = EvaluateExpression(statement.CollectionExpression);
            LoggerService.Logger.Debug($"[EXECUTOR] CYCLE WHILE condition evaluated to: {conditionValue}");

            // Check if condition is true
            if (!IsConditionTrue(conditionValue))
            {
                break;
            }

            // Set the loop variable value
            _context.SetVariable(statement.LoopVariableName, iteration);
            LoggerService.Logger.Debug($"[EXECUTOR] CYCLE WHILE iteration {iteration}, {statement.LoopVariableName} = {iteration}");

            // Execute the loop body
            if (statement.LoopBody != null)
            {
                lastResult = ExecuteScope(statement.LoopBody);
            }

            // Break out of loop if a RETURN statement was executed
            if (_hasReturned)
            {
                LoggerService.Logger.Debug($"[EXECUTOR] CYCLE WHILE loop interrupted by RETURN statement");
                break;
            }

            iteration++;

            // Safety check to prevent infinite loops
            if (iteration > 1000000)
            {
                throw new InvalidOperationException("CYCLE WHILE loop exceeded maximum iteration count (1000000)");
            }
        }

        return lastResult;
    }

    /// <summary>
    /// Executes a range loop (CYCLE start TO end).
    /// </summary>
    private object? ExecuteRangeLoop(CycleLoopStatement statement)
    {
        // Evaluate start and end values
        var startValue = EvaluateExpression(statement.CollectionExpression);
        var endValue = EvaluateExpression(statement.RangeEndExpression!);

        LoggerService.Logger.Debug($"[EXECUTOR] CYCLE range: {startValue} TO {endValue}");

        if (startValue is not int start)
        {
            throw new InvalidOperationException($"Range start must be an integer, got {startValue?.GetType().Name ?? "null"}");
        }

        if (endValue is not int end)
        {
            throw new InvalidOperationException($"Range end must be an integer, got {endValue?.GetType().Name ?? "null"}");
        }

        object? lastResult = null;

        // Iterate from start to end (inclusive)
        for (int i = start; i <= end; i++)
        {
            // Set the loop variable value
            _context.SetVariable(statement.LoopVariableName, i);
            LoggerService.Logger.Debug($"[EXECUTOR] CYCLE range iteration {i}, {statement.LoopVariableName} = {i}");

            // Execute the loop body
            if (statement.LoopBody != null)
            {
                lastResult = ExecuteScope(statement.LoopBody);
            }

            // Break out of loop if a RETURN statement was executed
            if (_hasReturned)
            {
                LoggerService.Logger.Debug($"[EXECUTOR] CYCLE range loop interrupted by RETURN statement");
                break;
            }
        }

        return lastResult;
    }

    /// <summary>
    /// Executes a count-based CYCLE loop.
    /// </summary>
    private object? ExecuteCountBasedLoop(CycleLoopStatement statement, int count)
    {
        object? lastResult = null;

        for (int i = 0; i < count; i++)
        {
            // Set the loop variable value
            _context.SetVariable(statement.LoopVariableName, i);
            LoggerService.Logger.Debug($"[EXECUTOR] CYCLE iteration {i}, {statement.LoopVariableName} = {i}");

            // Execute the loop body
            if (statement.LoopBody != null)
            {
                lastResult = ExecuteScope(statement.LoopBody);
            }

            // Break out of loop if a RETURN statement was executed
            if (_hasReturned)
            {
                LoggerService.Logger.Debug($"[EXECUTOR] CYCLE loop interrupted by RETURN statement");
                break;
            }
        }

        return lastResult;
    }

    /// <summary>
    /// Executes a collection-based CYCLE loop (CYCLE collection, CYCLE ELEMENTS OF collection).
    /// </summary>
    private object? ExecuteCollectionLoop(CycleLoopStatement statement, System.Collections.IEnumerable collection)
    {
        object? lastResult = null;
        int index = 0;

        foreach (var item in collection)
        {
            // Set the loop variable value to the current item
            _context.SetVariable(statement.LoopVariableName, item);
            LoggerService.Logger.Debug($"[EXECUTOR] CYCLE collection iteration {index}, {statement.LoopVariableName} = {item}");

            // Execute the loop body
            if (statement.LoopBody != null)
            {
                lastResult = ExecuteScope(statement.LoopBody);
            }

            // Break out of loop if a RETURN statement was executed
            if (_hasReturned)
            {
                LoggerService.Logger.Debug($"[EXECUTOR] CYCLE collection loop interrupted by RETURN statement");
                break;
            }

            index++;
        }

        return lastResult;
    }

    /// <summary>
    /// Determines if a condition value should be considered true.
    /// </summary>
    private bool IsConditionTrue(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            int i => i != 0,
            double d => d != 0.0,
            string s => !string.IsNullOrEmpty(s),
            _ => true
        };
    }
}