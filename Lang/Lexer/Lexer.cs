using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lang
{
    public class Lexer : LexableStreamBase<String>
    {
        public Lexer(String source)
            : base(() => source.ToCharArray().Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList())
        {

        }
    }
}
