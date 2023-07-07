using System;
using System.Text;
using static Axis.Ion.IO.Text.SerializerOptions;

namespace Axis.Ion.IO.Text
{
    public readonly struct SerializingContext
    {
        public SerializerOptions Options { get; }

        public int IndentationLevel { get; }

        public SerializingContext(
            SerializerOptions options,
            ushort indentationLevel)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            IndentationLevel = indentationLevel;
        }

        public SerializingContext(SerializerOptions options)
        : this(options, 0)
        {

        }

        public string Indentation(ushort additionalIndentationLevels = 0)
        {
            var indentation = Options.IndentationStyle switch
            {
                 IndentationStyles.None => "",
                 IndentationStyles.Spaces => "    ",
                 IndentationStyles.Tabs => "\t",
                 _ => throw new ArgumentException($"Invalid indentation style: {Options.IndentationStyle}")
            };

            if ("".Equals(indentation))
                return "";

            var sb = new StringBuilder();
            (IndentationLevel + additionalIndentationLevels).Repeat(() => sb.Append(indentation));

            return sb.ToString();
        }

        public SerializingContext IndentContext(ushort additionalIndentationLevels = 0)
        {
            return new SerializingContext(
                indentationLevel: (ushort)(additionalIndentationLevels + IndentationLevel + 1),
                options: Options);
        }
    }
}
