using System;
using System.Collections.Generic;
using System.Linq;
using XSS.AST;

namespace XSS
{
    interface IValue
    {
        ValType Type { get; }
        object Value { get; }
    }

    interface IFunction
    {
        string Name { get; }
        FunctionSignature FunctionSignature { get; }
        Func<IValue[], IValue> Function { get; }
        bool IsCompatibleWith(FunctionSignature funcDecl);
    }

    static class InterpreterHelperMethod
    {
        public static T CastTo<T>(this IValue value)
        {
            return (T)value.Value;
        }
    }
    #region stack value
    struct StackValue : IValue
    {
        public static readonly StackValue Null = new StackValue(ValType.Null, null);

        public ValType Type { get; set; }
        public object Value { get; set; }

        public StackValue(ValType type, object value = null)
        {
            Type = type;
            Value = value;
        }

        public static StackValue CreateStackValue(FunctionCall functionCall)
        {
            return new StackValue(ValType.FunctionCall, functionCall);
        }

        public static StackValue CreateStackValue(Token token)
        {
            switch (token.type)
            {
                case TokenType.PLUS:
                case TokenType.MINUS:
                case TokenType.MULTIPLY:
                case TokenType.DIVIDE:
                case TokenType.MODULO:
                case TokenType.EXPONENT:
                case TokenType.ASSIGN:
                case TokenType.VAR:
                case TokenType.AND:
                case TokenType.OR:
                case TokenType.XOR:
                case TokenType.NOT:
                case TokenType.EQUAL:
                case TokenType.NOTEQUAL:
                case TokenType.LARGER:
                case TokenType.LARGEREQUAL:
                case TokenType.LESSER:
                case TokenType.LESSEREQUAL:
                case TokenType.IS:
                case TokenType.TYPEOF:
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
                case TokenType.NULL:
                    return new StackValue(ValType.Null, "null");
                case TokenType.TYPE:
                    var type = token.lexeme.ToValType();
                    return new StackValue(ValType.Type, type);
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
                    case ValType.Type:
                        return value?.ToString();
                    case ValType.Char:
                        return "'" + value?.ToString() + "'";
                    case ValType.String:
                        return '"' + value?.ToString() + '"';
                    case ValType.Function:
                        return (value as IFunction).Name;
                    default:
                        return "null";
                }
            }
            return "<" + Type + " : " + representation(Type, Value) + ">";
        }

    }
    #endregion

    #region function

    class NativeFunction : IFunction
    {
        public NativeFunction(string name, FunctionDeclaration funcDecl)
        {
            //Function = FunctionRunner(funcDecl);
            Name = name;
            FunctionDeclaration = funcDecl;
        }

        public Func<IValue[], IValue> Function { get; private set; }
        public string Name { get; private set; }

        public FunctionDeclaration FunctionDeclaration { get; private set; }
        public FunctionSignature FunctionSignature { get => FunctionDeclaration.FunctionSignature; }

        public override int GetHashCode()
        {
            unchecked
            {
                return FunctionDeclaration.GetHashCode();
            }
        }

        public bool IsCompatibleWith(FunctionSignature funsig)
        {
            return FunctionSignature.GetHashCode() == funsig.GetHashCode();
        }

        public override string ToString()
        {
            return FunctionDeclaration.Value();
        }
    }

    #endregion

    class Interpreter : IVisitor
    {
        Scope Global;
        Scope CurrentScope;
        Stack<StackValue> EvaluationStack;
        private bool breakFlag;
        private bool isInFunction;

        public Interpreter()
        {
            Global = new Scope();
            CurrentScope = Global;
            EvaluationStack = new Stack<StackValue>();
        }
        public Interpreter(Scope environment)
        {
            Global = environment;
            CurrentScope = Global;
            EvaluationStack = new Stack<StackValue>();
        }

        #region visitor
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
            var tttt = StackValue.CreateStackValue(op.token);
            EvaluationStack.Push(tttt);
        }

        public void Visit(Assignment ass)
        {
            ass.expression.Accept(this);
            ass.ident.Accept(this);
            EvaluationStack.Push(StackValue.CreateStackValue(new Token(TokenType.ASSIGN, "=")));
        }

        public void Visit(VariableDeclareStatement vardecl)
        {
            if (vardecl.init == null)
            {
                EvaluationStack.Push(new StackValue(ValType.Null, null));
            }
            else
            {
                vardecl.init.Accept(this);
            }

            vardecl.ident.Accept(this);
            EvaluationStack.Push(StackValue.CreateStackValue(new Token(TokenType.VAR)));
        }

        public void Visit(ExpressionStatement exprstmt)
        {
            exprstmt.Expression.Accept(this);
        }

        public void Visit(FunctionCall functionCall)
        {
            EvaluationStack.Push(StackValue.CreateStackValue(functionCall));
        }

        public void Visit(IfStatement ifstmt)
        {

        }

        public void Visit(MatchStatement matchStatement)
        {

        }

        public void Visit(WhileStatement whilestmt)
        {

        }

        public void Visit(FunctionDeclaration funcDecl)
        {

        }

        public void Visit(ReturnStatement retstmt)
        {

        }

        public void Visit(Block block)
        {
            foreach (var stmt in block.Statements)
            {
                stmt.Accept(this);
            }
        }
        #endregion

        void RuntimeError(string message = "")
        {
            throw new Exception($"Runtime error: {message}");
        }
        void BinaryRuntimeError(StackValue operand1, StackValue operand2, TokenType op, string message = "")
        {
            RuntimeError("Undefined behaviour :" + operand1 + " " + op + " " + operand2 + "\n" + message);
        }
        void UnaryRuntimeError(StackValue operand, TokenType op, string message = "")
        {
            RuntimeError("Undefined behaviour :" + op + " " + operand + "\n" + message);
        }

        #region stack manipulation
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
                    return CurrentScope.Get((string)value.Value);
                case ValType.Type:
                    return value.CastTo<ValType>();
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
                var varname = stackValue.CastTo<string>();
                if (!CurrentScope.Contain(varname))
                {
                    RuntimeError($"{varname} is not defined");
                }
                var value = CurrentScope.Get(varname);
                switch (value)
                {
                    case null:
                        return new StackValue(ValType.Null, null);
                    case float f:
                        return new StackValue(ValType.Float, value);
                    case bool b:
                        return new StackValue(ValType.Bool, value);
                    case char c:
                        return new StackValue(ValType.Char, value);
                    case string s:
                        return new StackValue(ValType.String, value);
                    case int i:
                        return new StackValue(ValType.Integer, value);
                    case NativeFunction nativeFunction:
                        return new StackValue(ValType.Function, value);
                    default:
                        throw new Exception($"unknow value type {value.GetType().Name}");
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

        private void Push(ValType value)
        {
            EvaluationStack.Push(new StackValue(ValType.Type, value));
        }

        #endregion

        private StackValue Evaluate(ASTNode Expression)
        {
            #region operation
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
                    case TokenType.MODULO:
                        return i1 % i2;
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
                bool b1 = (bool)operand1.Value;
                bool b2 = op == TokenType.NOT ? true : (bool)operand2.Value;

                switch (op)
                {
                    case TokenType.AND:
                        return b1 && b2;
                    case TokenType.OR:
                        return b1 || b2;
                    case TokenType.XOR:
                        return b1 ^ b2;
                    case TokenType.NOT:
                        return !b1;
                }

                return false;
            }
            bool IntComparison(StackValue operand1, StackValue operand2, TokenType op)
            {
                int i1 = (int)operand1.Value;
                int i2 = (int)operand2.Value;
                switch (op)
                {
                    case TokenType.EQUAL:
                        return i1 == i2;
                    case TokenType.NOTEQUAL:
                        return i1 != i2;
                    case TokenType.LARGER:
                        return i1 > i2;
                    case TokenType.LARGEREQUAL:
                        return i1 >= i2;
                    case TokenType.LESSER:
                        return i1 < i2;
                    case TokenType.LESSEREQUAL:
                        return i1 <= i2;
                }
                return false;
            }
            bool FloatComparison(StackValue operand1, StackValue operand2, TokenType op)
            {
                float f1 = (float)operand1.Value;
                float f2 = (float)operand2.Value;
                switch (op)
                {
                    case TokenType.EQUAL:
                        return f1 == f2;
                    case TokenType.NOTEQUAL:
                        return f1 != f2;
                    case TokenType.LARGER:
                        return f1 > f2;
                    case TokenType.LARGEREQUAL:
                        return f1 >= f2;
                    case TokenType.LESSER:
                        return f1 < f2;
                    case TokenType.LESSEREQUAL:
                        return f1 <= f2;
                }
                return false;
            }
            bool CharComparison(StackValue operand1, StackValue operand2, TokenType op)
            {
                char c1 = (char)operand1.Value;
                char c2 = (char)operand2.Value;
                switch (op)
                {
                    case TokenType.EQUAL:
                        return c1 == c2;
                    case TokenType.NOTEQUAL:
                        return c1 != c2;
                    case TokenType.LARGER:
                        return c1 > c2;
                    case TokenType.LARGEREQUAL:
                        return c1 >= c2;
                    case TokenType.LESSER:
                        return c1 < c2;
                    case TokenType.LESSEREQUAL:
                        return c1 <= c2;
                }
                return false;
            }
            bool StringComparison(StackValue operand1, StackValue operand2, TokenType op)
            {
                string s1 = (string)operand1.Value;
                string s2 = (string)operand2.Value;
                var comparison = s1.CompareTo(s2);
                switch (op)
                {
                    case TokenType.EQUAL:
                        return s1.Equals(s2);
                    case TokenType.NOTEQUAL:
                        return !s1.Equals(s2);
                    case TokenType.LARGER:
                        return comparison > 0;
                    case TokenType.LARGEREQUAL:
                        return comparison >= 0;
                    case TokenType.LESSER:
                        return comparison < 0;
                    case TokenType.LESSEREQUAL:
                        return comparison <= 0;
                }
                return false;
            }
            bool BoolComparison(StackValue operand1, StackValue operand2, TokenType op)
            {
                bool b1 = (bool)operand1.Value;
                bool b2 = (bool)operand2.Value;
                switch (op)
                {
                    case TokenType.EQUAL:
                        return b1 == b2;
                    case TokenType.NOTEQUAL:
                        return b1 != b2;
                }
                return false;
            }
            bool Comparison(StackValue operand1, StackValue operand2, TokenType op)
            {
                bool result = true;
                switch (operand1.Type)
                {
                    case ValType.Integer:
                        int i1 = (int)operand1.Value;
                        if (operand2.Type == ValType.Float)
                        {
                            //do int comparison
                            int i2 = (int)(float)operand2.Value;
                            switch (op)
                            {
                                case TokenType.EQUAL:
                                    result = i1 == i2;
                                    break;
                                case TokenType.NOTEQUAL:
                                    result = i1 != i2;
                                    break;
                                case TokenType.LARGER:
                                    result = i1 > i2;
                                    break;
                                case TokenType.LARGEREQUAL:
                                    result = i1 >= i2;
                                    break;
                                case TokenType.LESSER:
                                    result = i1 < i2;
                                    break;
                                case TokenType.LESSEREQUAL:
                                    result = i1 <= i2;
                                    break;
                            }
                        }
                        else if (operand2.Type == ValType.Char)
                        {
                            //do int comparison
                            int i2 = (int)(char)operand2.Value;
                            switch (op)
                            {
                                case TokenType.EQUAL:
                                    result = i1 == i2;
                                    break;
                                case TokenType.NOTEQUAL:
                                    result = i1 != i2;
                                    break;
                                case TokenType.LARGER:
                                    result = i1 > i2;
                                    break;
                                case TokenType.LARGEREQUAL:
                                    result = i1 >= i2;
                                    break;
                                case TokenType.LESSER:
                                    result = i1 < i2;
                                    break;
                                case TokenType.LESSEREQUAL:
                                    result = i1 <= i2;
                                    break;
                            }
                        }
                        else
                        {
                            BinaryRuntimeError(operand1, operand2, op);
                        }
                        break;

                    case ValType.Float:
                        float f1 = (float)operand1.Value;
                        if (operand2.Type == ValType.Integer)
                        {
                            //do flt math
                            float f2 = (float)(int)operand2.Value;
                            switch (op)
                            {
                                case TokenType.EQUAL:
                                    result = f1 == f2;
                                    break;
                                case TokenType.NOTEQUAL:
                                    result = f1 != f2;
                                    break;
                                case TokenType.LARGER:
                                    result = f1 > f2;
                                    break;
                                case TokenType.LARGEREQUAL:
                                    result = f1 >= f2;
                                    break;
                                case TokenType.LESSER:
                                    result = f1 < f2;
                                    break;
                                case TokenType.LESSEREQUAL:
                                    result = f1 <= f2;
                                    break;
                            }
                        }
                        else
                        {
                            BinaryRuntimeError(operand1, operand2, op);
                        }
                        break;
                    case ValType.Char:
                        int c1 = (int)(char)operand1.Value;
                        if (operand2.Type == ValType.Integer)
                        {
                            //do int comparison
                            int c2 = (int)(char)operand2.Value;
                            switch (op)
                            {
                                case TokenType.EQUAL:
                                    result = c1 == c2;
                                    break;
                                case TokenType.NOTEQUAL:
                                    result = c1 != c2;
                                    break;
                                case TokenType.LARGER:
                                    result = c1 > c2;
                                    break;
                                case TokenType.LARGEREQUAL:
                                    result = c1 >= c2;
                                    break;
                                case TokenType.LESSER:
                                    result = c1 < c2;
                                    break;
                                case TokenType.LESSEREQUAL:
                                    result = c1 <= c2;
                                    break;
                            }
                        }
                        else
                        {
                            BinaryRuntimeError(operand1, operand2, op);
                        }
                        break;

                    case ValType.Bool:
                        //cause there is no boolean comparison operator with mixed type
                        BinaryRuntimeError(operand1, operand2, op);
                        break;

                    case ValType.String:
                        //cause there is no string comparison operator with mixed type
                        BinaryRuntimeError(operand1, operand2, op);
                        break;

                    case ValType.Null:
                        BinaryRuntimeError(operand1, operand2, op);
                        break;
                }
                return result;
            }
            bool TypeTesting(StackValue operand1, StackValue operand2, TokenType op)
            {
                ValType type = operand2.CastTo<ValType>();

                switch (op)
                {
                    case TokenType.EQUAL:
                    case TokenType.NOTEQUAL:
                        if (operand1.Type != ValType.Type)
                        {
                            return false;
                        }
                        else
                        {
                            var type2 = operand1.CastTo<ValType>();
                            return op == TokenType.EQUAL ? type == type2 : !(type == type2);
                        }
                    case TokenType.IS:
                        // <value> is <Type>
                        return operand1.Type == type;
                }

                return false;
            }
            #endregion

            #region evaluate expression
            void EvaluateBinaryOperation(StackValue operand1, StackValue operand2, TokenType op)
            {
                var v1 = ResolveStackValue(operand1);
                var v2 = ResolveStackValue(operand2);
                if (v1.Type == v2.Type)
                {
                    switch (v1.Type)
                    {
                        case ValType.Integer:
                            switch (op)
                            {
                                case TokenType.PLUS:
                                case TokenType.MINUS:
                                case TokenType.MULTIPLY:
                                case TokenType.DIVIDE:
                                case TokenType.MODULO:
                                    Push(IntegerOperation(v1, v2, op));
                                    break;
                                case TokenType.EXPONENT:
                                    Push(FloatOperation(v1, v2, TokenType.EXPONENT));
                                    break;
                                case TokenType.EQUAL:
                                case TokenType.NOTEQUAL:
                                case TokenType.LARGER:
                                case TokenType.LARGEREQUAL:
                                case TokenType.LESSER:
                                case TokenType.LESSEREQUAL:
                                    Push(IntComparison(v1, v2, op));
                                    break;

                                default:
                                    BinaryRuntimeError(v1, v2, op);
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
                                    Push(FloatOperation(v1, v2, op));
                                    break;
                                case TokenType.EQUAL:
                                case TokenType.NOTEQUAL:
                                case TokenType.LARGER:
                                case TokenType.LARGEREQUAL:
                                case TokenType.LESSER:
                                case TokenType.LESSEREQUAL:
                                    Push(FloatComparison(v1, v2, op));
                                    break;

                                default:
                                    BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;

                        case ValType.Bool:
                            switch (op)
                            {
                                case TokenType.AND:
                                case TokenType.OR:
                                case TokenType.XOR:
                                    Push(BoolOperation(v1, v2, op));
                                    break;
                                case TokenType.EQUAL:
                                case TokenType.NOTEQUAL:
                                    Push(BoolComparison(v1, v2, op));
                                    break;

                                default:
                                    BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;

                        case ValType.Char:
                            //concat char
                            switch (op)
                            {
                                case TokenType.PLUS:
                                    Push(StringOperation(v1, v2, TokenType.PLUS));
                                    break;

                                case TokenType.EQUAL:
                                case TokenType.NOTEQUAL:
                                case TokenType.LARGER:
                                case TokenType.LARGEREQUAL:
                                case TokenType.LESSER:
                                case TokenType.LESSEREQUAL:
                                    Push(CharComparison(v1, v2, op));
                                    break;

                                default:
                                    BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;

                        case ValType.String:
                            //concat string
                            switch (op)
                            {
                                case TokenType.PLUS:
                                    Push(StringOperation(v1, v2, TokenType.PLUS));
                                    break;

                                case TokenType.EQUAL:
                                case TokenType.NOTEQUAL:
                                case TokenType.LARGER:
                                case TokenType.LARGEREQUAL:
                                case TokenType.LESSER:
                                case TokenType.LESSEREQUAL:
                                    Push(StringComparison(v1, v2, op));
                                    break;

                                default:
                                    BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;
                        case ValType.Null:
                            BinaryRuntimeError(v1, v2, op);
                            break;
                        case ValType.Type:
                            switch (op)
                            {
                                case TokenType.EQUAL:
                                case TokenType.NOTEQUAL:
                                    Push(TypeTesting(v1, v2, op));
                                    break;
                                default:
                                    BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;

                        default:
                            switch (op)
                            {
                                case TokenType.IS:
                                    Push(TypeTesting(v1, v2, op));
                                    break;
                                default:
                                    BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    switch (v1.Type)
                    {
                        case ValType.Integer:
                            if (v2.Type == ValType.Float)
                            {
                                //do flt math
                                switch (op)
                                {
                                    case TokenType.PLUS:
                                    case TokenType.MINUS:
                                    case TokenType.MULTIPLY:
                                    case TokenType.DIVIDE:
                                    case TokenType.EXPONENT:
                                        Push(FloatOperation(v1, v2, op));
                                        break;

                                    case TokenType.EQUAL:
                                    case TokenType.NOTEQUAL:
                                    case TokenType.LARGER:
                                    case TokenType.LARGEREQUAL:
                                    case TokenType.LESSER:
                                    case TokenType.LESSEREQUAL:
                                        Push(Comparison(v1, v2, op));
                                        break;
                                }
                            }
                            else if (v2.Type == ValType.Char)
                            {
                                //do int math
                                switch (op)
                                {
                                    case TokenType.PLUS:
                                    case TokenType.MINUS:
                                    case TokenType.MULTIPLY:
                                    case TokenType.DIVIDE:
                                        Push(IntegerOperation(v1, v2, op));
                                        break;
                                    case TokenType.EXPONENT:
                                        Push(FloatOperation(v1, v2, TokenType.EXPONENT));
                                        break;

                                    case TokenType.EQUAL:
                                    case TokenType.NOTEQUAL:
                                    case TokenType.LARGER:
                                    case TokenType.LARGEREQUAL:
                                    case TokenType.LESSER:
                                    case TokenType.LESSEREQUAL:
                                        Push(Comparison(v1, v2, op));
                                        break;
                                }
                            }
                            else if (op == TokenType.IS)
                            {
                                Push(TypeTesting(v1, v2, op));
                            }
                            else
                            {
                                BinaryRuntimeError(v1, v2, op);
                            }
                            break;

                        case ValType.Float:
                            if (v2.Type == ValType.Integer)
                            {
                                //do flt math
                                switch (op)
                                {
                                    case TokenType.PLUS:
                                    case TokenType.MINUS:
                                    case TokenType.MULTIPLY:
                                    case TokenType.DIVIDE:
                                    case TokenType.EXPONENT:
                                        Push(FloatOperation(v1, v2, op));
                                        break;

                                    case TokenType.EQUAL:
                                    case TokenType.NOTEQUAL:
                                    case TokenType.LARGER:
                                    case TokenType.LARGEREQUAL:
                                    case TokenType.LESSER:
                                    case TokenType.LESSEREQUAL:
                                        Push(Comparison(v1, v2, op));
                                        break;
                                }
                            }
                            else if (op == TokenType.IS)
                            {
                                Push(TypeTesting(v1, v2, op));
                            }
                            else
                            {
                                BinaryRuntimeError(v1, v2, op);
                            }
                            break;
                        case ValType.Char:
                            switch (op)
                            {

                                case TokenType.EQUAL:
                                case TokenType.NOTEQUAL:
                                case TokenType.LARGER:
                                case TokenType.LARGEREQUAL:
                                case TokenType.LESSER:
                                case TokenType.LESSEREQUAL:
                                    Push(Comparison(v1, v2, op));
                                    break;
                                case TokenType.IS:
                                    Push(TypeTesting(v1, v2, op));
                                    break;
                                default:
                                    BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;

                        case ValType.Bool:
                            if (op == TokenType.IS)
                            {
                                Push(TypeTesting(v1, v2, op));
                            }
                            else
                            {
                                BinaryRuntimeError(v1, v2, op);
                            }
                            break;

                        case ValType.String:
                            //concat string
                            if (op == TokenType.PLUS)
                            {
                                Push(StringOperation(v1, v2, TokenType.PLUS));
                            }
                            else if (op == TokenType.IS)
                            {
                                Push(TypeTesting(v1, v2, op));
                            }
                            else if (op == TokenType.IS)
                            {
                                Push(TypeTesting(v1, v2, op));
                            }
                            else
                            {
                                BinaryRuntimeError(v1, v2, op);
                            }
                            break;

                        default:
                            switch (op)
                            {
                                case TokenType.IS:
                                    Push(TypeTesting(v1, v2, op));
                                    break;
                                default:
                                    BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;
                    }
                }
            }
            void EvaluateUnaryOperation(StackValue operand, TokenType op)
            {
                var v = ResolveStackValue(operand);

                switch (op)
                {
                    case TokenType.MINUS:
                        if (v.Type == ValType.Integer)
                        {
                            int i = v.CastTo<int>();
                            Push(-i);
                        }
                        else if (v.Type == ValType.Float)
                        {
                            float f = v.CastTo<float>();
                            Push(-f);
                        }
                        else
                        {
                            UnaryRuntimeError(operand, op);
                        }
                        break;
                    case TokenType.NOT:
                        if (v.Type == ValType.Bool)
                        {
                            bool b = v.CastTo<bool>();
                            Push(!b);
                        }
                        break;
                    case TokenType.TYPEOF:
                        Push(v.Type);
                        break;
                }
            }
            void EvaluateFunctionCall(FunctionCall functionCall)
            {
                //Console.WriteLine(functionCall.Value());

                //resolve all parameters
                List<(ValType type, object value)> parameters = new List<(ValType type, object value)>();
                foreach (var parameter in functionCall.Parameters)
                {
                    StackValue temp;
                    switch (parameter)
                    {
                        case Operand operand:
                            {
                                temp = ResolveStackValue(StackValue.CreateStackValue(operand.token));
                            }
                            break;

                        case BinaryOperation binop:
                            {
                                temp = Evaluate(binop);
                            }
                            break;
                        case UnaryOperation unaop:
                            {
                                temp = Evaluate(unaop);
                            }
                            break;
                        case FunctionCall funcCall:
                            {
                                temp = Evaluate(funcCall);
                            }
                            break;
                        default:
                            temp = StackValue.Null;
                            break;
                    }

                    parameters.Add((temp.Type, temp.Value));
                }

                //generate function signature from the function call
                FunctionSignature funsig = new FunctionSignature(parameters.Select(p => p.type).ToArray(), functionCall.FunctionName);

                //find the function to be called
                //var function = (NativeFunction)Functions.FirstOrDefault(f => f.Name.Equals(functionCall.FunctionName) && f.IsCompatibleWith(funsig));
                CurrentScope.Contain(functionCall.FunctionName);
                var function = CurrentScope.Get(functionCall.FunctionName) as NativeFunction;

                var localScope = new Scope(CurrentScope);

                for (int i = 0; i < parameters.Count; i++)
                {
                    localScope.Define(function.FunctionDeclaration.ParameterNames[i], parameters[i].value);
                }

                CurrentScope = localScope;

                Execute(function.FunctionDeclaration.Body);

                CurrentScope = Global;

                //Functions.FirstOrDefault()

                //Push(1);
            }
            #endregion

            #region visit the astnode
            Expression.Accept(this);
            #endregion

            #region process operand
            if (EvaluationStack.Count == 0)
            {
                return StackValue.Null;
            }

            ReverseStack();
            while (EvaluationStack.Count > 0)
            {
                StackValue operand1 = EvaluationStack.Pop();
                StackValue operand2;
                TokenType op;
                if (operand1.Type == ValType.FunctionCall)
                {
                    EvaluateFunctionCall((FunctionCall)operand1.Value);
                }
                else
                {
                    if (EvaluationStack.Count > 0)
                    {
                        operand2 = EvaluationStack.Pop();
                        if (operand2.Type == ValType.Operator)
                        {
                            //unary opearation
                            op = (TokenType)operand2.Value;
                            EvaluateUnaryOperation(operand1, op);
                        }
                        else
                        {
                            if (EvaluationStack.Count > 0)
                            {
                                op = (TokenType)EvaluationStack.Pop().Value;

                                if (op == TokenType.ASSIGN)
                                {
                                    //do assignment
                                    //todo check type
                                    var varname = (string)operand2.Value;
                                    if (!CurrentScope.Contain(varname))
                                    {
                                        RuntimeError($"{varname} is not defined");
                                    }
                                    CurrentScope.Assign(varname, ResolveStackValue(operand1).Value);
                                    Push(ResolveStackValue(operand2));
                                }
                                else if (op == TokenType.VAR)
                                {
                                    //declare a variable in the environment
                                    var varname = operand2.CastTo<string>();
                                    if (CurrentScope.Contain(varname))
                                    {
                                        RuntimeError($"{varname} is already defined");
                                    }
                                    CurrentScope.Define(varname, ResolveStackValue(operand1).Value);
                                    Push(ResolveStackValue(operand2));
                                }
                                else
                                {
                                    EvaluateBinaryOperation(operand1, operand2, op);
                                }
                            }
                            else
                            {
                                return ResolveStackValue(operand1);
                            }
                        }
                    }
                    else
                    {
                        return ResolveStackValue(operand1);
                    }
                }
            }

            return ResolveStackValue(EvaluationStack.Pop());
            #endregion
        }

        public void Execute(ASTNode program)
        {
            StackValue v;
            switch (program)
            {
                case VariableDeclareStatement varDecl:
                    {
                        v = Evaluate(varDecl);
                        Console.WriteLine($"var {varDecl.ident.token.lexeme} <- {Stringify(v)}");
                    }
                    break;
                case ExpressionStatement expr:
                    {
                        v = Evaluate(expr);
                        if (expr.Expression is AST.Assignment)
                        {
                            var ass = expr.Expression as Assignment;
                            //Console.WriteLine($"{ass.ident.token.lexeme} <- {Stringify(v)}");
                        }
                        Console.WriteLine(Stringify(v));
                    }
                    break;
                case Block block:
                    {
                        foreach (var stmt in block.Statements)
                        {
                            Execute(stmt);
                        }
                    }
                    break;
                case IfStatement ifStmt:
                    {
                        var condition = Evaluate(ifStmt.condition);
                        if (Truthify(condition))
                        {
                            Execute(ifStmt.ifBody);
                        }
                        else if (ifStmt.elseBody != null)
                        {
                            Execute(ifStmt.elseBody);
                        }
                    }
                    break;
                case WhileStatement whileStmt:
                    {
                        while (Truthify(Evaluate(whileStmt.condition)))
                        {
                            Execute(whileStmt.body);
                            if (breakFlag)
                            {
                                break;
                            }
                        }
                    }
                    break;
                case MatchStatement matchStmt:
                    {
                        var value = Evaluate(matchStmt.expression);
                        var matchedCase = matchStmt.matchCases.FirstOrDefault(mc => mc.Type == value.Type);
                        if (matchedCase != null)
                        {
                            Execute(matchedCase.Statement);
                        }
                        else
                        {
                            if (matchStmt.defaultCase != null)
                            {
                                Execute(matchStmt.defaultCase);
                            }
                        }
                    }
                    break;
                case ReturnStatement retStmt:
                    {
                        if (isInFunction)
                        {
                            //exit function or something, idk
                            isInFunction = false;
                        }
                        //return from main program;
                        //Console.WriteLine()
                        var retValue = Evaluate(retStmt.ReturnValue);
                        //Console.WriteLine(Stringify(retValue));
                        Push(retValue);
                    }
                    break;
                case FunctionDeclaration funcDecl:
                    {
                        //if (Functions.FirstOrDefault(f => f.Name == funcDecl.Name) != null)
                        if (CurrentScope.Contain(funcDecl.Name))
                        {
                            throw new Exception($"function {funcDecl} is already defined");
                        }
                        NativeFunction natFunc = new NativeFunction(funcDecl.Name, funcDecl);
                        //ns.Add(natFunc);
                        CurrentScope.Define(funcDecl.Name, natFunc);
                    }
                    break;
                //case FunctionCall functionCall:
                //    {
                //        //lookup a function
                //        //throw undefine if function is not found

                //        //define function parameter in the scope

                //        //undefine funciton parameter from the scope

                //        //assign return value
                //    }
                //    break;
                default:
                    break;
            }
        }

        private bool Truthify(StackValue value)
        {
            switch (value.Type)
            {
                case ValType.Null:
                    return false;

                case ValType.Bool:
                    return value.CastTo<bool>();

                case ValType.Identifier:
                    return Truthify(ResolveStackValue(value));

                default:
                    return true;
            }
        }

        private string Stringify(StackValue value)
        {
            switch (value.Type)
            {
                case ValType.Null:
                    return "null";

                case ValType.Integer:
                case ValType.Float:
                case ValType.Bool:
                case ValType.Operator:
                case ValType.Type:
                    return value.Value.ToString();

                case ValType.Char:
                    return $"'{value.Value}'";

                case ValType.String:
                    return $"\"{value.Value}\"";

                case ValType.Identifier:
                    return Stringify(ResolveStackValue(value));

                default:
                    return value.Value.ToString();
            }
        }
    }
}

