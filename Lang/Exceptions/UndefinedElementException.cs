using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Exceptions
{
    public class UndefinedElementException : Exception
    {
        public UndefinedElementException(string msg, params string[] param) : base(String.Format(msg, param))
        {
            
        }
    }
}
