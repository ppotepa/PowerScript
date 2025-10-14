
using Tokenez.Common.Logging;
using Tokenez.Parser.Processors.Base;
using Tokenez.Core.Exceptions;
using Tokenez.Core.AST;
using Tokenez.Core.AST.Expressions;
using Tokenez.Core.AST.Statements;
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Delimiters;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Keywords;
using Tokenez.Core.Syntax.Tokens.Scoping;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Parser.Processors
{
    /// <summary>
    ///     Processes CYCLE keyword for loops.
    ///     Syntax:
    ///     Count-based: CYCLE 5 { ... } or CYCLE 10 AS i { ... }
    ///     Collection-based: CYCLE IN collection { ... } or CYCLE IN collection AS variableName { ... }
    ///     Automatic index variables: A, B, C, D, ... based on nesting level (when AS is not specified)
    /// </summary>
    public class CycleLoopProcessor(IScopeBuilder scopeBuilder) : ITokenProcessor
    {
        private readonly IScopeBuilder _scopeBuilder = scopeBuilder;

        public bool CanProcess(Token token)
        {
            return token is CycleKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            LoggerService.Logger.Info(
                $"[CycleLoopProcessor] Processing CYCLE loop in scope '{context.CurrentScope.ScopeName}'");

            CycleKeywordToken? cycleToken = token as CycleKeywordToken;
            Token? currentToken = cycleToken!.Next;

            Expression collectionExpression;
            string loopVariableName;
            int nestingLevel = context.CycleNestingDepth;
            bool isCountBased = false;

            // Check if this is a count-based loop (CYCLE <number>), expression-based (CYCLE n-2), or collection-based (CYCLE IN)
            if (currentToken is ValueToken countToken)
            {
                // Count-based loop: CYCLE 5 { ... } or CYCLE 10 AS i { ... }
                isCountBased = true;
                collectionExpression = new LiteralExpression(countToken);
                loopVariableName = GetAutomaticIndexName(nestingLevel);

                LoggerService.Logger.Debug($"Count-based loop: {countToken.RawToken?.Text} iterations");

                currentToken = currentToken.Next;

                // Check for AS keyword to customize loop variable name
                if (currentToken is AsKeywordToken)
                {
                    currentToken = currentToken.Next;

                    if (currentToken is not IdentifierToken customNameToken)
                    {
                        throw new UnexpectedTokenException(currentToken, typeof(IdentifierToken));
                    }

                    loopVariableName = customNameToken.RawToken!.Text;
                    currentToken = currentToken.Next;

                    LoggerService.Logger.Debug($"Custom loop variable name: {loopVariableName}");
                }
            }
            else if (currentToken is IdentifierToken)
            {
                // Expression-based loop: CYCLE n - 2 { ... } or simple identifier: CYCLE iterations { ... }
                isCountBased = true;
                var startToken = currentToken;
                var expressionTokens = new List<Token>();

                // Collect tokens that could be part of an expression
                while (currentToken is not ScopeStartToken && currentToken is not AsKeywordToken && currentToken != null)
                {
                    expressionTokens.Add(currentToken);
                    currentToken = currentToken.Next;
                }

                // If we only got one identifier, treat as simple variable reference
                if (expressionTokens.Count == 1 && expressionTokens[0] is IdentifierToken singleIdent)
                {
                    collectionExpression = new IdentifierExpression(singleIdent);
                    LoggerService.Logger.Debug($"Variable-based loop count: {singleIdent.RawToken?.Text}");
                }
                else
                {
                    // Build expression from tokens (e.g., "n - 2", "len / 2")
                    collectionExpression = BuildExpressionFromTokens(expressionTokens);
                    LoggerService.Logger.Debug($"Expression-based loop count: {string.Join(" ", expressionTokens.Select(t => t.RawToken?.Text))}");
                }

                loopVariableName = GetAutomaticIndexName(nestingLevel);

                // Check for AS keyword to customize loop variable name
                if (currentToken is AsKeywordToken)
                {
                    currentToken = currentToken.Next;

                    if (currentToken is not IdentifierToken customNameToken)
                    {
                        throw new UnexpectedTokenException(currentToken, typeof(IdentifierToken));
                    }

                    loopVariableName = customNameToken.RawToken!.Text;
                    currentToken = currentToken.Next;

                    LoggerService.Logger.Debug($"Custom loop variable name: {loopVariableName}");
                }
            }
            else if (currentToken is InKeywordToken)
            {
                // Collection-based loop: CYCLE IN collection { ... }
                currentToken = currentToken.Next;

                // Parse the collection expression
                collectionExpression = ParseCollectionExpression(ref currentToken);

                LoggerService.Logger.Debug($"Collection expression: {collectionExpression}");

                loopVariableName = GetAutomaticIndexName(nestingLevel);

                // Check for AS keyword to rename the loop variable
                if (currentToken is AsKeywordToken)
                {
                    currentToken = currentToken.Next;

                    if (currentToken is not IdentifierToken customNameToken)
                    {
                        throw new UnexpectedTokenException(currentToken, typeof(IdentifierToken));
                    }

                    loopVariableName = customNameToken.RawToken!.Text;
                    currentToken = currentToken.Next;

                    LoggerService.Logger.Debug($"Custom loop variable name: {loopVariableName}");
                }
                else
                {
                    LoggerService.Logger.Debug(
                        $"Automatic loop variable: {loopVariableName} (nesting level {nestingLevel})");
                }
            }
            else
            {
                throw new UnexpectedTokenException(currentToken, typeof(ValueToken), typeof(InKeywordToken));
            }

            // Expect opening brace for loop body
            if (currentToken is not ScopeStartToken)
            {
                if (currentToken == null)
                {
                    throw new UnexpectedTokenException(cycleToken, "Unexpected end of tokens, expected { to start loop body", typeof(ScopeStartToken));
                }
                throw new UnexpectedTokenException(currentToken, typeof(ScopeStartToken));
            }

            // Create the loop statement
            CycleLoopStatement loopStatement = new(collectionExpression, loopVariableName, nestingLevel)
            {
                StartToken = cycleToken,
                IsCountBased = isCountBased
            };

            // Create a new scope for the loop body
            Scope loopScope = new($"CYCLE_{loopVariableName}")
            {
                Type = ScopeType.Block,
                OuterScope = context.CurrentScope
            };

            // Register the loop variable as a dynamic variable in the loop scope
            loopScope.AddDynamicVariable(loopVariableName);

            // Create a new context with incremented cycle nesting depth for nested loops
            ProcessingContext loopContext = context.Clone();
            loopContext.CurrentScope = loopScope;
            loopContext.CycleNestingDepth = context.CycleNestingDepth + 1;

            // Build the loop body scope
            _scopeBuilder.BuildScope(currentToken, loopScope, loopContext);

            loopStatement.LoopBody = loopScope;

            // Add the loop statement to the current scope
            context.CurrentScope.Statements.Add(loopStatement);

            LoggerService.Logger.Success(
                $"[CycleLoopProcessor] Registered CYCLE loop with variable '{loopVariableName}' in scope '{context.CurrentScope.ScopeName}'");

            // Find the closing brace to continue processing
            Token? nextToken = currentToken;
            int braceDepth = 1;
            while (nextToken != null && braceDepth > 0)
            {
                nextToken = nextToken.Next;
                if (nextToken is ScopeStartToken)
                {
                    braceDepth++;
                }

                if (nextToken is ScopeEndToken)
                {
                    braceDepth--;
                }
            }

            return nextToken == null
                ? throw new UnexpectedTokenException(cycleToken, "Unmatched braces in CYCLE loop - missing closing }", typeof(ScopeEndToken))
                : new TokenProcessingResult
                {
                    NextToken = nextToken.Next ?? nextToken,
                    ShouldValidateExpectations = false
                };
        }

        /// <summary>
        ///     Parses the collection expression (identifier, function call, or property access)
        /// </summary>
        private static Expression ParseCollectionExpression(ref Token? token)
        {
            if (token is IdentifierToken identifierToken)
            {
                // Simple identifier or function call
                if (identifierToken.Next is ParenthesisOpen)
                {
                    // Function call returning a collection
                    FunctionCallExpression funcCallExpr = new()
                    {
                        FunctionName = identifierToken
                    };

                    token = identifierToken.Next; // Move to (
                    token = token!.Next; // Move past (

                    // Function call arguments are not yet supported in CYCLE loops

                    // Skip to )
                    while (token is not null and not ParenthesisClosed)
                    {
                        token = token.Next;
                    }

                    if (token is ParenthesisClosed)
                    {
                        token = token.Next;
                    }

                    return funcCallExpr;
                }

                // Simple collection variable
                IdentifierExpression expr = new(identifierToken);
                token = token.Next;
                return expr;
            }

            throw new NotImplementedException(
                $"Collection expression type {token?.GetType().Name} not yet supported in CYCLE loops");
        }

        /// <summary>
        ///     Gets the automatic index variable name based on nesting level
        ///     Level 0 = 'A', Level 1 = 'B', Level 2 = 'C', etc.
        /// </summary>
        private static string GetAutomaticIndexName(int nestingLevel)
        {
            if (nestingLevel < 26)
            {
                char indexChar = (char)('A' + nestingLevel);
                return indexChar.ToString();
            }

            // For deep nesting, use AA, AB, AC, etc.
            int firstChar = (nestingLevel / 26) - 1;
            int secondChar = nestingLevel % 26;
            return $"{(char)('A' + firstChar)}{(char)('A' + secondChar)}";
        }

        /// <summary>
        ///     Builds a binary expression from a list of tokens (e.g., "n - 2", "len / 2")
        ///     Supports simple binary expressions with +, -, *, /, %
        /// </summary>
        private static Expression BuildExpressionFromTokens(List<Token> tokens)
        {
            if (tokens.Count == 0)
                throw new InvalidOperationException("Cannot build expression from empty token list");

            if (tokens.Count == 1)
            {
                // Single token - should be identifier or value
                if (tokens[0] is IdentifierToken ident)
                    return new IdentifierExpression(ident);
                if (tokens[0] is ValueToken val)
                    return new LiteralExpression(val);
                throw new UnexpectedTokenException(tokens[0], "Expected identifier or value");
            }

            // Simple binary expression: <left> <operator> <right>
            // For now, support only 3-token expressions
            if (tokens.Count == 3)
            {
                Expression left;
                if (tokens[0] is IdentifierToken leftIdent)
                    left = new IdentifierExpression(leftIdent);
                else if (tokens[0] is ValueToken leftVal)
                    left = new LiteralExpression(leftVal);
                else
                    throw new UnexpectedTokenException(tokens[0], "Expected identifier or value as left operand");

                Expression right;
                if (tokens[2] is IdentifierToken rightIdent)
                    right = new IdentifierExpression(rightIdent);
                else if (tokens[2] is ValueToken rightVal)
                    right = new LiteralExpression(rightVal);
                else
                    throw new UnexpectedTokenException(tokens[2], "Expected identifier or value as right operand");

                return new BinaryExpression(left, tokens[1], right);
            }

            throw new NotImplementedException($"Complex expressions with {tokens.Count} tokens not yet supported in CYCLE");
        }
    }
}
