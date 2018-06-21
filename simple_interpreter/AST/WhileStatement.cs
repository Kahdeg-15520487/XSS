using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simple_interpreter.AST
{
    class WhileStatement : ASTNode
    {
        public ASTNode condition;
        public ASTNode body;

        public WhileStatement(ASTNode condition, ASTNode body)
        {
            this.condition = condition;
            this.body = body;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string Value()
        {
            return null;
        }
    }
}
