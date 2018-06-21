using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simple_interpreter.AST
{
    class MatchStatement : ASTNode
    {
        public class MatchCase
        {
            public ValType Type;
            public ASTNode Statement;

            public MatchCase(ValType type, ASTNode statement)
            {
                Type = type;
                Statement = statement;
            }
        }
        public ASTNode expression;
        public List<MatchCase> matchCases;
        public ASTNode defaultCase;

        public MatchStatement(ASTNode expr, List<MatchCase> mtcs,ASTNode defCase)
        {
            expression = expr;
            matchCases = mtcs;
            defaultCase = defCase;
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
