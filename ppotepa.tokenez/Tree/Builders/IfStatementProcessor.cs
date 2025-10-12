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
    internal class IfStatementProcessor : ITokenProcessor
    {
        private readonly ScopeBuilder _scopeBuilder;
        private readonly ExpectationValidator _validator;

        public IfStatementProcessor(ExpectationValidator validator, ScopeBuilder scopeBuilder)
        {
            _validator = validator;
            _scopeBuilder = scopeBuilder;
        }

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

            var ifToken = token as IfKeywordToken;
            var currentToken = ifToken!.Next;

            // Parse the condition expression
            var condition = ParseConditionExpression(ref currentToken);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[IfStatementProcessor] Condition: {condition}");
            Console.ResetColor();

            // Expect opening brace for THEN block
            if (currentToken is not ScopeStartToken)
                throw new Exception($"Expected {{ after IF condition, got {currentToken?.GetType().Name}");

            // Create THEN scope
            var thenScopeName = $"IF_THEN_{context.CurrentScope.Statements.Count}";
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
                    throw new Exception($"Expected {{ after ELSE keyword, got {currentToken?.GetType().Name}");

                // Create ELSE scope
                var elseScopeName = $"IF_ELSE_{context.CurrentScope.Statements.Count}";
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
            var nextToken = currentToken;
            var braceDepth = 0;

            // Skip past the THEN scope
            while (nextToken != null)
            {
                if (nextToken is ScopeStartToken) braceDepth++;

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
                    if (nextToken is ScopeStartToken) braceDepth++;

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
        private Expression ParseConditionExpression(ref Token currentToken)
        {
            // Parse the left side of the condition
            var left = ParseComparisonExpression(ref currentToken);

            // Check for logical operators (AND, OR)
            while (currentToken is AndKeywordToken or OrKeywordToken)
            {
                var logicalOp = currentToken;
                currentToken = currentToken.Next;

                var right = ParseComparisonExpression(ref currentToken);
                left = new LogicalExpression(left, logicalOp, right);
            }

            return left;
        }

        /// <summary>
        ///     Parse a comparison expression: leftValue operator rightValue
        ///     Operators: >, <, >=, <=, ==, !=
        ///     Handles == as two consecutive = tokens if needed
        /// </summary>
        private Expression ParseComparisonExpression(ref Token currentToken)
        {
            // Parse left operand
            var left = ParseValue(ref currentToken);

            // Expect a comparison operator
            if (!IsComparisonOperator(currentToken))
                throw new Exception(
                    $"Expected comparison operator (>, <, >=, <=, ==, !=), got {currentToken?.GetType().Name}");

            var comparisonOp = currentToken;

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
            var right = ParseValue(ref currentToken);

            return new BinaryExpression(left, comparisonOp, right);
        }

        /// <summary>
        ///     Parse a value (identifier, array element, number, or string literal)
        /// </summary>
        private Expression ParseValue(ref Token currentToken)
        {
            if (currentToken is IdentifierToken identifierToken)
            {
                // Check for array indexing: identifier[index]
                if (identifierToken.Next is BracketOpen)
                {
                    currentToken = identifierToken.Next.Next; // Move past identifier and '['

                    // Parse the index expression
                    var indexExpr = ParseSimpleValue(ref currentToken);

                    // Expect closing bracket
                    if (currentToken is not BracketClosed)
                        throw new Exception($"Expected ']' after array index, got {currentToken?.GetType().Name}");

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

            throw new Exception($"Expected identifier, number, or string, got {currentToken?.GetType().Name}");
        }

        /// <summary>
        ///     Parse a simple value for array index (identifier or number)
        /// </summary>
        private Expression ParseSimpleValue(ref Token currentToken)
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

            throw new Exception($"Expected value or identifier for array index, got {currentToken?.GetType().Name}");
        }

        /// <summary>
        ///     Check if the token is a comparison operator.
        ///     Handles == as two consecutive EqualsToken if needed.
        /// </summary>
        private bool IsComparisonOperator(Token token)
        {
            if (token is EqualsToken && token.Next is EqualsToken)
                // Handle == as two EqualsToken (tokenizer fallback)
                return true;

            return token is GreaterThanToken
                or LessThanToken
                or GreaterThanOrEqualToken
                or LessThanOrEqualToken
                or EqualsEqualsToken
                or NotEqualsToken;
        }
    }
}