namespace XSS.AST
{
    class BinaryOperation : ASTNode
    {
        public ASTNode leftnode { get; private set; }
        public Token op { get; private set; }
        public ASTNode rightnode { get; private set; }

        public BinaryOperation(ASTNode left, Token op, ASTNode right)
        {
            leftnode = left;
            this.op = op;
            rightnode = right;
        }

        public override string Value()
        {
            return leftnode.Value() + op.lexeme + rightnode.Value();
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
                hash = hash * 71 + leftnode.GetHashCode();
                hash = hash * 71 + op.GetHashCode();
                hash = hash * 71 + rightnode.GetHashCode();
                return hash;
            }
        }
    }
}
