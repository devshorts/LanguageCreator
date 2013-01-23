using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.Matches
{
    public interface IMatcher 
    {
        Token IsMatch(Lexer lexer);
    }
}
