using Axis.Luna.Extensions;
using System.Text;

namespace Axis.Ion.Tests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void PeekTextEncoding_Tests()
        {
            // Ascii
            var stream = ExtensionTests
                .EncodeText(Encoding.ASCII)
                .PeekTextEncoding(out var encoding);

            Assert.AreEqual(Encoding.ASCII, encoding);
            Assert.AreEqual(Text, new StreamReader(stream).ReadToEnd());

            // utf-8
            stream = ExtensionTests
                .EncodeText(Encoding.UTF8)
                .PeekTextEncoding(out encoding);

            Assert.AreEqual(Encoding.UTF8, encoding);
            Assert.AreEqual(Text, new StreamReader(stream).ReadToEnd());

            // utf-16
            stream = ExtensionTests
                .EncodeText(Encoding.Unicode)
                .PeekTextEncoding(out encoding);

            Assert.AreEqual(Encoding.Unicode, encoding);
            Assert.AreEqual(Text, new StreamReader(stream).ReadToEnd());

            // utf-16 be
            stream = ExtensionTests
                .EncodeText(Encoding.BigEndianUnicode)
                .PeekTextEncoding(out encoding);

            Assert.AreEqual(Encoding.BigEndianUnicode, encoding);
            Assert.AreEqual(Text, new StreamReader(stream).ReadToEnd());

            // utf-32
            stream = ExtensionTests
                .EncodeText(Encoding.UTF32)
                .PeekTextEncoding(out encoding);

            Assert.AreEqual(Encoding.UTF32, encoding);
            Assert.AreEqual(Text, new StreamReader(stream).ReadToEnd());

            // utf-32 be
            stream = ExtensionTests
                .EncodeText(new UTF32Encoding(bigEndian: true, byteOrderMark: true))
                .PeekTextEncoding(out encoding);

            Assert.AreEqual(new UTF32Encoding(bigEndian: true, byteOrderMark: true), encoding);
            Assert.AreEqual(Text, new StreamReader(stream).ReadToEnd());
        }

        private static Stream EncodeText(Encoding encoding)
        {
            return new MemoryStream(
                encoding
                    .GetPreamble()
                    .Concat(encoding.GetBytes(Text))
                    .ToArray());
        }

        private static readonly string Text = @"
# String
$ion-string -> +[$annotation-list.? ?[$null-string $singleline-string $multiline-string]]
$null-string -> ""null.string""
$singleline-string -> @Singleline-DQDString
$multiline-string -> +[$block-space.? $ml-string].+
$ml-string -> @Multiline-3SQDString
";
    }
}
