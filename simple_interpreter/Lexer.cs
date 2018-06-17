using System;
using System.Text;

namespace simple_interpreter
{
    class Lexer
    {
        string text;
        int pos;
        char current_char;

        public Lexer(string t)
        {
            text = t;
            pos = 0;
            current_char = text[pos];
        }

        public Lexer(Lexer other)
        {
            text = other.text;
            pos = other.pos;
            current_char = other.current_char;
        }

        public void Error()
        {
            throw new Exception("Error parsing input : " + current_char);
        }

        void Advance()
        {
            pos += 1;
            if (pos > text.Length - 1)
            {
                current_char = '\0';
            }
            else
            {
                current_char = text[pos];
            }
        }
        char Peek()
        {
            if (pos > text.Length - 1)
            {
                current_char = '\0';
            }
            return text[pos + 1];
        }

        void SkipWhitespace()
        {
            while (current_char != '\0' && char.IsWhiteSpace(current_char))
                Advance();
        }

        void SkipComment()
        {
            while (current_char != '\0' && current_char != '\n')
            {
                Advance();
            }
        }

        Token Interger()
        {
            StringBuilder result = new StringBuilder();
            while (current_char != '\0' && current_char.IsNumeric())
            {
                result.Append(current_char);
                Advance();
            }

            //check for dot for float number
            if (current_char == '.')
            {
                if (Peek().IsNumeric())
                {
                    result.Append(current_char);
                    Advance();
                    while (current_char != '\0' && current_char.IsNumeric())
                    {
                        result.Append(current_char);
                        Advance();
                    }

                    return new Token(TokenType.FLOAT, result.ToString());
                }
                else
                {
                    //invalid integer, ex: "12."
                    Error();
                    return new Token(TokenType.INTERGER, result.ToString());
                }
            }
            else
            {
                return new Token(TokenType.INTERGER, result.ToString());
            }
        }

        Token Char()
        {
            Advance();
            char result = current_char;
            TokenType tokentype = TokenType.CHAR;
            Advance();
            if (current_char != '\'')
            {
                this.Error();
            }
            Advance();
            return new Token(tokentype, (result).ToString());
        }

        Token String()
        {
            string result = "";
            Advance();
            while (current_char != '\0' && current_char != '"')
            {
                result += current_char;
                Advance();
            }
            if (current_char != '"')
            {
                Error();
            }
            Advance();
            return new Token(TokenType.STRING, result);
        }

        Token Ident()
        {
            StringBuilder temp = new StringBuilder();
            while (current_char != '\0' && current_char.IsIdent())
            {
                temp.Append(current_char);
                Advance();
            }

            var result = temp.ToString();

            switch (result)
            {
                case "true":
                case "false":
                    return new Token(TokenType.BOOL, result);

                case "and":
                    return new Token(TokenType.AND, result);
                case "or":
                    return new Token(TokenType.OR, result);
                case "xor":
                    return new Token(TokenType.XOR, result);
                case "not":
                    return new Token(TokenType.NOT, result);

                case "null":
                    return new Token(TokenType.NULL, result);

                case "if":
                    return new Token(TokenType.IF, result);
                case "while":
                    return new Token(TokenType.WHILE, result);
                case "var":
                    return new Token(TokenType.VAR, result);
                case "func":
                    return new Token(TokenType.FUNC, result);
                case "typeof":
                    return new Token(TokenType.TYPEOF, result);
                case "is":
                    return new Token(TokenType.IS, result);

                case "INT":
                case "FLT":
                case "CHR":
                case "STR":
                case "BOOL":
                case "NULL":
                    return new Token(TokenType.TYPE, result);

            }

            return new Token(TokenType.IDENT, result);
        }

        public Token GetNextToken()
        {
            while (current_char != '\0')
            {
                if (char.IsWhiteSpace(current_char))
                {
                    SkipWhitespace();
                    continue;
                }

                if (current_char == '/' && Peek() == '/')
                {
                    SkipComment();
                    continue;
                }

                if (current_char == '\'')
                {
                    return Char();
                }

                if (current_char == '"')
                {
                    return String();
                }

                if (current_char.IsNumeric())
                {
                    return Interger();
                }

                if (current_char.IsIdent())
                {
                    return Ident();
                }

                if (current_char == '!' && Peek() == '=')
                {
                    Advance();
                    Advance();
                    return new Token(TokenType.NOTEQUAL, "!=");
                }

                if (current_char == '>')
                {
                    Advance();
                    if (current_char == '=')
                    {
                        Advance();
                        return new Token(TokenType.LARGEREQUAL, ">=");
                    }
                    return new Token(TokenType.LARGER, ">");
                }

                if (current_char == '<')
                {
                    Advance();
                    if (current_char == '=')
                    {
                        Advance();
                        return new Token(TokenType.LESSEREQUAL, "<=");
                    }
                    return new Token(TokenType.LESSER, "<");
                }

                if (current_char == '=')
                {
                    Advance();
                    if (current_char == '>')
                    {
                        Advance();
                        return new Token(TokenType.LAMBDA, "=>");
                    }
                    else if (current_char == '=')
                    {
                        Advance();
                        return new Token(TokenType.EQUAL, "==");
                    }
                    return new Token(TokenType.ASSIGN, "=");
                }

                if (current_char == '+')
                {
                    Advance();
                    return new Token(TokenType.PLUS, "+");
                }

                if (current_char == '-')
                {
                    Advance();
                    return new Token(TokenType.MINUS, "-");
                }

                if (current_char == '*')
                {
                    Advance();
                    return new Token(TokenType.MULTIPLY, "*");
                }

                if (current_char == '/')
                {
                    Advance();
                    return new Token(TokenType.DIVIDE, "/");
                }

                if (current_char == '%')
                {
                    Advance();
                    return new Token(TokenType.MODULO, "%");
                }

                if (current_char == '^')
                {
                    Advance();
                    return new Token(TokenType.EXPONENT, "^");
                }

                if (current_char == '(')
                {
                    Advance();
                    return new Token(TokenType.LPAREN, "(");
                }

                if (current_char == ')')
                {
                    Advance();
                    return new Token(TokenType.RPAREN, ")");
                }

                if (current_char == '{')
                {
                    Advance();
                    return new Token(TokenType.LBRACE, "{");
                }

                if (current_char == '}')
                {
                    Advance();
                    return new Token(TokenType.RBRACE, "}");
                }

                if (current_char == '[')
                {
                    Advance();
                    return new Token(TokenType.LBRACKET, "[");
                }

                if (current_char == ']')
                {
                    Advance();
                    return new Token(TokenType.RBRACKET, "]");
                }

                if (current_char == ';')
                {
                    Advance();
                    return new Token(TokenType.SEMICOLON, ";");
                }

                Error();
            }
            return new Token(TokenType.EOF, null);
        }

        public Token PeekNextToken()
        {
            Lexer peeker = new Lexer(this);
            return peeker.GetNextToken();
        }
    }
}
