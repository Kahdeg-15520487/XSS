namespace XSS.AST
{
    public enum ValType
    {
        Null,
        Integer,
        Float,
        Char,
        Bool,
        String,
        Identifier,
        Operator,
        Type,
        FunctionCall,
        Function,
        Any
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
                case TokenType.TYPE:
                    type = ValType.Type;
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

        public override string ToString()
        {
            string representation(ValType t)
            {
                switch (t)
                {
                    case ValType.Integer:
                    case ValType.Float:
                    case ValType.Bool:
                    case ValType.Identifier:
                    case ValType.Operator:
                    case ValType.Type:
                        return token.lexeme;
                    case ValType.Char:
                        return "'" + token.lexeme + "'";
                    case ValType.String:
                        return '"' + token.lexeme + '"';
                    default:
                        return "null";
                }
            }
            return "<" + representation(type) + ">";
        }
    }
}
