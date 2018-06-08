using System;

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

        string Interger()
        {
            string result = "";
            while (current_char != '\0' && char.IsDigit(current_char))
            {
                result += current_char;
                Advance();
            }
            return result;
        }

        string Ident()
        {
            string resutlt = "";
            while (current_char != '\0' && char.IsLetterOrDigit(current_char))
            {
                resutlt += current_char;
                Advance();
            }
            return resutlt;
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

                if (current_char == ';')
                {
                    SkipComment();
                    continue;
                }

                if (char.IsDigit(current_char))
                {
                    return new Token(TokenType.INTERGER, Interger());
                }

                if (char.IsLetter(current_char))
                {
                    return new Token(TokenType.IDENT, Ident());
                }

                if (current_char == '=')
                {
                    Advance();
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
