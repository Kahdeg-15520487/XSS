using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using simple_interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XSS.Utility;
using XSS.Visitor;
using XSS.Visitor.InterpreterEngine;

namespace XSS
{
    class Program
    {
        static void Main(string[] args)
        {
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                return settings;
            };
            if (args.Length == 0)
            {
                bool isPrintParsed = true;
                Scope global = new Scope();
                SetupPrimitives(global);
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
                        AST.ASTNode compileResult = parser.Parse();

                        if (isPrintParsed)
                        {
                            Console.WriteLine();
                            Console.WriteLine("==pretty print==");
                            compileResult.Accept(new PrettyPrinter());
                            Console.WriteLine("==pretty print==");
                            Console.WriteLine();
                        }

                        File.AppendAllText($"{}")

                        interpreter.Execute(compileResult);

                        DotVisualizer dotvisitor = new DotVisualizer();
                        compileResult.Accept(dotvisitor);
                        File.WriteAllText("ast.dot", dotvisitor.Output);
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
                SetupPrimitives(variables);
                Interpreter interpreter = new Interpreter(variables);
                string text = File.ReadAllText(args[0]);
                //foreach (var text in File.ReadLines(args[0]))
                {
                    Console.WriteLine(text);
                    Lexer lexer = new Lexer(text);
                    Parser parser = new Parser(lexer);
                    try
                    {
                        AST.ASTNode result = parser.Parse();

                        result.Accept(new PrettyPrinter());

                        interpreter.Execute(result);

                        DotVisualizer dotvisitor = new DotVisualizer();
                        result.Accept(dotvisitor);
                        File.WriteAllText($"{args[0]}.dot", dotvisitor.Output);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                Console.ReadLine();
            }
        }

        private static void SetupPrimitives(Scope scope)
        {
            scope.Define("print", new NativeFunction("print",
                    new AST.FunctionSignature(new AST.ValType[] { }, "print")
                    , (objs) =>
                    {
                        Console.WriteLine(string.Join(" ", objs));
                        return null;
                    }));
        }

        private static void RunCommand(string text, Scope global)
        {
            try
            {
                NativeCommand cmd = NativeCommand.Parse(text);
                switch (cmd.Verb)
                {
                    case "clrscr":
                        Console.Clear();
                        break;
                    case "clear":
                        global.Clear();
                        SetupPrimitives(global);
                        break;
                    case "print":
                        if (cmd.Parameters[0] == "scope")
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(global, Formatting.Indented));
                            break;
                        }
                        Console.WriteLine(string.Join(" ", cmd.Parameters));
                        break;
                    case "run":
                        break;
                    case "json":
                        {
                            object obj = global.Get(cmd.Parameters[0]);
                            Console.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
                        }
                        break;
                    case "typeof":
                        {
                            object obj = global.Get(cmd.Parameters[0]);
                            Console.WriteLine(obj.GetType().FullName);
                        }
                        break;
                    case "log":
                        {
                            string logfile = cmd.Parameters[0];
                            string command = string.Join(" ", cmd.Parameters.Skip(1)).Insert(0, ".");
                            using (FileStream fs = new FileStream(logfile, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                using (StreamWriter sw = new StreamWriter(fs))
                                {
                                    TextWriter orgOutput = Console.Out;
                                    Console.SetOut(sw);
                                    Console.WriteLine(command);
                                    RunCommand(command, global);
                                    Console.SetOut(orgOutput);
                                }
                            }
                        }
                        break;
                    default:
                        if (string.IsNullOrWhiteSpace(cmd.Verb))
                        {
                            Console.WriteLine("No verb");
                        }
                        else
                        {
                            Console.WriteLine("Unknown verb: {0}", cmd.Verb);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
