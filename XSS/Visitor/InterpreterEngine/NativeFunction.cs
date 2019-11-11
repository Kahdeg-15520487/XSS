using System;
using XSS.AST;

namespace XSS.Visitor.InterpreterEngine
{
    class NativeFunction : IFunction
    {
        public NativeFunction(string name, FunctionSignature funcSig, Func<object[], object> nativeFunction)
        {
            this.Name = name;
            this.FunctionSignature = funcSig;
            this.nativeFunction = nativeFunction;
        }

        public Func<IValue[], IValue> Function { get; private set; }
        public string Name { get; private set; }

        public FunctionDeclaration FunctionDeclaration => null;

        private Func<object[], object> nativeFunction;

        public FunctionSignature FunctionSignature { get; private set; }

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

        internal object Invoke(object[] parameters)
        {
            return nativeFunction.Invoke(parameters);
        }
    }
}
