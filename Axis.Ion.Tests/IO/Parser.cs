using Axis.Ion.IO;
using Axis.Pulsar.Grammar;
using Axis.Pulsar.Grammar.CST;
using Axis.Pulsar.Grammar.Exceptions;
using Axis.Pulsar.Grammar.Recognizers.Results;
using Axis.Pulsar.Languages.xBNF;

namespace Axis.Ion.Tests.IO
{
    [TestClass]
    public class Parser
    {
        [TestMethod]
        public void GrammarImportationTests()
        {
            try
            {
                using var ionXbnfStream = typeof(IonIO).Assembly
                    .GetManifestResourceStream($"{typeof(IonIO).Namespace}.IonGrammar.xbnf");

                var importer = new Importer();
                var ionGrammar = importer
                    .ImportGrammar(ionXbnfStream);

                Assert.IsNotNull(ionGrammar);
                Assert.AreEqual("ion", ionGrammar.RootSymbol);
            }
            catch(Exception e)
            {
                // do stuff
                throw;
            }
        }

        [TestMethod]
        public void TestAnnotation()
        {
            using var ionXbnfStream = typeof(IonIO).Assembly
                .GetManifestResourceStream($"{typeof(IonIO).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var annotationParser = ionGrammar.GetRecognizer("annotation-list");
            Assert.IsNotNull(annotationParser);

            var result = annotationParser.Recognize(new BufferedTokenReader("abcd::"));
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("abcd::", success.Symbol.TokenValue());

            result = annotationParser.Recognize(new BufferedTokenReader("'some thing'::abcd::"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("'some thing'::abcd::", success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestNull()
        {
            using var ionXbnfStream = typeof(IonIO).Assembly
                .GetManifestResourceStream($"{typeof(IonIO).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var nullParser = ionGrammar.GetRecognizer("ion-null");
            Assert.IsNotNull(nullParser);

            var result = nullParser.Recognize(new BufferedTokenReader("null.null"));
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.null", success.Symbol.TokenValue());

            result = nullParser.Recognize(new BufferedTokenReader("null"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null", success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestBool()
        {
            using var ionXbnfStream = typeof(IonIO).Assembly
                .GetManifestResourceStream($"{typeof(IonIO).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-bool");
            Assert.IsNotNull(recognizer);

            var result = recognizer.Recognize(new BufferedTokenReader("null.bool"));
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.bool", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("false"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("false", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("False"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("False", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("TRUE"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("TRUE", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("truE"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("truE", success.Symbol.TokenValue());


            // fail
            result = recognizer.Recognize(new BufferedTokenReader("NotABool"));
            var fail = result as FailureResult;
            Assert.IsNotNull(fail);
        }

        [TestMethod]
        public void TestInt()
        {
            using var ionXbnfStream = typeof(IonIO).Assembly
                .GetManifestResourceStream($"{typeof(IonIO).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-int");
            Assert.IsNotNull(recognizer);

            var result = recognizer.Recognize(new BufferedTokenReader("null.int"));
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.int", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("67"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("67", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("-10_000"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("-10_000", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("0x1000765e-6776"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("0x1000765e", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("e34"));
            var failure = result as FailureResult;
            Assert.IsNotNull(failure);
        }

        [TestMethod]
        public void TestFloat()
        {
            using var ionXbnfStream = typeof(IonIO).Assembly
                .GetManifestResourceStream($"{typeof(IonIO).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-float");
            Assert.IsNotNull(recognizer);

            var result = recognizer.Recognize(new BufferedTokenReader("null.float"));
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.float", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("nan"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("nan", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("+inf"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("+inf", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("-inf"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("-inf", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("67.0E6"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("67.0E6", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("1_100.1e-5"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("1_100.1e-5", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("65e-2"));
            var failure = result as FailureResult;
            Assert.IsNotNull(failure);
        }

        [TestMethod]
        public void TestDecimal()
        {
            using var ionXbnfStream = typeof(IonIO).Assembly
                .GetManifestResourceStream($"{typeof(IonIO).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-decimal");
            Assert.IsNotNull(recognizer);
            Assert.IsNotNull(recognizer);

            var result = recognizer.Recognize(new BufferedTokenReader("null.decimal"));
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.decimal", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("0."));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("0.", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("0.0"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("0.0", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("0d0"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("0d0", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("0d-0"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("0d-0", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("-324.222"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("-324.222", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("-6.9887D-6"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("-6.9887D-6", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("-6.9_887E-6"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("-6.9_887", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("-6"));
            var failure = result as FailureResult;
            Assert.IsNotNull(failure);

            result = recognizer.Recognize(new BufferedTokenReader("0"));
            failure = result as FailureResult;
            Assert.IsNotNull(failure);
        }

        [TestMethod]
        public void TestTimestamp()
        {
            using var ionXbnfStream = typeof(IonIO).Assembly
                .GetManifestResourceStream($"{typeof(IonIO).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-timestamp");
            Assert.IsNotNull(recognizer);
            Assert.IsNotNull(recognizer);

            var result = recognizer.Recognize(new BufferedTokenReader("null.timestamp"));
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.timestamp", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("2007-02-23T12:14Z"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("2007-02-23T12:14Z", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("2007-02-23T12:14:33.079-08:00"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("2007-02-23T12:14:33.079-08:00", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("2007-02-23T20:14:33.079Z"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("2007-02-23T20:14:33.079Z", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("2007-02-23T20:14:33.079-00:00"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("2007-02-23T20:14:33.079-00:00", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("2007-01-01T00:00-00:00"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("2007-01-01T00:00-00:00", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("2007-01-01"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("2007-01-01", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("2007-01-01T"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("2007-01-01T", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("2007-01T"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("2007-01T", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader("2007T"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("2007T", success.Symbol.TokenValue());

            // failure
            result = recognizer.Recognize(new BufferedTokenReader("2007"));
            var failure = result as FailureResult;
            Assert.IsNotNull(failure);

            result = recognizer.Recognize(new BufferedTokenReader("2007-11"));
            failure = result as FailureResult;
            Assert.IsNotNull(failure);

            result = recognizer.Recognize(new BufferedTokenReader("2007-02-23T20:14:33.Z"));
            var error = result as ErrorResult;
            Assert.IsNotNull(error);
            var pex = error.Exception as PartialRecognitionException;
            Assert.IsNotNull(pex);
            var aggFailure = pex.FailureReason as IReason.AggregationFailure;
            Assert.IsNotNull(aggFailure);
            Assert.AreEqual(12, aggFailure.AggregationCount);
        }

        [TestMethod]
        public void TestString()
        {
            using var ionXbnfStream = typeof(IonIO).Assembly
                .GetManifestResourceStream($"{typeof(IonIO).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-string");
            Assert.IsNotNull(recognizer);
            Assert.IsNotNull(recognizer);

            var result = recognizer.Recognize(new BufferedTokenReader("null.string"));
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.string", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader(@""" """));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual(@""" """, success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader(@""""""));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual(@"""""", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader(@"""abcd""bleh"));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual(@"""abcd""", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader(@"""\"""));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual(@"""\""", success.Symbol.TokenValue());
        }
    }
}
