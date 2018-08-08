using System;

using XSS.AST;

namespace XSS
{
    class PrettyPrinter : IVisitor
    {
        int tablevel;
        public PrettyPrinter()
        {
            tablevel = 0;
        }

        void Tab()
        {
            for (int i = 0; i < tablevel; i++)
            {
                Console.Write("  ");
            }
        }

        public void Visit(ASTNode root)
        {
            Console.WriteLine("root");
        }

        public void Visit(BinaryOperation binop)
        {
            binop.leftnode.Accept(this);
            Console.Write($" {binop.op.lexeme} ");
            binop.rightnode.Accept(this);
            Console.WriteLine();
        }

        public void Visit(UnaryOperation unop)
        {
            Console.Write($" {unop.op.lexeme} ");
            unop.operand.Accept(this);
            Console.WriteLine();
        }

        public void Visit(Assignment ass)
        {
            Console.WriteLine($" {ass.ident.token.lexeme} <- ");
            ass.expression.Accept(this);
            Console.WriteLine();
        }

        public void Visit(Block block)
        {
            Console.WriteLine("{");
            foreach (var statement in block.Statements)
            {
                statement.Accept(this);
                Console.WriteLine();
            }
            Console.WriteLine("}");
        }

        public void Visit(Operand op)
        {
            Console.Write(op);
        }

        public void Visit(VariableDeclareStatement vardecl)
        {
            Console.Write(vardecl.Value());
        }

        public void Visit(ExpressionStatement exprstmt)
        {
            exprstmt.Expression.Accept(this);
        }

        public void Visit(IfStatement ifstmt)
        {
            Console.Write("if (");
            ifstmt.condition.Accept(this);
            Console.WriteLine(")");
            ifstmt.ifBody.Accept(this);
            if (ifstmt.elseBody != null)
            {
                Console.WriteLine("else");
                ifstmt.elseBody.Accept(this);
            }
        }

        public void Visit(WhileStatement whilestmt)
        {
            Console.Write("while (");
            whilestmt.condition.Accept(this);
            Console.WriteLine(")");
            whilestmt.body.Accept(this);
        }

        public void Visit(MatchStatement matchstmt)
        {
            Console.Write("match (");
            matchstmt.expression.Accept(this);
            Console.WriteLine(") {");
            foreach (var matchcase in matchstmt.matchCases)
            {
                Console.Write($"\t{matchcase.Type} : ");
                matchcase.Statement.Accept(this);
                Console.WriteLine();
            }
            if (matchstmt.defaultCase != null)
            {
                Console.Write("\tdefault : ");
                matchstmt.defaultCase.Accept(this);
            }
            Console.WriteLine();
            Console.WriteLine("}");
        }
    }
}
