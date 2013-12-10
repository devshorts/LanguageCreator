using Lang.Lexers;
using Lang.Parser;
using Lang.Visitors;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
			var files = new List<string> (args).Where (File.Exists);

			var str = files.Select (File.ReadAllText)
						   .Aggregate (string.Empty, (acc, item) => acc + Environment.NewLine + item);

			if (String.IsNullOrWhiteSpace (str)) {
				Console.WriteLine ("No available files for compilation");

				return;
			}

            var ast = new LanguageParser(new Lexer(str)).Parse();

            new InterpretorVisitor().Start(ast);
        }
    }
}
