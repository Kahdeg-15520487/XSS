using System;
using System.Linq;

namespace XSS.AST
{
    struct FunctionSignature
    {
        public FunctionSignature(ValType[] parameters, string name) : this()
        {
            Parameters = parameters;
            Name = name;
        }

        public ValType[] Parameters { get; private set; }
        public string Name { get; private set; }

        public override string ToString()
        {
            return $"({string.Join(", ", Parameters)})";
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(FunctionSignature) && obj.GetHashCode() == this.GetHashCode();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 91;
                hash = hash * 71 + Name.GetHashCode();
                foreach (var parameter in Parameters)
                {
                    hash = hash * 71 + parameter.GetHashCode();
                }
                return hash;
            }
        }
    }

    class FunctionDeclaration : ASTNode
    {
        public FunctionDeclaration(FunctionSignature functionSignature, ASTNode body)
        {
            FunctionSignature = functionSignature;
        }

        public FunctionDeclaration(string name, ASTNode body, ValType[] parameterTypes, string[] parameterNames, ValType returnType = ValType.Null)
        {
            ReturnType = returnType;
            FunctionSignature = new FunctionSignature(parameterTypes, name);
            ParameterNames = parameterNames;
            Body = body;
        }

        public FunctionSignature FunctionSignature { get; private set; }
        public ASTNode Body { get; private set; }

        public string Name { get => FunctionSignature.Name; }
        public ValType ReturnType { get; private set; }
        public ValType[] ParameterTypes { get => FunctionSignature.Parameters; }
        public string[] ParameterNames { get; private set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string Value()
        {
            return $"{Name} {FunctionSignature}->{ReturnType}";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 91;
                hash = hash * 71 + ReturnType.GetHashCode();
                hash = hash * 71 + FunctionSignature.GetHashCode();
                return hash;
            }
        }
    }
}
