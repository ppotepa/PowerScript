
using ppotepa.tokenez.Logging;
using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Scoping;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Processes CYCLE keyword for loops.
    ///     Syntax:
    ///     Count-based: CYCLE 5 { ... } or CYCLE 10 AS i { ... }
    ///     Collection-based: CYCLE IN collection { ... } or CYCLE IN collection AS variableName { ... }
    ///     Automatic index variables: A, B, C, D, ... based on nesting level (when AS is not specified)
    /// </summary>
    internal class CycleLoopProcessor(ScopeBuilder scopeBuilder) : ITokenProcessor
    {
        private readonly ScopeBuilder _scopeBuilder = scopeBuilder;

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

            // Check if this is a count-based loop (CYCLE <number>) or collection-based (CYCLE IN)
            if (currentToken is ValueToken countToken)
            {
                // Count-based loop: CYCLE 5 { ... } or CYCLE 10 AS i { ... }
                isCountBased = true;
                collectionExpression = new LiteralExpression(countToken);
                loopVariableName = GetAutomaticIndexName(nestingLevel);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[CycleLoopProcessor] Count-based loop: {countToken.RawToken?.Text} iterations");
                Console.ResetColor();

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

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[CycleLoopProcessor] Custom loop variable name: {loopVariableName}");
                    Console.ResetColor();
                }
            }
            else if (currentToken is InKeywordToken)
            {
                // Collection-based loop: CYCLE IN collection { ... }
                currentToken = currentToken.Next;

                // Parse the collection expression
                collectionExpression = ParseCollectionExpression(ref currentToken);

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[CycleLoopProcessor] Collection expression: {collectionExpression}");
                Console.ResetColor();

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

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[CycleLoopProcessor] Custom loop variable name: {loopVariableName}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(
                        $"[CycleLoopProcessor] Automatic loop variable: {loopVariableName} (nesting level {nestingLevel})");
                    Console.ResetColor();
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
    }
}