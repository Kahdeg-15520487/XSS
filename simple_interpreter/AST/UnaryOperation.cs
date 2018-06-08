namespace simple_interpreter.AST
{
    class UnaryOperation : ASTNode
    {
        public Token op { get; private set; }
        public ASTNode operand { get; private set; }

        public UnaryOperation(Token op, ASTNode operand)
        {
            this.op = op;
            this.operand = operand;
        }

        public override string Value()
        {
            return op.lexeme + operand.Value();
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
                hash = hash * 71 + op.GetHashCode();
                hash = hash * 71 + operand.GetHashCode();
                return hash;
            }
        }
    }
}
