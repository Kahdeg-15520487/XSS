using System;
using XSS.AST;

namespace XSS.Visitor.InterpreterEngine
{

    interface IFunction
    {
        string Name { get; }
        FunctionSignature FunctionSignature { get; }
        FunctionDeclaration FunctionDeclaration { get; }
        Func<IValue[], IValue> Function { get; }
        bool IsCompatibleWith(FunctionSignature funcDecl);
    }
}
