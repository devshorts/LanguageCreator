using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lang.Data;

namespace Lang.Matches
{
    public class MatchNumber : MatcherBase
    {
        protected override Token IsMatchImpl(Lexer lexer)
        {

            var leftOperand = GetIntegers(lexer);

            if (leftOperand != null)
            {
                if (lexer.Current == ".")
                {
                    lexer.Consume();

                    var rightOperand = GetIntegers(lexer);

                    // found a float
                    if (rightOperand != null)
                    {
                        return new Token(TokenType.Float, leftOperand + "." + rightOperand);
                    }
                }

                return new Token(TokenType.Int, leftOperand);
            }
            
            return null;
        }

        private String GetIntegers(Lexer lexer)
        {
            var regex = new Regex("[0-9]");

            String num = null;

            while (lexer.Current != null && regex.IsMatch(lexer.Current))
            {
                num += lexer.Current;
                lexer.Consume();
            }

            if (num != null)
            {
                return num;
            }

            return null;
            
        }
    }

}
