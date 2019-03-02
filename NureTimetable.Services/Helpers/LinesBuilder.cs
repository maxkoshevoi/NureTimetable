using System;
using System.Collections.Generic;
using System.Text;

namespace NureTimetable.Services.Helpers
{
    public class LinesBuilder
    {
        static readonly string separator = Environment.NewLine;

        public IEnumerable<string> Lines { get; set; }

        public LinesBuilder() { }

        public LinesBuilder(params string[] lines)
        {
            Lines = lines;
        }

        public override string ToString()
        {
            return string.Join(separator, Lines);
        }
    }
}
