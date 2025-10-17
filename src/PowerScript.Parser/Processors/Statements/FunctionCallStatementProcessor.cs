using PowerScript.Common.Logging;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;

namespace PowerScript.Parser.Processors.Statements;

/// <summary>
///     Processes function call statements.
///     Supports two syntaxes:
///     1. Single parameter without parentheses: FUNCTIONNAME argument
///     2. Multiple parameters with parentheses: FUNCTIONNAME(arg1, arg2, ...)
/// </summary>
public class FunctionCallStatementProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        // Can process if:
        // 1. Token is an identifier (potential function name)
        // 2. Followed by a valid argument token or opening parenthesis
        if (token is not IdentifierToken identifierToken)
            return false;

        Token? next = identifierToken.Next;
        if (next == null)
            return false;

        // Check for function call with parentheses: FUNC(...)
        if (next is ParenthesisOpen)
            return true;

        // Check for single-parameter syntax: FUNC argument
        // Valid argument tokens: identifier, value, string, template string
        if (next is IdentifierToken or ValueToken or StringLiteralToken or TemplateStringToken)
            return true;

        return false;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        IdentifierToken functionNameToken = (IdentifierToken)token;
        string functionName = functionNameToken.RawToken!.Text;

        LoggerService.Logger.Debug(
            $"FunctionCallStatementProcessor: Processing function call '{functionName}' in scope '{context.CurrentScope.ScopeName}'");

        Token? nextToken = functionNameToken.Next;
        List<Expression> arguments = new();

        // Parse arguments based on syntax
        if (nextToken is ParenthesisOpen openParen)
        {
            // Multi-parameter syntax: FUNC(arg1, arg2, ...)
            (arguments, nextToken) = ParseFunctionArguments(openParen);
        }
        else
        {
            // Single-parameter syntax: FUNC argument
            Expression? argument = ParseSingleArgument(ref nextToken);
            if (argument != null)
            {
                arguments.Add(argument);
            }
        }

        // Create a function call expression
        FunctionCallExpression funcCall = new FunctionCallExpression
        {
            FunctionName = functionNameToken
        };
        funcCall.Arguments.AddRange(arguments);

        // Wrap in an expression statement (executes function but discards return value)
        ExpressionStatement statement = new ExpressionStatement
        {
            StartToken = functionNameToken,
            Expression = funcCall
        };

        context.CurrentScope.Statements.Add(statement);

        LoggerService.Logger.Debug(
            $"FunctionCallStatementProcessor: Registered function call '{functionName}' with {arguments.Count} argument(s)");

        return TokenProcessingResult.Continue(nextToken!);
    }

    /// <summary>
    ///     Parses a single argument for the single-parameter syntax.
    /// </summary>
    private Expression? ParseSingleArgument(ref Token? token)
    {
        if (token == null)
            return null;

        Expression? expression = null;

        if (token is StringLiteralToken stringToken)
        {
            expression = new StringLiteralExpression(stringToken);
            token = stringToken.Next;
        }
        else if (token is TemplateStringToken templateToken)
        {
            expression = new TemplateStringExpression(templateToken);
            token = templateToken.Next;
        }
        else if (token is ValueToken valueToken)
        {
            expression = new LiteralExpression(valueToken);
            token = valueToken.Next;
        }
        else if (token is IdentifierToken identifierToken)
        {
            // Could be a variable reference or a function call
            if (identifierToken.Next is ParenthesisOpen)
            {
                // Nested function call: PRINT(FUNC())
                Token openParen = identifierToken.Next;
                var (arguments, tokenAfterArgs) = ParseFunctionArguments(openParen);

                FunctionCallExpression funcCall = new FunctionCallExpression
                {
                    FunctionName = identifierToken
                };
                funcCall.Arguments.AddRange(arguments);
                expression = funcCall;
                token = tokenAfterArgs;
            }
            else
            {
                // Simple variable reference
                expression = new IdentifierExpression(identifierToken);
                token = identifierToken.Next;
            }
        }

        return expression;
    }

    /// <summary>
    ///     Parses function arguments within parentheses.
    ///     Returns the list of argument expressions and the token after the closing parenthesis.
    /// </summary>
    private (List<Expression>, Token?) ParseFunctionArguments(Token openParen)
    {
        List<Expression> arguments = new();
        Token? current = openParen.Next;

        // Handle empty argument list: FUNC()
        if (current is ParenthesisClosed closeParen)
        {
            return (arguments, closeParen.Next);
        }

        // Parse arguments separated by commas
        while (current != null)
        {
            Expression? arg = ParseArgumentExpression(ref current);
            if (arg != null)
            {
                arguments.Add(arg);
            }

            // Check for both CommaToken and CommaSeparatorToken (token type inconsistency)
            if (current is CommaSeparatorToken or CommaToken)
            {
                current = current.Next; // Skip comma
                continue;
            }

            if (current is ParenthesisClosed)
            {
                return (arguments, current.Next);
            }

            // Unexpected token
            break;
        }

        return (arguments, current);
    }

    /// <summary>
    ///     Parses a single argument expression within a function call.
    /// </summary>
    private Expression? ParseArgumentExpression(ref Token? token)
    {
        if (token == null)
            return null;

        // Use ExpressionParser to handle all expression types including binary operations
        var parser = new ExpressionParser();
        var expression = parser.Parse(ref token);

        return expression;
    }
}

/// <summary>
///     Expression statement that executes an expression (typically a function call)
///     and discards the return value.
/// </summary>
public class ExpressionStatement : Statement
{
    public required Expression Expression { get; set; }

    public override string StatementType => "EXPRESSION";
}
