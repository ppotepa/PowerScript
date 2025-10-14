using Tokenez.Core.AST;
using Tokenez.Core.Syntax.Tokens.Base;

namespace Tokenez.Parser.Processors.Base;

/// <summary>
///     Interface for building scope hierarchies by processing tokens.
/// </summary>
public interface IScopeBuilder
{
    /// <summary>
    ///     Builds a complete scope by processing tokens from start to end.
    /// </summary>
    /// <param name="startToken">The starting token</param>
    /// <param name="scope">The scope to build</param>
    /// <param name="depth">The nesting depth (default 0)</param>
    /// <returns>The built scope</returns>
    Scope BuildScope(Token startToken, Scope scope, int depth = 0);

    /// <summary>
    ///     Builds a complete scope by processing tokens from start to end with an existing context.
    /// </summary>
    /// <param name="startToken">The starting token</param>
    /// <param name="scope">The scope to build</param>
    /// <param name="context">The processing context</param>
    /// <returns>The built scope</returns>
    Scope BuildScope(Token startToken, Scope scope, ProcessingContext context);
}