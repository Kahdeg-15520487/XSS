using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSS.AST
{
    class IfStatement : ASTNode
    {
        public ASTNode condition;
        public ASTNode ifBody;
        public ASTNode elseBody;

        public IfStatement(ASTNode condition, ASTNode ifBody, ASTNode elseBody)
        {
            this.condition = condition;
            this.ifBody = ifBody;
            this.elseBody = elseBody;
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
