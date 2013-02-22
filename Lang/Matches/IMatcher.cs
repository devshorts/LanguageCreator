using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Lexers;

namespace Lang.Matches
{
    public interface IMatcher 
    {
        Token IsMatch(Tokenizer tokenizer);
    }
}
