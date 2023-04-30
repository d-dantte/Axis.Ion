using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Luna.Common.Results;
using Axis.Pulsar.Grammar.CST;
using System;

namespace Axis.Ion.IO.Text.Streamers
{
    public abstract class AbstractIonTypeSerializer<TIonType> : IIonTypeTextSerializer<TIonType>
    where TIonType : IIonType
    {
        public TIonType ParseString(string ionValueText)
        {
            _ = TryParse(ionValueText, out var result);
            return result.Resolve();
        }

        public TIonType ParseToken(CSTNode tokenNode)
        {
            _ = TryParse(tokenNode, out var result);
            return result.Resolve();
        }

        public string SerializeText(TIonType ionValue, SerializingContext context)
        {
            if (ionValue is null)
                throw new ArgumentNullException(nameof(ionValue));

            var annotationText = IonAnnotationSerializer.StreamText(ionValue.Annotations);
            var valueText = GetIonValueText(ionValue, context);
            return $"{annotationText}{valueText}";
        }

        public bool TryParse(string ionValueText, out IResult<TIonType> result)
        {
            var recognition = TextSerializer.IonGrammar
                .GetRecognizer(IonValueSymbolName)
                .Recognize(ionValueText);

            return recognition.TryParseRecognition(TryParse, out result);
        }

        protected (IIonType.Annotation[] annotations, CSTNode typeToken) DeconstructAnnotations(
            CSTNode annotatedTypeToken)
        {
            var annotationToken = annotatedTypeToken.FirstNode();
            return annotationToken.SymbolName.Equals(IonAnnotationSerializer.AnnotationListSymbolName)
                ? (IonAnnotationSerializer.ParseToken(annotationToken), annotatedTypeToken.NodeAt(1))
                : (Array.Empty<IIonType.Annotation>(), annotationToken);
        }

        #region Abstract members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenNode"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract bool TryParse(CSTNode tokenNode, out IResult<TIonType> result);

        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="ionValue"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract string GetIonValueText(TIonType ionValue, SerializingContext context);

        /// <summary>
        /// TODO: add description
        /// </summary>
        protected abstract string IonValueSymbolName { get; }
        #endregion
    }
}
