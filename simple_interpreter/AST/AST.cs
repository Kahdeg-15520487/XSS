using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simple_interpreter
{
    interface IVisitable
    {
        void Accept(IVisitor visitor);
    }

    abstract class ASTNode : IVisitable
    {
        public abstract string Value();

        public virtual void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    class BinaryOperation : ASTNode
    {
        public ASTNode leftnode { get; private set; }
        public Token op { get; private set; }
        public ASTNode rightnode { get; private set; }

        public BinaryOperation(ASTNode left,Token op,ASTNode right)
        {
            leftnode = left;
            this.op = op;
            rightnode = right;
        }

        public override string Value()
        {
            return leftnode.Value() + op.lexeme + rightnode.Value();
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 91;
                hash = hash * 71 + leftnode.GetHashCode();
                hash = hash * 71 + op.GetHashCode();
                hash = hash * 71 + rightnode.GetHashCode();
                return hash;
            }
        }
    }

    class Assignment : ASTNode
    {
        public Operand ident { get; private set; }
        public ASTNode expression { get; private set; }

        public Assignment(Operand i,ASTNode expr)
        {
            ident = i;
            expression = expr;
        }

        public override string Value()
        {
            return ident.Value() + ":=" + expression.Value();
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 91;
                hash = hash * 71 + ident.GetHashCode();
                hash = hash * 71 + expression.GetHashCode();
                return hash;
            }
        }
    }

    class Operand : ASTNode
    {
        public enum ValType
        {
            Number,
            Identifier
        }

        public Token token { get; private set;}
        public ValType type { get; private set; }

        public Operand(Token t)
        {
            token = t;
            switch (t.type)
            {
                case TokenType.INTERGER:
                    type = ValType.Number;
                    break;
                case TokenType.IDENT:
                    type = ValType.Identifier;
                    break;
            }
        }

        public override string Value()
        {
            return token.lexeme;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 91;
                hash = hash * 71 + token.GetHashCode();
                hash = hash * 71 + type.GetHashCode();
                return hash;
            }
        }
    }
}
