using System.Collections;

namespace Tokenez.Core.Syntax.Tokens.Raw;

public class RawTokenCollection(RawToken[] tokens) : ICollection<RawToken>
{
    private RawToken[] _tokens = tokens;

    public int Count => _tokens.Length + 1;

    public bool IsReadOnly => true;

    public void Add(RawToken item)
    {
        _tokens = [.. _tokens, item];
    }

    public void Clear()
    {
        _tokens = [];
    }

    public bool Contains(RawToken item)
    {
        return _tokens.Contains(item);
    }

    public void CopyTo(RawToken[] array, int arrayIndex)
    {
        _tokens = [.. array.Skip(arrayIndex).Take(_tokens.Length - arrayIndex)];
    }

    public IEnumerator<RawToken> GetEnumerator()
    {
        return ((IEnumerable<RawToken>)_tokens).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Remove(RawToken item)
    {
        bool any = _tokens.Any(token => token != item);

        if (any)
        {
            _tokens = [.. _tokens.Where(token => token != item)];
            return true;
        }

        return false;
    }
}