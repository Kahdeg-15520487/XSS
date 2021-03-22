using System;
using System.Collections.Generic;
using System.Text;

using XSS.Compiler.Contract;
using XSS.Compiler.Implementation;

namespace XSS.Compiler
{
    public class Compiler
    {
        public static void Compile(string source)
        {
            ILexer lexer = new Lexer(source);
            IParser parser = new Parser(lexer);
        }
    }
}
