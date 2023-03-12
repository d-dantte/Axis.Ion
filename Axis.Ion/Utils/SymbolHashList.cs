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
    public class SymbolHashList : IEnumerable<IIonTextSymbol>
    {
        private readonly Dictionary<int, IIonTextSymbol> _isMap = new Dictionary<int, IIonTextSymbol>();
        private readonly Dictionary<IIonTextSymbol, int> _siMap = new Dictionary<IIonTextSymbol, int>();

        public int Count => _isMap.Count;

        public bool IsEmpty => Count == 0;

        public SymbolHashList()
        {
        }

        public SymbolHashList(IEnumerable<IIonTextSymbol> types)
        :this(types?.ToArray() ?? throw new ArgumentNullException(nameof(types)))
        {
        }

        public SymbolHashList(params IIonTextSymbol[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));

            foreach (var item in types)
                Add(item);
        }

        public bool Remove(IIonTextSymbol value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

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

        public int Add(IIonTextSymbol value)
        {
            value = StripAnnotations(value);
            _ = TryAdd(value, out var index);
            return index;
        }

        public bool TryAdd(IIonTextSymbol value, out int index)
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

        public IIonTextSymbol this[int index]
        {
            get => _isMap[index];
        }

        public int IndexOf(IIonTextSymbol value)
        {
            value = StripAnnotations(value);

            return _siMap.TryGetValue(value, out var index)
                ? index
                : -1;
        }

        public bool Contains(IIonTextSymbol value)
        {
            value = StripAnnotations(value);
            return _siMap.ContainsKey(value);
        }

        public bool TryGetSymbol(int index, out IIonTextSymbol symbol)
        {
            if (!_isMap.TryGetValue(index, out symbol))
                return false;

            return true;
        }

        public bool TryGetIndex(IIonTextSymbol symbol, out int index)
        {
            if (!_siMap.TryGetValue(symbol, out index))
                return false;

            return true;
        }

        public bool TryGetSymbolID(
            IIonTextSymbol symbol,
            out IonSymbolPayload.IonSymbolID id)
        {
            id = TryGetIndex(symbol, out var index)
                ? new IonSymbolPayload.IonSymbolID(symbol.Type, index)
                : default;

            return !id.IsNull;
        }

        /// <summary>
        /// Adds the given symbol to the symbol-table following these rules:
        /// <list type="number">
        /// <item>If the symbol doesn't exist in the table, add it and return the same item</item>
        /// <item>If the symbol exists in the table, return the symbol-id for that symbol</item>
        /// </list>
        /// </summary>
        /// <param name="symbol">the symbol</param>
        /// <returns>The symbol, or it's ID depending on the state of the symbol-table</returns>
        public IIonTextSymbol AddOrGetID(IIonTextSymbol symbol)
        {
            if (symbol is IonSymbolPayload.IonSymbolID)
                throw new InvalidOperationException($"Invalid symbol type: {typeof(IonSymbolPayload.IonSymbolID)}");

            if (symbol.IsNull)
                return symbol;

            var stripped = StripAnnotations(symbol);

            if (_siMap.TryGetValue(stripped, out var index))
                return new IonSymbolPayload.IonSymbolID(symbol.Type, index);

            _ = Add(symbol);
            return symbol;
        }

        private IIonTextSymbol StripAnnotations(IIonTextSymbol symbol)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            if (symbol is not IonIdentifier
                && symbol is not IonQuotedSymbol)
                throw new ArgumentException($"Invalid symbol type: {(symbol?.GetType().Name ?? "")}");

            if (symbol.Annotations.Length == 0)
                return symbol;

            // else
            return symbol switch
            {
                IonIdentifier id => new IonIdentifier(id.Value),
                IonQuotedSymbol quoted => new IonQuotedSymbol(quoted.Value),
                _ => throw new ArgumentException($"Invalid text-symbol: {symbol.GetType()}")
            };
        }

        #region IEnumerable
        public IEnumerator<IIonTextSymbol> GetEnumerator() => _siMap.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
