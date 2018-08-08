using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSS.AST
{
    class ExpressionStatement : ASTNode
    {
        public ASTNode Expression;

        public ExpressionStatement(ASTNode expression)
        {
            Expression = expression;
        }

        public override string Value()
        {
            return null;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
