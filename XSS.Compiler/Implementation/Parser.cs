using System;
using System.Collections.Generic;

using XSS.Compiler;
using XSS.Compiler.Contract;

namespace XSS
{
    class Parser : IParser
    {
        ILexer lexer;

        Token current_token;
        private bool isInFunction;

        public Parser(ILexer l)
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

        #region Operand

        void Bool(Token value)
        {

        }

        void Int(Token value)
        {

        }

        void Float(Token value)
        {

        }

        void String(Token value)
        {

        }

        void Char(Token value)
        {

        }

        void Null()
        {

        }

        void Ident(Token value)
        {

        }

        void FunctionCall(Token value)
        {

        }

        void Type(Token value)
        {

        }

        #endregion

        void FunctionCall()
        {
            /*
             * functionCall : IDENT LPAREN (expression)*? RPAREN
             */

            var token = current_token;
            Eat(TokenType.IDENT);
            var fn = token.lexeme;
            Eat(TokenType.LPAREN);
            //var fps = new List<ASTNode>();
            while (current_token.type != TokenType.RPAREN)
            {
                if (current_token.type == TokenType.EOF)
                {
                    Error(TokenType.RPAREN);
                }

                //fps.Add(Expression());
                Expression();

                if (current_token.type == TokenType.COMMA)
                {
                    Eat(TokenType.COMMA);
                }
            }
            Eat(TokenType.RPAREN);

            //return new FunctionCall(fn, fps);
        }

        void Factor()
        {
            /*
             * factor : INTERGER | FLOAT | BOOL| CHAR | STRING | NULL | IDENT | funcitonCall | TYPE | LPAREN expression RPAREN
             */

            var token = current_token;

            switch (token.type)
            {
                case TokenType.INTERGER:
                    Eat(TokenType.INTERGER);
                    Int(token);
                    break;

                case TokenType.FLOAT:
                    Eat(TokenType.FLOAT);
                    Float(token);
                    break;

                case TokenType.BOOL:
                    Eat(TokenType.BOOL);
                    Bool(token);
                    break;

                case TokenType.CHAR:
                    Eat(TokenType.CHAR);
                    Char(token);
                    break;

                case TokenType.STRING:
                    Eat(TokenType.STRING);
                    String(token);
                    break;

                case TokenType.NULL:
                    Eat(TokenType.NULL);
                    Null();
                    break;

                case TokenType.IDENT:
                    if (lexer.PeekNextToken().type == TokenType.LPAREN)
                    {
                        //this is a function call
                        FunctionCall();
                        break;
                    }
                    Eat(TokenType.IDENT);
                    Ident(token);
                    break;

                case TokenType.LPAREN:
                    Eat(TokenType.LPAREN);
                    Expression();
                    Eat(TokenType.RPAREN);
                    break;

                case TokenType.TYPE:
                    Eat(TokenType.TYPE);
                    Type(token);
                    break;
            }

            Error();
        }

        void Unary()
        {
            /*
             * unary : (MINUS (INTERGER | FLOAT | IDENT)) | (NOT BOOL | IDENT) | TYPEOF factor | factor
             */

            Token token = current_token;
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
                        Error("Expecting numeral");
                    }
                    //emit bytecode
                    break;

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
                        Error("Expecting boolean");
                    }
                    //emit bytecode
                    break;

                case TokenType.TYPEOF:
                    Eat(TokenType.TYPEOF);
                    Factor();
                    //emit bytecode get type of 
                    break;

                case TokenType.INTERGER:
                case TokenType.FLOAT:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.STRING:
                case TokenType.NULL:
                case TokenType.IDENT:
                case TokenType.LPAREN:
                case TokenType.TYPE:
                    Factor();
                    break;
            }

            Error("Expecting unary operator");
        }

        void Exponent()
        {
            /*
             * exponent : factor (EXP factor)*?
             */

            Unary();

            while (current_token.type == TokenType.EXPONENT)
            {
                var token = current_token;
                Eat(token.type);

                //node = new BinaryOperation(node, token, Unary());
                //emit bytecode exponent
            }

            //return node;
        }

        void Multiplication()
        {
            /*
             * multiplication : exponent ((MUL | DIV) exponent)*?
             */

            Exponent();

            while (current_token.type == TokenType.MULTIPLY
                || current_token.type == TokenType.DIVIDE
                || current_token.type == TokenType.MODULO)
            {
                var token = current_token;
                Eat(token.type);

                //emit bytecode multiply
            }
        }

        void Addition()
        {
            /*
             * addition : multiplication ((PLUS | MINUS) multiplication)*?
             */

            Multiplication();

            while (current_token.type == TokenType.PLUS ||
                    current_token.type == TokenType.MINUS)
            {
                var token = current_token;
                Eat(token.type);

                //emit bytecode addition 
            }
        }

        void Comparison()
        {
            /*
             * comparison : addition ( ( ">" | ">=" | "<" | "<=" ) addition )*?
             */

            Addition();

            while (current_token.type == TokenType.LARGER
                || current_token.type == TokenType.LARGEREQUAL
                || current_token.type == TokenType.LESSER
                || current_token.type == TokenType.LESSEREQUAL)
            {
                var token = current_token;
                Eat(token.type);

                //emit bytecode compare
            }
        }

        void Equality()
        {
            /*
             * equality : comparison ( ( "!=" | "==" ) comparison )*?
             */

            Comparison();

            while (current_token.type == TokenType.NOTEQUAL ||
                    current_token.type == TokenType.EQUAL)
            {
                var token = current_token;
                Eat(token.type);

                //emit bytecode equal
            }
        }

        void LogicXor()
        {
            /*
             * xor : equality ( XOR equality )*?
             */

            Equality();

            while (current_token.type == TokenType.XOR)
            {
                var token = current_token;
                Eat(token.type);

                //emit bytecode xor
            }
        }

        void LogicAnd()
        {
            /*
             * and : xor ( AND xor )*?
             */

            LogicXor();

            while (current_token.type == TokenType.AND)
            {
                var token = current_token;
                Eat(token.type);

                //emit bytecode and
            }
        }

        void LogicOr()
        {
            /*
             * or : and ( OR and )*?
             */

            LogicAnd();

            while (current_token.type == TokenType.OR)
            {
                var token = current_token;
                Eat(token.type);

                //emit bytecode and
            }
        }

        void TypeIdentify()
        {
            /*
             * type_identify : or (IS TYPE )? SEMICOLON
             */
            LogicOr();

            var token = current_token;
            if (token.type == TokenType.IS)
            {
                Eat(TokenType.IS);

                //emit bytecode or
            }
        }

        void Assignment()
        {
            /*
             * assignment : IDENT ASSIGN expression SEMICOLON
             */

            var token = current_token;
            Eat(TokenType.IDENT);
            Ident(token);
            Eat(TokenType.ASSIGN);
            Expression();
            //emit bytecode assignment
        }

        void Expression()
        {
            /*
             * expression : type_identify | assignment
             */

            var nextToken = lexer.PeekNextToken();
            switch (nextToken.type)
            {
                case TokenType.ASSIGN:
                    Assignment();
                    break;
                default:
                    TypeIdentify();
                    break;
            }

        }

        void ExpressionStatement()
        {
            /*
             * expressionStatement : expression SEMICOLON
             */

            Expression();
            Eat(TokenType.SEMICOLON);
            //emit bytecode to clean the stack?
        }

        void VariableDeclare()
        {
            /*
             * varDeclarationStatement : VAR IDENT ( ASSIGN expression )? SEMICOLON
             */

            Eat(TokenType.VAR);
            var token = current_token;
            Eat(TokenType.IDENT);
            Ident(token);

            if (current_token.type == TokenType.ASSIGN)
            {
                Eat(TokenType.ASSIGN);
                Expression();
                //emit bytecode assign
            }

            Eat(TokenType.SEMICOLON);
        }

        void Block()
        {
            /*
             * block : LBRACE statement*? RBRACE
             */
            Eat(TokenType.LBRACE);

            while (current_token.type != TokenType.RBRACE)
            {
                Statement();
            }

            Eat(TokenType.RBRACE);
        }

        void IfStatement()
        {
            /*
             * ifstatement : IF LPAREN expression RPAREN statement ( ELSE statement )?
             */
            Eat(TokenType.IF);
            Eat(TokenType.LPAREN);
            //emit bytecode for condition
            Expression();
            Eat(TokenType.RPAREN);
            var ifBody = Statement();
            //todo emit jump

            if (current_token.type == TokenType.ELSE)
            {
                Eat(TokenType.ELSE);
                var elseBody = Statement();
                IfStatement();
            }

            IfStatement();
        }

        void WhileStatement()
        {
            /*
             * whilestatement : WHILE LPAREN expression RPAREN statement
             */
            Eat(TokenType.WHILE);
            Eat(TokenType.LPAREN);
            //emit bytecode for condition
            Expression();
            Eat(TokenType.RPAREN);
            var body = Statement();

            WhileStatement();
        }

        //void MatchStatement()
        //{
        //    /*
        //     * matchstatement : MATCH LPAREN expression RPAREN LBRACE ( (TYPE COLON statement)*? | UNDERSCORE COLON statement? ) RBRACE
        //     */

        //    Eat(TokenType.MATCH);
        //    Eat(TokenType.LPAREN);
        //    Expression();
        //    Eat(TokenType.RPAREN);
        //    Eat(TokenType.LBRACE);
        //    List<MatchStatement.MatchCase> matchCases = new List<MatchStatement.MatchCase>();
        //    ASTNode defaultCase = null;
        //    while (current_token.type != TokenType.RBRACE && current_token.type != TokenType.EOF)
        //    {
        //        Token token = current_token;
        //        if (token.type == TokenType.UNDERSCORE)
        //        {
        //            //default case
        //            if (defaultCase is null)
        //            {
        //                Eat(TokenType.UNDERSCORE);
        //                Eat(TokenType.COLON);
        //                defaultCase = Statement();
        //                continue;
        //            }
        //            else
        //            {
        //                Error("More than one default case");
        //            }
        //        }
        //        Eat(TokenType.TYPE);
        //        ValType type = token.lexeme.ToValType();
        //        Eat(TokenType.COLON);
        //        var stmt = Statement();
        //        matchCases.Add(new MatchStatement.MatchCase(type, stmt));
        //    }
        //    Eat(TokenType.RBRACE);

        //    return new MatchStatement(expr, matchCases, defaultCase);
        //}

        private void FunctionDeclare()
        {
            /*
             * functionDeclareStatement : FUN IDENT LPAREN (TYPE IDENt)*? RPAREN COLON TYPE block
             */

            isInFunction = true;
            Eat(TokenType.FUN);
            var token = current_token;
            Eat(TokenType.IDENT);
            var name = token.lexeme;
            Eat(TokenType.LPAREN);
            List<ValType> pts = new List<ValType>();
            List<string> pns = new List<string>();
            if (lexer.PeekNextToken().type == TokenType.RPAREN)
            {
                pts = new List<ValType>();
                pns = new List<string>();
            }
            else
            {
                while (current_token.type != TokenType.RPAREN)
                {
                    if (current_token.type == TokenType.EOF)
                    {
                        Error(TokenType.RPAREN);
                    }

                    token = current_token;
                    Eat(TokenType.TYPE);
                    var pt = token.lexeme.ToValType();
                    token = current_token;
                    Eat(TokenType.IDENT);
                    var pn = token.lexeme;

                    pts.Add(pt);
                    pns.Add(pn);

                    if (current_token.type == TokenType.COMMA)
                    {
                        Eat(TokenType.COMMA);
                    }
                }
            }
            Eat(TokenType.RPAREN);
            Eat(TokenType.COLON);
            token = current_token;
            Eat(TokenType.TYPE);
            var returnType = token.lexeme.ToValType();

            var body = Block();
            return new FunctionDeclaration(name, body, pts.ToArray(), pns.ToArray(), returnType);
        }

        /// <summary>
        /// return from function will leave a value which is assignable <para/>
        /// return from global scope will terminate the program and set <para/>
        /// exit code to the return value if return value's type is INT
        /// </summary>
        private ASTNode ReturnStatement()
        {
            /*
             * returnstatement : RETURN (expression)? SEMICOLON
             */

            Eat(TokenType.RETURN);

            if (current_token.type == TokenType.SEMICOLON)
            {
                Eat(TokenType.SEMICOLON);
                return new ReturnStatement(new Operand(new Token(TokenType.NULL)));
            }

            var retValue = Expression();
            Eat(TokenType.SEMICOLON);

            return new ReturnStatement(retValue);
        }

        ASTNode Statement()
        {
            /*
             * statement : expressionStatement | variableDeclareStatement | functionDeclareStatement | ifstatement | whilestatement | matchstatement | returnstatement | block
             */

            switch (current_token.type)
            {
                case TokenType.LBRACE:
                    return Block();
                case TokenType.VAR:
                    return VariableDeclare();
                case TokenType.FUN:
                    if (isInFunction)
                    {
                        Error("Nested function is not supported");
                    }
                    return FunctionDeclare();
                case TokenType.RETURN:
                    return ReturnStatement();
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
