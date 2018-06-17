namespace simple_interpreter.AST
{

    class VariableDeclareStatement : ASTNode
    {
        public Operand ident { get; private set; }
        public ASTNode init { get; private set; }

        public VariableDeclareStatement(Operand i, ASTNode init)
        {
            ident = i;
            this.init = init;
        }

        public override string Value()
        {
            return $"var  {ident.Value()}  <- {init.Value()}";
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
                hash = hash * 71 + init.GetHashCode();
                return hash;
            }
        }
    }
}
