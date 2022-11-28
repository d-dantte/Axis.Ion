using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Ion.IO
{
    public class WriterOptions
    {
        public bool UseIndentation { get; set; }

        public bool IgnoreNullProperties { get; set; }

        public int RootValueSpacing { get; set; }

        public bool IgnoreValueAnnotations { get; set; } = false;
    }
}
