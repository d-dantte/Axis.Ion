using Axis.Ion.Types;
using Axis.Luna.Common;
using Axis.Pulsar.Grammar.CST;
using System;

namespace Axis.Ion.IO.Text.Streamers
{
    public abstract class AbstractIonTypeStreamer<TIonType> : ITextStreamer<TIonType>
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

        public string StreamText(TIonType ionValue, StreamingContext context)
        {
            if (ionValue is null)
                throw new ArgumentNullException(nameof(ionValue));

            var annotationText = IonAnnotationStreamer.StreamText(ionValue.Annotations);
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
            return annotationToken.SymbolName.Equals(IonAnnotationStreamer.AnnotationListSymbolName)
                ? (IonAnnotationStreamer.ParseToken(annotationToken), annotatedTypeToken.NodeAt(1))
                : (Array.Empty<IIonType.Annotation>(), annotationToken);
        }

        #region Abstract members
        public abstract bool TryParse(CSTNode tokenNode, out IResult<TIonType> result);

        /// <summary>
        /// TODO: add description
        /// </summary>
        /// <param name="ionValue"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract string GetIonValueText(TIonType ionValue, StreamingContext context);

        /// <summary>
        /// TODO: add description
        /// </summary>
        protected abstract string IonValueSymbolName { get; }
        #endregion
    }
}
