using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simple_interpreter
{
    class Parser
    {
        Lexer lexer;

        Token current_token;

        public Parser(Lexer l)
        {
            lexer = l;
            current_token = lexer.GetNextToken();
        }

        void Error()
        {
            throw new Exception("Invalid syntax: " + current_token);
        }

        void Eat(TokenType t)
        {
            if (current_token.type == t)
            {
                current_token = lexer.GetNextToken();
            }
            else
            {
                Error();
            }
        }

        ASTNode Factor()
        {
            /*
             * factor : INTERGER | IDENT | LPAREN EXPR RPAREN
             */

            var token = current_token;

            switch (token.type)
            {
                case TokenType.INTERGER:
                    Eat(TokenType.INTERGER);
                    return new Operand(token);
                case TokenType.IDENT:
                    Eat(TokenType.IDENT);
                    return new Operand(token);
                case TokenType.LPAREN:
                    Eat(TokenType.LPAREN);
                    var node = PrecendenceLevel3();
                    Eat(TokenType.RPAREN);
                    return node;
            }

            Error();
            return null;
        }

        ASTNode PrecendenceLevel1()
        {
            /*
             * precendencelevel1 : factor (EXP factor) *
             */

            var node = Factor();

            while (current_token.type == TokenType.EXPONENT)
            {
                var token = current_token;
                switch (token.type)
                {
                    case TokenType.EXPONENT:
                        Eat(TokenType.EXPONENT);
                        break;
                }

                node = new BinaryOperation(node, token, Factor());
            }

            return node;
        }

        ASTNode PrecendenceLevel2()
        {
            /*
             * precendencelevel2 : precendencelevel1 ((MUL | DIV) precendencelevel1) *
             */

            var node = PrecendenceLevel1();

            while (current_token.type == TokenType.MULTIPLY ||
                    current_token.type == TokenType.DIVIDE)
            {
                var token = current_token;
                switch (token.type)
                {
                    case TokenType.MULTIPLY:
                        Eat(TokenType.MULTIPLY);
                        break;
                    case TokenType.DIVIDE:
                        Eat(TokenType.DIVIDE);
                        break;
                }

                node = new BinaryOperation(node, token, Factor());
            }

            return node;
        }

        ASTNode PrecendenceLevel3()
        {
            /*
             * precendencelevel3 : precendencelevel2 ((PLUS | MINUS) precendencelevel2) *
             */

            var node = PrecendenceLevel2();

            while (current_token.type == TokenType.PLUS ||
                    current_token.type == TokenType.MINUS)
            {
                var token = current_token;
                switch (token.type)
                {
                    case TokenType.PLUS:
                        Eat(TokenType.PLUS);
                        break;
                    case TokenType.MINUS:
                        Eat(TokenType.MINUS);
                        break;
                }
                node = new BinaryOperation(node, token, PrecendenceLevel2());
            }

            return node;
        }

        ASTNode Assignment()
        {
            /*
             * assignment : IDENT ASSIGN ( precendencelevel3 | assignment )
             */

            var token = current_token;
            Eat(TokenType.IDENT);
            var ident = new Operand(token);
            Eat(TokenType.ASSIGN);
            var expr = PrecendenceLevel3();
            return new Assignment(ident, expr);
        }

        ASTNode Expression()
        {
            /*
             * expression : precendencelevel3 | assignment
             */
             
            if (lexer.PeekNextToken().type == TokenType.ASSIGN)
            {
                return Assignment();
            }
            else
            {
                return PrecendenceLevel3();
            }
        }

        public ASTNode Parse()
        {
            return Expression();
        }
    }
}
