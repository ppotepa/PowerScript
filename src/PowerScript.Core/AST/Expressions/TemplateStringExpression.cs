using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.AST.Expressions;

/// <summary>
///     Represents a template string expression with variable interpolation.
///     Example: `Hello @name, you are @age years old`
///     At runtime, @variable references are replaced with their values.
/// </summary>
public class TemplateStringExpression : Expression
{
    public TemplateStringExpression(TemplateStringToken template)
    {
        Template = template;
        StartToken = template;
    }

    /// <summary>The template string token containing the parts</summary>
    public TemplateStringToken Template { get; set; }

    public override string ExpressionType { get; set; } = "TemplateString";

    public override string ToString()
    {
        return $"TemplateString(`{Template.TemplateText}`)";
    }
}