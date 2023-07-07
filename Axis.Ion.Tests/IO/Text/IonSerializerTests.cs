using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.IO.Text.Serializers;
using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.Recognizers.Results;

namespace Axis.Ion.Tests.IO.Text
{
    [TestClass]
    public class IonTextSerializerTests
    {
        [TestMethod]
        public void StringGrammarTest()
        {
            var recognizer = IonGrammar.Grammar.GetRecognizer(IonStringSerializer.IonStringSymbol);
            var result = recognizer.Recognize("'''some\nthing'''");
            Assert.IsTrue(result is SuccessResult);
        }

        [TestMethod]
        public void Parse_TestData()
        {
            var valid = new DirectoryInfo($"{Environment.CurrentDirectory}/../../../test-data/good");
            var invalid = new DirectoryInfo($"{Environment.CurrentDirectory}/../../../test-data/bad");
            Assert.IsTrue(valid.Exists);
            Assert.IsTrue(invalid.Exists);

            ParseCorrectly(valid);
            ParseIncorrectly(invalid);
        }

        private void ParseCorrectly(DirectoryInfo directory)
        {
            directory
                .EnumerateFiles("*.ion")
                .ForAll(file =>
                {
                    using var stream = file.OpenRead();
                    var reader = stream
                        .PeekTextEncoding(out var encoding)
                        .ApplyTo(str => new StreamReader(str, encoding));
                    var text = reader.ReadToEnd();
                    var parseResult = IonTextSerializer.Parse<IonPacket>(text);
                    if(parseResult is not IResult<IonPacket>.DataResult)
                    {
                        Console.WriteLine("Failed at: " + file.FullName);
                        Console.WriteLine(text);
                        Assert.IsTrue(parseResult is IResult<IonPacket>.DataResult);
                    }
                });

            directory
                .EnumerateDirectories()
                .ForAll(ParseCorrectly);
        }

        private void ParseIncorrectly(DirectoryInfo directory)
        {
            directory
                .EnumerateFiles("*.ion")
                .ForAll(file =>
                {
                    using var stream = file.OpenRead();
                    var reader = stream
                        .PeekTextEncoding(out var encoding)
                        .ApplyTo(str => new StreamReader(str, encoding));
                    var text = reader.ReadToEnd();
                    var parseResult = IonTextSerializer.Parse<IonPacket>(text);
                    Assert.IsTrue(parseResult is not SuccessResult);
                });

            directory
                .EnumerateDirectories()
                .ForAll(ParseCorrectly);
        }

        [TestMethod]
        public void SingleTests()
        {
            var parseResult = IonTextSerializer.Parse<IonPacket>(Text);
        }

        private static readonly string Text = @"123456.0
123456d0
123456d1
123456d2
123456d3
123456d42
123456d-0
123456d-1
123456d-2
123456d-42
0.123456
1.23456
12.3456
123.456
1234.56
12345.6
12345.60
12345.600
12300456.0
123.00456
1230.0456
12300.456
123.456d42
123.456d+42
123.456d-42
77777.7d0007
77777.7d-0007
77777.7d+0007
77777.7d00700
77777.7d-00700
77777.7d+00700
-123456.0
-123456d0
-123456d1
-123456d2
-123456d3
-123456d42
-123456d-0
-123456d-1
-123456d-2
-123456d-42
-0.123456
-1.23456
-12.3456
-123.456
-1234.56
-12345.6
-12345.60
-12345.600
-12300456.0
-123.00456
-1230.0456
-12300.456
-123.456d42
-123.456d+42
-123.456d-42
-77777.7d0007
-77777.7d-0007
-77777.7d+0007
-77777.7d00700
-77777.7d-00700
-77777.7d+00700
";
    }
}
