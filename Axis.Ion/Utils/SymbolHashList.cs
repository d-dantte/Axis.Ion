using Axis.Ion.IO.Axion.Payload;
using Axis.Ion.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Ion.Utils
{
    /// <summary>
    /// NOTE:
    /// 1. null-symbols should NEVER be added to this hash-list
    /// 2. annotations MUST be stripped away from symbols before performing any operation on them in this class
    /// </summary>
    public class SymbolHashList : IEnumerable<IonTextSymbol>
    {
        private readonly Dictionary<int, IonTextSymbol> _isMap = new Dictionary<int, IonTextSymbol>();
        private readonly Dictionary<IonTextSymbol, int> _siMap = new Dictionary<IonTextSymbol, int>();

        public int Count => _isMap.Count;

        public bool IsEmpty => Count == 0;

        public SymbolHashList()
        {
        }

        public SymbolHashList(IEnumerable<IonTextSymbol> types)
        : this(types?.ToArray() ?? throw new ArgumentNullException(nameof(types)))
        {
        }

        public SymbolHashList(params IonTextSymbol[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));

            foreach (var item in types)
                Add(item);
        }

        public bool Remove(IonTextSymbol value)
        {
            value = StripAnnotations(value);

            if (!_siMap.ContainsKey(value))
                return false;

            var index = _siMap[value];

            _siMap.Remove(value);
            _isMap.Remove(index);

            return true;
        }

        public bool Remove(int index)
        {
            if (!_isMap.ContainsKey(index))
                return false;

            var value = _isMap[index];

            _siMap.Remove(value);
            _isMap.Remove(index);

            return true;
        }

        public int Add(IonTextSymbol value)
        {
            value = StripAnnotations(value);
            _ = TryAdd(value, out var index);
            return index;
        }

        /// <summary>
        /// Adds the given symbol to the list.
        /// </summary>
        /// <param name="value">The symbol to add</param>
        /// <param name="index">It's index within the list</param>
        /// <returns>True if the symbol was absent before this call, otherwise false</returns>
        public bool TryAdd(IonTextSymbol value, out int index)
        {
            value = StripAnnotations(value);

            // value already exists, return false, and out it's index
            if (_siMap.TryGetValue(value, out index))
                return false;

            // add the value
            index = _siMap.Count;

            _siMap.Add(value, index);
            _isMap.Add(index, value);

            return true;
        }

        public IonTextSymbol this[int index]
        {
            get => _isMap[index];
        }

        public int IndexOf(IonTextSymbol value)
        {
            value = StripAnnotations(value);

            return _siMap.TryGetValue(value, out var index)
                ? index
                : -1;
        }

        public bool Contains(IonTextSymbol value)
        {
            value = StripAnnotations(value);
            return _siMap.ContainsKey(value);
        }

        public bool TryGetSymbol(int index, out IonTextSymbol symbol)
        {
            if (!_isMap.TryGetValue(index, out symbol))
                return false;

            return true;
        }

        public bool TryGetIndex(IonTextSymbol symbol, out int index)
        {
            if (!_siMap.TryGetValue(symbol, out index))
                return false;

            return true;
        }

        public bool TryGetSymbolID(
            IonTextSymbol symbol,
            out IonSymbolPayload.IonSymbolID id)
        {
            id = TryGetIndex(symbol, out var index)
                ? new IonSymbolPayload.IonSymbolID(index)
                : default;

            return !id.IsNull;
        }

        private IonTextSymbol StripAnnotations(IonTextSymbol symbol)
        {
            if (symbol.Annotations.Length == 0)
                return symbol;

            return new IonTextSymbol(symbol.Value);
        }

        #region IEnumerable
        public IEnumerator<IonTextSymbol> GetEnumerator() => _siMap.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
