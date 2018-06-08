namespace simple_interpreter.AST
{
    interface IVisitable
    {
        void Accept(IVisitor visitor);
    }

    abstract class ASTNode : IVisitable
    {
        public abstract string Value();

        public virtual void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
