using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Serializers;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using Axis.Pulsar.Grammar.Recognizers.Results;
using System;

namespace Axis.Ion.IO
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IIonTextSerializer<TValue>
    {
        /// <summary>
        /// The grammar symbol for this parser
        /// </summary>
        abstract static string GrammarSymbol { get; }

        /// <summary>
        /// Parse the given <see cref="CSTNode"/> into the appropriate <typeparamref name="TValue"/>
        /// </summary>
        /// <param name="symbolNode">the recognized symbol node</param>
        /// <returns>result of the parse operation</returns>
        abstract static IResult<TValue> Parse(CSTNode symbolNode);

        /// <summary>
        /// Serialize the given value into text form
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        abstract static string Serialize(TValue value);

        /// <summary>
        /// Serialize the given value into text form
        /// </summary>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        abstract static string Serialize(TValue value, SerializingContext context);
    }

    /// <summary>
    /// 
    /// </summary>
    public static class IonTextSerializer
    {
        /// <summary>
        /// Parse the given text into the appropriate <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <param name="result">The result of the parsing operation</param>
        /// <returns>true if successfully parsed, false otherwise</returns>
        public static bool TryParse<TValue>(
            string text,
            out IResult<TValue> result)
            => (result = Parse<TValue>(text)) is IResult<TValue>.DataResult;

        /// <summary>
        /// Parse the given text into the appropriate <typeparamref name="TValue"/>
        /// </summary>
        /// <param name="text">the text</param>
        /// <returns>result of the parse operation</returns>
        public static IResult<TValue> Parse<TValue>(string text)
        {
            var valueType = typeof(TValue);

            if (valueType == typeof(IonPacket))
                return IonTextSerializer
                    .Parse(text, IonPacketSerializer.GrammarSymbol, IonPacketSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IIonValue))
                return IonTextSerializer
                    .Parse(text, IonValueSerializer.GrammarSymbol, IonValueSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IIonValue.Annotation[]))
                return IonTextSerializer
                    .Parse(text, IonAnnotationSerializer.GrammarSymbol, IonAnnotationSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonNull))
                return IonTextSerializer
                    .Parse(text, IonNullSerializer.GrammarSymbol, IonNullSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonBool))
                return IonTextSerializer
                    .Parse(text, IonBoolSerializer.GrammarSymbol, IonBoolSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonInt))
                return IonTextSerializer
                    .Parse(text, IonIntSerializer.GrammarSymbol, IonIntSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonFloat))
                return IonTextSerializer
                    .Parse(text, IonFloatSerializer.GrammarSymbol, IonFloatSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonDecimal))
                return IonTextSerializer
                    .Parse(text, IonDecimalSerializer.GrammarSymbol, IonDecimalSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonTimestamp))
                return IonTextSerializer
                    .Parse(text, IonTimestampSerializer.GrammarSymbol, IonTimestampSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonString))
                return IonTextSerializer
                    .Parse(text, IonStringSerializer.GrammarSymbol, IonStringSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonTextSymbol))
                return IonTextSerializer
                    .Parse(text, IonTextSymbolSerializer.GrammarSymbol, IonTextSymbolSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonOperator))
                return IonTextSerializer
                    .Parse(text, IonOperatorSerializer.GrammarSymbol, IonOperatorSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonClob))
                return IonTextSerializer
                    .Parse(text, IonClobSerializer.GrammarSymbol, IonClobSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonBlob))
                return IonTextSerializer
                    .Parse(text, IonBlobSerializer.GrammarSymbol, IonBlobSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonSexp))
                return IonTextSerializer
                    .Parse(text, IonSexpSerializer.GrammarSymbol, IonSexpSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonList))
                return IonTextSerializer
                    .Parse(text, IonListSerializer.GrammarSymbol, IonListSerializer.Parse)
                    .Map(value => value.As<TValue>());

            if (valueType == typeof(IonStruct))
                return IonTextSerializer
                    .Parse(text, IonStructSerializer.GrammarSymbol, IonStructSerializer.Parse)
                    .Map(value => value.As<TValue>());

            else return Result.Of<TValue>(new ArgumentException($"Invalid value type: {valueType}"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="text"></param>
        /// <param name="grammarSymbol"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static IResult<TValue> Parse<TValue>(
            string text,
            string grammarSymbol,
            Func<CSTNode, IResult<TValue>> parser)
        {
            ArgumentNullException.ThrowIfNull(parser);

            var parseResult = IonGrammar.Grammar
                .GetRecognizer(grammarSymbol)
                .Recognize(text);

            return parseResult switch
            {
                SuccessResult success => parser.Invoke(success.Symbol),
                null => Result.Of<TValue>(new Exception("Unknown Error")),
                _ => Result.Of<TValue>(new ParseException(parseResult))
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string Serialize<TValue>(
            TValue value,
            SerializingContext context)
        {
            return value switch
            {
                IIonValue.Annotation[] annotations => IonAnnotationSerializer.Serialize(annotations, context),
                IonPacket packet => IonPacketSerializer.Serialize(packet, context),
                IIonValue ionValue => IonValueSerializer.Serialize(ionValue, context),
                _ => throw new ArgumentException($"Invalid value type: '{value?.GetType()}'")
            };
        }


        #region helpers

        internal static (IIonValue.Annotation[] annotations, CSTNode valueToken) DeconstructAnnotations(
            CSTNode annotatedTypeToken)
        {
            var annotationToken = annotatedTypeToken.FirstNode();
            return annotationToken.SymbolName.Equals(IonAnnotationSerializer.GrammarSymbol)
                ? (IonAnnotationSerializer.Parse(annotationToken).Resolve(), annotatedTypeToken.NodeAt(1))
                : (Array.Empty<IIonValue.Annotation>(), annotationToken);
        }
        #endregion
    }
}
