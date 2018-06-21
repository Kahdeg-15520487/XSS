namespace simple_interpreter.AST
{
    interface IVisitable
    {
        void Accept(IVisitor visitor);
    }

    abstract class ASTNode : IVisitable
    {
        public abstract string Value();

        public abstract void Accept(IVisitor visitor);
    }

    interface IVisitor
    {
        void Visit(BinaryOperation binop);
        void Visit(UnaryOperation unop);
        void Visit(Assignment ass);
        void Visit(VariableDeclareStatement vardecl);
        void Visit(ExpressionStatement exprstmt);
        void Visit(IfStatement ifstmt);
        void Visit(MatchStatement matchstmt);
        void Visit(WhileStatement whilestmt);
        void Visit(Block block);
        void Visit(Operand op);
    }
}
