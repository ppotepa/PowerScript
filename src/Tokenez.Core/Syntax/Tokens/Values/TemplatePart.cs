namespace Tokenez.Core.Syntax.Tokens.Values
{
    /// <summary>
    ///     Represents a part of a template string - either literal text or a variable reference
    /// </summary>
    public class TemplatePart
    {
        public bool IsLiteral { get; set; }
        public string Text { get; set; } = "";
    }
}