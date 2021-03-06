﻿using XSS.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSS
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

        void Error(string msg)
        {
            throw new Exception($"{msg} at ({lexer.CurrentLine}:{lexer.CurrentPosInLine}) : {lexer.CurrentLineSource} ");
        }

        void Error()
        {
            Error($"Invalid Token: {current_token}");
        }

        void Error(TokenType expecting)
        {
            Error($"Expecting: {expecting}");
        }

        void Eat(TokenType t)
        {
            if (current_token.type == t)
            {
                current_token = lexer.GetNextToken();
            }
            else
            {
                Error(t);
            }
        }

        ASTNode Factor()
        {
            /*
             * factor : INTERGER | FLOAT | BOOL| CHAR | STRING | IDENT | TYPE | LPAREN expression RPAREN
             */

            var token = current_token;

            switch (token.type)
            {
                case TokenType.INTERGER:
                    Eat(TokenType.INTERGER);
                    return new Operand(token);

                case TokenType.FLOAT:
                    Eat(TokenType.FLOAT);
                    return new Operand(token);

                case TokenType.BOOL:
                    Eat(TokenType.BOOL);
                    return new Operand(token);

                case TokenType.CHAR:
                    Eat(TokenType.CHAR);
                    return new Operand(token);

                case TokenType.STRING:
                    Eat(TokenType.STRING);
                    return new Operand(token);

                case TokenType.IDENT:
                    Eat(TokenType.IDENT);
                    return new Operand(token);

                case TokenType.LPAREN:
                    Eat(TokenType.LPAREN);
                    var node = Expression();
                    Eat(TokenType.RPAREN);
                    return node;

                case TokenType.TYPE:
                    Eat(TokenType.TYPE);
                    return new Operand(token);
            }

            Error();
            return null;
        }

        ASTNode Unary()
        {
            /*
             * unary : (MINUS (INTERGER | FLOAT | IDENT)) | (NOT BOOL | IDENT) | TYPEOF factor | factor
             */

            var token = current_token;
            Token op;

            switch (token.type)
            {
                case TokenType.MINUS:
                    Eat(TokenType.MINUS);
                    op = current_token;
                    if (op.type == TokenType.INTERGER)
                    {
                        Eat(TokenType.INTERGER);
                    }
                    else if (op.type == TokenType.FLOAT)
                    {
                        Eat(TokenType.FLOAT);
                    }
                    else if (op.type == TokenType.IDENT)
                    {
                        Eat(TokenType.IDENT);
                    }
                    else
                    {
                        Error();
                    }
                    return new UnaryOperation(token, new Operand(op));

                case TokenType.NOT:
                    Eat(TokenType.NOT);
                    op = current_token;
                    if (op.type == TokenType.BOOL)
                    {
                        Eat(TokenType.BOOL);
                    }
                    else if (op.type == TokenType.IDENT)
                    {
                        Eat(TokenType.IDENT);
                    }
                    else
                    {
                        Error();
                    }
                    return new UnaryOperation(token, new Operand(op));

                case TokenType.TYPEOF:
                    Eat(TokenType.TYPEOF);
                    return new UnaryOperation(token, Factor());

                case TokenType.INTERGER:
                case TokenType.FLOAT:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.STRING:
                case TokenType.IDENT:
                case TokenType.LPAREN:
                case TokenType.TYPE:
                    return Factor();
            }

            Error();
            return null;
        }

        ASTNode Exponent()
        {
            /*
             * exponent : factor (EXP factor)? *
             */

            var node = Unary();

            while (current_token.type == TokenType.EXPONENT)
            {
                var token = current_token;
                Eat(token.type);

                node = new BinaryOperation(node, token, Unary());
            }

            return node;
        }

        ASTNode Multiplication()
        {
            /*
             * multiplication : exponent ((MUL | DIV) exponent)? *
             */

            var node = Exponent();

            while (current_token.type == TokenType.MULTIPLY
                || current_token.type == TokenType.DIVIDE
                || current_token.type == TokenType.MODULO)
            {
                var token = current_token;
                Eat(token.type);

                node = new BinaryOperation(node, token, Unary());
            }

            return node;
        }

        ASTNode Addition()
        {
            /*
             * addition : multiplication ((PLUS | MINUS) multiplication)? *
             */

            var node = Multiplication();

            while (current_token.type == TokenType.PLUS ||
                    current_token.type == TokenType.MINUS)
            {
                var token = current_token;
                Eat(token.type);
                node = new BinaryOperation(node, token, Multiplication());
            }

            return node;
        }

        ASTNode Comparison()
        {
            /*
             * comparison : addition ( ( ">" | ">=" | "<" | "<=" ) addition )*
             */

            var node = Addition();

            while (current_token.type == TokenType.LARGER
                || current_token.type == TokenType.LARGEREQUAL
                || current_token.type == TokenType.LESSER
                || current_token.type == TokenType.LESSEREQUAL)
            {
                var token = current_token;
                Eat(token.type);
                node = new BinaryOperation(node, token, Addition());
            }

            return node;
        }

        ASTNode Equality()
        {
            /*
             * equality : comparison ( ( "!=" | "==" ) comparison )*
             */

            var node = Comparison();

            while (current_token.type == TokenType.NOTEQUAL ||
                    current_token.type == TokenType.EQUAL)
            {
                var token = current_token;
                Eat(token.type);
                node = new BinaryOperation(node, token, Comparison());
            }

            return node;
        }

        ASTNode LogicXor()
        {
            /*
             * xor : equality ( XOR equality )*
             */

            var node = Equality();

            while (current_token.type == TokenType.XOR)
            {
                var token = current_token;
                Eat(token.type);
                node = new BinaryOperation(node, token, Equality());
            }

            return node;
        }

        ASTNode LogicAnd()
        {
            /*
             * and : xor ( AND xor )*
             */

            var node = LogicXor();

            while (current_token.type == TokenType.AND)
            {
                var token = current_token;
                Eat(token.type);
                node = new BinaryOperation(node, token, LogicXor());
            }

            return node;
        }

        ASTNode LogicOr()
        {
            /*
             * or : and ( OR and )*
             */

            var node = LogicAnd();

            while (current_token.type == TokenType.OR)
            {
                var token = current_token;
                Eat(token.type);
                node = new BinaryOperation(node, token, LogicAnd());
            }

            return node;
        }

        ASTNode TypeIdentify()
        {
            /*
             * type_identify : or (IS TYPE )? SEMICOLON
             */
            var node = LogicOr();

            var token = current_token;
            if (token.type == TokenType.IS)
            {
                Eat(TokenType.IS);
                node = new BinaryOperation(node, token, LogicOr());
            }
            return node;
        }

        ASTNode Assignment()
        {
            /*
             * assignment : IDENT ASSIGN expression SEMICOLON
             */

            var token = current_token;
            Eat(TokenType.IDENT);
            var ident = new Operand(token);
            Eat(TokenType.ASSIGN);
            var expr = Expression();
            return new Assignment(ident, expr);
        }

        ASTNode Expression()
        {
            /*
             * expression : type_identify | assignment
             */

            var nextToken = lexer.PeekNextToken();
            switch (nextToken.type)
            {
                case TokenType.ASSIGN:
                    return Assignment();
                default:
                    return TypeIdentify();
            }

        }

        ASTNode ExpressionStatement()
        {
            /*
             * expressionStatement : expression SEMICOLON
             */

            var expr = Expression();
            Eat(TokenType.SEMICOLON);
            return new ExpressionStatement(expr);
        }

        ASTNode VariableDeclare()
        {
            /*
             * vardecl : VAR IDENT ( ASSIGN expression )? SEMICOLON
             */

            Eat(TokenType.VAR);
            var token = current_token;
            Eat(TokenType.IDENT);
            var ident = new Operand(token);
            ASTNode init = null;
            if (current_token.type == TokenType.ASSIGN)
            {
                Eat(TokenType.ASSIGN);
                init = Expression();
            }

            Eat(TokenType.SEMICOLON);
            return new VariableDeclareStatement(ident, init);
        }

        ASTNode Block()
        {
            /*
             * block : LBRACE statement* RBRACE
             */
            List<ASTNode> statements = new List<ASTNode>();
            Eat(TokenType.LBRACE);

            while (current_token.type != TokenType.RBRACE)
            {
                statements.Add(Statement());
            }

            Eat(TokenType.RBRACE);
            return new Block(statements);
        }

        ASTNode IfStatement()
        {
            /*
             * ifstatement : IF LPAREN expression RPAREN statement ( ELSE statement )?
             */
            Eat(TokenType.IF);
            Eat(TokenType.LPAREN);
            var condition = Expression();
            Eat(TokenType.RPAREN);
            var ifBody = Statement();

            if (current_token.type == TokenType.ELSE)
            {
                Eat(TokenType.ELSE);
                var elseBody = Statement();
                return new IfStatement(condition, ifBody, elseBody);
            }

            return new IfStatement(condition, ifBody, null);
        }

        ASTNode WhileStatement()
        {
            /*
             * whilestatement : WHILE LPAREN expression RPAREN statement
             */
            Eat(TokenType.WHILE);
            Eat(TokenType.LPAREN);
            var condition = Expression();
            Eat(TokenType.RPAREN);
            var body = Statement();

            return new WhileStatement(condition, body);
        }

        ASTNode MatchStatement()
        {
            /*
             * matchstatement : MATCH LPAREN expression RPAREN LBRACE ( (TYPE COLON statement)*? | UNDERSCORE COLON statement? ) RBRACE
             */

            Eat(TokenType.MATCH);
            Eat(TokenType.LPAREN);
            var expr = Expression();
            Eat(TokenType.RPAREN);
            Eat(TokenType.LBRACE);
            List<MatchStatement.MatchCase> matchCases = new List<MatchStatement.MatchCase>();
            ASTNode defaultCase = null;
            while (current_token.type != TokenType.RBRACE && current_token.type != TokenType.EOF)
            {
                Token token = current_token;
                if (token.type == TokenType.UNDERSCORE)
                {
                    //default case
                    if (defaultCase is null)
                    {
                        Eat(TokenType.UNDERSCORE);
                        Eat(TokenType.COLON);
                        defaultCase = Statement();
                        continue;
                    }
                    else
                    {
                        Error("More than one default case");
                    }
                }
                Eat(TokenType.TYPE);
                ValType type = token.lexeme.ToValType();
                Eat(TokenType.COLON);
                var stmt = Statement();
                matchCases.Add(new MatchStatement.MatchCase(type, stmt));
            }
            Eat(TokenType.RBRACE);

            return new MatchStatement(expr, matchCases, defaultCase);
        }

        ASTNode Statement()
        {
            /*
             * statement : expressionStatement | variableDeclareStatement | ifstatement | block
             */

            switch (current_token.type)
            {
                case TokenType.LBRACE:
                    return Block();
                case TokenType.VAR:
                    return VariableDeclare();
                case TokenType.IF:
                    return IfStatement();
                case TokenType.WHILE:
                    return WhileStatement();
                case TokenType.MATCH:
                    return MatchStatement();
                default:
                    return ExpressionStatement();
            }
        }

        public ASTNode Parse()
        {
            var stmts = new List<ASTNode>();
            while (lexer.PeekNextToken().type != TokenType.EOF)
            {
                stmts.Add(Statement());
            }

            return new Block(stmts);
        }
    }
}
