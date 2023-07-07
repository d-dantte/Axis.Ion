using Axis.Pulsar.Grammar.Recognizers;
using System;

namespace Axis.Ion.IO.Text
{
    public class ParseException: Exception
    {
        public IRecognitionResult RecognitionResult { get; }

        public ParseException(IRecognitionResult recognitionResult)
        : base("An error occured while tokenizing the string")
        {
            RecognitionResult = recognitionResult;
        }
    }
}
