using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Exceptions
{
    public class ReturnException : Exception
    {
        public dynamic Value { get; private set; }

        public ReturnException(dynamic value)
        {
            Value = value;
        }
    }
}
