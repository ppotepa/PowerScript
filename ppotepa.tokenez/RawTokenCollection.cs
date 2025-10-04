using System.Collections;
using System.Collections.Specialized;

namespace ppotepa.tokenez
{
    internal class RawTokenCollection : ICollection<RawToken>
    {
        private RawToken[] _tokens = default;

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
            return (IEnumerator<RawToken>)_tokens.GetEnumerator().Current;
        }

        public bool Remove(RawToken item)
        {
            var any = _tokens.Any(token => token != item);

            if (any)
            {
                _tokens = [.. _tokens.Where(token => token != item)];
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}