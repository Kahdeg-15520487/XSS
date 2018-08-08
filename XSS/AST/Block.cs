using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSS.AST
{
    class Block : ASTNode
    {
        public List<ASTNode> Statements;

        public Block(List<ASTNode> statements)
        {
            Statements = statements;
        }

        public override string Value()
        {
            return Statements.Count.ToString();
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
                foreach (var stmt in Statements)
                {
                    hash = hash * 71 + stmt.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            foreach (var stmt in Statements)
            {
                sb.AppendLine(stmt.ToString());
            }
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
