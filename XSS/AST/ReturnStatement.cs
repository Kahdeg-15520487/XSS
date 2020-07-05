using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSS.AST
{
    class ReturnStatement : ASTNode
    {
        public ASTNode ReturnValue;

        public ReturnStatement(ASTNode returnValue)
        {
            ReturnValue = returnValue;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string Value()
        {
            return ReturnValue.Value();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ReturnValue.GetHashCode();
            }
        }
    }
}
