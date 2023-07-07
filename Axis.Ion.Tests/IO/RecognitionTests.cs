using Axis.Ion.IO.Text;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar;
using Axis.Pulsar.Grammar.CST;
using Axis.Pulsar.Grammar.Exceptions;
using Axis.Pulsar.Grammar.Recognizers.Results;

namespace Axis.Ion.Tests.IO
{
    [TestClass]
    public class RecognitionTests
    {
        [TestMethod]
        public void GrammarImportationTests()
        {
            try
            {
                var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

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
            success = result as SuccessResult;
            Assert.IsNotNull(success);
        }

        [TestMethod]
        public void TestDecimal()
        {
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

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

            result = recognizer.Recognize(@"'''bleh''' '''other bleh'''");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual(@"'''bleh''' '''other bleh'''", success.Symbol.TokenValue());
        }

        [TestMethod]
        public void TestSymbol()
        {
            var ionGrammar = IonGrammar.Grammar;

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


            result = recognizer.Recognize("null");
            var failure = result as FailureResult;
            Assert.IsNotNull(failure);
        }

        [TestMethod]
        public void TestBlob()
        {
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

            var recognizer = ionGrammar.GetRecognizer("ion-clob");
            Assert.IsNotNull(recognizer);


            var result = recognizer.Recognize("null.clob");
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("null.clob", success.Symbol.TokenValue());

            result = recognizer.Recognize("abc::xy::null.clob");
            success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("abc::xy::null.clob", success.Symbol.TokenValue());


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
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

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
            var ionGrammar = IonGrammar.Grammar;

            var recognizer = ionGrammar.GetRecognizer("block-space");
            Assert.IsNotNull(recognizer);


            var result = recognizer.Recognize("// the comment is here\n");
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            Assert.AreEqual("// the comment is here\n", success.Symbol.TokenValue());
        }


        [TestMethod]
        public void TestDataRecognition_ShouldPassAllRecognitions()
        {
            var valid = new DirectoryInfo($"{Environment.CurrentDirectory}/../../../test-data/good");
            Assert.IsTrue(valid.Exists);

            RecognizeCorrectly(valid);
        }

        private void RecognizeCorrectly(DirectoryInfo directory)
        {
            var grammar = IonGrammar.Grammar;
            directory
                .EnumerateFiles("*.ion")
                .ForAll(file =>
                {
                    using var stream = file.OpenRead();
                    var reader = stream
                        .PeekTextEncoding(out var encoding, System.Text.Encoding.UTF8)
                        .ApplyTo(str => new StreamReader(str, encoding));

                    var ion = reader.ReadToEnd();
                    var result = grammar
                        .RootRecognizer()
                        .Recognize(ion);

                    var success = result as SuccessResult;
                    if (success is null)// || !ion.Equals(success.Symbol.TokenValue()))
                    {
                        Console.WriteLine($"Failing at: {file.FullName}");
                        Console.WriteLine(result);
                        Console.WriteLine(ion);
                        Assert.IsTrue(result is SuccessResult);
                    }
                });

            directory
                .EnumerateDirectories()
                .ForAll(RecognizeCorrectly);
        }


        [TestMethod]
        public void TestDataRecognition_ShouldFailAllRecognitions()
        {
            var valid = new DirectoryInfo($"{Environment.CurrentDirectory}/../../../test-data/bad");
            Assert.IsTrue(valid.Exists);

            var files = FailRecognition(valid).ToArray();
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
            Assert.AreEqual(0, files.Length);
        }

        private static HashSet<string> ExclusionList = new HashSet<string>
        {
            "annotationSymbolIDUnmapped.ion"
        };

        private IEnumerable<string> FailRecognition(DirectoryInfo directory)
        {
            var grammar = IonGrammar.Grammar;
            var failing = directory
                .EnumerateFiles("*.ion")
                .Select(file =>
                {
                    if (ExclusionList.Contains(file.Name))
                        return null;

                    using var stream = file.OpenRead();
                    var reader = stream
                        .PeekTextEncoding(out var encoding, System.Text.Encoding.UTF8)
                        .ApplyTo(str => new StreamReader(str, encoding));

                    var ion = reader.ReadToEnd();
                    var result = grammar
                        .RootRecognizer()
                        .Recognize(ion);

                    if (result is SuccessResult success
                        && success.Symbol.TokenValue().Equals(ion))
                    {
                        return file.FullName;
                    }
                    return null;
                })
                .Where(file => file != null)
                .Select(file => file!);

            return failing.Concat(directory
                .EnumerateDirectories()
                .SelectMany(FailRecognition));
        }

        [TestMethod]
        public void Multiple()
        {
            var recognizer = IonGrammar.Grammar.GetRecognizer("ion-decimal");
            var result = recognizer.Recognize(Text);
            var success = result as SuccessResult;
            Assert.IsNotNull(success);
            var tokens = success.Symbol.TokenValue();
            Assert.IsTrue(Text.Equals(tokens));
        }

        [TestMethod]
        public void Misc()
        {
            var legal = new HashSet<string>(IonGrammar.ClobSinglelineStringLegalCharacters());
            var tt = Text[6..^7];
            Console.WriteLine(tt);
            for(int index = 0; index < tt.Length; index++)
            {
                var c = tt[index];
                if (!legal.Contains(c.ToString()))
                    Console.WriteLine($"char: {c}, code: \\x{((int)c).ToString("x")}, position: {index}");
            }
        }


        private static readonly string Text = @"0.000000027182818284590450000000000d+8";
    }
}
