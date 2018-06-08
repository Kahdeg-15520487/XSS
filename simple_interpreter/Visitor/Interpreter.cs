using System;
using System.Collections.Generic;

using simple_interpreter.AST;

namespace simple_interpreter
{
    interface IVisitor
    {
        void Visit(ASTNode root);
        void Visit(BinaryOperation binop);
        void Visit(UnaryOperation binop);
        void Visit(Assignment ass);
        void Visit(Operand op);
    }

    class Interpreter : IVisitor
    {
        public struct StackValue
        {
            public ValType Type { get; set; }
            public object Value { get; set; }

            public StackValue(ValType type, object value)
            {
                Type = type;
                Value = value;
            }

            public static StackValue CreateStackValue(Token token)
            {
                switch (token.type)
                {
                    case TokenType.PLUS:
                    case TokenType.MINUS:
                    case TokenType.MULTIPLY:
                    case TokenType.DIVIDE:
                    case TokenType.EXPONENT:
                    case TokenType.ASSIGN:
                    case TokenType.AND:
                    case TokenType.OR:
                    case TokenType.XOR:
                    case TokenType.NOT:
                        return new StackValue(ValType.Operator, token.type);
                    case TokenType.INTERGER:
                        return new StackValue(ValType.Integer, int.Parse(token.lexeme));
                    case TokenType.FLOAT:
                        return new StackValue(ValType.Float, float.Parse(token.lexeme));
                    case TokenType.BOOL:
                        return new StackValue(ValType.Bool, bool.Parse(token.lexeme));
                    case TokenType.CHAR:
                        return new StackValue(ValType.Char, token.lexeme[0]);
                    case TokenType.STRING:
                        return new StackValue(ValType.String, token.lexeme);
                    default:
                        return new StackValue(ValType.Identifier, token.lexeme);
                }
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 91;
                    hash = hash * 71 + Type.GetHashCode();
                    hash = hash * 71 + Value.GetHashCode();
                    return hash;
                }
            }
            public override string ToString()
            {
                string representation(ValType t, object value)
                {
                    switch (t)
                    {
                        case ValType.Integer:
                        case ValType.Float:
                        case ValType.Bool:
                        case ValType.Identifier:
                        case ValType.Operator:
                            return value.ToString();
                        case ValType.Char:
                            return "'" + value.ToString() + "'";
                        case ValType.String:
                            return '"' + value.ToString() + '"';
                        default:
                            return "null";
                    }
                }
                return "<" + Type + " : " + representation(Type, Value) + ">";
            }

        }

        Dictionary<string, object> Variables;
        Stack<StackValue> EvaluationStack;

        public Interpreter()
        {
            Variables = new Dictionary<string, object>();
            EvaluationStack = new Stack<StackValue>();
        }
        public Interpreter(Dictionary<string, object> vars)
        {
            Variables = vars;
            EvaluationStack = new Stack<StackValue>();
        }

        private void SetVariableValue(string var, object value)
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

        private object GetVariableValue(string var)
        {
            if (Variables.ContainsKey(var))
            {
                return Variables[var];
            }
            else
            {
                Variables.Add(var, null);
                return null;
            }
        }

        public void Visit(ASTNode root)
        {
            Console.WriteLine(root.GetType().Name);
        }

        public void Visit(BinaryOperation binop)
        {
            binop.leftnode.Accept(this);
            binop.rightnode.Accept(this);
            EvaluationStack.Push(StackValue.CreateStackValue(binop.op));
        }

        public void Visit(UnaryOperation unop)
        {
            unop.operand.Accept(this);
            EvaluationStack.Push(StackValue.CreateStackValue(unop.op));
        }

        public void Visit(Operand op)
        {
            EvaluationStack.Push(StackValue.CreateStackValue(op.token));
        }

        public void Visit(Assignment ass)
        {
            ass.expression.Accept(this);
            ass.ident.Accept(this);
            EvaluationStack.Push(StackValue.CreateStackValue(new Token(TokenType.ASSIGN, "=")));
        }

        private void ReverseStack()
        {
            Stack<StackValue> temp = new Stack<StackValue>();

            while (EvaluationStack.Count > 0)
            {
                temp.Push(EvaluationStack.Pop());
            }

            EvaluationStack = temp;
        }

        private object EvaluateOperand(StackValue value)
        {
            switch (value.Type)
            {
                case ValType.Integer:
                    return (int)value.Value;
                case ValType.Float:
                    return (float)value.Value;
                case ValType.Bool:
                    return (bool)value.Value;
                case ValType.Char:
                    return (char)value.Value;
                case ValType.String:
                    return (string)value.Value;
                case ValType.Identifier:
                    return GetVariableValue((string)value.Value);
                case ValType.Null:
                    return null;
                default:
                    return 0;
            }
        }

        StackValue ResolveStackValue(StackValue stackValue)
        {
            if (stackValue.Type == ValType.Identifier)
            {

                var value = GetVariableValue((string)stackValue.Value);
                if (value is null)
                {
                    return new StackValue(ValType.Null, null);
                }
                else if (value.GetType() == typeof(float))
                {
                    return new StackValue(ValType.Float, value);
                }
                else if (value.GetType() == typeof(bool))
                {
                    return new StackValue(ValType.Bool, value);
                }
                else if (value.GetType() == typeof(char))
                {
                    return new StackValue(ValType.Char, value);
                }
                else if (value.GetType() == typeof(string))
                {
                    return new StackValue(ValType.String, value);
                }
                else
                {
                    return new StackValue(ValType.Integer, value);
                }
            }
            else
            {
                return stackValue;
            }
        }

        private void Push(StackValue stackValue)
        {
            EvaluationStack.Push(stackValue);
        }

        private void Push(int value)
        {
            EvaluationStack.Push(new StackValue(ValType.Integer, value));
        }

        private void Push(float value)
        {
            EvaluationStack.Push(new StackValue(ValType.Float, value));
        }

        private void Push(bool value)
        {
            EvaluationStack.Push(new StackValue(ValType.Bool, value));
        }

        private void Push(char value)
        {
            EvaluationStack.Push(new StackValue(ValType.Char, value));
        }

        private void Push(string value)
        {
            EvaluationStack.Push(new StackValue(ValType.String, value));
        }

        int IntegerOperation(StackValue operand1, StackValue operand2, TokenType op)
        {
            int i1 = (int)operand1.Value;
            int i2 = (int)operand2.Value;
            switch (op)
            {
                case TokenType.PLUS:
                    return i1 + i2;
                case TokenType.MINUS:
                    return i1 - i2;
                case TokenType.MULTIPLY:
                    return i1 * i2;
                case TokenType.DIVIDE:
                    return i1 / i2;
            }

            return 0;
        }

        float FloatOperation(StackValue operand1, StackValue operand2, TokenType op)
        {
            float f1 = operand1.Value is float ? (float)operand1.Value : (int)operand1.Value;
            float f2 = operand2.Value is float ? (float)operand2.Value : (int)operand2.Value;
            switch (op)
            {
                case TokenType.PLUS:
                    return f1 + f2;
                case TokenType.MINUS:
                    return f1 - f2;
                case TokenType.MULTIPLY:
                    return f1 * f2;
                case TokenType.DIVIDE:
                    return f1 / f2;
                case TokenType.EXPONENT:
                    return (float)Math.Pow(f1, f2);
            }

            return 0;
        }

        string StringOperation(StackValue operand1, StackValue operand2, TokenType op)
        {
            string s1 = operand1.Value.ToString();
            string s2 = operand2.Value.ToString();

            if (op == TokenType.PLUS)
            {
                return s1 + s2;
            }

            return "";
        }

        bool BoolOperation(StackValue operand1, StackValue operand2, TokenType op)
        {
            bool And(bool b1, bool b2)
            {
                return b1 && b2;
            }
            bool Or(bool b1, bool b2)
            {
                return b1 || b2;
            }
            bool Xor(bool b1, bool b2)
            {
                return b1 ^ b2;
            }
            bool Not(bool b)
            {
                return !b;
            }

            return true;
        }

        public object Evaluate()
        {
            void RuntimeError(StackValue operand1, StackValue operand2, TokenType op)
            {
                throw new Exception("Undefined behaviour :" + operand1 + " " + op + " " + operand2);
            }

            void EvaluateBinaryOperation(StackValue op1, StackValue op2, TokenType op)
            {
                var operand1 = ResolveStackValue(op1);
                var operand2 = ResolveStackValue(op2);
                if (operand1.Type == operand2.Type)
                {
                    switch (operand1.Type)
                    {
                        case ValType.Integer:
                            switch (op)
                            {
                                case TokenType.PLUS:
                                case TokenType.MINUS:
                                case TokenType.MULTIPLY:
                                case TokenType.DIVIDE:
                                    Push(IntegerOperation(operand1, operand2, op));
                                    break;
                                case TokenType.EXPONENT:
                                    Push(FloatOperation(operand1, operand2, TokenType.EXPONENT));
                                    break;
                            }
                            break;

                        case ValType.Float:
                            switch (op)
                            {
                                case TokenType.PLUS:
                                case TokenType.MINUS:
                                case TokenType.MULTIPLY:
                                case TokenType.DIVIDE:
                                case TokenType.EXPONENT:
                                    Push(FloatOperation(operand1, operand2, op));
                                    break;
                            }
                            break;

                        case ValType.Bool:
                            //bool b1 = (bool)operand1.Value;
                            //bool b2 = (bool)operand2.Value;
                            break;

                        case ValType.Char:
                            //concat char
                            if (op == TokenType.PLUS)
                            {
                                Push(StringOperation(operand1, operand2, TokenType.PLUS));
                            }
                            else
                            {
                                RuntimeError(operand1, operand2, op);
                            }
                            break;

                        case ValType.String:
                            //concat string
                            if (op == TokenType.PLUS)
                            {
                                Push(StringOperation(operand1, operand2, TokenType.PLUS));
                            }
                            else
                            {
                                RuntimeError(operand1, operand2, op);
                            }
                            break;
                    }
                }
                else
                {
                    switch (operand1.Type)
                    {
                        case ValType.Integer:
                            if (operand2.Type == ValType.Float)
                            {
                                //do flt math
                                switch (op)
                                {
                                    case TokenType.PLUS:
                                    case TokenType.MINUS:
                                    case TokenType.MULTIPLY:
                                    case TokenType.DIVIDE:
                                    case TokenType.EXPONENT:
                                        Push(FloatOperation(operand1, operand2, op));
                                        break;
                                }
                            }
                            else if (operand2.Type == ValType.Char)
                            {
                                //do int math
                                switch (op)
                                {
                                    case TokenType.PLUS:
                                    case TokenType.MINUS:
                                    case TokenType.MULTIPLY:
                                    case TokenType.DIVIDE:
                                        Push(IntegerOperation(operand1, operand2, op));
                                        break;
                                    case TokenType.EXPONENT:
                                        Push(FloatOperation(operand1, operand2, TokenType.EXPONENT));
                                        break;
                                }
                            }
                            else
                            {
                                RuntimeError(operand1, operand2, op);
                            }
                            break;

                        case ValType.Float:
                            if (operand2.Type == ValType.Integer)
                            {
                                //do flt math
                                switch (op)
                                {
                                    case TokenType.PLUS:
                                    case TokenType.MINUS:
                                    case TokenType.MULTIPLY:
                                    case TokenType.DIVIDE:
                                    case TokenType.EXPONENT:
                                        Push(FloatOperation(operand1, operand2, op));
                                        break;
                                }
                            }
                            else
                            {
                                RuntimeError(operand1, operand2, op);
                            }
                            break;
                        case ValType.Char:
                            //cause there is no boolean operator with mixed type
                            RuntimeError(operand1, operand2, op);
                            break;

                        case ValType.Bool:
                            //cause there is no boolean operator with mixed type
                            RuntimeError(operand1, operand2, op);
                            break;

                        case ValType.String:
                            //concat string
                            if (op == TokenType.PLUS)
                            {
                                Push(StringOperation(operand1, operand2, TokenType.PLUS));
                            }
                            else
                            {
                                RuntimeError(operand1, operand2, op);
                            }
                            break;

                        case ValType.Null:
                            Push(new StackValue(ValType.Null, null));
                            break;
                        default:
                            break;
                    }
                }
            }

            void EvaluateUnaryOperation(StackValue operand, TokenType op)
            {
                var operand1 = ResolveStackValue(operand);
                switch (operand1.Type)
                {
                    case ValType.Integer:
                        if (op == TokenType.MINUS)
                        {
                            int i = (int)operand1.Value;
                            Push(-i);
                        }
                        break;
                    case ValType.Float:
                        if (op == TokenType.MINUS)
                        {
                            float f = (float)operand1.Value;
                            Push(-f);
                        }
                        break;
                    case ValType.Bool:
                        if (op == TokenType.NOT)
                        {
                            bool b = (bool)operand1.Value;
                            Push(!b);
                        }
                        break;

                    default:
                        break;
                }
            }

            if (EvaluationStack.Count == 0)
            {
                return null;
            }

            ReverseStack();
            while (EvaluationStack.Count > 1)
            {
                StackValue operand1 = EvaluationStack.Pop();
                StackValue operand2 = EvaluationStack.Pop();
                TokenType op;
                if (operand2.Type == ValType.Operator)
                {
                    //unary opearation
                    op = (TokenType)operand2.Value;
                    EvaluateUnaryOperation(operand1, op);
                }
                else
                {
                    op = (TokenType)EvaluationStack.Pop().Value;

                    if (op == TokenType.ASSIGN)
                    {
                        //do assignment
                        SetVariableValue((string)operand2.Value, ResolveStackValue(operand1).Value);
                        Push(ResolveStackValue(operand2));
                    }
                    else
                    {
                        EvaluateBinaryOperation(operand1, operand2, op);
                    }
                }

                #region test
                //if (operand1.Type == operand2.Type)
                //{
                //    //same type operator
                //    switch (operand1.Type)
                //    {
                //        case ValType.Integer:
                //            int i1 = (int)operand1.Value;
                //            int i2 = (int)operand2.Value;
                //            switch (op)
                //            {
                //                case TokenType.PLUS:
                //                    Push(IntPlus(i1, i2));
                //                    break;
                //                case TokenType.MINUS:
                //                    Push(IntSub(i1, i2));
                //                    break;
                //                case TokenType.MULTIPLY:
                //                    Push(IntMul(i1, i2));
                //                    break;
                //                case TokenType.DIVIDE:
                //                    Push(IntDiv(i1, i2));
                //                    break;
                //                case TokenType.EXPONENT:
                //                    Push(FltExp(i1, i2));
                //                    break;
                //            }
                //            break;

                //        case ValType.Float:
                //            float f1 = (float)operand1.Value;
                //            float f2 = (float)operand2.Value;
                //            switch (op)
                //            {
                //                case TokenType.PLUS:
                //                    Push(FltPlus(f1, f2));
                //                    break;
                //                case TokenType.MINUS:
                //                    Push(FltSub(f1, f2));
                //                    break;
                //                case TokenType.MULTIPLY:
                //                    Push(FltMul(f1, f2));
                //                    break;
                //                case TokenType.DIVIDE:
                //                    Push(FltDiv(f1, f2));
                //                    break;
                //                case TokenType.EXPONENT:
                //                    Push(FltExp(f1, f2));
                //                    break;
                //            }
                //            break;

                //        case ValType.Bool:
                //            bool b1 = (bool)operand1.Value;
                //            bool b2 = (bool)operand2.Value;
                //            break;

                //        case ValType.String:
                //            //concat string
                //            if (op == TokenType.PLUS)
                //            {
                //                string s1 = (string)operand1.Value;
                //                string s2 = (string)operand2.Value;
                //                Push(Concat(s1, s2));
                //            }
                //            else
                //            {
                //                //throw runtime error
                //            }
                //            break;

                //        case ValType.Identifier:
                //            //get value and check type
                //            Evaluate(operand1, operand2, op);
                //            break;
                //    }
                //}
                //else
                //{
                //    switch (operand1.Type)
                //    {
                //        case ValType.Integer:
                //            if (operand2.Type == ValType.Float)
                //            {
                //                //do flt mathfloat 
                //                int i1 = (int)operand1.Value;
                //                float f2 = (float)operand2.Value;
                //                switch (op)
                //                {
                //                    case TokenType.PLUS:
                //                        Push(FltPlus(i1, f2));
                //                        break;
                //                    case TokenType.MINUS:
                //                        Push(FltSub(i1, f2));
                //                        break;
                //                    case TokenType.MULTIPLY:
                //                        Push(FltMul(i1, f2));
                //                        break;
                //                    case TokenType.DIVIDE:
                //                        Push(FltDiv(i1, f2));
                //                        break;
                //                    case TokenType.EXPONENT:
                //                        Push(FltExp(i1, f2));
                //                        break;
                //                }
                //            }
                //            else
                //            {
                //                //throw runtime error
                //            }
                //            break;

                //        case ValType.Float:
                //            if (operand2.Type == ValType.Integer)
                //            {
                //                //do flt math
                //                float f1 = (int)operand1.Value;
                //                int i2 = (int)operand2.Value;
                //                switch (op)
                //                {
                //                    case TokenType.PLUS:
                //                        Push(FltPlus(f1, i2));
                //                        break;
                //                    case TokenType.MINUS:
                //                        Push(FltSub(f1, i2));
                //                        break;
                //                    case TokenType.MULTIPLY:
                //                        Push(FltMul(f1, i2));
                //                        break;
                //                    case TokenType.DIVIDE:
                //                        Push(FltDiv(f1, i2));
                //                        break;
                //                    case TokenType.EXPONENT:
                //                        Push(FltExp(f1, i2));
                //                        break;
                //                }
                //            }
                //            else
                //            {
                //                //throw runtime error
                //            }
                //            break;

                //        case ValType.Bool:
                //            //cause there is no boolean operator with mixed type
                //            //throw runtime error
                //            break;

                //        case ValType.String:
                //            //concactenate to string represntation of the operand2
                //            if (op == TokenType.PLUS)
                //            {
                //                string s1 = (string)operand1.Value;
                //                string s2 = operand2.Value.ToString();
                //                Push(Concat(s1, s2));
                //            }
                //            break;

                //        case ValType.Identifier:
                //            //get value and check type
                //            Evaluate(operand1, operand2, op);
                //            break;
                //    }
                //}

                //check type at runtime
                //switch (opeartor)
                //{
                //    case TokenType.PLUS:
                //        Push(EvaluateOperand(operand1) + EvaluateOperand(operand2));
                //        break;
                //    case TokenType.MINUS:
                //        Push(EvaluateOperand(operand1) - EvaluateOperand(operand2));
                //        break;
                //    case TokenType.MULTIPLY:
                //        Push(EvaluateOperand(operand1) * EvaluateOperand(operand2));
                //        break;
                //    case TokenType.DIVIDE:
                //        Push(EvaluateOperand(operand1) / EvaluateOperand(operand2));
                //        break;
                //    case TokenType.ASSIGN:
                //        SetValue(operand2.lexeme, EvaluateOperand(operand1));
                //        Push(EvaluateOperand(operand1));
                //        break;
                //    case TokenType.EXPONENT:
                //        Push((int)(Math.Pow(EvaluateOperand(operand1), EvaluateOperand(operand2))));
                //        break;
                //    default:
                //        break;
                //}

                #endregion
            }

            return EvaluateOperand(EvaluationStack.Pop());
        }
    }
}

