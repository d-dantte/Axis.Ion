using Axis.Ion.Types;
using Axis.Luna.Common.Utils;
using Axis.Luna.Extensions;
using System.Text;

namespace Axis.Ion.Tests
{
    [TestClass]
    public class DefaultIonTypeTests
    {
        [TestMethod]
        public void GeneralIonTypeTests()
        {
            Enum.GetValues<IonTypes>().ForAll(type =>
            {
                // creation test
                var stypes = type == IonTypes.Symbol ? SymbolTypes : new[] { (Type)null };

                foreach (var stype in stypes)
                {
                    var annotations = CreateAnnotations(3);
                    var value = CreateValue(type, stype, annotations);
                    var ion = CreateWithConstructor(type, value, stype, annotations);
                    TestCreation(type, ion);

                    // default test
                    if(type != IonTypes.Null)
                        TestDefault(type, ion, false);
                    TestAnnotatedDefault(type, stype, annotations);

                    // annotation test
                    TestAnnotations(ion, annotations);

                    // value test
                    TestValue(ion, value);

                    // string test
                    //TestIonTextAndToString(ion);

                    // equality test
                    var ion2 = CreateWithConstructor(type, value, stype, annotations);
                    TestEquality(ion, ion2);

                    // test implicit
                    if (type != IonTypes.Null)
                        TestImplicitOperator(type, value, stype);
                }
            });
        }

        [TestMethod]
        public void dummyTest()
        {
            IonStruct struct1 = new IonStruct.Initializer("abc::xyz::a12");
            IonStruct struct2 = new IonStruct.Initializer("abc::xyz::a12");

            Console.WriteLine($"struct1: {struct1}");
            Console.WriteLine($"struct1: {struct2}");

            Assert.AreEqual(struct1, struct2);
        }

        #region Tests
        private void TestCreation(IonTypes type, IIonType value)
        {
            Assert.IsNotNull(value);
            Assert.IsTrue(type.Equals(value?.Type));
        }

        private void TestDefault(IonTypes type, IIonType value, bool isDefault = true)
        {
            var symbolType = type == IonTypes.Symbol ? SecureRandom.NextValue(SymbolTypes) : null;
            var @default = CreateDefault(type, symbolType);
            if (isDefault)
                Assert.AreEqual(value, @default);

            else Assert.AreNotEqual(value, @default);
        }

        private void TestAnnotatedDefault(IonTypes type, Type? symbolType, IIonType.Annotation[] annotations)
        {
            var @default = CreateDefault(type, symbolType, annotations);
            Assert.IsTrue(annotations.SequenceEqual(@default.Annotations));
        }

        private void TestValue(IIonType ionValue, object? value)
        {
            switch(ionValue)
            {
                case IonNull @null:
                    break;

                case IonBool @bool:
                    Assert.AreEqual(value, @bool.Value);
                    break;

                case IonInt @int:
                    Assert.AreEqual(value, @int.Value);
                    break;

                case IonFloat @float:
                    Assert.AreEqual(value, @float.Value);
                    break;

                case IonDecimal @decimal:
                    Assert.AreEqual(value, @decimal.Value);
                    break;

                case IonTimestamp timestamp:
                    Assert.AreEqual(value, timestamp.Value);
                    break;

                case IonString @string:
                    Assert.AreEqual(value, @string.Value);
                    break;

                case IIonSymbol symbol:
                    if (ionValue is IIonSymbol.Identifier id)
                        Assert.AreEqual(value, id.Symbol);

                    if (ionValue is IIonSymbol.QuotedSymbol q)
                        Assert.AreEqual(value, q.Symbol);

                    if (ionValue is IIonSymbol.Operator o)
                        Assert.AreEqual(value, o.Symbol);

                    break;

                case IonBlob blob:
                    Assert.IsTrue(blob.Value.NullOrTrue(value as byte[], Enumerable.SequenceEqual));
                    break;

                case IonClob clob:
                    Assert.IsTrue(clob.Value.NullOrTrue(value as byte[], Enumerable.SequenceEqual));
                    break;

                case IonStruct @struct:
                    var props = (value as IonStruct.Initializer)?.Properties;
                    Assert.IsTrue(@struct.Value.NullOrTrue(props, Enumerable.SequenceEqual));
                    break;

                case IonList list:
                    var elements = (value as IonList.Initializer)?.Elements;
                    Assert.IsTrue(list.Value.NullOrTrue(elements, Enumerable.SequenceEqual));
                    break;

                case IonSexp sexp:
                    var sexps = (value as IonSexp.Initializer)?.Elements;
                    Assert.IsTrue(sexp.Value.NullOrTrue(sexps, Enumerable.SequenceEqual));
                    break;

                default:
                    throw new Exception("Invalid IonType: " + ionValue?.Type);
            }
        }

        private void TestAnnotations(IIonType value, IIonType.Annotation[] annotations)
        {
            Assert.IsTrue(value.Annotations.SequenceEqual(annotations));
        }

        /// <summary>
        /// These tests will be performed separately 
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private void TestIonTextAndToString(IIonType value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var annotations = value.Annotations.Select(a => a.ToString()).JoinUsing("");
            var toIonText = value switch
            {
                IonBool i => i.Value?.ToString() ?? "null.bool",
                IonInt i => i.Value?.ToString() ?? "null.int",
                IonFloat i => i.Value?.ToString() ?? "null.float",
                IonDecimal i => i.Value?.ToString() ?? "null.decimal",
                IonTimestamp i => i.Value?.ToString(IonTimestamp._Format) ?? "null.timestamp",
                IonString i => i.Value ?? "null.string",
                IIonSymbol => value switch
                {
                    IIonSymbol.QuotedSymbol i => i.Symbol?.WrapIn("'"),
                    IIonSymbol.Identifier i => i.Symbol,
                    IIonSymbol.Operator i => i.Symbol == null? "": i.Symbol.Select(s => (char)s).ApplyTo(Extensions.AsString),
                    _ => throw new InvalidOperationException($"Invalid symbol type: {value?.GetType()}")
                } ?? "null.symbol",
                IonBlob i => i.Value?.ApplyTo(Convert.ToBase64String).WrapIn("{{ ", " }}") ?? "null.blob",
                IonClob i => i.Value?.ApplyTo(Encoding.ASCII.GetString) ?? "null.clob",
                IonStruct i => i.Value?.Select(p => p.ToString()).JoinUsing(", ").WrapIn("{", "}") ?? "null.struct",
                IonList i => i.Value?.Select(e => e.ToString()).JoinUsing(", ").WrapIn("[", "]") ?? "null.list",
                IonSexp i => i.Value?.Select(e => e.ToString()).JoinUsing(" ").WrapIn("(", ")") ?? "null.list",
                IonNull i => "null.null",
                _ => throw new InvalidOperationException($"Invalid ion type: {value?.GetType()}")
            };
            var toString = annotations + (
                value.Type == IonTypes.String && !toIonText.Equals("null.string") ? toIonText.WrapIn("\"") :
                value.Type == IonTypes.Clob && !toIonText.Equals("null.clob") ? toIonText.WrapIn("{{ \"", "\" }}") :
                toIonText);


            if (value is not IonFloat && value is not IonDecimal)
            {
                Assert.AreEqual(toIonText, value.ToIonText());
                Assert.AreEqual(toString, value.ToString());
            }
        }

        private void TestEquality(IIonType first, IIonType second)
        {
            Assert.AreEqual(first, second);
        }

        private void TestImplicitOperator(IonTypes expectedType, object? value, Type? symbolType = null)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            IIonType ion;
            switch(expectedType)
            {
                case IonTypes.Bool:
                    IonBool ib = (bool?)value;
                    ion = ib;
                    break;

                case IonTypes.Int:
                    IonInt ii = (long?)value;
                    ion = ii;
                    break;

                case IonTypes.Float:
                    IonFloat @if = (double?)value;
                    ion = @if;
                    break;

                case IonTypes.Decimal:
                    IonDecimal id = (decimal?)value;
                    ion = id;
                    break;

                case IonTypes.Timestamp:
                    IonTimestamp it = (DateTimeOffset?)value;
                    ion = it;
                    break;

                case IonTypes.String:
                    IonString @is = (string)value;
                    ion = @is;
                    break;

                case IonTypes.Symbol:
                    if (typeof(IIonSymbol.Operator).Equals(symbolType))
                    {
                        IIonSymbol.Operator o = (IIonSymbol.Operators[])value;
                        ion = o;
                    }
                    else if(typeof(IIonSymbol.QuotedSymbol).Equals(symbolType))
                    {
                        IIonSymbol.QuotedSymbol q = (string)value;
                        ion = q;
                    }
                    else
                    {
                        IIonSymbol.Identifier i = (string)value;
                        ion = i;
                    }
                    break;

                case IonTypes.Blob:
                    IonBlob iblob = (byte[]?)value;
                    ion = iblob;
                    break;

                case IonTypes.Clob:
                    IonBlob iclob = (byte[]?)value;
                    ion = iclob;
                    break;

                case IonTypes.Struct:
                    IonStruct istruct = (IonStruct.Initializer)value;
                    ion = istruct;
                    break;

                case IonTypes.List:
                    IonList ilist = (IonList.Initializer)value;
                    ion = ilist;
                    break;

                case IonTypes.Sexp:
                    IonSexp isexp = (IonSexp.Initializer)value;
                    ion = isexp;
                    break;

                case IonTypes.Null:
                    ion = IIonType.OfNull();
                    break;

                default:
                    throw new Exception("Invalid ion type" + expectedType);
            }

            Assert.IsNotNull(ion);
        }
        #endregion

        #region creators
        private IIonType CreateWithConstructor(IonTypes type, object? value, Type? symbolType, params IIonType.Annotation[] annotations)
        {
            return type switch
            {
                IonTypes.Null => new IonNull(annotations),
                IonTypes.Bool => new IonBool(value as bool?, annotations),
                IonTypes.Int => new IonInt(value as long?, annotations),
                IonTypes.Float => new IonFloat(value as double?, annotations),
                IonTypes.Decimal => new IonDecimal(value as decimal?, annotations),
                IonTypes.Timestamp => new IonTimestamp(value as DateTimeOffset?, annotations),
                IonTypes.String => new IonString(value as string, annotations),
                IonTypes.Symbol =>
                    typeof(IIonSymbol.Operator).Equals(symbolType) ? new IIonSymbol.Operator((IIonSymbol.Operators[])value, annotations) :
                    typeof(IIonSymbol.QuotedSymbol).Equals(symbolType) ? new IIonSymbol.QuotedSymbol(value as string, annotations) :
                    typeof(IIonSymbol.Identifier).Equals(symbolType) ? new IIonSymbol.Identifier(value as string, annotations) :
                    throw new FormatException($"Invalid symbol format: {value}"),

                IonTypes.Blob => new IonBlob(value as byte[], annotations),
                IonTypes.Clob => new IonClob(value as byte[], annotations),
                IonTypes.Struct => new IonStruct(value as IonStruct.Initializer),
                IonTypes.List => new IonList(value as IonList.Initializer),
                IonTypes.Sexp => new IonSexp(value as IonSexp.Initializer),

                _ => throw new ArgumentException("Invalid value")
            };
        }

        private IIonType CreateWithOf(IonTypes type, object value, params IIonType.Annotation[] annotations)
        {
            return type switch
            {
                IonTypes.Null => IIonType.OfNull(annotations),
                IonTypes.Bool => IIonType.Of(value as bool?, annotations),
                IonTypes.Int => IIonType.Of(value as long?, annotations),
                IonTypes.Float => IIonType.Of(value as double?, annotations),
                IonTypes.Decimal => IIonType.Of(value as decimal?, annotations),
                IonTypes.Timestamp => IIonType.Of(value as DateTimeOffset?, annotations),
                IonTypes.String => IIonType.Of(value as string, annotations),
                IonTypes.Symbol =>
                    value is null ? new IIonSymbol.Identifier(null, annotations) :
                    value is IIonSymbol.Operators[] ops ? new IIonSymbol.Operator(ops, annotations) :
                    value.As<string>().Trim().StartsWith('\'') ? new IIonSymbol.QuotedSymbol(value as string, annotations) :
                    new IIonSymbol.Identifier(value as string, annotations),

                IonTypes.Blob => IIonType.OfBlob(value as byte[], annotations),
                IonTypes.Clob => IIonType.OfClob(value as byte[], annotations),
                IonTypes.Struct => IIonType.Of(value as IonStruct.Initializer),
                IonTypes.List => IIonType.Of(value as IonList.Initializer),
                IonTypes.Sexp => IIonType.Of(value as IonSexp.Initializer),

                _ => throw new ArgumentException("Invalid value")
            };
        }

        private IIonType CreateDefault(IonTypes type, Type? symbolType, params IIonType.Annotation[] annotations)
        {
            return type switch
            {
                IonTypes.Null => annotations is null
                    ? default(IonNull)
                    : new IonNull(annotations),

                IonTypes.Bool => annotations is null
                    ? default(IonBool)
                    : new IonBool(null, annotations),

                IonTypes.Int => annotations is null
                    ? default(IonInt)
                    : new IonInt(null, annotations),

                IonTypes.Float => annotations is null
                    ? default(IonFloat)
                    : new IonFloat(null, annotations),

                IonTypes.Decimal => annotations is null
                    ? default(IonDecimal)
                    : new IonDecimal(null, annotations),

                IonTypes.Timestamp => annotations is null
                    ? default(IonTimestamp)
                    : new IonTimestamp(null, annotations),

                IonTypes.String => annotations is null
                    ? default(IonString)
                    : new IonString(null, annotations),

                IonTypes.Symbol =>
                    annotations == null && typeof(IIonSymbol.QuotedSymbol).Equals(symbolType) ? default(IIonSymbol.Operator) :
                    annotations != null && typeof(IIonSymbol.QuotedSymbol).Equals(symbolType) ? new IIonSymbol.QuotedSymbol(null, annotations) :
                    annotations == null && typeof(IIonSymbol.Identifier).Equals(symbolType) ? default(IIonSymbol.Identifier) :
                    annotations != null && typeof(IIonSymbol.Identifier).Equals(symbolType) ? new IIonSymbol.Identifier(null, annotations) :
                    annotations == null && typeof(IIonSymbol.Operator).Equals(symbolType) ? default(IIonSymbol.Operator) :
                    annotations != null && typeof(IIonSymbol.Operator).Equals(symbolType) ? new IIonSymbol.Operator(null, annotations) :
                    throw new InvalidOperationException("invalid symbol creation parameters"),

                IonTypes.Blob => annotations is null
                    ? default(IonBlob)
                    : new IonBlob(null, annotations),

                IonTypes.Clob => annotations is null
                    ? default(IonClob)
                    : new IonClob(null, annotations),

                IonTypes.Struct => annotations is null
                    ? default(IonString)
                    : new IonStruct(annotations),

                IonTypes.List => annotations is null
                    ? default(IonList)
                    : new IonList(annotations),

                IonTypes.Sexp => annotations is null
                    ? default(IonSexp)
                    : new IonSexp(annotations),

                _ => throw new Exception("Invalid ion type: " + type)
            };
        }

        private IIonType.Annotation[] CreateAnnotations(int? count = null)
        {
            var list = new List<IIonType.Annotation>();
            var limit = count ?? 3;
            for(int i = 0; i < limit; i++)
            {
                list.Add(new IIonType.Annotation(SecureRandom.NextValue(Words)));
            }

            return list.ToArray();
        }

        private object? CreateValue(IonTypes type, Type? symbolType = null, params IIonType.Annotation[] annotations)
        {
            var sampleTypes = new[] { IonTypes.Int, IonTypes.Float, IonTypes.Bool, IonTypes.Decimal, IonTypes.Timestamp, IonTypes.String };
            return type switch
            {
                IonTypes.Null => null,
                IonTypes.Bool => SecureRandom.NextBool(),
                IonTypes.Int => SecureRandom.NextSignedLong(),
                IonTypes.Float => SecureRandom.NextSignedDouble(),
                IonTypes.Decimal => Convert.ToDecimal(SecureRandom.NextSignedLong()/2D),
                IonTypes.Timestamp => DateTimeOffset.Now + TimeSpan.FromMilliseconds(SecureRandom.NextSignedInt()),
                IonTypes.String => SecureRandom.NextValue(Words),
                IonTypes.Symbol =>
                    typeof(IIonSymbol.Operator).Equals(symbolType) ? RandomOperatorSymbols() :
                    typeof(IIonSymbol.QuotedSymbol).Equals(symbolType) ? SecureRandom.NextValue(Words).WrapIn("'") :
                    typeof(IIonSymbol.Identifier).Equals(symbolType) ? SecureRandom.NextValue(Words) :
                    throw new InvalidOperationException("Invaid symbol type: null"),
                IonTypes.Blob => SecureRandom.NextBytes(SecureRandom.NextInt(1000)),
                IonTypes.Clob => SecureRandom.NextValue(Words).ApplyTo(Encoding.ASCII.GetBytes),
                IonTypes.Struct => new IonStruct.Initializer(
                    annotations,
                    CreateRandomProperties(SecureRandom.NextInt(5))),
                IonTypes.List => new IonList.Initializer(
                    annotations,
                    CreateRandomIons(SecureRandom.NextInt(10), sampleTypes)),
                IonTypes.Sexp => new IonSexp.Initializer(
                    annotations,
                    CreateRandomIons(SecureRandom.NextInt(10), sampleTypes)),
                _ => throw new ArgumentException($"Invalid ion type: {type}")
            };
        }

        private IonStruct.Property[] CreateRandomProperties(int count)
        {
            count = Math.Abs(count);
            var sampleTypes = new[] { IonTypes.Int, IonTypes.Float, IonTypes.Bool, IonTypes.Decimal, IonTypes.Timestamp, IonTypes.String };
            var names = CreateUniqueNames(count);
            var ions = CreateRandomIons(count, sampleTypes);
            return names
                .PairWith(ions)
                .Select(tuple => new IonStruct.Property(tuple.Item1, tuple.Item2))
                .ToArray();
        }

        private string[] CreateUniqueNames(int count)
        {
            count = Math.Abs(count);
            var names = new HashSet<string>();
            while(names.Count < count)
            {
                names.Add(SecureRandom.NextValue(Words).Replace("-", "_"));
            }

            return names.ToArray();
        }

        private IIonType[] CreateRandomIons(int count, params IonTypes[] types)
        {
            count = Math.Abs(count);
            var values = new List<IIonType>();
            for (int cnt = 0; cnt < count; cnt++)
            {
                var type = SecureRandom.NextValue(types);
                var stype = type == IonTypes.Symbol
                    ? SecureRandom.NextValue(SymbolTypes)
                    : null;
                var value = CreateValue(type, stype);
                values.Add(CreateWithConstructor(type, value, stype));
            }
            return values.ToArray();
        }

        private IIonSymbol[] CreateRandomSymbols(int count)
        {
            count = Math.Abs(count);
            var symbols = new List<IIonSymbol>();
            for(int cnt = 0; cnt < count; cnt++)
            {
                var stype = SecureRandom.NextValue(SymbolTypes);
                object value =
                    stype.Equals(typeof(IIonSymbol.Operator)) ? SecureRandom.NextValue(Enum.GetValues<IIonSymbol.Operators>()) :
                    stype.Equals(typeof(IIonSymbol.QuotedSymbol)) ? SecureRandom.NextValue(Words).WrapIn("'") :
                    SecureRandom.NextValue(Words);
                symbols.Add(CreateWithConstructor(IonTypes.Symbol, value, stype) as IIonSymbol);
            }

            return symbols.ToArray();
        }
        #endregion

        private static readonly Type[] SymbolTypes = new[]
        {
            typeof(IIonSymbol.Operator),
            typeof(IIonSymbol.Identifier),
            typeof(IIonSymbol.QuotedSymbol)
        };
        private static readonly string[] Words = new[]
        {
            "aback", "abaft", "abandoned", "abashed", "aberrant", "abhorrent", "abiding", "abject", "ablaze", "able", "abnormal", "aboard", "aboriginal", "abortive", "abounding", "abrasive", "abrupt",
            "absent", "absorbed", "absorbing", "abstracted", "absurd", "abundant", "abusive", "accept", "acceptable", "accessible", "accidental", "account", "accurate", "achiever", "acid", "acidic",
            "aunt", "auspicious", "authority", "automatic", "available", "average", "avoid", "awake", "aware", "awesome", "awful", "axiomatic", "babies", "baby", "back", "bad", "badge", "bag", "bait", "bake",
            "balance", "ball", "ban", "bang", "barbarous", "bare", "base", "baseball", "bashful", "basin", "basket", "basketball", "bat", "bath", "bathe", "battle", "bawdy", "bead", "beam", "bear", "beautiful",            
            "brave", "brawny", "breakable", "breath", "breathe", "breezy", "brick", "bridge", "brief", "bright", "broad", "broken", "brother", "brown", "bruise", "brush", "bubble", "bucket", "building", "bulb",
            "bump", "bumpy", "burly", "burn", "burst", "bury", "bushes", "business", "bustling", "busy", "butter", "button", "buzz", "cabbage", "cable", "cactus", "cagey", "cake", "cakes", "calculate",
            "calculating", "calculator", "calendar", "call", "callous", "calm", "camera", "camp", "can", "cannon", "canvas", "cap", "capable", "capricious", "caption", "car", "card", "care", "careful",
            "careless", "caring", "carpenter", "carriage", "carry", "cars", "cart", "carve", "cast", "cat", "cats", "cattle", "cause", "cautious", "cave", "ceaseless", "celery", "cellar", "cemetery", "cent",
            "certain", "chalk", "challenge", "chance", "change", "changeable", "channel", "charge", "charming", "chase", "cheap", "cheat", "check", "cheer", "cheerful", "cheese", "chemical", "cherries",
            "cherry", "chess", "chew", "chicken", "chickens", "chief", "childlike", "children", "chilly", "chin", "chivalrous", "choke", "chop", "chubby", "chunky", "church", "circle", "claim", "clam",
            "clammy", "clap", "class", "classy", "clean", "clear", "clever", "clip", "cloistered", "close", "closed", "cloth", "cloudy", "clover", "club", "clumsy", "cluttered", "coach", "coal", "coast",
            "coat", "cobweb", "coherent", "coil", "cold", "collar", "collect", "color", "colorful", "colossal", "colour", "comb", "combative", "comfortable", "command", "committee", "common", "communicate",
            "company", "compare", "comparison", "compete", "competition", "complain", "complete", "complex", "concentrate", "concern", "concerned", "condemned", "condition", "confess", "confuse", "confused",
            "connect", "connection", "conscious", "consider", "consist", "contain", "continue", "control", "cooing", "cook", "cool", "cooperative", "coordinated", "copper", "copy", "corn", "correct", "cough",
            "count", "country", "courageous", "cover", "cow", "cowardly", "cows", "crabby", "crack", "cracker", "crash", "crate", "craven", "crawl", "crayon", "crazy", "cream", "creator", "creature", "credit",
            "creepy", "crib", "crime", "crook", "crooked", "cross", "crow", "crowd", "crowded", "crown", "cruel", "crush", "cry", "cub", "cuddly", "cultured", "cumbersome", "cup", "cure", "curious", "curl",
            "curly", "current", "curtain", "curve", "curved", "curvy", "cushion", "cut", "cute", "cycle", "cynical", "dad", "daffy", "daily", "dam", "damage", "damaged", "damaging", "damp", "dance", "dangerous",
            "dapper", "dare", "dark", "dashing", "daughter", "day", "dazzling", "dead", "deadpan", "deafening", "dear", "death", "debonair", "debt", "decay", "deceive", "decide", "decision", "decisive",
            "decorate", "decorous", "deep", "deeply", "deer", "defeated", "defective", "defiant", "degree", "delay", "delicate", "delicious", "delight", "delightful", "delirious", "deliver", "demonic", "depend",
            "dependent", "depressed", "deranged", "describe", "descriptive", "desert", "deserted", "deserve", "design", "desire", "desk", "destroy", "destruction", "detail", "detailed", "detect", "determined",
            "develop", "development", "devilish", "didactic", "different", "difficult", "digestion", "diligent", "dime", "dinner", "dinosaurs", "direction", "direful", "dirt", "dirty", "disagree",
            "disagreeable", "disappear", "disapprove", "disarm", "disastrous", "discover", "discovery", "discreet", "discussion", "disgusted", "disgusting", "disillusioned", "dislike", "dispensable",
            "distance", "distinct", "distribution", "disturbed", "divergent", "divide", "division", "dizzy", "dock", "doctor", "dog", "dogs", "doll", "dolls", "domineering", "donkey", "door", "double", "doubt",
            "doubtful", "downtown", "drab", "draconian", "drag", "drain", "dramatic", "drawer", "dream", "dreary", "dress", "drink", "drip", "driving", "drop", "drown", "drum", "drunk", "dry", "duck", "ducks",
            "dull", "dust", "dusty", "dynamic", "dysfunctional", "eager", "ear", "early", "earn", "earsplitting", "earth", "earthquake", "earthy", "easy", "eatable", "economic", "edge", "educate", "educated",
            "education", "effect", "efficacious", "efficient", "egg", "eggnog", "eggs", "eight", "elastic", "elated", "elbow", "elderly", "electric", "elegant", "elfin", "elite", "embarrass", "embarrassed",
            "eminent", "employ", "empty", "enchanted", "enchanting", "encourage", "encouraging", "end", "endurable", "energetic", "engine", "enjoy", "enormous", "enter", "entertain", "entertaining",
            "enthusiastic", "envious", "equable", "equal", "erect", "erratic", "error", "escape", "ethereal", "evanescent", "evasive", "even", "event", "examine", "example", "excellent", "exchange",
            "excite", "excited", "exciting", "exclusive", "excuse", "exercise", "exist", "existence", "exotic", "expand", "expansion", "expect", "expensive", "experience", "expert", "explain", "explode",
            "extend", "extra-large", "extra-small", "exuberant", "exultant", "eye", "eyes", "fabulous", "face", "fact", "fade", "faded", "fail", "faint", "fair", "fairies", "faithful", "fall", "fallacious",
            "false", "familiar", "famous", "fanatical", "fancy", "fang", "fantastic", "far", "far-flung", "farm", "fascinated", "fast", "fasten", "fat", "faulty", "fax", "fear", "fearful", "fearless",
            "feeble", "feeling", "feigned", "female", "fence", "fertile", "festive", "fetch", "few", "field", "fierce", "file", "fill", "film", "filthy", "fine", "finger", "finicky", "fire", "fireman", "first",
            "fish", "fit", "five", "fix", "fixed", "flag", "flagrant", "flaky", "flame", "flap", "flash", "flashy", "flat", "flavor", "flawless", "flesh", "flight", "flimsy", "flippant", "float", "flock",
            "flood", "floor", "flow", "flower", "flowers", "flowery", "fluffy", "fluttering", "fly", "foamy", "fog", "fold", "follow", "food", "fool", "foolish", "foot", "force", "foregoing", "forgetful",
            "fork", "form", "fortunate", "found", "four", "fowl", "fragile", "frail", "frame", "frantic", "free", "freezing", "frequent", "fresh", "fretful", "friction", "friend", "friendly", "friends",
            "frighten", "frightened", "frightening", "frog", "frogs", "front", "fruit", "fry", "fuel", "full", "fumbling", "functional", "funny", "furniture", "furry", "furtive", "future", "futuristic",
            "fuzzy", "gabby", "gainful", "gamy", "gaping", "garrulous", "gate", "gather", "gaudy", "gaze", "geese", "general", "gentle", "ghost", "giant", "giants", "giddy", "gifted", "gigantic", "giraffe",
            "girl", "girls", "glamorous", "glass", "gleaming", "glib", "glistening", "glorious", "glossy", "glove", "glow", "glue", "godly", "gold", "good", "goofy", "gorgeous", "government", "governor",
            "grab", "graceful", "grade", "grain", "grandfather", "grandiose", "grandmother", "grape", "grass", "grate", "grateful", "gratis", "gray", "grease", "greasy", "great", "greedy", "green", "greet",
            "grey", "grieving", "grin", "grip", "groan", "groovy", "grotesque", "grouchy", "ground", "group", "growth", "grubby", "gruesome", "grumpy", "guarantee", "guard", "guarded", "guess", "guide",
            "guiltless", "guitar", "gullible", "gun", "gusty", "guttural", "habitual", "hair", "haircut", "half", "hall", "hallowed", "halting", "hammer", "hand", "handle", "hands", "handsome", "handsomely",
            "handy", "hang", "hanging", "hapless", "happen", "happy", "harass", "harbor", "hard", "hard-to-find", "harm", "harmonious", "harmony", "harsh", "hat", "hate", "hateful", "haunt", "head", "heady",
            "heal", "health", "healthy", "heap", "heartbreaking", "heat", "heavenly", "heavy", "hellish", "help", "helpful", "helpless", "hesitant", "hideous", "high", "high-pitched", "highfalutin",
            "hilarious", "hill", "hissing", "historical", "history", "hobbies", "hole", "holiday", "holistic", "hollow", "home", "homeless", "homely", "honey", "honorable", "hook", "hop", "hope", "horn",
            "horrible", "horse", "horses", "hose", "hospitable", "hospital", "hot", "hour", "house", "houses", "hover", "hug", "huge", "hulking", "hum", "humdrum", "humor", "humorous", "hungry", "hunt",
            "hurried", "hurry", "hurt", "hushed", "husky", "hydrant", "hypnotic", "hysterical", "ice", "icicle", "icky", "icy", "idea", "identify", "idiotic", "ignorant", "ignore", "ill", "ill-fated",
            "ill-informed", "illegal", "illustrious", "imaginary", "imagine", "immense", "imminent", "impartial", "imperfect", "impolite", "important", "imported", "impossible", "impress", "improve",
            "impulse", "incandescent", "include", "income", "incompetent", "inconclusive", "increase", "incredible", "industrious", "industry", "inexpensive", "infamous", "influence", "inform", "inject",
            "injure", "ink", "innate", "innocent", "inquisitive", "insect", "insidious", "instinctive", "instruct", "instrument", "insurance", "intelligent", "intend", "interest", "interesting", "interfere",
            "internal", "interrupt", "introduce", "invent", "invention", "invincible", "invite", "irate", "iron", "irritate", "irritating", "island", "itch", "itchy", "jaded", "jagged", "jail", "jam", "jar",
            "jazzy", "jealous", "jeans", "jelly", "jellyfish", "jewel", "jittery", "jobless", "jog", "join", "joke", "jolly", "joyous", "judge", "judicious", "juggle", "juice", "juicy", "jumbled", "jump",
            "jumpy", "juvenile", "kaput", "keen", "kettle", "key", "kick", "kill", "kind", "kindhearted", "kindly", "kiss", "kittens", "kitty", "knee", "kneel", "knife", "knit", "knock", "knot", "knotty",
            "knowing", "knowledge", "knowledgeable", "known", "label", "labored", "laborer", "lace", "lackadaisical", "lacking", "ladybug", "lake", "lame", "lamentable", "lamp", "land", "language",
            "languid", "large", "last", "late", "laugh", "laughable", "launch", "lavish", "lazy", "lean", "learn", "learned", "leather", "left", "leg", "legal", "legs", "lethal", "letter", "letters",
            "lettuce", "level", "lewd", "library", "license", "lick", "lie", "light", "lighten", "like", "likeable", "limit", "limping", "line", "linen", "lip", "liquid", "list", "listen", "literate",
            "little", "live", "lively", "living", "load", "loaf", "lock", "locket", "lonely", "long", "long-term", "longing", "look", "loose", "lopsided", "loss", "loud", "loutish", "love", "lovely",
            "loving", "low", "lowly", "lucky", "ludicrous", "lumber", "lumpy", "lunch", "lunchroom", "lush", "luxuriant", "lying", "lyrical", "macabre", "machine", "macho", "maddening", "madly", "magenta",
            "magic", "magical", "magnificent", "maid", "mailbox", "majestic", "makeshift", "male", "malicious", "mammoth", "man", "manage", "maniacal", "many", "marble", "march", "mark", "marked", "market",
            "married", "marry", "marvelous", "mask", "mass", "massive", "match", "mate", "material", "materialistic", "matter", "mature", "meal", "mean", "measly", "measure", "meat", "meaty", "meddle",
            "medical", "meek", "meeting", "mellow", "melodic", "melt", "melted", "memorize", "memory", "men", "mend", "merciful", "mere", "mess up", "messy", "metal", "mice", "middle", "mighty", "military",
            "milk", "milky", "mind", "mindless", "mine", "miniature", "minister", "minor", "mint", "minute", "miscreant", "miss", "mist", "misty", "mitten", "mix", "mixed", "moan", "moaning", "modern", "moldy",
            "mom", "momentous", "money", "monkey", "month", "moon", "moor", "morning", "mother", "motion", "motionless", "mountain", "mountainous", "mourn", "mouth", "move", "muddle", "muddled", "mug",
            "multiply", "mundane", "murder", "murky", "muscle", "mushy", "mute", "mysterious", "nail", "naive", "name", "nappy", "narrow", "nasty", "nation", "natural", "naughty", "nauseating", "near", "neat",
            "nebulous", "necessary", "neck", "need", "needle", "needless", "needy", "neighborly", "nerve", "nervous", "nest", "new", "next", "nice", "nifty", "night", "nimble", "nine", "nippy", "nod", "noise",
            "noiseless", "noisy", "nonchalant", "nondescript", "nonstop", "normal", "north", "nose", "nostalgic", "nosy", "note", "notebook", "notice", "noxious", "null", "number", "numberless", "numerous",
            "nut", "nutritious", "nutty", "oafish", "oatmeal", "obedient", "obeisant", "obese", "obey", "object", "obnoxious", "obscene", "obsequious", "observant", "observation", "observe", "obsolete",
            "obtain", "obtainable", "occur", "ocean", "oceanic", "odd", "offbeat", "offend", "offer", "office", "oil", "old", "old-fashioned", "omniscient", "one", "onerous", "open", "opposite", "optimal",
            "orange", "oranges", "order", "ordinary", "organic", "ossified", "outgoing", "outrageous", "outstanding", "oval", "oven", "overconfident", "overflow", "overjoyed", "overrated", "overt",
            "overwrought", "owe", "own", "pack", "paddle", "page", "pail", "painful", "painstaking", "paint", "pale", "paltry", "pan", "pancake", "panicky", "panoramic", "paper", "parallel", "parcel",
            "parched", "park", "parsimonious", "part", "partner", "party", "pass", "passenger", "past", "paste", "pastoral", "pat", "pathetic", "pause", "payment", "peace", "peaceful", "pear", "peck",
            "pedal", "peel", "peep", "pen", "pencil", "penitent", "perfect", "perform", "periodic", "permissible", "permit", "perpetual", "person", "pest", "pet", "petite", "pets", "phobic", "phone",
            "physical", "picayune", "pick", "pickle", "picture", "pie", "pies", "pig", "pigs", "pin", "pinch", "pine", "pink", "pipe", "piquant", "pizzas", "place", "placid", "plain", "plan", "plane",
            "planes", "plant", "plantation", "plants", "plastic", "plate", "plausible", "play", "playground", "pleasant", "please", "pleasure", "plot", "plough", "plucky", "plug", "pocket", "point", "pointless",
            "poised", "poison", "poke", "polish", "polite", "political", "pollution", "poor", "pop", "popcorn", "porter", "position", "possess", "possessive", "possible", "post", "pot", "potato", "pour",
            "powder", "power", "powerful", "practice", "pray", "preach", "precede", "precious", "prefer", "premium", "prepare", "present", "preserve", "press", "pretend", "pretty", "prevent", "previous",
            "price", "pricey", "prick", "prickly", "print", "private", "probable", "produce", "productive", "profit", "profuse", "program", "promise", "property", "prose", "protect", "protective", "protest",
            "proud", "provide", "psychedelic", "psychotic", "public", "puffy", "pull", "pump", "pumped", "punch", "puncture", "punish", "punishment", "puny", "purple", "purpose", "purring", "push", "pushy",
            "puzzled", "puzzling", "quack", "quaint", "quarrelsome", "quarter", "quartz", "queen", "question", "questionable", "queue", "quick", "quickest", "quicksand", "quiet", "quill", "quilt", "quince",
            "quirky", "quiver", "quixotic", "quizzical", "rabbit", "rabbits", "rabid", "race", "racial", "radiate", "ragged", "rail", "railway", "rain", "rainstorm", "rainy", "raise", "rake", "rambunctious",
            "rampant", "range", "rapid", "rare", "raspy", "rat", "rate", "ratty", "ray", "reach", "reaction", "reading", "ready", "real", "realize", "reason", "rebel", "receipt", "receive", "receptive",
            "recess", "recognise", "recondite", "record", "red", "reduce", "redundant", "reflect", "reflective", "refuse", "regret", "regular", "reign", "reject", "rejoice", "relation", "relax", "release",
            "relieved", "religion", "rely", "remain", "remarkable", "remember", "remind", "reminiscent", "remove", "repair", "repeat", "replace", "reply", "report", "representative", "reproduce", "repulsive",
            "request", "rescue", "resolute", "resonant", "respect", "responsible", "rest", "retire", "return", "reward", "rhetorical", "rhyme", "rhythm", "rice", "rich", "riddle", "rifle", "right", "righteous",
            "rightful", "rigid", "ring", "rings", "rinse", "ripe", "risk", "ritzy", "river", "road", "roasted", "rob", "robin", "robust", "rock", "rod", "roll", "romantic", "roof", "room", "roomy", "root",
            "rose", "rot", "rotten", "rough", "round", "route", "royal", "rub", "ruddy", "rude", "ruin", "rule", "run", "rural", "rush", "rustic", "ruthless", "sable", "sack", "sad", "safe", "sail", "salt",
            "salty", "same", "sand", "sassy", "satisfy", "satisfying", "save", "savory", "saw", "scale", "scandalous", "scarce", "scare", "scarecrow", "scared", "scarf", "scary", "scatter", "scattered",
            "scene", "scent", "school", "science", "scientific", "scintillating", "scissors", "scold", "scorch", "scrape", "scratch", "scrawny", "scream", "screeching", "screw", "scribble", "scrub", "sea",
            "seal", "search", "seashore", "seat", "second", "second-hand", "secret", "secretary", "secretive", "sedate", "seed", "seemly", "selection", "selective", "self", "selfish", "sense", "separate",
            "serious", "servant", "serve", "settle", "shade", "shaggy", "shake", "shaky", "shallow", "shame", "shape", "share", "sharp", "shave", "sheep", "sheet", "shelf", "shelter", "shiny", "ship", "shirt",
            "shiver", "shivering", "shock", "shocking", "shoe", "shoes", "shop", "short", "show", "shrill", "shrug", "shut", "shy", "sick", "side", "sidewalk", "sigh", "sign", "signal", "silent", "silk",
            "silky", "silly", "silver", "simple", "simplistic", "sin", "sincere", "sink", "sip", "sister", "sisters", "six", "size", "skate", "ski", "skillful", "skin", "skinny", "skip", "skirt", "sky",
            "slap", "slave", "sleep", "sleepy", "sleet", "slim", "slimy", "slip", "slippery", "slope", "sloppy", "slow", "small", "smart", "smash", "smell", "smelly", "smile", "smiling", "smoggy", "smoke",
            "smooth", "snail", "snails", "snake", "snakes", "snatch", "sneaky", "sneeze", "sniff", "snobbish", "snore", "snotty", "snow", "soak", "soap", "society", "sock", "soda", "sofa", "soft", "soggy",
            "solid", "somber", "son", "song", "songs", "soothe", "sophisticated", "sordid", "sore", "sort", "sound", "soup", "sour", "space", "spade", "spare", "spark", "sparkle", "sparkling", "special",
            "spectacular", "spell", "spicy", "spiders", "spiffy", "spiky", "spill", "spiritual", "spiteful", "splendid", "spoil", "sponge", "spooky", "spoon", "spot", "spotless", "spotted", "spotty", "spray",
            "spring", "sprout", "spurious", "spy", "squalid", "square", "squash", "squeak", "squeal", "squealing", "squeamish", "squeeze", "squirrel", "stage", "stain", "staking", "stale", "stamp", "standing",
            "star", "stare", "start", "statement", "station", "statuesque", "stay", "steadfast", "steady", "steam", "steel", "steep", "steer", "stem", "step", "stereotyped", "stew", "stick", "sticks", "sticky",
            "stiff", "stimulating", "stingy", "stir", "stitch", "stocking", "stomach", "stone", "stop", "store", "stormy", "story", "stove", "straight", "strange", "stranger", "strap", "straw", "stream",
            "street", "strengthen", "stretch", "string", "strip", "striped", "stroke", "strong", "structure", "stuff", "stupendous", "stupid", "sturdy", "subdued", "subsequent", "substance", "substantial",
            "subtract", "succeed", "successful", "succinct", "suck", "sudden", "suffer", "sugar", "suggest", "suggestion", "suit", "sulky", "summer", "sun", "super", "superb", "superficial", "supply",
            "support", "suppose", "supreme", "surprise", "surround", "suspect", "suspend", "swanky", "sweater", "sweet", "sweltering", "swift", "swim", "swing", "switch", "symptomatic", "synonymous", "system",
            "table", "taboo", "tacit", "tacky", "tail", "talented", "talk", "tall", "tame", "tan", "tangible", "tangy", "tank", "tap", "tart", "taste", "tasteful", "tasteless", "tasty", "tawdry", "tax",
            "teaching", "team", "tearful", "tease", "tedious", "teeny", "teeny-tiny", "teeth", "telephone", "telling", "temper", "temporary", "tempt", "ten", "tendency", "tender", "tense", "tent", "tenuous",
            "terrible", "terrific", "terrify", "territory", "test", "tested", "testy", "texture", "thank", "thankful", "thaw", "theory", "therapeutic", "thick", "thin", "thing", "things", "thinkable", "third",
            "thirsty", "thought", "thoughtful", "thoughtless", "thread", "threatening", "three", "thrill", "throat", "throne", "thumb", "thunder", "thundering", "tick", "ticket", "tickle", "tidy", "tie",
            "tiger", "tight", "tightfisted", "time", "tin", "tiny", "tip", "tire", "tired", "tiresome", "title", "toad", "toe", "toes", "tomatoes", "tongue", "tooth", "toothbrush", "toothpaste", "toothsome",
            "top", "torpid", "touch", "tough", "tour", "tow", "towering", "town", "toy", "toys", "trace", "trade", "trail", "train", "trains", "tramp", "tranquil", "transport", "trap", "trashy", "travel",
            "tray", "treat", "treatment", "tree", "trees", "tremble", "tremendous", "trick", "tricky", "trip", "trite", "trot", "trouble", "troubled", "trousers", "truck", "trucks", "truculent", "true",
            "trust", "truthful", "try", "tub", "tug", "tumble", "turkey", "turn", "twig", "twist", "two", "type", "typical", "ubiquitous", "ugliest", "ugly", "ultra", "umbrella", "unable", "unaccountable",
            "unadvised", "unarmed", "unbecoming", "unbiased", "uncle", "uncovered", "understood", "underwear", "undesirable", "undress", "unequal", "unequaled", "uneven", "unfasten", "unhealthy", "uninterested",
            "unique", "unit", "unite", "unkempt", "unknown", "unlock", "unnatural", "unpack", "unruly", "unsightly", "unsuitable", "untidy", "unused", "unusual", "unwieldy", "unwritten", "upbeat", "uppity",
            "upset", "uptight", "use", "used", "useful", "useless", "utopian", "utter", "uttermost", "vacation", "vacuous", "vagabond", "vague", "valuable", "value", "van", "vanish", "various", "vase", "vast",
            "vegetable", "veil", "vein", "vengeful", "venomous", "verdant", "verse", "versed", "vessel", "vest", "victorious", "view", "vigorous", "violent", "violet", "visit", "visitor", "vivacious", "voice",
            "voiceless", "volatile", "volcano", "volleyball", "voracious", "voyage", "vulgar", "wacky", "waggish", "wail", "wait", "waiting", "wakeful", "walk", "wall", "wander", "wandering", "want", "wanting",
            "war", "warlike", "warm", "warn", "wary", "wash", "waste", "wasteful", "watch", "water", "watery", "wave", "waves", "wax", "way", "weak", "wealth", "wealthy", "weary", "weather", "week", "weigh",
            "weight", "welcome", "well-groomed", "well-made", "well-off", "well-to-do", "wet", "wheel", "whimsical", "whine", "whip", "whirl", "whisper", "whispering", "whistle", "white", "whole", "wholesale",
            "wicked", "wide", "wide-eyed", "wiggly", "wild", "wilderness", "willing", "wind", "window", "windy", "wine", "wing", "wink", "winter", "wipe", "wire", "wiry", "wise", "wish", "wistful", "witty",
            "wobble", "woebegone", "woman", "womanly", "women", "wonder", "wonderful", "wood", "wooden", "wool", "woozy", "word", "work", "workable", "worm", "worried", "worry", "worthless", "wound", "wrap",
            "wrathful", "wreck", "wren", "wrench", "wrestle", "wretched", "wriggle", "wrist", "writer", "writing", "wrong", "wry", "x-ray", "yak", "yam", "yard", "yarn", "yawn", "year", "yell", "yellow",
            "yielding", "yoke", "young", "youthful", "yummy", "zany", "zealous", "zebra", "zephyr", "zesty", "zinc", "zip", "zipper", "zippy", "zonked", "zoo", "zoom"
        };

        private static string Output<T>(T[] arr)
        {
            if (arr == null)
                return "";

            return arr
                .Select(a => a.ToString())
                .JoinUsing(", ")
                .WrapIn("[", "]");
        }

        private static IIonSymbol.Operators[] RandomOperatorSymbols()
        {
            var length = SecureRandom.NextInt(5) + 1;
            var enums = Enum.GetValues<IIonSymbol.Operators>();
            return SecureRandom
                .NextSequence(length, enums.Length)
                .Select(index => enums[index])
                .ToArray();
        }
    }
}