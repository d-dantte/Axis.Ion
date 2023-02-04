using Axis.Ion.IO.Text;
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
                using var ionXbnfStream = typeof(TextSerializer).Assembly
                    .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

                var importer = new Importer();
                var ionGrammar = importer
                    .ImportGrammar(ionXbnfStream);

                Assert.IsNotNull(ionGrammar);
                Assert.AreEqual("ion", ionGrammar.RootSymbol);
            }
            catch (Exception e)
            {
                // do stuff
                throw;
            }
        }

        [TestMethod]
        public void TestAnnotation()
        {
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

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
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

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
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

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
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

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
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

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
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-decimal");
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
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-timestamp");
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
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-string");
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

            result = recognizer.Recognize(new BufferedTokenReader(@"""\ua342"""));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual(@"""\ua342""", success.Symbol.TokenValue());

            result = recognizer.Recognize(new BufferedTokenReader(@"""
stuff
other stuff
multiline stuff
"""));
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual(@"""
stuff
other stuff
multiline stuff
""", success.Symbol.TokenValue());

            result = recognizer.Recognize(@"('''bleh''' '''other bleh''')");
            result = recognizer.Recognize(@"('''bleh''' '''other bleh''')");
            result = recognizer.Recognize(@"('''bleh''' '''other bleh''')");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual(@"('''bleh''' '''other bleh''')", success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestSymbol()
        {
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-symbol");
            Assert.IsNotNull(recognizer);


            var result = recognizer.Recognize("null.symbol");
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.symbol", success.Symbol.TokenValue());


            result = recognizer.Recognize("'bleh symbol'");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("'bleh symbol'", success.Symbol.TokenValue());


            result = recognizer.Recognize("*");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("*", success.Symbol.TokenValue());


            result = recognizer.Recognize("abra_ka_dabra");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("abra_ka_dabra", success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestBlob()
        {
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-blob");
            Assert.IsNotNull(recognizer);


            var result = recognizer.Recognize("null.blob");
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.blob", success.Symbol.TokenValue());


            result = recognizer.Recognize("{{ bABvAHIAZQBtACAAaQBwAHMAdQBtACAAbwB0AGgAZQByACAAcwB0AHUAZgBmACAAYQBuAGQAIAB0AGgAZQAgAG0AYQBpAG4AIABzAHQAdQBmAGYA }}");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual(
                "{{ bABvAHIAZQBtACAAaQBwAHMAdQBtACAAbwB0AGgAZQByACAAcwB0AHUAZgBmACAAYQBuAGQAIAB0AGgAZQAgAG0AYQBpAG4AIABzAHQAdQBmAGYA }}",
                success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestClob()
        {
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-clob");
            Assert.IsNotNull(recognizer);


            var result = recognizer.Recognize("null.clob");
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.clob", success.Symbol.TokenValue());


            result = recognizer.Recognize("{{ \"regular string\" }}");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual(
                @"{{ ""regular string"" }}",
                success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestList()
        {
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-list");
            Assert.IsNotNull(recognizer);


            var result = recognizer.Recognize("null.list");
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.list", success.Symbol.TokenValue());


            result = recognizer.Recognize("[]");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("[]", success.Symbol.TokenValue());


            result = recognizer.Recognize("[null.int]");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("[null.int]", success.Symbol.TokenValue());


            result = recognizer.Recognize("[null.int, abc::[], null.struct]");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("[null.int, abc::[], null.struct]", success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestStruct()
        {
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-struct");
            Assert.IsNotNull(recognizer);


            var result = recognizer.Recognize("null.struct");
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.struct", success.Symbol.TokenValue());


            result = recognizer.Recognize("{}");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("{}", success.Symbol.TokenValue());


            result = recognizer.Recognize("{abc:null.int}");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("{abc:null.int}", success.Symbol.TokenValue());

            result = recognizer.Recognize("{abc:null.int, 'stuff':null.bool, \"bleh\":2005T}");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("{abc:null.int, 'stuff':null.bool, \"bleh\":2005T}", success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestSexp()
        {
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("ion-sexp");
            Assert.IsNotNull(recognizer);


            var result = recognizer.Recognize("null.sexp");
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.sexp", success.Symbol.TokenValue());


            result = recognizer.Recognize("(abcd + null.bool / 2019T 89.7)");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("(abcd + null.bool / 2019T 89.7)", success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestBlockComments()
        {
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("block-comment");
            Assert.IsNotNull(recognizer);


            var result = recognizer.Recognize("/* comment here * / */");
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("/* comment here * / */", success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestLineComments()
        {
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("line-comment");
            Assert.IsNotNull(recognizer);


            var result = recognizer.Recognize("// the comment is here\n");
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("// the comment is here", success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestBlockSpace()
        {
            using var ionXbnfStream = typeof(TextSerializer).Assembly
                .GetManifestResourceStream($"{typeof(TextSerializer).Namespace}.IonGrammar.xbnf");

            var ionGrammar = new Importer()
                .ImportGrammar(ionXbnfStream);

            var recognizer = ionGrammar.GetRecognizer("block-space");
            Assert.IsNotNull(recognizer);


            var result = recognizer.Recognize("// the comment is here\n");
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("// the comment is here\n", success.Symbol.TokenValue());
        }
    }
}
