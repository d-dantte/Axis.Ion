using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Common.Results;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Globalization;
using static Axis.Ion.IO.Text.SerializerOptions;

namespace Axis.Ion.IO.Text.Streamers
{
    public class IonTimestampSerializer : AbstractIonTypeSerializer<IonTimestamp>
    {
        public const string IonTimestampSymbol = "ion-timestamp";
        public const string NullTimestampSymbol = "null-timestamp";
        public const string MilliSecondSymbol = "millisecond-precision";
        public const string SecondSymbol = "second-precision";
        public const string MinuteSymbol = "minute-precision";
        public const string DaySymbol = "day-precision";
        public const string MonthSymbol = "month-precision";
        public const string YearSymbol = "year-precision";

        private static readonly string YearFormat = "yyyyT";
        private static readonly string MonthFormat = "yyyy-MMT";
        private static readonly string DayFormat = "yyyy-MM-dd";
        private static readonly string MinuteFormat = "yyyy-MM-ddTHH:mmzzz";
        private static readonly string SecondFormat = "yyyy-MM-ddTHH:mm:sszzz";
        private static readonly string MillisecondFormat = "yyyy-MM-ddTHH:mm:ss.ffffffzzz";

        protected override string IonValueSymbolName => IonTimestampSymbol;

        public override string GetIonValueText(IonTimestamp ionValue, SerializingContext context)
        {
            if (ionValue.Value is null)
                return "null.timestamp";

            var timestamp = ionValue.Value.Value;
            return context.Options.Timestamps.TimestampPrecision switch
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
        }

        public override bool TryParse(CSTNode tokenNode, out IResult<IonTimestamp> result)
        {
            if (tokenNode is null)
                throw new ArgumentNullException(nameof(tokenNode));

            try
            {
                if (!IonTimestampSymbol.Equals(tokenNode.SymbolName))
                    return $"Invalid token node name: {tokenNode.SymbolName}".FailWithMessage(out result);

                (var annotations, var timestampToken) = DeconstructAnnotations(tokenNode);

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

                    _ => throw new InvalidOperationException(
                        $"Invalid ion bool token: {tokenNode.FirstNode().SymbolName}")
                };

                return new IonTimestamp(timestamp, annotations).PassWithValue(out result);
            }
            catch (Exception e)
            {
                result = Result.Of<IonTimestamp>(e);
                return false;
            }
        }
    }
}
