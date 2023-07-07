using Axis.Luna.Common.Results;
using Axis.Luna.Extensions;
using Axis.Pulsar.Grammar.CST;
using System;
using System.Linq;

namespace Axis.Ion.IO.Text.Serializers
{
    public class IonPacketSerializer : IIonTextSerializer<IonPacket>
    {
        public static string GrammarSymbol => IonSymbol;

        #region Symbols
        internal const string IonSymbol = "ion";
        internal const string ValueSymbol = "ion-value";
        #endregion

        public static string Serialize(IonPacket value) => Serialize(value, new SerializingContext());

        public static string Serialize(IonPacket value, SerializingContext context)
        {
            return value.IonValues
                .Select(value => IonValueSerializer.Serialize(value, context))
                .JoinUsing(Environment.NewLine);
        }

        public static IResult<IonPacket> Parse(CSTNode ionSymbol)
        {
            return ionSymbol.SymbolName switch
            {
                IonSymbol => ionSymbol
                    .FindAllNodes(ValueSymbol)
                    .Select(IonValueSerializer.Parse)
                    .Fold()
                    .Map(values => new IonPacket(values)),

                _ => Result.Of<IonPacket>(new ArgumentException(
                    $"Invalid symbol encountered: '{ionSymbol.SymbolName}', "
                    + $"expected '{IonSymbol}'"))
            };
        }
    }
}
