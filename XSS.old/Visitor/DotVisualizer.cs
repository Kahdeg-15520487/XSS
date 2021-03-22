using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSS.AST;

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

        private int GetNodeNum()
        {
            return ++nodecount;
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

        public void Visit(UnaryOperation unop)
        {
            string rootnode = string.Format("  node{0} [label=\"{1}\"];", GetNodeNum(unop), unop.op.lexeme);
            nodecount++;

            unop.operand.Accept(this);
            string leftnode = string.Format("  node{0} -> node{1};", GetNodeNum(unop), GetNodeNum(unop.operand));

            output.AppendLine(rootnode);
            output.AppendLine(leftnode);
        }

        public void Visit(VariableDeclareStatement vardecl)
        {
            string rootnode = string.Format("  node{0} [label=\"define: \"];", GetNodeNum(vardecl));
            nodecount++;

            vardecl.ident.Accept(this);
            string leftnode = string.Format("  node{0} -> node{1};", GetNodeNum(vardecl), GetNodeNum(vardecl.ident));
            string rightnode = string.Empty;
            if (vardecl.init != null)
            {
                vardecl.init.Accept(this);
                rightnode = string.Format("  node{0} -> node{1};", GetNodeNum(vardecl), GetNodeNum(vardecl.init));
            }

            output.AppendLine(rootnode);
            output.AppendLine(leftnode);
            output.AppendLine(rightnode);
        }

        public void Visit(ExpressionStatement exprstmt)
        {
            string node = string.Format("  node{0} [label=\"expr: \"];", GetNodeNum(exprstmt));
            nodecount++;
            exprstmt.Expression.Accept(this);
            string expr = string.Format("  node{0} -> node{1};", GetNodeNum(exprstmt), GetNodeNum(exprstmt.Expression));
            output.AppendLine(node);
            output.AppendLine(expr);
        }

        public void Visit(IfStatement ifstmt)
        {
            string rootnode = string.Format("  node{0} [label=\"if: \"];", GetNodeNum(ifstmt));
            nodecount++;

            ifstmt.condition.Accept(this);
            string condition = string.Format("  node{0} -> node{1};", GetNodeNum(ifstmt), GetNodeNum(ifstmt.condition));
            ifstmt.ifBody.Accept(this);
            string ifbody = string.Format("  node{0} -> node{1};", GetNodeNum(ifstmt.condition), GetNodeNum(ifstmt.ifBody));
            string elsebody = string.Empty;
            if (ifstmt.elseBody != null)
            {
                ifstmt.elseBody.Accept(this);
                elsebody = string.Format("  node{0} -> node{1};", GetNodeNum(ifstmt.condition), GetNodeNum(ifstmt.elseBody));
            }

            output.AppendLine(rootnode);
            output.AppendLine(condition);
            output.AppendLine(ifbody);
            output.AppendLine(elsebody);
        }

        public void Visit(MatchStatement matchstmt)
        {
            string rootnode = string.Format("  node{0} [label=\"match: \"];", GetNodeNum(matchstmt));
            nodecount++;

            matchstmt.expression.Accept(this);
            string expression = string.Format("  node{0} -> node{1};", GetNodeNum(matchstmt), GetNodeNum(matchstmt.expression));

            output.AppendLine(rootnode);
            output.AppendLine(expression);

            foreach (MatchStatement.MatchCase matchCase in matchstmt.matchCases)
            {
                int typeNodeNum = this.GetNodeNum();
                string type = string.Format(" node{0} -> node{1} [label=\"on {2}: \"];", GetNodeNum(matchstmt.expression), typeNodeNum, matchCase.Type);
                matchCase.Statement.Accept(this);
                string stmt = string.Format(" node{0} -> node{1};", typeNodeNum, GetNodeNum(matchCase.Statement));
                output.AppendLine(type);
                output.AppendLine(stmt);
            }

            if (matchstmt.defaultCase != null)
            {
                matchstmt.defaultCase.Accept(this);
                string defaultcase = string.Format(" node{0} -> node{1} [label=\"default: \"];", GetNodeNum(matchstmt), GetNodeNum(matchstmt.defaultCase));
                output.AppendLine(defaultcase);
            }
        }

        public void Visit(WhileStatement whilestmt)
        {
            string rootnode = string.Format("  node{0} [label=\"while: \"];", GetNodeNum(whilestmt));
            nodecount++;

            whilestmt.condition.Accept(this);
            string condition = string.Format("  node{0} -> node{1};", GetNodeNum(whilestmt), GetNodeNum(whilestmt.condition));
            whilestmt.body.Accept(this);
            string body = string.Format("  node{0} -> node{1};", GetNodeNum(whilestmt.condition), GetNodeNum(whilestmt.body));

            output.AppendLine(rootnode);
            output.AppendLine(condition);
            output.AppendLine(body);
        }

        public void Visit(FunctionDeclaration function)
        {
            string rootnode = string.Format("  node{0} [label=\"function: \"];", GetNodeNum(function));
            nodecount++;

            string funcSig = string.Format("  node{0} -> node{1} [label=\"{2} {3})\"];", GetNodeNum(function), GetNodeNum(), function.FunctionSignature.Name, function.FunctionSignature.ToString());
            function.Body.Accept(this);
            string body = string.Format("  node{0} -> node{1};", GetNodeNum(function), GetNodeNum(function.Body));
            output.AppendLine(rootnode);
            output.AppendLine(funcSig);
            output.AppendLine(body);
        }

        public void Visit(FunctionCall functionCall)
        {
            string rootnode = string.Format("  node{0} [label=\"func call: \"];", GetNodeNum(functionCall));
            nodecount++;

            string funcname = string.Format("  node{0} -> node{1} [label=\"{2}\"];", GetNodeNum(functionCall), GetNodeNum(), functionCall.FunctionName);

            output.AppendLine(rootnode);
            output.AppendLine(funcname);
            foreach (ASTNode parameter in functionCall.Parameters)
            {
                parameter.Accept(this);
                string param = string.Format("  node{0} -> node{1};", GetNodeNum(functionCall), this.GetNodeNum(parameter));
                output.AppendLine(param);
            }
        }

        public void Visit(ReturnStatement retstmt)
        {
            string rootnode = string.Format("  node{0} [label=\"return \"];", GetNodeNum(retstmt));
            nodecount++;
            retstmt.ReturnValue.Accept(this);
            string retvalue = string.Format("  node{0} -> node{1};", GetNodeNum(retstmt), GetNodeNum(retstmt.ReturnValue));
            output.AppendLine(rootnode);
            output.AppendLine(retvalue);
        }

        public void Visit(Block block)
        {
            string rootnode = string.Format("  node{0} [label=\"block \"];", GetNodeNum(block));
            nodecount++;
            output.AppendLine(rootnode);
            foreach (ASTNode statement in block.Statements)
            {
                statement.Accept(this);
                string stmt = string.Format("  node{0} -> node{1};", GetNodeNum(block), GetNodeNum(statement));
                output.AppendLine(stmt);
            }
        }
    }
}
