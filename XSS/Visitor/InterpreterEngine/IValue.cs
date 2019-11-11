using XSS.AST;

namespace XSS.Visitor.InterpreterEngine
{
    interface IValue
    {
        ValType Type { get; }
        object Value { get; }
    }
}
