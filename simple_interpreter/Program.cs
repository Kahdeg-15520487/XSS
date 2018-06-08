using System;
using System.Collections.Generic;
using System.IO;

namespace simple_interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Dictionary<string, object> variables = new Dictionary<string, object>();
                Interpreter interpreter = new Interpreter(variables);
                while (true)
                {
                    Console.Write("> ");
                    string text = Console.ReadLine();
                    if (text.Length == 0)
                    {
                        continue;
                    }
                    Lexer lexer = new Lexer(text);
                    try
                    {
                        Parser parser = new Parser(lexer);
                        var compileResult = parser.Parse();
                        compileResult.Accept(interpreter);
                        var result = interpreter.Evaluate();

                        if (compileResult is AST.Assignment)
                        {
                            var ass = compileResult as AST.Assignment;
                            Console.Write(ass.ident.token.lexeme + " <- ");
                        }

                        if (result is null)
                        {
                            Console.WriteLine("null");
                        }
                        else if (result.GetType() == typeof(string))
                        {
                            Console.WriteLine("\"{0}\"", result);
                        }
                        else if (result.GetType() == typeof(char))
                        {
                            Console.WriteLine("'{0}'", result);
                        }
                        else
                        {
                            Console.WriteLine(result);
                        }

                        //DotVisualizer dotvisitor = new DotVisualizer();
                        //result.Accept(dotvisitor);
                        //File.WriteAllText("ast.dot", dotvisitor.Output);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            else
            {
                Dictionary<string, object> variables = new Dictionary<string, object>();
                Interpreter interpreter = new Interpreter(variables);
                //var text = File.ReadAllText(args[0]);
                foreach (var text in File.ReadLines(args[0]))
                {
                    Lexer lexer = new Lexer(text);
                    Parser parser = new Parser(lexer);
                    try
                    {
                        var result = parser.Parse();
                        result.Accept(interpreter);
                        Console.WriteLine(interpreter.Evaluate());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                Console.ReadLine();
            }
        }
    }
}
