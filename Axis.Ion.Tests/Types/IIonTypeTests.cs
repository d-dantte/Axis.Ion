using Axis.Ion.Types;

namespace Axis.Ion.Tests.Types
{
    [TestClass]
    public class IIonTypeTests
    {
        [TestMethod]
        public void IonTypeOf_And_IonSymbolOf_DoNotClash()
        {
            // make sure that ionsymbol.of(string, ...), and iontype.of(string, ...) do not clash.

            var x = IIonType.OfSymbol("stuff");
        }
    }
}
