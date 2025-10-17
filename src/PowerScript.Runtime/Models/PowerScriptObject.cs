namespace PowerScript.Runtime.Models;

/// <summary>
/// Represents a PowerScript object at runtime.
/// Supports property access, type annotation, and strict/flexible typing.
/// Example: {name = "John", age = 30} or {x = 1, y = 2} as Point
/// </summary>
public class PowerScriptObject
{
    private readonly Dictionary<string, object?> _properties = new();

    /// <summary>Optional type name for this object (e.g., "Person", "Point")</summary>
    public string? TypeName { get; }

    /// <summary>
    /// Whether this is a strict type (non-extendable).
    /// Strict types (marked with !) cannot have new properties added after creation.
    /// </summary>
    public bool IsStrict { get; }

    public PowerScriptObject(Dictionary<string, object?> properties, string? typeName = null, bool isStrict = false)
    {
        _properties = new Dictionary<string, object?>(properties);
        TypeName = typeName;
        IsStrict = isStrict;
    }

    /// <summary>Gets all property names in this object</summary>
    public IEnumerable<string> PropertyNames => _properties.Keys;

    /// <summary>Gets the number of properties in this object</summary>
    public int PropertyCount => _properties.Count;

    /// <summary>Checks if a property exists in this object</summary>
    public bool HasProperty(string name)
    {
        return _properties.ContainsKey(name.ToUpper());
    }

    /// <summary>Gets the value of a property</summary>
    public object? GetProperty(string name)
    {
        var key = name.ToUpper();
        if (_properties.TryGetValue(key, out var value))
        {
            return value;
        }

        throw new InvalidOperationException($"Property '{name}' not found on object{(TypeName != null ? $" of type '{TypeName}'" : "")}");
    }

    /// <summary>Sets the value of a property</summary>
    public void SetProperty(string name, object? value)
    {
        var key = name.ToUpper();

        // Check if we're adding a new property to a strict type
        if (!_properties.ContainsKey(key) && IsStrict)
        {
            throw new InvalidOperationException(
                $"Cannot add property '{name}' to strict object of type '{TypeName}'. " +
                $"Strict types (marked with !) are not extendable.");
        }

        _properties[key] = value;
    }

    /// <summary>Tries to get a property value without throwing an exception</summary>
    public bool TryGetProperty(string name, out object? value)
    {
        return _properties.TryGetValue(name.ToUpper(), out value);
    }

    /// <summary>Returns a string representation of this object</summary>
    public override string ToString()
    {
        var props = string.Join(", ", _properties.Select(p => $"{p.Key} = {p.Value}"));
        var typeAnnotation = TypeName != null ? $" as {TypeName}{(IsStrict ? "!" : "")}" : "";
        return $"{{{props}}}{typeAnnotation}";
    }

    /// <summary>Creates a copy of this object with all property values</summary>
    public PowerScriptObject Clone()
    {
        return new PowerScriptObject(
            new Dictionary<string, object?>(_properties),
            TypeName,
            IsStrict
        );
    }
}
