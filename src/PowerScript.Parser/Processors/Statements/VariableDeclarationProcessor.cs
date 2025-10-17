using PowerScript.Common.Logging;
using PowerScript.Core.AST;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Keywords.Types;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;
using PowerScript.Parser.Processors.Expressions;

namespace PowerScript.Parser.Processors.Statements;

/// <summary>
///     Processes VAR keyword tokens for variable declarations.
///     Responsible for:
///     - Parsing variable name (required)
///     - Parsing optional type declaration (INT, etc.)
///     - Parsing equals sign and initial value expression
///     - Checking for duplicate declarations in current scope
///     - Registering variable declaration in scope
/// </summary>
public class VariableDeclarationProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        return token is VarKeywordToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        LoggerService.Logger.Debug(
            $"VariableDeclarationProcessor: Processing VAR token in scope '{context.CurrentScope.ScopeName}'");

        VarKeywordToken? varToken = token as VarKeywordToken;
        Token currentToken = varToken!.Next!;

        // Determine if we have explicit type or inferred type
        // Syntax: VAR x = 10         (inferred)
        //     OR: VAR INT x = 10     (explicit)

        Token? typeToken = null;
        Token identifierToken;

        // Check if next token is a type (INT, etc.)
        if (currentToken is IBaseTypeToken)
        {
            typeToken = currentToken;
            currentToken = currentToken.Next!;
        }

        // Next token must be an identifier (variable name)
        if (currentToken is not IdentifierToken)
        {
            throw new InvalidOperationException(
                $"Expected identifier after VAR{(typeToken != null ? " " + typeToken.RawToken?.Text : "")}, found {currentToken.GetType().Name}");
        }

        identifierToken = currentToken;
        string variableName = identifierToken.RawToken?.Text ?? "";

        LoggerService.Logger.Debug($"Variable name: {variableName}");
        if (typeToken != null)
        {
            LoggerService.Logger.Debug($"Variable type: {typeToken.RawToken?.Text}");
        }

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

        // Special case: array literals must use dedicated parser since ExpressionParser doesn't handle them yet
        if (currentToken is BracketOpen)
        {
            initialValue = ParseArrayLiteral(ref currentToken);
        }
        else
        {
            // Use ExpressionParser for all other expressions (binary ops, function calls, etc.)
            var parser = new ExpressionParser();
            initialValue = parser.Parse(ref currentToken);
        }

        // Create the variable declaration
        VariableDeclaration declaration = typeToken != null
            ? new VariableDeclaration(typeToken, identifierToken)
            : new VariableDeclaration(identifierToken);
        declaration.InitialValue = initialValue;

        // Register the declaration in the scope
        context.CurrentScope.Decarations.Add(variableName, declaration);

        // Create and register the statement
        VariableDeclarationStatement statement = new(declaration, initialValue)
        {
            StartToken = varToken
        };

        context.CurrentScope.Statements.Add(statement);

        LoggerService.Logger.Success(
            $"✓ Registered variable '{variableName}' in scope '{context.CurrentScope.ScopeName}'");

        return TokenProcessingResult.Continue(currentToken);
    }

    /// <summary>
    ///     Parses an array literal expression: [1, 2, 3] or [[1, 2], [3, 4]]
    ///     Supports nested arrays for multidimensional arrays.
    /// </summary>
    private static ArrayLiteralExpression ParseArrayLiteral(ref Token? token)
    {
        if (token is not BracketOpen)
        {
            throw new InvalidOperationException("Expected '[' at start of array literal");
        }

        token = token.Next; // Move past '['

        List<Expression> elements = [];

        // Handle empty array []
        if (token is BracketClosed)
        {
            token = token.Next; // Move past ']'
            return new ArrayLiteralExpression(elements);
        }

        // Parse array elements
        while (token != null)
        {
            // Parse element expression (recursively handles nested arrays)
            Expression elementExpr;

            if (token is BracketOpen)
            {
                // Nested array: [[1, 2], [3, 4]]
                elementExpr = ParseArrayLiteral(ref token);
            }
            else if (token is ValueToken valueToken)
            {
                elementExpr = new LiteralExpression(valueToken);
                token = token.Next;
            }
            else if (token is StringLiteralToken stringToken)
            {
                elementExpr = new StringLiteralExpression(stringToken);
                token = token.Next;
            }
            else if (token is IdentifierToken identifierToken)
            {
                elementExpr = new IdentifierExpression(identifierToken);
                token = token.Next;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unexpected token in array literal: {token.GetType().Name}");
            }

            elements.Add(elementExpr);

            // Check for comma or closing bracket
            if (token is CommaToken)
            {
                token = token.Next; // Move past comma

                // Allow trailing comma: [1, 2, 3,]
                if (token is BracketClosed)
                {
                    break;
                }
            }
            else if (token is BracketClosed)
            {
                break;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Expected ',' or ']' in array literal, found {token?.GetType().Name}");
            }
        }

        if (token is not BracketClosed)
        {
            throw new InvalidOperationException("Expected ']' at end of array literal");
        }

        token = token.Next; // Move past ']'
        return new ArrayLiteralExpression(elements);
    }

    /// <summary>
    ///     Parses a function call expression: FUNCTION_NAME(arg1, arg2, ...)
    /// </summary>
    private static FunctionCallExpression ParseFunctionCall(IdentifierToken functionNameToken, out Token nextToken)
    {
        // Use ExpressionParser to handle the function call which supports nested calls and complex expressions
        var parser = new ExpressionParser();
        Token currentToken = functionNameToken;
        var expression = parser.Parse(ref currentToken);

        nextToken = currentToken;

        if (expression is not FunctionCallExpression funcCall)
        {
            throw new InvalidOperationException($"Expected function call expression but got {expression.GetType().Name}");
        }

        return funcCall;
    }
}
