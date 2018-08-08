namespace XSS.AST
{

    class Assignment : ASTNode
    {
        public Operand ident { get; private set; }
        public ASTNode expression { get; private set; }

        public Assignment(Operand i, ASTNode expr)
        {
            ident = i;
            expression = expr;
        }

        public override string Value()
        {
            return ident.Value() + " <- " + expression.Value();
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
                hash = hash * 71 + ident.GetHashCode();
                hash = hash * 71 + expression.GetHashCode();
                return hash;
            }
        }
    }
}
