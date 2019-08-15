using System;
using System.Collections.Generic;
using System.IO;
using XSS.Utility;

namespace XSS
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                bool isPrintParsed = true;
                Scope global = new Scope();
                Interpreter interpreter = new Interpreter(global);
                while (true)
                {
                    Console.Write("> ");
                    string text = Console.ReadLine();
                    if (text.Length == 0)
                    {
                        continue;
                    }
                    if (text.StartsWith("."))
                    {
                        RunCommand(text, global);
                        continue;
                    }
                    text = text.TrimEnd() + ';';
                    Lexer lexer = new Lexer(text);
                    try
                    {
                        Parser parser = new Parser(lexer);
                        var compileResult = parser.Parse();

                        if (isPrintParsed)
                        {
                            Console.WriteLine();
                            Console.WriteLine("==pretty print==");
                            compileResult.Accept(new PrettyPrinter());
                            Console.WriteLine("==pretty print==");
                            Console.WriteLine();
                        }

                        interpreter.Execute(compileResult);

                        //if (compileResult is AST.Assignment)
                        //{
                        //    var ass = compileResult as AST.Assignment;
                        //    Console.Write(ass.ident.token.lexeme + " <- ");
                        //}
                        //else if (compileResult is AST.VariableDeclareStatement)
                        //{
                        //    var vardecl = compileResult as AST.VariableDeclareStatement;
                        //    Console.Write($"var {vardecl.ident.token.lexeme} <- ");
                        //}

                        //if (result is null)
                        //{
                        //    Console.WriteLine("null");
                        //}
                        //else if (result.GetType() == typeof(string))
                        //{
                        //    Console.WriteLine("\"{0}\"", result);
                        //}
                        //else if (result.GetType() == typeof(char))
                        //{
                        //    Console.WriteLine("'{0}'", result);
                        //}
                        //else
                        //{
                        //    Console.WriteLine(result);
                        //}

                        //DotVisualizer dotvisitor = new DotVisualizer();
                        //result.Accept(dotvisitor);
                        //File.WriteAllText("ast.dot", dotvisitor.Output);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        //throw;
                    }
                }
            }
            else
            {
                Scope variables = new Scope();
                Interpreter interpreter = new Interpreter(variables);
                var text = File.ReadAllText(args[0]);
                //foreach (var text in File.ReadLines(args[0]))
                {
                    Console.WriteLine(text);
                    Lexer lexer = new Lexer(text);
                    Parser parser = new Parser(lexer);
                    try
                    {
                        var result = parser.Parse();

                        result.Accept(new PrettyPrinter());

                        interpreter.Execute(result);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                Console.ReadLine();
            }
        }

        private static void RunCommand(string text, Scope global)
        {
            NativeCommand cmd = NativeCommand.Parse(text);
            switch (cmd.Verb)
            {
                case "clear":
                    global.Clear();
                    break;
                default:
                    Console.WriteLine("Unknown verb: {0}", cmd.Verb);
                    break;
            }
        }
    }
}
