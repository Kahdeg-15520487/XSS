using System;

using simple_interpreter.AST;

namespace simple_interpreter
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
            Console.WriteLine();
        }

        public void Visit(Operand op)
        {
            Console.Write(op);
        }

        public void Visit(VariableDeclareStatement vardecl)
        {
            Console.Write(vardecl);
        }

        public void Visit(ExpressionStatement exprstmt)
        {
            exprstmt.Expression.Accept(this);
        }

        public void Visit(IfStatement ifstmt)
        {
            throw new NotImplementedException();
        }

        public void Visit(WhileStatement whilestmt)
        {
            throw new NotImplementedException();
        }
    }
}
