using Axis.Ion.Types;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using static Axis.Ion.Types.IIonType;
using System.Globalization;
using Axis.Luna.Extensions;
using System.Text;
using Axis.Pulsar.Grammar.Language;
using Axis.Pulsar.Languages.xBNF;
using Axis.Pulsar.Grammar.CST;
using Axis.Pulsar.Grammar;
using Axis.Pulsar.Grammar.Recognizers.Results;
using Axis.Pulsar.Languages;

namespace Axis.Ion.IO
{
    //public static class IonIO
    //{
    //    //private static readonly Regex _IndependentNewLinePattern = new Regex(@"(?<=[\r\n])\n|^\n|\n$");
    //    private static Grammar _IonGrammar;
    //    static IonIO()
    //    {
    //        using var ionXbnfStream = typeof(IonIO).Assembly
    //            .GetManifestResourceStream($"{typeof(IonIO).Namespace}.IonGrammar.xbnf");

    //        _IonGrammar = new Importer()
    //            .ImportGrammar(ionXbnfStream);
    //    }

    //    #region IO methods for textual representation of ion

    //    #region OUT
    //    public static int Write(this
    //        IIonType[] ionArray,
    //        TextWriter writer,
    //        WriterOptions? options = null)
    //    {
    //        if (writer == null)
    //            throw new ArgumentNullException(nameof(writer));

    //        return ionArray
    //            .Select(ionType => SerializeType(ionType, options))
    //            .Select(valueString => $"{valueString}{AppendSpaces(options)}")
    //            .WithEach(writer.Write)
    //            .Count();
    //    }

    //    public static async Task<int> WriteAsync(this
    //        IIonType[] ionArray,
    //        TextWriter writer,
    //        WriterOptions? options = null)
    //    {
    //        if (writer == null)
    //            throw new ArgumentNullException(nameof(writer));

    //        foreach (var ionType in ionArray)
    //        {
    //            var serialized = SerializeType(ionType);
    //            serialized += AppendSpaces(options);
    //            await writer.WriteAsync(serialized);
    //        }

    //        return ionArray.Length;
    //    }

    //    public static int Write(this
    //        IIonType ion,
    //        TextWriter writer,
    //        WriterOptions? options = null)
    //        => Write(new[] { ion }, writer, options);

    //    public static Task<int> WriteAsync(this
    //        IIonType ion,
    //        TextWriter writer,
    //        WriterOptions? options = null)
    //        => WriteAsync(new[] { ion }, writer, options);


    //    internal static string SerializeType(
    //        IIonType ionType,
    //        WriterOptions? options = null,
    //        int indentation = 0)
    //    {
    //        options ??= new WriterOptions();

    //        var ionText = ionType switch
    //        {
    //            null => "null.null",

    //            IonSexp sexp => SerializeContainer(
    //                options: options,
    //                ionContainer: sexp,
    //                collectionOptions: new CollectionSerializationOptions("(", ")", " ", false, indentation)),

    //            IonList list => SerializeContainer(
    //                options: options,
    //                ionContainer: list,
    //                collectionOptions: new CollectionSerializationOptions("[", "]", ", ", false, indentation)),

    //            IonStruct @struct => SerializeContainer(
    //                options: options,
    //                ionContainer: @struct,
    //                collectionOptions: new CollectionSerializationOptions("{", "}", ", ", false, indentation)),

    //            _ => options.IgnoreValueAnnotations
    //                ? ionType.ToIonText()
    //                : ionType.ToString(),
    //        };

    //        return $"{(options.UseIndentation? Indent(indentation): "")}{ionText}";
    //    }

    //    internal static string SerializeContainer<TValue>(
    //        WriterOptions? options,
    //        CollectionSerializationOptions collectionOptions,
    //        IIonConainer<TValue> ionContainer)
    //    {
    //        options ??= new WriterOptions();
    //        var sb = new StringBuilder();
    //        var items = ionContainer.Value ?? Array.Empty<TValue>();

    //        // append the start delimiter
    //        sb.Append(options.UseIndentation && !collectionOptions.IgnoreStartDelimiterIndentation
    //            ? $"{Indent(collectionOptions.Indentation)}{collectionOptions.StartDelimiter}"
    //            : $"{collectionOptions.StartDelimiter}");

    //        var itemIndentationCount = collectionOptions.Indentation + 1;
    //        var itemIndentations = Indent(itemIndentationCount);
            
    //        return items
    //            .Select(item => item switch
    //            {
    //                IIonType ionType => SerializeType(ionType, options, itemIndentationCount),
    //                IonStruct.Property property => property.Value switch
    //                {
    //                    IonSexp sexp => SerializeContainer(
    //                        options: options,
    //                        ionContainer: sexp,
    //                        collectionOptions: new CollectionSerializationOptions("(", ")", " ", true, itemIndentationCount)),

    //                    IonList list => SerializeContainer(
    //                        options: options,
    //                        ionContainer: list,
    //                        collectionOptions: new CollectionSerializationOptions("[", "]", ", ", true, itemIndentationCount)),

    //                    IonStruct @struct => SerializeContainer(
    //                        options: options,
    //                        ionContainer: @struct,
    //                        collectionOptions: new CollectionSerializationOptions("{", "}", ", ", true, itemIndentationCount)),

    //                    _ => $"{itemIndentations}{property.Name}:{SerializeType(property.Value)}"
    //                },
    //                _ => throw new InvalidOperationException($"Invalid item: {item}")
    //            })
    //            .JoinUsing($"{collectionOptions.ItemSeparator}{(options.UseIndentation ? Environment.NewLine : "")}")
    //            .ApplyTo(sb.Append)
    //            .Append(options.UseIndentation && !collectionOptions.IgnoreStartDelimiterIndentation
    //                ? $"{Indent(collectionOptions.Indentation)}{collectionOptions.EndDelimiter}"
    //                : $"{collectionOptions.StartDelimiter}")
    //            .ToString();
    //    }

    //    internal static string Indent(int indentations)
    //    {
    //        if (indentations < 0)
    //            throw new ArgumentException($"Invalid indentation: {indentations}");

    //        var sb = new StringBuilder();
    //        for (int cnt = 0; cnt < indentations; cnt++)
    //            sb.Append("\t");

    //        return sb.ToString();
    //    }

    //    internal static string AppendSpaces(WriterOptions? options)
    //    {
    //        var spaceCount = options?.RootValueSpacing ?? 1;
    //        if (spaceCount <= 0)
    //            spaceCount = 1;

    //        var sb = new StringBuilder();
    //        for (int cnt = 0; cnt < spaceCount; cnt++)
    //            sb.AppendLine();

    //        return sb.ToString();
    //    }
    //    #endregion

    //    #region IN

    //    public static IIonType[] ReadIon(this TextReader reader)
    //    {
    //        var ionText = reader.ReadToEnd();
    //        var bufferedReader = new BufferedTokenReader(ionText);
    //        var parseResult = _IonGrammar.RootRecognizer().Recognize(bufferedReader);

    //        return parseResult switch
    //        {
    //            SuccessResult success => success.Symbol
    //                .FindNodes("ion-value")
    //                .Select(ToIonType)
    //                .ToArray(),

    //            FailureResult failed => throw new FormatException($"An error occured while parsing the text at position: {failed.Position}"),

    //            ErrorResult exception => throw new FormatException($"An error occured while parsing the text: {exception.Exception.Message}", exception.Exception),

    //            _ => throw new InvalidOperationException($"Invalid result type: {parseResult}")
    //        };
    //    }

    //    public static async Task<IIonType[]> ReadionAsync(this TextReader reader)
    //    {
    //        var ionText = await reader.ReadToEndAsync();
    //        var bufferedReader = new BufferedTokenReader(ionText);
    //        var parseResult = _IonGrammar.RootRecognizer().Recognize(bufferedReader);

    //        return parseResult switch
    //        {
    //            SuccessResult success => success.Symbol
    //                .FindNodes("ion-value")
    //                .Select(ToIonType)
    //                .ToArray(),

    //            FailureResult failed => throw new FormatException($"An error occured while parsing the text at position: {failed.Position}"),

    //            ErrorResult error => throw new FormatException($"An error occured while parsing the text: {error.Exception.Message}", error.Exception),

    //            _ => throw new InvalidOperationException($"Invalid result type: {parseResult}")
    //        };
    //    }

    //    private static IIonType ToIonType(CSTNode ionValue)
    //    {
    //        var ionType = ionValue.FindNode("value").FirstNode();
    //        var annotations = ionValue
    //            .FindNodes("annotatio-list.annotation")?
    //            .Select(ToIonAnnotation)
    //            .ToArray()
    //            ?? Array.Empty<Annotation>();

    //        return ionType.SymbolName switch
    //        {
    //            "ion-null" => new IonNull(annotations),

    //            "ion-bool" => new IonBool(
    //                bool.TryParse(ionType.TokenValue(), out var value) ? value : (bool?) null,
    //                annotations),

    //            "ion-int" => new IonInt(
    //                long.TryParse(ionType.TokenValue(), out var value) ? value : (long?)null,
    //                annotations),

    //            "ion-float" => new IonFloat(
    //                double.TryParse(ionType.TokenValue(), out var value) ? value : (double?)null,
    //                annotations),

    //            "ion-decimal" => new IonDecimal(
    //                decimal.TryParse(
    //                    NormalizeDecimalString(ionType.TokenValue()),
    //                    NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint,
    //                    null,
    //                    out var value)
    //                ? value
    //                : (decimal?)null,
    //                annotations),

    //            "ion-timestamp" => new IonTimestamp(
    //                ToTimestamp(ionType),
    //                annotations),

    //            "ion-string" => new IonString(
    //                ToString(ionType),
    //                annotations),

    //            "ion-symbol" => ToIonSymbol(ionType, annotations),

    //            "ion-blob" => new IonBlob(
    //                ToBlobBytes(ionType),
    //                annotations),

    //            "ion-clob" => new IonClob(
    //                ToClobBytes(ionType),
    //                annotations),

    //            "ion-list" => new IonList(
    //                new IonList.Initializer(
    //                    annotations,
    //                    ionType
    //                        .FindNodes("ion-value")
    //                        .Select(ToIonType)
    //                        .ToArray())),

    //            "ion-sexp" => new IonSexp(
    //                new IonSexp.Initializer(
    //                    annotations,
    //                    ionType
    //                        .FindNodes("ion-symbol")
    //                        .Select(ToIonType)
    //                        .ToArray())),

    //            "ion-struct" => new IonStruct(
    //                new IonStruct.Initializer(
    //                    annotations,
    //                    ionType
    //                        .FindNodes("struct-field")
    //                        .Select(ToProperty)
    //                        .ToArray())),

    //            _ => throw new InvalidOperationException($"Invalid ion-value: {ionType}")
    //        };
    //    }

    //    internal static DateTimeOffset? ToTimestamp(CSTNode ionTimestamp)
    //    {
    //        var precision = ionTimestamp.FirstNode();
    //        if (precision.SymbolName == "null-timestamp")
    //            return null;

    //        var year = precision.FindNode("year").TokenValue();
    //        var month = precision.FindNode("month")?.TokenValue() ?? "01";
    //        var day = precision.FindNode("day")?.TokenValue() ?? "01";
    //        var hour = precision.FindNode("hour")?.TokenValue() ?? "00";
    //        var minute = precision.FindNode("minute")?.TokenValue() ?? "00";
    //        var second = precision.FindNode("second")?.TokenValue() ?? "00";
    //        var milliseconds = precision.FindNode("millisecond")?.TokenValue() ?? "000";
    //        var timezoneOffset = precision.FindNode("time-zone-offset")?.TokenValue().Replace("+", "") ?? "00:00";

    //        return new DateTimeOffset(
    //            int.Parse(year),
    //            int.Parse(month),
    //            int.Parse(day),
    //            int.Parse(hour),
    //            int.Parse(minute),
    //            int.Parse(second),
    //            int.Parse(milliseconds),
    //            timezoneOffset.Equals("Z")
    //                ? TimeSpan.Parse("00:00")
    //                : TimeSpan.Parse(timezoneOffset));
    //    }

    //    internal static string? ToString(CSTNode ionString)
    //    {
    //        var stringKind = ionString.FirstNode();

    //        return stringKind.SymbolName switch
    //        {
    //            "null-string" => null,

    //            "singleline-string" => stringKind
    //                .TokenValue()
    //                .Trim("\"")
    //                .ApplyTo(ApplyEscapeForQuotedString),

    //            "multiline-string" => stringKind
    //                .FindNodes("ml-string")
    //                .Select(node => node
    //                    .TokenValue()
    //                    .Trim("'''")
    //                    .TrimStart("\\\n"))
    //                .JoinUsing("")
    //                .ApplyTo(ApplyEscapeForSQuotedString),

    //            _ => throw new InvalidOperationException($"Invalid symbol node: {stringKind.SymbolName}")
    //        };
    //    }

    //    internal static byte[]? ToBlobBytes(CSTNode ionBlob)
    //    {
    //        var valueNode = ionBlob.NodeAt(0);
    //        return valueNode.SymbolName switch
    //        {
    //            "null-blob" => null,
    //            "blob-value" => valueNode
    //                .FindNode("base-64")
    //                .TokenValue()
    //                .ApplyTo(Convert.FromBase64String),

    //            _ => throw new InvalidOperationException($"Invalid node: {valueNode.SymbolName}")
    //        };
    //    }

    //    internal static byte[]? ToClobBytes(CSTNode ionClob)
    //    {
    //        var valueNode = ionClob.NodeAt(0);
    //        return valueNode.SymbolName switch
    //        {
    //            "null-clob" => null,

    //            "clob-value" => valueNode.FindNode("singleline-string|clob-multiline-string") switch
    //            {
    //                CSTNode rstring when rstring.SymbolName.Equals("singleline-string") => rstring
    //                    .TokenValue()
    //                    .Trim("\"")
    //                    .ApplyTo(ApplyEscapeForQuotedString)
    //                    .ApplyTo(Encoding.Unicode.GetBytes),

    //                CSTNode cmstring => cmstring
    //                    .FindNodes("ml-string")
    //                    .Select(node => node
    //                        .TokenValue()
    //                        .Trim("'''")
    //                        .TrimStart("\\\n"))
    //                    .JoinUsing("")
    //                    .ApplyTo(ApplyEscapeForSQuotedString)
    //                    .ApplyTo(Encoding.Unicode.GetBytes),

    //                _ => throw new InvalidOperationException("Invalid symbol")
    //            },
    //            _ => throw new InvalidOperationException($"Invalid symbol name: {valueNode.SymbolName}")
    //        };
    //    }

    //    internal static IonStruct.Property ToProperty(CSTNode structField)
    //    {
    //        var propertyNameNode = structField.NodeAt(0);

    //        return new IonStruct.Property(
    //            value: ToIonType(structField.FindNode("ion-value")),
    //            name: propertyNameNode.SymbolName switch
    //            {
    //                "identifier" => propertyNameNode.TokenValue(),

    //                "regular-string" => propertyNameNode
    //                    .FindNode("d-unquoted-text")
    //                    .TokenValue(),

    //                "quoted-symbol" => propertyNameNode
    //                    .FindNode("s-unquoted-text")
    //                    .TokenValue(),

    //                _ => throw new InvalidOperationException($"Invalid property name symbol: {propertyNameNode.SymbolName}")
    //            });
    //    }

    //    internal static IIonType.Annotation ToIonAnnotation(CSTNode annotation) => IIonType.Annotation.Parse(annotation.TokenValue());

    //    internal static string NormalizeDecimalString(string decimalString)
    //    {
    //        if (string.IsNullOrWhiteSpace(decimalString))
    //            throw new ArgumentNullException(nameof(decimalString));

    //        if (decimalString.StartsWith("null"))
    //            return decimalString;

    //        if (decimalString.Any(c => c == 'd' || c == 'D'))
    //            return decimalString.Replace('d', 'e').Replace('D', 'E');

    //        return decimalString;
    //    }

    //    internal static string ApplyEscapeForQuotedString(string value) => value.ApplyEscape();

    //    internal static string ApplyEscapeForSQuotedString(string value) => value.ApplyEscape();

    //    internal static string? NewLines(int count)
    //    {
    //        var sb = new StringBuilder();
    //        for (int cnt = 0; cnt < count; cnt++)
    //            sb.Append(Environment.NewLine);

    //        return sb.ToString();
    //    }

    //    internal static int ExtractIndependentNewLineCount(CSTNode blockSpace)
    //    {
    //        return blockSpace
    //            .FindNodes("char-space")
    //            .Select(cs => cs.TokenValue())
    //            .JoinUsing("")
    //            .Count(c => c == '\n');
    //    }

    //    internal static IIonType ToIonSymbol(CSTNode ionSymbol, Annotation[] annotations)
    //    {
    //        var symbolType = ionSymbol.FirstNode();

    //        return symbolType.SymbolName switch
    //        {
    //            "null-symbol" => IIonType.NullOf(IonTypes.IdentifierSymbol),
    //            "quoted-symbol" => new IonQuotedSymbol(
    //                ApplyEscapeForSQuotedString(symbolType.TokenValue()),
    //                annotations),
    //            "identifier" => new IonIdentifier(
    //                symbolType.TokenValue(),
    //                annotations),
    //            "operator-symbol" => IonOperator.Parse(
    //                symbolType.TokenValue(),
    //                annotations),
    //            _ => throw new ArgumentException($"Invalid symbol: {symbolType.SymbolName}")
    //        };
    //    }
    //    #endregion

    //    #endregion


    //    #region IO methods for binary representation of ion

    //    #endregion


    //    internal record struct CollectionSerializationOptions(
    //        string StartDelimiter,
    //        string EndDelimiter,
    //        string ItemSeparator,
    //        bool IgnoreStartDelimiterIndentation,
    //        int Indentation);
    //}
}
