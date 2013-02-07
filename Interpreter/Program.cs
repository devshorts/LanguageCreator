using Lang;
using Lang.Parser;
using Lang.Visitors;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = args[0];

            var ast = new LanguageParser(new Tokenizer(str)).Parse();

            new InterpretorVisitor().Start(ast);
        }
    }
}
