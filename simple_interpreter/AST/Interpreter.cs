using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simple_interpreter
{
    interface IVisitor
    {
        void Visit(ASTNode root);
        void Visit(BinaryOperation binop);
        void Visit(Assignment ass);
        void Visit(Operand op);
    }

    class Interpreter : IVisitor
    {
        public int Output
        {
            get
            {
                return Evaluate();
            }
        }

        Dictionary<string, int> Variables;
        Stack<Token> EvaluationStack;

        public Interpreter()
        {
            Variables = new Dictionary<string, int>();
            EvaluationStack = new Stack<Token>();
        }
        public Interpreter(Dictionary<string, int> vars)
        {
            Variables = vars;
            EvaluationStack = new Stack<Token>();
        }

        private void SetValue(string var, int value)
        {
            if (Variables.ContainsKey(var))
            {
                Variables[var] = value;
            }
            else
            {
                Variables.Add(var, value);
            }
        }

        private int GetValue(string var)
        {
            if (Variables.ContainsKey(var))
            {
                return Variables[var];
            }
            else
            {
                Variables.Add(var, 0);
                return 0;
            }
        }

        public void Visit(ASTNode root)
        {
        }

        public void Visit(BinaryOperation binop)
        {
            binop.leftnode.Accept(this);
            binop.rightnode.Accept(this);
            EvaluationStack.Push(binop.op);
        }

        public void Visit(Operand op)
        {
            EvaluationStack.Push(op.token);
        }

        public void Visit(Assignment ass)
        {
            ass.expression.Accept(this);
            ass.ident.Accept(this);
            EvaluationStack.Push(new Token(TokenType.ASSIGN, "="));
        }

        private void ReverseStack()
        {
            Stack<Token> temp = new Stack<Token>();

            while (EvaluationStack.Count > 0)
            {
                temp.Push(EvaluationStack.Pop());
            }

            EvaluationStack = temp;
        }

        private int EvaluateOperand(Token op)
        {
            switch (op.type)
            {
                case TokenType.INTERGER:
                    return int.Parse(op.lexeme);
                case TokenType.IDENT:
                    return GetValue(op.lexeme);
                default:
                    return 0;
            }
        }

        private void Push(int value)
        {
            EvaluationStack.Push(new Token(TokenType.INTERGER, value.ToString()));
        }

        public int Evaluate()
        {
            ReverseStack();
            while (EvaluationStack.Count > 1)
            {
                Token operand1 = EvaluationStack.Pop();
                Token operand2 = EvaluationStack.Pop();
                Token opeartor = EvaluationStack.Pop();

                switch (opeartor.type)
                {
                    case TokenType.PLUS:
                        Push(EvaluateOperand(operand1) + EvaluateOperand(operand2));
                        break;
                    case TokenType.MINUS:
                        Push(EvaluateOperand(operand1) - EvaluateOperand(operand2));
                        break;
                    case TokenType.MULTIPLY:
                        Push(EvaluateOperand(operand1) * EvaluateOperand(operand2));
                        break;
                    case TokenType.DIVIDE:
                        Push(EvaluateOperand(operand1) / EvaluateOperand(operand2));
                        break;
                    case TokenType.ASSIGN:
                        SetValue(operand2.lexeme, EvaluateOperand(operand1));
                        Push(EvaluateOperand(operand1));
                        break;
                    case TokenType.EXPONENT:
                        Push((int)(Math.Pow(EvaluateOperand(operand1), EvaluateOperand(operand2))));
                        break;
                    default:
                        break;
                }
            }
            return EvaluateOperand(EvaluationStack.Pop());
        }
    }
}
