using Axis.Luna.Common;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using Axis.Pulsar.Grammar.Recognizers;
using Axis.Pulsar.Grammar.Recognizers.Results;
using System;
using System.Collections;
using System.Text;

namespace Axis.Ion.IO.Text
{
    internal static class TextExtensions
    {
        internal static bool FailWithMessage<TValue>(this
            string message,
            out IResult<TValue> result)
        {
            result = Result.Of<TValue>(new Exception(message));
            return false;
        }

        internal static bool PassWithValue<TValue>(this
            TValue value,
            out IResult<TValue> result)
        {
            result = Result.Of(value);
            return true;
        }

        internal static bool TryParseRecognition<T>(this
            IRecognitionResult recognition,
            TryParseFunc<T> tryParseFunc,
            out IResult<T> result)
        {
            return recognition switch
            {
                SuccessResult success => tryParseFunc.Invoke(success.Symbol, out result),
                ErrorResult error => error.Exception.Message.FailWithMessage(out result),
                FailureResult failure => failure.Reason switch
                {
                    IReason.AggregationFailure aggregation => TextExtensions
                        .FailWithMessage(
                            $"Recognition thresholds were not met at position: {failure.Position}",
                            out result),

                    IReason.ValidationFailure validation => FailWithMessage(
                        $"A validation error occured at position: {failure.Position}",
                        out result),

                    IReason.TokenMisMatch => FailWithMessage(
                        $"Token mismatch occured at position: {failure.Position}",
                        out result),

                    _ => throw new InvalidOperationException($"Unknown recognition failure reason: {failure.Reason}")
                },
                _ => throw new InvalidOperationException($"Unknown recognition result: {recognition}")
            };
        }

        #region Nested types
        internal delegate bool TryParseFunc<T>(CSTNode node, out IResult<T> result);
        #endregion
    }
}
