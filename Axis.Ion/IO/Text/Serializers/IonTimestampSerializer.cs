using Axis.Ion.Types;
using Axis.Luna.Common.Results;
using Axis.Pulsar.Grammar.CST;
using System;
using static Axis.Ion.IO.Text.SerializerOptions;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonTimestampSerializer : IIonTextSerializer<IonTimestamp>
    {
        public static string GrammarSymbol => IonTimestampSymbol;

        #region Symbols
        public const string IonTimestampSymbol = "ion-timestamp";
        public const string NullTimestampSymbol = "null-timestamp";
        public const string MilliSecondSymbol = "millisecond-precision";
        public const string SecondSymbol = "second-precision";
        public const string MinuteSymbol = "minute-precision";
        public const string DaySymbol = "day-precision";
        public const string MonthSymbol = "month-precision";
        public const string YearSymbol = "year-precision";
        #endregion

        #region Formats
        private static readonly string YearFormat = "yyyyT";
        private static readonly string MonthFormat = "yyyy-MMT";
        private static readonly string DayFormat = "yyyy-MM-dd";
        private static readonly string MinuteFormat = "yyyy-MM-ddTHH:mmzzz";
        private static readonly string SecondFormat = "yyyy-MM-ddTHH:mm:sszzz";
        private static readonly string MillisecondFormat = "yyyy-MM-ddTHH:mm:ss.ffffffzzz";
        #endregion

        public static IResult<IonTimestamp> Parse(CSTNode symbolNode)
        {
            ArgumentNullException.ThrowIfNull(symbolNode);

            try
            {
                if (!IonTimestampSymbol.Equals(symbolNode.SymbolName))
                    return Result.Of<IonTimestamp>(new ArgumentException(
                        $"Invalid symbol name: '{symbolNode.SymbolName}'. "
                        + $"Expected '{IonTimestampSymbol}'"));

                (var annotations, var timestampToken) = IonTextSerializer.DeconstructAnnotations(symbolNode);

                var culture = CultureInfo.InvariantCulture;
                DateTimeOffset? timestamp = timestampToken.SymbolName switch
                {
                    NullTimestampSymbol => null,

                    YearSymbol => DateTimeOffset.ParseExact(
                            timestampToken.TokenValue(),
                            YearFormat,
                            culture),

                    MonthSymbol => DateTimeOffset.ParseExact(
                            timestampToken.TokenValue(),
                            MonthFormat,
                            culture),

                    DaySymbol => DateTimeOffset.ParseExact(
                            timestampToken.TokenValue(),
                            DayFormat,
                            culture),

                    MinuteSymbol => DateTimeOffset.ParseExact(
                            timestampToken.TokenValue(),
                            MinuteFormat,
                            culture),

                    SecondSymbol => DateTimeOffset.ParseExact(
                            timestampToken.TokenValue(),
                            SecondFormat,
                            culture),

                    MilliSecondSymbol => DateTimeOffset.ParseExact(
                            timestampToken.TokenValue(),
                            MillisecondFormat,
                            culture),

                    _ => throw new ArgumentException(
                        $"Invalid symbol encountered: '{timestampToken.SymbolName}'. "
                        + $"Expected '{NullTimestampSymbol}', '{YearSymbol}', '{MonthSymbol}', etc")
                };

                return Result.Of(new IonTimestamp(timestamp, annotations));
            }
            catch (Exception e)
            {
                return Result.Of<IonTimestamp>(e);
            }
        }

        public static string Serialize(IonTimestamp value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonTimestamp value, SerializingContext context)
        {
            var annotationText = IonAnnotationSerializer.Serialize(value.Annotations, context);

            if (value.Value is null)
                return $"{annotationText}null.timestamp";

            var timestamp = value.Value.Value;
            var text = context.Options.Timestamps.TimestampPrecision switch
            {
                TimestampPrecision.Year => timestamp.ToString(YearFormat),
                TimestampPrecision.Month => timestamp.ToString(MonthFormat),
                TimestampPrecision.Day => timestamp.ToString(DayFormat),
                TimestampPrecision.Minute => timestamp.ToString(MinuteFormat),
                TimestampPrecision.Second => timestamp.ToString(SecondFormat),
                TimestampPrecision.MilliSecond => timestamp.ToString(MillisecondFormat),
                _ => throw new InvalidOperationException(
                    $"Invalid timestamp precision: {context.Options.Timestamps.TimestampPrecision}")
            };

            return $"{annotationText}{text}";
        }
    }
}
