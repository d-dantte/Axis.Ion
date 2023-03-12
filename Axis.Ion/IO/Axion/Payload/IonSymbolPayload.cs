using Axis.Ion.Types;
using Axis.Ion.Utils;
using Axis.Luna.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Axis.Ion.IO.Axion.Payload
{
    public readonly struct IonSymbolPayload : ITypePayload
    {
        #region TextSymbol Custom data
        public const byte RawTextSymbolMask = 63;
        public const byte Int8IDMask = 1;
        public const byte Int16IDMask = 2;
        public const byte IntUnlimitedIDMask = 3;
        #endregion

        #region Operator Custom data
        public const byte Char1Mask = 0;
        public const byte Char2Mask = 1;
        public const byte Char3Mask = 2;
        public const byte CharXMask = 3;
        #endregion

        public IonSymbolPayload(IonOperator symbol)
        {
            IonType = symbol;
            Metadata = SerializeSymbolMetadata(symbol);
        }

        public IonSymbolPayload(IIonTextSymbol symbol)
        {
            if (symbol is IonSymbolID && symbol.IsNull)
                throw new ArgumentException("Cannot create a payload from a null-symbolID");

            IonType = symbol;
            Metadata = SerializeSymbolMetadata(symbol);
        }

        private static byte SerializeSymbolMetadata(IIonType type)
        {
            var metadata = TypeMetadata.SerializeMetadata(type);

            return metadata |= type switch
            {
                // ID
                IonSymbolID id => (id.ID?.GetByteCount() ?? 0) switch
                {
                    1 => Int8IDMask << 6,
                    2 => Int16IDMask << 6,
                    > 2 => IntUnlimitedIDMask << 6,

                    // note that  (for now) null symbol ids (id.IsNull) are not allowed.
                    0 => throw new InvalidOperationException("null symbol ids are not allowed"),
                    _ => throw new InvalidOperationException($"Invalid id byte count: {id.ID?.GetByteCount() ?? 0}")
                },

                // operator
                IonOperator @operator => (@operator.Value?.Length ?? 0) switch
                {
                    0 => 0, // null operator
                    1 => 0, // Char1Mask << 6,
                    2 => Char2Mask << 6,
                    3 => Char3Mask << 6,
                    > 3 => CharXMask << 6,
                    _ => throw new InvalidOperationException($"Invalid operator character count: {@operator.Value?.Length ?? 0}")
                },

                // text symbol
                IIonTextSymbol => 0,

                // unknown symbol types
                _ => throw new InvalidOperationException($"Invalid ion-type: {type.GetType()}")
            };
        }

        private static bool IsQuotedSymbol(TypeMetadata metadata) => metadata.IonType == IonTypes.QuotedSymbol;

        #region Read
        public static bool TryRead(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable,
            out IonSymbolPayload payload)
        {
            try
            {
                payload = Read(stream, metadata, options, symbolTable);
                return true;
            }
            catch
            {
                payload = default;
                return false;
            }
        }

        public static IonSymbolPayload Read(
            Stream stream,
            TypeMetadata metadata,
            SerializerOptions options,
            SymbolHashList symbolTable)
        {
            if (metadata.IonType != IonTypes.OperatorSymbol
                && metadata.IonType != IonTypes.IdentifierSymbol
                && metadata.IonType != IonTypes.QuotedSymbol)
                throw new ArgumentException($"Invalid symbol type-metadata: {metadata.IonType}.");

            // annotations
            var annotations = metadata.HasAnnotations
                ? TypeMetadata.ReadAnnotations(stream, options, symbolTable)
                : Array.Empty<IIonType.Annotation>();

            // null?
            if (metadata.IsNull)
            {
                var nullSymbol = IIonType.NullOf(metadata.IonType, annotations);
                return metadata.IonType switch
                {
                    IonTypes.OperatorSymbol => new IonSymbolPayload((IonOperator)nullSymbol),
                    IonTypes.IdentifierSymbol => new IonSymbolPayload((IonIdentifier)nullSymbol),
                    IonTypes.QuotedSymbol => new IonSymbolPayload((IonQuotedSymbol)nullSymbol),
                    _ => throw new InvalidOperationException($"Invalid symbol metadata type: {metadata.IonType}")
                };
            }

            // non-null?
            else
            {
                return metadata.IonType switch
                {
                    // operators
                    IonTypes.OperatorSymbol => metadata.CustomMetadata switch
                    {
                        0 or 1 or 2 => !stream.TryReadExactBytes(metadata.CustomMetadata + 1, out var bytes)
                            ? throw new EndOfStreamException()
                            : new IonSymbolPayload(
                                new IonOperator(
                                    bytes.Select(b => (Operators)b).ToArray(),
                                    annotations)),

                        3 => !stream.TryReadVarByteInteger(out var chars)
                            ? throw new EndOfStreamException()
                            : chars.ToByteArray().ApplyTo(bytes =>
                                new IonSymbolPayload(
                                    new IonOperator(
                                        bytes.Select(b => (Operators)b).ToArray(),
                                        annotations))),

                        _ => throw new InvalidOperationException($"Invalid custom data: {metadata.CustomMetadata}")
                    },

                    _ => metadata.CustomMetadata switch
                    {
                        // text symbols
                        0 => !stream.TryReadVarByteInteger(out var textCount)
                            ? throw new EndOfStreamException()
                            : !stream.TryReadExactBytes(2 * textCount.CastToInt(), out var bytes)
                                ? throw new EndOfStreamException()
                                : new IonSymbolPayload(
                                    // this SHOULD always be the first time adding the symbol to the table
                                    symbolTable.AddOrGetID(
                                        IIonTextSymbol.Parse(
                                            Encoding.Unicode.GetString(bytes).WrapIf(_ => IsQuotedSymbol(metadata), "'"),
                                            annotations))),

                        // IDs
                        1 or 2 => !stream.TryReadExactBytes(metadata.CustomMetadata, out var bytes)
                            ? throw new EndOfStreamException()
                            : !symbolTable.TryGetSymbol(new BigInteger(bytes).CastToInt(), out var symbol)
                                ? throw new InvalidOperationException($"symbol ID not found in symbol table: {new BigInteger(bytes)}")
                                : new IonSymbolPayload(IIonTextSymbol.Parse(symbol.ToIonText(), annotations)),

                        3 => !stream.TryReadVarByteInteger(out var id)
                            ? throw new EndOfStreamException()
                            : !symbolTable.TryGetSymbol(id.CastToInt(), out var symbol)
                                ? throw new InvalidOperationException($"symbol ID not found in symbol table: {id}")
                                : new IonSymbolPayload(IIonTextSymbol.Parse(symbol.ToIonText(), annotations)),

                        _ => throw new InvalidOperationException($"Invalid custom data: {metadata.CustomMetadata}")
                    }
                };
            }
        }
        #endregion

        #region ITypePayload

        public TypeMetadata Metadata { get; }

        public IIonType IonType { get; }

        public byte[] SerializeData(
            SerializerOptions options,
            SymbolHashList hashList)
        {
            if (IonType.IsNull)
                return Array.Empty<byte>();

            return IonType switch
            {
                // id
                IonSymbolID id => Metadata.CustomMetadata switch
                {
                    1 or 2 => id.ID?
                        .ToByteArray()
                        ?? Array.Empty<byte>(),

                    3 => id.ID?
                        .ToVarBytes()
                        ?? Array.Empty<byte>(),

                    _ => throw new InvalidOperationException($"Invalid symbol id custom-metadata: {Metadata.CustomMetadata}")
                },

                // operator
                IonOperator @operator => Metadata.CustomMetadata switch
                {
                    0 or 1 or 2 => @operator.ToIonText()
                        .Select(op => (byte)op)
                        .ToArray(),

                    3 => @operator.ToIonText()
                        .Select(op => (byte)op)
                        .ToArray()
                        .ApplyTo(bytes => new BigInteger(bytes))
                        .ToVarBytes(),

                    _ => throw new InvalidOperationException($"Invalid operator id custom-metadata: {Metadata.CustomMetadata}")
                },

                // IonTextSymbol
                IIonTextSymbol text => (text.Value?.Length ?? 0)
                    .ToVarBytes() // storing text length, not byte length
                    .Concat(Encoding.Unicode.GetBytes(text.Value ?? ""))
                    .ToArray(),

                // unknown symbol type
                _ => throw new InvalidOperationException($"Invalid ion-type: {IonType.GetType()}")
            };
        }

        #endregion

        #region Nested types
        public readonly struct IonSymbolID : IIonTextSymbol
        {
            public BigInteger? ID { get; }

            public IonSymbolID(IonTypes type, BigInteger id)
            {
                if (id < 0)
                    throw new ArgumentException("ID cannot be less than 0");

                ID = id;
                Type = type;
            }

            #region IIonTextSymbol
            public string? Value => ID?.ToString();

            public IonTypes Type { get; }

            public bool IsNull => ID is null;

            public IIonType.Annotation[] Annotations => Array.Empty<IIonType.Annotation>();

            public string ToIonText() => ToString();

            public bool ValueEquals(IRefValue<string> other)
                => Type == other.Type && Value.NullOrEquals(other.Value);
            #endregion

            #region Record Implementation
            public override int GetHashCode() => HashCode.Combine(Value, Type);

            public override bool Equals(object? obj)
            {
                return obj is IonSymbolID other
                    && ValueEquals(other);
            }

            public override string ToString() => Value ?? "";


            public static bool operator ==(IonSymbolID first, IonSymbolID second) => first.Equals(second);

            public static bool operator !=(IonSymbolID first, IonSymbolID second) => !first.Equals(second);
            #endregion
        }
        #endregion
    }
}
