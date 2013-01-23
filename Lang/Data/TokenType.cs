using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Data
{
    public enum TokenType
    {
        Void,
        Int,
        WhiteSpace,
        LBracket,
        RBracket,
        Plus,
        Minus,
        Equals,
        HashTag,
        QuotedString,
        Word,
        Comma,
        OpenParenth,
        CloseParenth,
        Asterix,
        Slash,
        Carat,
        DeRef,
        Ampersand,
        Fun,
        GreaterThan,
        LessThan,
        SemiColon,
        ScopeStart,
        EOF
    }
}
