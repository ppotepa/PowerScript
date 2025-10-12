using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Operators;
using ppotepa.tokenez.Tree.Tokens.Scoping;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Processes IF keyword for conditional statements.
    ///     SQL-style syntax: IF condition { ... } or IF condition { ... } ELSE { ... }
    ///     Supports: >, <, >=, <=, ==, !=, AND, OR
    /// </summary>
    internal class IfStatementProcessor(ScopeBuilder scopeBuilder) : ITokenProcessor
    {
        private readonly ScopeBuilder _scopeBuilder = scopeBuilder;

        public bool CanProcess(Token token)
        {
            return token is IfKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(
                $"[IfStatementProcessor] Processing IF statement in scope '{context.CurrentScope.ScopeName}'");
            Console.ResetColor();

            IfKeywordToken? ifToken = token as IfKeywordToken;
            Token currentToken = ifToken!.Next;

            // Parse the condition expression
            Expression condition = ParseConditionExpression(ref currentToken);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[IfStatementProcessor] Condition: {condition}");
            Console.ResetColor();

            // Expect opening brace for THEN block
            if (currentToken is not ScopeStartToken)
            {
                throw new InvalidOperationException($"Expected {{ after IF condition, got {currentToken?.GetType().Name}");
            }

            // Create THEN scope
            string thenScopeName = $"IF_THEN_{context.CurrentScope.Statements.Count}";
            Scope thenScope = new(thenScopeName)
            {
                Type = ScopeType.Block,
                OuterScope = context.CurrentScope
            };

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[IfStatementProcessor] Created THEN scope: {thenScopeName}");
            Console.ResetColor();

            // Build the THEN scope body
            _scopeBuilder.BuildScope(currentToken, thenScope, context.Depth + 1);

            Scope? elseScope = null;

            // Check for optional ELSE keyword
            if (currentToken is ElseKeywordToken)
            {
                currentToken = currentToken.Next;

                // Expect opening brace for ELSE block
                if (currentToken is not ScopeStartToken)
                {
                    throw new InvalidOperationException($"Expected {{ after ELSE keyword, got {currentToken?.GetType().Name}");
                }

                // Create ELSE scope
                string elseScopeName = $"IF_ELSE_{context.CurrentScope.Statements.Count}";
                elseScope = new Scope(elseScopeName)
                {
                    Type = ScopeType.Block,
                    OuterScope = context.CurrentScope
                };

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[IfStatementProcessor] Created ELSE scope: {elseScopeName}");
                Console.ResetColor();

                // Build the ELSE scope body
                _scopeBuilder.BuildScope(currentToken, elseScope, context.Depth + 1);
            }

            // Create the IF statement
            IfStatement ifStatement = new(condition, thenScope, elseScope)
            {
                StartToken = token
            };
            context.CurrentScope.Statements.Add(ifStatement);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[IfStatementProcessor] IF statement created successfully");
            Console.ResetColor();

            // Find the closing brace and continue from there
            Token? nextToken = currentToken;
            int braceDepth = 0;

            // Skip past the THEN scope
            while (nextToken != null)
            {
                if (nextToken is ScopeStartToken)
                {
                    braceDepth++;
                }

                if (nextToken is ScopeEndToken)
                {
                    braceDepth--;
                    if (braceDepth == 0)
                    {
                        nextToken = nextToken.Next;
                        break;
                    }
                }

                nextToken = nextToken.Next;
            }

            // If there's an ELSE, skip past that scope too
            if (nextToken is ElseKeywordToken && elseScope != null)
            {
                nextToken = nextToken.Next; // Skip ELSE
                braceDepth = 0;
                while (nextToken != null)
                {
                    if (nextToken is ScopeStartToken)
                    {
                        braceDepth++;
                    }

                    if (nextToken is ScopeEndToken)
                    {
                        braceDepth--;
                        if (braceDepth == 0)
                        {
                            nextToken = nextToken.Next;
                            break;
                        }
                    }

                    nextToken = nextToken.Next;
                }
            }

            return TokenProcessingResult.Continue(nextToken!);
        }

        /// <summary>
        ///     Parse a condition expression with support for comparisons and logical operators.
        ///     Examples: a > b, x == y AND z < w, m != n OR p>= q
        /// </summary>
        private static Expression ParseConditionExpression(ref Token currentToken)
        {
            // Parse the left side of the condition
            Expression left = ParseComparisonExpression(ref currentToken);

            // Check for logical operators (AND, OR)
            while (currentToken is AndKeywordToken or OrKeywordToken)
            {
                Token logicalOp = currentToken;
                currentToken = currentToken.Next;

                Expression right = ParseComparisonExpression(ref currentToken);
                left = new LogicalExpression(left, logicalOp, right);
            }

            return left;
        }

        /// <summary>
        ///     Parse a comparison expression: leftValue operator rightValue
        ///     Operators: >, <, >=, <=, ==, !=
        ///     Handles == as two consecutive = tokens if needed
        /// </summary>
        private static BinaryExpression ParseComparisonExpression(ref Token currentToken)
        {
            // Parse left operand
            Expression left = ParseValue(ref currentToken);

            // Expect a comparison operator
            if (!IsComparisonOperator(currentToken))
            {
                throw new InvalidOperationException(
                    $"Expected comparison operator (>, <, >=, <=, ==, !=), got {currentToken?.GetType().Name}");
            }

            Token comparisonOp = currentToken;

            // Handle == as two EqualsToken (tokenizer fallback)
            if (currentToken is EqualsToken && currentToken.Next is EqualsToken)
            {
                currentToken = currentToken.Next; // Skip first =
                currentToken = currentToken.Next; // Skip second =, now at right operand
            }
            else
            {
                currentToken = currentToken.Next;
            }

            // Parse right operand
            Expression right = ParseValue(ref currentToken);

            return new BinaryExpression(left, comparisonOp, right);
        }

        /// <summary>
        ///     Parse a value (identifier, array element, number, or string literal)
        /// </summary>
        private static Expression ParseValue(ref Token currentToken)
        {
            if (currentToken is IdentifierToken identifierToken)
            {
                // Check for array indexing: identifier[index]
                if (identifierToken.Next is BracketOpen)
                {
                    currentToken = identifierToken.Next.Next; // Move past identifier and '['

                    // Parse the index expression
                    Expression indexExpr = ParseSimpleValue(ref currentToken);

                    // Expect closing bracket
                    if (currentToken is not BracketClosed)
                    {
                        throw new InvalidOperationException($"Expected ']' after array index, got {currentToken?.GetType().Name}");
                    }

                    currentToken = currentToken.Next; // Move past ']'

                    return new IndexExpression
                    {
                        ArrayIdentifier = identifierToken,
                        Index = indexExpr
                    };
                }

                IdentifierExpression expr = new(identifierToken);
                currentToken = currentToken.Next;
                return expr;
            }

            if (currentToken is ValueToken valueToken)
            {
                LiteralExpression expr = new(valueToken);
                currentToken = currentToken.Next;
                return expr;
            }

            if (currentToken is StringLiteralToken stringToken)
            {
                StringLiteralExpression expr = new(stringToken);
                currentToken = currentToken.Next;
                return expr;
            }

            throw new InvalidOperationException($"Expected identifier, number, or string, got {currentToken?.GetType().Name}");
        }

        /// <summary>
        ///     Parse a simple value for array index (identifier or number)
        /// </summary>
        private static Expression ParseSimpleValue(ref Token currentToken)
        {
            if (currentToken is ValueToken valueToken)
            {
                LiteralExpression expr = new(valueToken);
                currentToken = currentToken.Next;
                return expr;
            }

            if (currentToken is IdentifierToken identifierToken)
            {
                IdentifierExpression expr = new(identifierToken);
                currentToken = currentToken.Next;
                return expr;
            }

            throw new InvalidOperationException($"Expected value or identifier for array index, got {currentToken?.GetType().Name}");
        }

        /// <summary>
        ///     Check if the token is a comparison operator.
        ///     Handles == as two consecutive EqualsToken if needed.
        /// </summary>
        private static bool IsComparisonOperator(Token token)
        {
            if (token is EqualsToken && token.Next is EqualsToken)
            {
                // Handle == as two EqualsToken (tokenizer fallback)
                return true;
            }

            return token is GreaterThanToken
                or LessThanToken
                or GreaterThanOrEqualToken
                or LessThanOrEqualToken
                or EqualsEqualsToken
                or NotEqualsToken;
        }
    }
}