using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Exceptions
{
    public class InvalidSyntax : Exception
    {
        public InvalidSyntax(string format) : base(format)
        {
        }
    }
}
