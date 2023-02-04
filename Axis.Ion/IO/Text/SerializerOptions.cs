using System;

namespace Axis.Ion.IO.Text
{
    /// <summary>
    /// make this a record, or a readonly struct, or something
    /// </summary>
    public class SerializerOptions
    {
        /// <summary>
        /// TODO
        /// </summary>
        public NullOptions Nulls { get; } = new NullOptions();

        /// <summary>
        /// TODO
        /// </summary>
        public IntOptions Ints { get; } = new IntOptions();

        /// <summary>
        /// TODO
        /// </summary>
        public BoolOptions Bools { get; } = new BoolOptions();

        /// <summary>
        /// TODO
        /// </summary>
        public DecimalOptions Decimals { get; } = new DecimalOptions();

        /// <summary>
        /// TODO
        /// </summary>
        public TimestampOptions Timestamps { get; } = new TimestampOptions();

        /// <summary>
        /// TODO
        /// </summary>
        public StringOptions Strings { get; } = new StringOptions();

        /// <summary>
        /// TODO
        /// </summary>
        public StringOptions Clobs { get; } = new StringOptions();

        /// <summary>
        /// TODO
        /// </summary>
        public IndentationStyles IndentationStyle { get; set; }


        #region nested enums
        public enum IndentationStyles
        {
            None,
            Tabs,
            Spaces
        }

        public enum IntFormat
        {
            Decimal,
            BigBinary,
            SmallBinary,
            BigHex,
            SmallHex
        }

        public enum Case
        {
            Lowercase,
            Uppercase,
            Titlecase
        }

        public enum TimestampPrecision
        {
            MilliSecond,
            Second,
            Minute,
            Day,
            Month,
            Year
        }

        public enum StringLineStyle
        {
            Singleline,
            Multiline
        }
        #endregion

        #region nested types
        public class NullOptions
        {
            /// <summary>
            /// Indicates if long-form nulls (null.null) should be used when serializing to text.
            /// </summary>
            public bool UseLongFormNulls { get; set; }
        }

        public class IntOptions
        {
            public IntFormat NumberFormat { get; set; }

            public bool UseDigitSeparator { get; set; }
        }

        public class BoolOptions
        {
            public Case ValueCase { get; set; }
        }

        public class DecimalOptions
        {
            public bool UseExponentNotation { get; set; }
        }

        public class TimestampOptions
        {
            public TimestampPrecision TimestampPrecision { get; set; }
        }

        public class StringOptions
        {
            private ushort lineBreakPoint = 100;

            public StringLineStyle LineStyle { get; set; }

            public ushort LineBreakPoint
            {
                get => lineBreakPoint;
                set => lineBreakPoint = value == 0
                    ? throw new ArgumentException("Line break cannot be 0")
                    : value;
            }
        }
        #endregion
    }
}
