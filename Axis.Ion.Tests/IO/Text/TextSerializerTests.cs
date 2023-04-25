using Axis.Ion.IO.Text;
using Axis.Pulsar.Grammar.Recognizers;
using Axis.Pulsar.Grammar.Recognizers.Results;

namespace Axis.Ion.Tests.IO.Text
{
    [TestClass]
    public class TextSerializerTests
    {
        [TestMethod]
        public void StringGrammarTest()
        {
            var recognizer = TextSerializer.IonGrammar.GetRecognizer(TextSerializer.IonStringSymbol);
            var result = recognizer.Recognize("'''some\nthing'''");
            Assert.IsTrue(result is SuccessResult);
        }
    }
}
