using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonValueSerializer : IIonTextSerializer<IIonValue>
    {
        public static string GrammarSymbol => ValueSymbol;

        #region Symbols
        internal const string ValueSymbol = "ion-value";
        internal const string NullSymbol = "ion-null";
        internal const string BoolSymbol = "ion-bool";
        internal const string IntSymbol = "ion-int";
        internal const string FloatSymbol = "ion-float";
        internal const string DecimalSymbol = "ion-decimal";
        internal const string TimestampSymbol = "ion-timestamp";
        internal const string StringSymbol = "ion-string";
        internal const string SymbolSymbol = "ion-symbol";
        internal const string BlobSymbol = "ion-blob";
        internal const string ClobSymbol = "ion-clob";
        internal const string SexpSymbol = "ion-sexp";
        internal const string ListSymbol = "ion-list";
        internal const string StructSymbol = "ion-struct";
        #endregion

        public static string Serialize(IIonValue value) => Serialize(value, new SerializingContext());

        public static string Serialize(IIonValue value, SerializingContext context)
        {
            return value switch
            {
                IonNull @null => IonNullSerializer.Serialize(@null, context),
                IonBool @bool => IonBoolSerializer.Serialize(@bool, context),
                IonInt @int => IonIntSerializer.Serialize(@int, context),
                IonFloat @float => IonFloatSerializer.Serialize(@float, context),
                IonDecimal @decimal => IonDecimalSerializer.Serialize(@decimal, context),
                IonTimestamp instant => IonTimestampSerializer.Serialize(instant, context),
                IonString @string => IonStringSerializer.Serialize(@string, context),
                IonOperator @operator => IonOperatorSerializer.Serialize(@operator, context),
                IonTextSymbol symbol => IonTextSymbolSerializer.Serialize(symbol, context),
                IonBlob blob => IonBlobSerializer.Serialize(blob, context),
                IonClob clob => IonClobSerializer.Serialize(clob, context),
                IonSexp sexp => IonSexpSerializer.Serialize(sexp, context),
                IonList list => IonListSerializer.Serialize(list, context),
                IonStruct @struct => IonStructSerializer.Serialize(@struct, context),
                _ => throw new ArgumentException($"Invalid ion type: '{value?.GetType()}'")
            };
        }

        public static IResult<IIonValue> Parse(CSTNode ionValueSymbol)
        {
            ArgumentNullException.ThrowIfNull(ionValueSymbol);

            return ionValueSymbol.SymbolName switch
            {
                ValueSymbol => ionValueSymbol
                    .FindNode(GetValueQueryString())
                    .ApplyTo(ParseValue),

                _ => Result.Of<IIonValue>(new ArgumentException(
                    $"Invalid symbol encountered: '{ionValueSymbol.SymbolName}'. "
                    + $"Expected '{ValueSymbol}'"))
            };
        }

        private static IResult<IIonValue> ParseValue(CSTNode valueSymbol)
        {
            return valueSymbol.SymbolName switch
            {
                NullSymbol => IonNullSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                BoolSymbol => IonBoolSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                IntSymbol => IonIntSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                FloatSymbol => IonFloatSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                DecimalSymbol => IonDecimalSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                TimestampSymbol => IonTimestampSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                StringSymbol => IonStringSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                SymbolSymbol => IonSymbolSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                BlobSymbol => IonBlobSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                ClobSymbol => IonClobSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                SexpSymbol => IonSexpSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                ListSymbol => IonListSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),
                StructSymbol => IonStructSerializer.Parse(valueSymbol).Map(value => (IIonValue)value),

                _ => Result.Of<IIonValue>(new ArgumentException(
                    $"Invalid symbol: '{valueSymbol.SymbolName}'. "
                    + $"Expected: '{NullSymbol}', or '{BoolSymbol}', etc..."))
            };
        }

        private static string GetValueQueryString()
        {
            return $"{NullSymbol}|{BoolSymbol}|{IntSymbol}|{FloatSymbol}|"
                + $"{DecimalSymbol}|{TimestampSymbol}|{StringSymbol}|{SymbolSymbol}|"
                + $"{BlobSymbol}|{ClobSymbol}|{SexpSymbol}|{ListSymbol}|{StructSymbol}";
        }
    }
}
