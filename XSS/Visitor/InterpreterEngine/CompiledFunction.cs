using System;
using XSS.AST;

namespace XSS.Visitor.InterpreterEngine
{
    class CompiledFunction : IFunction
    {
        public CompiledFunction(string name, FunctionDeclaration funcDecl)
        {
            this.Name = name;
            this.FunctionDeclaration = funcDecl;
        }

        public Func<IValue[], IValue> Function { get; private set; }
        public string Name { get; private set; }

        public FunctionDeclaration FunctionDeclaration { get; private set; }

        public FunctionSignature FunctionSignature => this.FunctionDeclaration.FunctionSignature;

        public override int GetHashCode()
        {
            unchecked
            {
                return this.FunctionDeclaration.GetHashCode();
            }
        }

        public bool IsCompatibleWith(FunctionSignature funsig)
        {
            return this.FunctionSignature.GetHashCode() == funsig.GetHashCode();
        }

        public override string ToString()
        {
            return this.FunctionDeclaration.Value();
        }
    }
}
