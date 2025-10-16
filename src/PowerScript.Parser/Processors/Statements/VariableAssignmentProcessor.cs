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
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Scoping;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;
using PowerScript.Parser.Processors.Expressions;

namespace PowerScript.Parser.Processors.Statements;

/// <summary>
///     Processes variable assignment statements: identifier = value
///     This handles reassignment of existing variables (both FLEX and static typed).
///     Example: x = 10, name = "John", arr = CHAIN 5
/// </summary>
public class VariableAssignmentProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        // Check if this is an identifier followed by equals
        return token is IdentifierToken idToken && idToken.Next is EqualsToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        IdentifierToken identifierToken = (IdentifierToken)token;
        string variableName = identifierToken.RawToken?.Text ?? "";

        LoggerService.Logger.Debug(
            $"[VariableAssignmentProcessor] Processing assignment to '{variableName}' in scope '{context.CurrentScope.ScopeName}'");

        Token currentToken = identifierToken.Next!; // Should be EqualsToken

        if (currentToken is not EqualsToken)
        {
            throw new InvalidOperationException(
                $"Expected '=' after identifier '{variableName}', found {currentToken.GetType().Name}");
        }

        currentToken = currentToken.Next!; // Move past '='

        // Parse the value expression using ExpressionParser for full expression support
        var parser = new ExpressionParser();
        Expression valueExpression = parser.Parse(ref currentToken);

        // Check if the variable exists in scope (either as declared or dynamic)
        bool isDynamic = context.CurrentScope.IsDynamicVariable(variableName);
        bool isDeclared = IsVariableDeclared(context.CurrentScope, variableName);

        if (!isDynamic && !isDeclared)
        {
            throw new InvalidOperationException(
                $"Variable '{variableName}' is not declared. Use FLEX or a type keyword (INT, STRING, etc.) to declare it first.");
        }

        // For dynamic (FLEX) variables, create a simple variable declaration
        // For typed variables, use the existing declaration
        VariableDeclarationStatement statement;

        if (isDynamic)
        {
            // Create a new dynamic variable declaration for the assignment
            var dynamicDeclaration = new VariableDeclaration(null!, identifierToken);
            statement = new VariableDeclarationStatement(dynamicDeclaration, valueExpression)
            {
                StartToken = identifierToken
            };
        }
        else
        {
            // Use existing typed variable declaration - search parent scopes if needed
            var existingDeclaration = GetVariableDeclaration(context.CurrentScope, variableName);

            if (existingDeclaration is null)
            {
                throw new InvalidOperationException(
                    $"Cannot find declaration for variable '{variableName}'");
            }

            statement = new VariableDeclarationStatement(existingDeclaration, valueExpression)
            {
                StartToken = identifierToken
            };
        }

        context.CurrentScope.Statements.Add(statement);

        LoggerService.Logger.Success(
            $"[VariableAssignmentProcessor] Registered assignment statement for '{variableName}' in scope '{context.CurrentScope.ScopeName}'");

        return TokenProcessingResult.Continue(currentToken);
    }

    /// <summary>
    /// Parses a value expression (copied from FlexVariableProcessor logic)
    /// </summary>
    private Expression ParseValueExpression(ref Token token)
    {
        // Collect all tokens until we hit a statement terminator or scope change
        List<Token> expressionTokens = new();

        while (token != null &&
               token is not ScopeEndToken &&
               token is not ScopeStartToken &&
               !IsStatementStart(token))
        {
            // Stop if we hit a variable assignment pattern: Identifier =
            if (token is IdentifierToken && token.Next is EqualsToken)
            {
                break;
            }

            // Stop if we hit a member access pattern: Identifier ->
            if (token is IdentifierToken && token.Next is ArrowToken)
            {
                break;
            }

            expressionTokens.Add(token);
            token = token.Next!;
        }

        if (expressionTokens.Count == 0)
        {
            throw new InvalidOperationException("Expected value expression after '='");
        }

        // Use FlexVariableProcessor's expression building logic
        return BuildExpression(expressionTokens);
    }

    /// <summary>
    /// Check if token starts a new statement
    /// </summary>
    private bool IsStatementStart(Token token)
    {
        return token is PrintKeywordToken ||
               token is FlexKeywordToken ||
               token is ReturnKeywordToken ||
               token is IfKeywordToken ||
               token is CycleKeywordToken ||
               token is FunctionToken ||
               token is ExecuteKeywordToken;
    }

    /// <summary>
    /// Builds an expression from a list of tokens (simplified version)
    /// </summary>
    private Expression BuildExpression(List<Token> tokens)
    {
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("Cannot build expression from empty token list");
        }

        // Check for single token or function call
        if (tokens.Count == 1)
        {
            return tokens[0] switch
            {
                ValueToken vt => new LiteralExpression(vt),
                StringToken st => new StringLiteralExpression(st),
                StringLiteralToken slt => new StringLiteralExpression(slt),
                IdentifierToken it => new IdentifierExpression(it),
                _ => throw new InvalidOperationException($"Unexpected single token type: {tokens[0].GetType().Name}")
            };
        }

        // Check if the entire expression is just a function call: FUNC(args)
        if (tokens[0] is IdentifierToken idToken && tokens.Count >= 3 && tokens[1] is ParenthesisOpen)
        {
            // Check if this is ONLY a function call (ends with closing paren)
            int parenDepth = 0;
            bool foundClosingParen = false;
            for (int i = 1; i < tokens.Count; i++)
            {
                if (tokens[i] is ParenthesisOpen) parenDepth++;
                else if (tokens[i] is ParenthesisClosed)
                {
                    parenDepth--;
                    if (parenDepth == 0 && i == tokens.Count - 1)
                    {
                        foundClosingParen = true;
                        break;
                    }
                }
            }

            // If it's just a function call, parse it directly
            if (foundClosingParen)
            {
                return ParseFunctionCall(idToken);
            }
        }

        // Check for arrow operator pattern: identifier -> MemberName() or identifier -> Property
        if (tokens.Count >= 3 && tokens[1] is ArrowToken)
        {
            return ParseArrowExpression(tokens);
        }

        // For multi-token expressions with operators, create a binary expression
        Expression left = tokens[0] switch
        {
            ValueToken vt => new LiteralExpression(vt),
            StringToken st => new StringLiteralExpression(st),
            StringLiteralToken slt => new StringLiteralExpression(slt),
            IdentifierToken it when it.Next is ParenthesisOpen => ParseFunctionCall(it),
            IdentifierToken it => new IdentifierExpression(it),
            _ => throw new InvalidOperationException($"Unexpected token type: {tokens[0].GetType().Name}")
        };

        // If we parsed a function call as the left side, we need to skip past its tokens
        int startIndex = 1;
        if (tokens[0] is IdentifierToken && tokens[0].Next is ParenthesisOpen)
        {
            // Skip past the function call tokens
            int parenDepth = 0;
            for (int i = 1; i < tokens.Count; i++)
            {
                if (tokens[i] is ParenthesisOpen) parenDepth++;
                else if (tokens[i] is ParenthesisClosed)
                {
                    parenDepth--;
                    if (parenDepth == 0)
                    {
                        startIndex = i + 1;
                        break;
                    }
                }
            }
        }

        // Process remaining tokens as binary operations
        for (int i = startIndex; i < tokens.Count; i += 2)
        {
            if (i + 1 >= tokens.Count)
            {
                throw new InvalidOperationException("Expected value after operator");
            }

            Token operatorToken = tokens[i];
            Token rightToken = tokens[i + 1];

            Expression right = rightToken switch
            {
                ValueToken vt => new LiteralExpression(vt),
                StringToken st => new StringLiteralExpression(st),
                StringLiteralToken slt => new StringLiteralExpression(slt),
                IdentifierToken it when it.Next is ParenthesisOpen => ParseFunctionCall(it),
                IdentifierToken it => new IdentifierExpression(it),
                _ => throw new InvalidOperationException($"Unexpected token type: {rightToken.GetType().Name}")
            };

            // If we parsed a function call on the right side, skip past its tokens
            if (rightToken is IdentifierToken && rightToken.Next is ParenthesisOpen)
            {
                int parenDepth = 0;
                for (int j = i + 2; j < tokens.Count; j++)
                {
                    if (tokens[j] is ParenthesisOpen) parenDepth++;
                    else if (tokens[j] is ParenthesisClosed)
                    {
                        parenDepth--;
                        if (parenDepth == 0)
                        {
                            i = j - 1; // Will be incremented by 2 in the loop, so set to j-1
                            break;
                        }
                    }
                }
            }

            left = new BinaryExpression(left, operatorToken, right);
        }

        return left;
    }

    /// <summary>
    /// Parses a function call expression with arguments
    /// </summary>
    private FunctionCallExpression ParseFunctionCall(IdentifierToken functionNameToken)
    {
        Token openParen = functionNameToken.Next!;
        var (arguments, _) = ParseFunctionArguments(openParen);

        FunctionCallExpression funcCall = new FunctionCallExpression
        {
            FunctionName = functionNameToken
        };
        funcCall.Arguments.AddRange(arguments);

        return funcCall;
    }

    /// <summary>
    /// Parses function call arguments, supporting nested function calls
    /// </summary>
    private (List<Expression> Arguments, Token? NextToken) ParseFunctionArguments(Token openParenToken)
    {
        List<Expression> arguments = new();
        Token? currentToken = openParenToken.Next; // Start after '('

        // Handle empty argument list
        if (currentToken is ParenthesisClosed)
        {
            return (arguments, currentToken.Next);
        }

        // Parse comma-separated arguments
        while (currentToken != null && currentToken is not ParenthesisClosed)
        {
            // Collect tokens for this argument until we hit a comma or closing paren
            List<Token> argumentTokens = new();
            int parenDepth = 0;

            while (currentToken != null)
            {
                // Track parenthesis depth for nested expressions
                if (currentToken is ParenthesisOpen)
                {
                    parenDepth++;
                    argumentTokens.Add(currentToken);
                    currentToken = currentToken.Next;
                    continue;
                }

                if (currentToken is ParenthesisClosed)
                {
                    if (parenDepth == 0)
                    {
                        // End of argument list
                        break;
                    }
                    parenDepth--;
                    argumentTokens.Add(currentToken);
                    currentToken = currentToken.Next;
                    continue;
                }

                // Comma at depth 0 means end of this argument
                if (currentToken is CommaToken && parenDepth == 0)
                {
                    currentToken = currentToken.Next; // Skip comma
                    break;
                }

                argumentTokens.Add(currentToken);
                currentToken = currentToken.Next;
            }

            // Build expression from collected tokens
            if (argumentTokens.Count > 0)
            {
                Expression argExpr = BuildArgumentExpression(argumentTokens);
                arguments.Add(argExpr);
            }
        }

        // CurrentToken should now be at ParenthesisClosed
        Token? nextToken = currentToken is ParenthesisClosed ? currentToken.Next : currentToken;
        return (arguments, nextToken);
    }

    /// <summary>
    /// Builds an expression from argument tokens, supporting nested function calls
    /// </summary>
    private Expression BuildArgumentExpression(List<Token> tokens)
    {
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("Cannot build expression from empty token list");
        }

        // Ensure tokens are properly linked
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            if (tokens[i].Next != tokens[i + 1])
            {
                tokens[i].Next = tokens[i + 1];
            }
        }

        // Use the new ExpressionParser to handle nested function calls properly
        var parser = new ExpressionParser();
        Token currentToken = tokens[0];
        var expression = parser.Parse(ref currentToken);

        return expression;
    }

    /// <summary>
    /// Parses an arrow operator expression: object -> MemberName() or object -> Property
    /// </summary>
    private Expression ParseArrowExpression(List<Token> tokens)
    {
        // Pattern: identifier -> member() or identifier -> member
        // tokens[0] = base object (identifier)
        // tokens[1] = arrow operator
        // tokens[2] = member name (identifier)
        // tokens[3+] = optional parentheses and arguments

        if (tokens[0] is not IdentifierToken baseObject)
        {
            throw new InvalidOperationException($"Expected identifier before arrow operator, found {tokens[0].GetType().Name}");
        }

        if (tokens[2] is not IdentifierToken memberName)
        {
            throw new InvalidOperationException($"Expected member name after arrow operator, found {tokens[2].GetType().Name}");
        }

        Expression baseExpression = new IdentifierExpression(baseObject);
        string member = memberName.RawToken?.OriginalText ?? memberName.RawToken?.Text ?? "";

        // Check if there are parentheses (method call)
        if (tokens.Count > 3 && tokens[3] is ParenthesisOpen)
        {
            // Method call: parse arguments
            List<Expression> arguments = new();

            // Find matching closing parenthesis
            int openIndex = 3;
            int closeIndex = -1;
            int depth = 0;

            for (int i = openIndex; i < tokens.Count; i++)
            {
                if (tokens[i] is ParenthesisOpen) depth++;
                if (tokens[i] is ParenthesisClosed)
                {
                    depth--;
                    if (depth == 0)
                    {
                        closeIndex = i;
                        break;
                    }
                }
            }

            if (closeIndex == -1)
            {
                throw new InvalidOperationException("Unmatched parentheses in arrow expression");
            }

            // Parse arguments between parentheses (if any)
            if (closeIndex > openIndex + 1)
            {
                List<Token> argTokens = new();
                for (int i = openIndex + 1; i < closeIndex; i++)
                {
                    argTokens.Add(tokens[i]);
                }

                // Simple argument parsing - split by commas at depth 0
                List<List<Token>> argumentGroups = SplitByCommas(argTokens);
                foreach (var argGroup in argumentGroups)
                {
                    if (argGroup.Count > 0)
                    {
                        arguments.Add(BuildExpression(argGroup));
                    }
                }
            }

            return new NetMemberAccessExpression(baseExpression, member, arguments);
        }
        else
        {
            // Property access (no parentheses)
            return new NetMemberAccessExpression(baseExpression, member);
        }
    }

    /// <summary>
    /// Split tokens by commas at depth 0 (not inside parentheses)
    /// </summary>
    private List<List<Token>> SplitByCommas(List<Token> tokens)
    {
        List<List<Token>> groups = new();
        List<Token> current = new();
        int depth = 0;

        foreach (var token in tokens)
        {
            if (token is ParenthesisOpen)
            {
                depth++;
                current.Add(token);
            }
            else if (token is ParenthesisClosed)
            {
                depth--;
                current.Add(token);
            }
            else if (token is CommaToken && depth == 0)
            {
                if (current.Count > 0)
                {
                    groups.Add(current);
                    current = new();
                }
            }
            else
            {
                current.Add(token);
            }
        }

        if (current.Count > 0)
        {
            groups.Add(current);
        }

        return groups;
    }

    /// <summary>
    ///     Checks if a variable is declared in the current scope or any parent scope.
    ///     This enables proper variable resolution in nested scopes (IF blocks, CYCLE loops, etc.)
    ///     Also checks function parameters if the scope is a function scope.
    /// </summary>
    private static bool IsVariableDeclared(Scope scope, string variableName)
    {
        // Check current scope
        if (scope.Decarations.ContainsKey(variableName))
        {
            return true;
        }

        // Check if it's a function parameter
        if (scope.FunctionDeclaration != null &&
            scope.FunctionDeclaration.Parameters.Any(p => p.Identifier.RawToken?.Text == variableName))
        {
            return true;
        }

        // Check parent scope recursively
        return scope.OuterScope != null && IsVariableDeclared(scope.OuterScope, variableName);
    }

    /// <summary>
    ///     Gets the variable declaration from the current scope or any parent scope.
    ///     Returns null if variable is not found in scope chain.
    ///     Also checks function parameters if the scope is a function scope.
    ///     For parameters, creates a VariableDeclaration wrapper to enable assignment.
    /// </summary>
    private static VariableDeclaration? GetVariableDeclaration(Scope scope, string variableName)
    {
        // Check current scope
        if (scope.Decarations.TryGetValue(variableName, out Declaration? declaration))
        {
            return declaration as VariableDeclaration;
        }

        // Check if it's a function parameter
        if (scope.FunctionDeclaration != null)
        {
            var param = scope.FunctionDeclaration.Parameters.FirstOrDefault(p => p.Identifier.RawToken?.Text == variableName);
            if (param != null)
            {
                // Parameters are ParameterDeclaration, but for assignment we can treat them like variables
                // Create a VariableDeclaration wrapper if needed
                if (param is ParameterDeclaration paramDecl)
                {
                    return new VariableDeclaration(paramDecl.DeclarativeType, paramDecl.Identifier, paramDecl.BitWidth);
                }
                return param as VariableDeclaration;
            }
        }

        // Check parent scope recursively
        if (scope.OuterScope != null)
        {
            return GetVariableDeclaration(scope.OuterScope, variableName);
        }

        return null;
    }
}
