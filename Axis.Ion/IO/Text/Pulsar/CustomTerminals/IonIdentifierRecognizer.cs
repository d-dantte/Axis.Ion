using Axis.Ion.IO.Text.Pulsar.CustomRules;
using Axis.Pulsar.Grammar;
using Axis.Pulsar.Grammar.CST;
using Axis.Pulsar.Grammar.Language;
using Axis.Pulsar.Grammar.Recognizers;
using Axis.Pulsar.Grammar.Recognizers.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Ion.IO.Text.Pulsar.CustomTerminals
{
    public class IonIdentifierRecognizer : IRecognizer
    {

        public Grammar Grammar { get; }

        public IRule Rule { get; }

        public IonIdentifierRecognizer(Grammar grammar, IRule rule)
        {
            Grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        }

        public IRecognitionResult Recognize(BufferedTokenReader tokenReader)
        {
            _ = TryRecognize(tokenReader, out var result);
            return result;
        }

        public bool TryRecognize(
            BufferedTokenReader tokenReader,
            out IRecognitionResult result)
        {
            var position = tokenReader.Position;

            try
            {
                var sbuffer = new StringBuilder();
                while (tokenReader.TryNextToken(out var token))
                {
                    if (!IonIdentifierRule.IdentifierPattern.IsMatch(sbuffer.Append(token).ToString()))
                    {
                        sbuffer.Remove(sbuffer.Length - 1, 1);
                        tokenReader.Back(1);
                        break;
                    }
                }

                var resultToken = sbuffer.ToString();
                if (IonIdentifierRule.IdentifierPattern.IsMatch(resultToken)
                    && !new HashSet<string>(IonIdentifierRule.KeyWords).Contains(resultToken))
                {
                    result = new SuccessResult(position + 1, CSTNode.Of(Rule.SymbolName, resultToken));
                    return true;
                }
                else
                {
                    result = new FailureResult(
                    position + 1,
                        IReason.Of(Rule.SymbolName));
                    tokenReader.Reset(position);
                    return false;
                }
            }
            catch(Exception ex)
            {
                _ = tokenReader.Reset(position);
                result = new ErrorResult(position + 1, ex);
                return false;
            }
        }
    }
}
