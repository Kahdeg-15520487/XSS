namespace XSS.Compiler
{
    enum TokenType
    {
        INTERGER,
        FLOAT,
        BOOL,
        CHAR,
        STRING,
        IDENT,
        NULL,

        PLUS,
        MINUS,
        MULTIPLY,
        DIVIDE,
        MODULO,
        EXPONENT,

        AND,
        OR,
        XOR,
        NOT,

        ASSIGN,

        EQUAL,
        NOTEQUAL,

        LARGER,
        LARGEREQUAL,
        LESSER,
        LESSEREQUAL,

        LAMBDA,

        TYPE,

        IF,
        MATCH,
        ELSE,
        WHILE,
        VAR,
        FUN,
        RETURN,
        TYPEOF,
        IS,

        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,
        LBRACKET,
        RBRACKET,

        COMMA,
        SEMICOLON,
        COLON,
        UNDERSCORE,
        EOF
    }

    class Token
    {
        public TokenType type { get; private set; }
        public string lexeme { get; private set; }

        public Token(TokenType t,string l = null)
        {
            type = t;
            lexeme = l;
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", type, lexeme);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 91;
                hash = hash * 71 + lexeme.GetHashCode();
                hash = hash * 71 + type.GetHashCode();
                return hash;
            }
        }
    }
}
