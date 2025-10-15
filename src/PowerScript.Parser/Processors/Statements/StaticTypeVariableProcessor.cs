using PowerScript.Common.Logging;
using PowerScript.Core.AST;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords.Types;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;

namespace PowerScript.Parser.Processors.Statements;

/// <summary>
///     Processes static type declarations: INT name = value, STRING name = value, NUMBER name = value
///     Also supports bit-width specification: INT[8] small = 100, NUMBER[16] medium = 1000
///     These are strongly-typed variables that cannot change type.
/// </summary>
public class StaticTypeVariableProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        return token is IBaseTypeToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        Token typeToken = token;
        string typeName = typeToken.RawToken?.Text ?? "";

        LoggerService.Logger.Debug(
            $"StaticTypeVariableProcessor: Processing {typeName} variable declaration in scope '{context.CurrentScope.ScopeName}'");

        Token currentToken = typeToken.Next!;
        int? bitWidth = null;

        // Check for optional bit-width specification: TYPE[bits]
        if (currentToken is BracketOpen)
        {
            currentToken = currentToken.Next!; // Move past '['

            if (currentToken is not ValueToken bitWidthToken)
            {
                throw new InvalidOperationException(
                    $"Expected bit-width value after '[' in type declaration, found {currentToken.GetType().Name}");
            }

            string bitWidthText = bitWidthToken.RawToken?.Text ?? "";
            if (!int.TryParse(bitWidthText, out int parsedBitWidth))
            {
                throw new InvalidOperationException($"Invalid bit-width value: {bitWidthText}");
            }

            bitWidth = parsedBitWidth;
            LoggerService.Logger.Debug($"[StaticTypeVariableProcessor] Bit-width: {bitWidth}");

            currentToken = bitWidthToken.Next!; // Move past bit width value

            if (currentToken is not BracketClosed)
            {
                throw new InvalidOperationException(
                    $"Expected ']' after bit-width value, found {currentToken.GetType().Name}");
            }

            currentToken = currentToken.Next!; // Move past ']'
        }

        // Next token must be an identifier (variable name)
        if (currentToken is not IdentifierToken)
        {
            throw new InvalidOperationException(
                $"Expected identifier after {typeName}{(bitWidth.HasValue ? $"[{bitWidth}]" : "")}, found {currentToken.GetType().Name}");
        }

        IdentifierToken identifierToken = (IdentifierToken)currentToken;
        string variableName = identifierToken.RawToken?.Text ?? "";

        string typeSpec = bitWidth.HasValue ? $"{typeName}[{bitWidth}]" : typeName;
        LoggerService.Logger.Debug($"[StaticTypeVariableProcessor] Variable name: {variableName}, Type: {typeSpec}");

        // Check for duplicate declarations in current scope
        if (context.CurrentScope.Decarations.ContainsKey(variableName))
        {
            throw new InvalidOperationException(
                $"Variable '{variableName}' is already declared in scope '{context.CurrentScope.ScopeName}'");
        }

        currentToken = identifierToken.Next!;

        // Expect equals sign
        if (currentToken is not EqualsToken)
        {
            throw new InvalidOperationException(
                $"Expected '=' after variable name '{variableName}', found {currentToken.GetType().Name}");
        }

        currentToken = currentToken.Next!;

        // Parse the initial value expression
        Expression initialValue;

        if (currentToken is ValueToken valueToken)
        {
            initialValue = new LiteralExpression(valueToken);
            currentToken = valueToken.Next!;
        }
        else if (currentToken is StringLiteralToken stringToken)
        {
            initialValue = new StringLiteralExpression(stringToken);
            currentToken = stringToken.Next!;
        }
        else if (currentToken is IdentifierToken idToken)
        {
            // Could be a variable reference or function call
            initialValue = new IdentifierExpression(idToken);
            currentToken = idToken.Next!;
        }
        else
        {
            throw new InvalidOperationException(
                $"Expected value, string, or identifier after '=' in variable declaration, found {currentToken.GetType().Name}");
        }

        // Create the variable declaration with explicit type and optional bit width
        VariableDeclaration declaration = new VariableDeclaration(typeToken, identifierToken, bitWidth);
        declaration.InitialValue = initialValue;

        // Register the declaration in the scope
        context.CurrentScope.Decarations.Add(variableName, declaration);

        // Create and register the statement
        VariableDeclarationStatement statement = new(declaration, initialValue)
        {
            StartToken = typeToken
        };

        context.CurrentScope.Statements.Add(statement);

        LoggerService.Logger.Success(
            $"[StaticTypeVariableProcessor] Registered {typeSpec} variable '{variableName}' in scope '{context.CurrentScope.ScopeName}'");

        return TokenProcessingResult.Continue(currentToken);
    }
}
