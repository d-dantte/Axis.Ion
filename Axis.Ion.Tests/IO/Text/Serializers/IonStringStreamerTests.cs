using Axis.Ion.IO;
using Axis.Ion.IO.Text;
using Axis.Ion.Types;
using Axis.Luna.Common.Results;

namespace Axis.Ion.Tests.IO.Text.Serializers
{
    [TestClass]
    public class IonStringStreamerTests
    {
        [TestMethod]
        public void Serialize()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());
            var @string = "the quick brown fox jumps over the lazy dog";

            var ion1 = new IonString(null);
            var ion2 = new IonString(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var ion3 = new IonString(@string);
            var ion4 = new IonString(@string, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());

            #region Single line
            context.Options.Strings.LineStyle = SerializerOptions.StringLineStyle.Singleline;
            var text1 = IonTextSerializer.Serialize(ion1, context);
            var text2 = IonTextSerializer.Serialize(ion2, context);
            var text3 = IonTextSerializer.Serialize(ion3, context);
            var text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.string", text1);
            Assert.AreEqual("stuff::other::null.string", text2);
            Assert.AreEqual(@$"""{@string}""", text3);
            Assert.AreEqual(@$"stuff::other::""{@string}""", text4);
            #endregion

            #region Multi line
            context.Options.Strings.LineStyle = SerializerOptions.StringLineStyle.Multiline;
            context.Options.Strings.LineBreakPoint = 10;
            context.Options.IndentationStyle = SerializerOptions.IndentationStyles.Tabs;
            text1 = IonTextSerializer.Serialize(ion1, context);
            text2 = IonTextSerializer.Serialize(ion2, context);
            text3 = IonTextSerializer.Serialize(ion3, context);
            text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.string", text1);
            Assert.AreEqual("stuff::other::null.string", text2);
            Assert.AreEqual(@"'''the quick '''
'''brown fox '''
'''jumps over'''
''' the lazy '''
'''dog'''", text3);
            Assert.AreEqual(@$"stuff::other::'''the quick '''
'''brown fox '''
'''jumps over'''
''' the lazy '''
'''dog'''", text4);
            #endregion

            #region Multi line with indentation
            context = context.IndentContext();
            text1 = IonTextSerializer.Serialize(ion1, context);
            text2 = IonTextSerializer.Serialize(ion2, context);
            text3 = IonTextSerializer.Serialize(ion3, context);
            text4 = IonTextSerializer.Serialize(ion4, context);

            Assert.AreEqual("null.string", text1);
            Assert.AreEqual("stuff::other::null.string", text2);
            Assert.AreEqual(@$"'''the quick '''
	'''brown fox '''
	'''jumps over'''
	''' the lazy '''
	'''dog'''", text3);
            Assert.AreEqual(@$"stuff::other::'''the quick '''
	'''brown fox '''
	'''jumps over'''
	''' the lazy '''
	'''dog'''", text4);
            #endregion
        }

        [TestMethod]
        public void TryParse()
        {
            SerializingContext context = new SerializingContext(new SerializerOptions());
            var @string = "the quick brown fox jumps over the lazy dog";

            var ion1 = new IonString(null);
            var ion2 = new IonString(null, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());
            var ion3 = new IonString(@string);
            var ion4 = new IonString(@string, IIonValue.Annotation.ParseCollection("stuff::other::").Resolve());

            #region Single line
            context.Options.Strings.LineStyle = SerializerOptions.StringLineStyle.Singleline;
            var text1 = IonTextSerializer.Serialize(ion1, context);
            var text2 = IonTextSerializer.Serialize(ion2, context);
            var text3 = IonTextSerializer.Serialize(ion3, context);
            var text4 = IonTextSerializer.Serialize(ion4, context);

            var result1 = IonTextSerializer.Parse<IonString>(text1);
            var result2 = IonTextSerializer.Parse<IonString>(text2);
            var result3 = IonTextSerializer.Parse<IonString>(text3);
            var result4 = IonTextSerializer.Parse<IonString>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
            #endregion

            #region Multi line
            context.Options.Strings.LineStyle = SerializerOptions.StringLineStyle.Multiline;
            context.Options.Strings.LineBreakPoint = 10;
            text1 = IonTextSerializer.Serialize(ion1, context);
            text2 = IonTextSerializer.Serialize(ion2, context);
            text3 = IonTextSerializer.Serialize(ion3, context);
            text4 = IonTextSerializer.Serialize(ion4, context);

            result1 = IonTextSerializer.Parse<IonString>(text1);
            result2 = IonTextSerializer.Parse<IonString>(text2);
            result3 = IonTextSerializer.Parse<IonString>(text3);
            result4 = IonTextSerializer.Parse<IonString>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
            #endregion

            #region Multi line with indentation
            context.Options.IndentationStyle = SerializerOptions.IndentationStyles.Tabs;
            context = context.IndentContext();
            text1 = IonTextSerializer.Serialize(ion1, context);
            text2 = IonTextSerializer.Serialize(ion2, context);
            text3 = IonTextSerializer.Serialize(ion3, context);
            text4 = IonTextSerializer.Serialize(ion4, context);

            result1 = IonTextSerializer.Parse<IonString>(text1);
            result2 = IonTextSerializer.Parse<IonString>(text2);
            result3 = IonTextSerializer.Parse<IonString>(text3);
            result4 = IonTextSerializer.Parse<IonString>(text4);

            Assert.AreEqual(ion1, result1.Resolve());
            Assert.AreEqual(ion2, result2.Resolve());
            Assert.AreEqual(ion3, result3.Resolve());
            Assert.AreEqual(ion4, result4.Resolve());
            #endregion
        }
    }
}
