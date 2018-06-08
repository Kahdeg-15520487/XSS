using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simple_interpreter
{
    class DotVisualizer : IVisitor
    {
        public string Output { get { return output.ToString() + "}"; } }
        private StringBuilder output;

        private int nodecount;
        Dictionary<ASTNode, int> nodenum;

        public DotVisualizer()
        {
            nodenum = new Dictionary<ASTNode, int>();
            nodecount = 0;
            output = new StringBuilder();
            output.AppendLine("digraph astgraph {");
            output.AppendLine("  node[shape = circle, fontsize = 12, fontname = \"Courier\", height = .1]; ");
            output.AppendLine("  ranksep = .3; edge[arrowsize = .5]");
        }

        private int GetNodeNum(ASTNode node)
        {
            if (nodenum.ContainsKey(node))
            {
                return nodenum[node];
            }
            else
            {
                nodenum.Add(node, nodecount);
                return nodenum[node];
            }
        }

        public void Visit(Operand op)
        {
            string node = string.Format("  node{0} [label=\"{1}\"];", GetNodeNum(op), op.Value());
            nodecount++;
            output.AppendLine(node);
        }

        public void Visit(BinaryOperation binop)
        {
            string rootnode = string.Format("  node{0} [label=\"{1}\"];", GetNodeNum(binop), binop.op.lexeme);
            nodecount++;

            binop.leftnode.Accept(this);
            string leftnode = string.Format("  node{0} -> node{1};", GetNodeNum(binop), GetNodeNum(binop.leftnode));

            binop.rightnode.Accept(this);
            string rightnode = string.Format("  node{0} -> node{1};", GetNodeNum(binop), GetNodeNum(binop.rightnode));

            output.AppendLine(rootnode);
            output.AppendLine(leftnode);
            output.AppendLine(rightnode);
        }

        public void Visit(ASTNode root)
        {
            
        }

        public void Visit(Assignment ass)
        {
            string rootnode = string.Format("  node{0} [label=\":=\"];", GetNodeNum(ass));
            nodecount++;

            ass.ident.Accept(this);
            string leftnode = string.Format("  node{0} -> node{1};", GetNodeNum(ass), GetNodeNum(ass.ident));

            ass.expression.Accept(this);
            string rightnode = string.Format("  node{0} -> node{1};", GetNodeNum(ass), GetNodeNum(ass.expression));

            output.AppendLine(rootnode);
            output.AppendLine(leftnode);
            output.AppendLine(rightnode);
        }
    }
}
