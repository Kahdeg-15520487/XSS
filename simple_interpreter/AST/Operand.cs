namespace simple_interpreter.AST
{

    public enum ValType
    {
        Integer,
        Float,
        Char,
        Bool,
        String,
        Identifier,
        Operator
    }

    class Operand : ASTNode
    {

        public Token token { get; private set; }
        public ValType type { get; private set; }

        public Operand(Token t)
        {
            token = t;
            switch (t.type)
            {
                case TokenType.INTERGER:
                    type = ValType.Integer;
                    break;
                case TokenType.FLOAT:
                    type = ValType.Float;
                    break;
                case TokenType.BOOL:
                    type = ValType.Bool;
                    break;
                case TokenType.CHAR:
                    type = ValType.Char;
                    break;
                case TokenType.STRING:
                    type = ValType.String;
                    break;
                case TokenType.IDENT:
                    type = ValType.Identifier;
                    break;
            }
        }

        public override string Value()
        {
            return token.lexeme;
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
                hash = hash * 71 + token.GetHashCode();
                hash = hash * 71 + type.GetHashCode();
                return hash;
            }
        }
    }
}
