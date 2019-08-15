using System;
using System.Collections.Generic;
using System.Linq;
using XSS.AST;
using XSS.Utility;

namespace XSS
{
    enum Operator
    {
        //arthimetic
        Plus,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Exponent,

        //assignment
        Assign,

        //define var
        DefineVar,

        //bitwise
        And,
        Or,
        Xor,
        Not,

        //comparative
        Equal,
        NotEqual,
        Larger,
        LargerOrEqual,
        Lesser,
        LesserOrEqual,

        //type
        Is,
        TypeOf,

        //scope
        EnterScope,
        ExitScope
    }

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
            this.Type = type;
            this.Value = value;
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
                    return new StackValue(ValType.Operator, token.type.ToOperator());
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
                hash = hash * 71 + this.Type.GetHashCode();
                hash = hash * 71 + this.Value.GetHashCode();
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
                    case ValType.Null:
                        return "null";
                    default:
                        throw new Exception($"unknown value type {t}");
                }
            }
            return "<" + this.Type + " : " + representation(this.Type, this.Value) + ">";
        }

    }
    #endregion

    #region function

    class NativeFunction : IFunction
    {
        public NativeFunction(string name, FunctionDeclaration funcDecl)
        {
            //Function = FunctionRunner(funcDecl);
            this.Name = name;
            this.FunctionDeclaration = funcDecl;
        }

        public Func<IValue[], IValue> Function { get; private set; }
        public string Name { get; private set; }

        public FunctionDeclaration FunctionDeclaration { get; private set; }
        public FunctionSignature FunctionSignature => this.FunctionDeclaration.FunctionSignature;

        public override int GetHashCode()
        {
            unchecked
            {
                return this.FunctionDeclaration.GetHashCode();
            }
        }

        public bool IsCompatibleWith(FunctionSignature funsig)
        {
            return this.FunctionSignature.GetHashCode() == funsig.GetHashCode();
        }

        public override string ToString()
        {
            return this.FunctionDeclaration.Value();
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
            this.Global = new Scope();
            this.CurrentScope = this.Global;
            this.EvaluationStack = new Stack<StackValue>();
        }
        public Interpreter(Scope environment)
        {
            this.Global = environment;
            this.CurrentScope = this.Global;
            this.EvaluationStack = new Stack<StackValue>();
        }

        #region visitor
        public void Visit(BinaryOperation binop)
        {
            this.Push(StackValue.CreateStackValue(binop.op));
            binop.rightnode.Accept(this);
            binop.leftnode.Accept(this);
        }

        public void Visit(UnaryOperation unop)
        {
            this.Push(StackValue.CreateStackValue(unop.op));
            unop.operand.Accept(this);
        }

        public void Visit(Operand op)
        {
            var tttt = StackValue.CreateStackValue(op.token);
            this.Push(tttt);
        }

        public void Visit(Assignment ass)
        {
            this.Push(StackValue.CreateStackValue(new Token(TokenType.ASSIGN, "=")));
            ass.ident.Accept(this);
            ass.expression.Accept(this);
        }

        public void Visit(VariableDeclareStatement vardecl)
        {
            this.Push(StackValue.CreateStackValue(new Token(TokenType.VAR)));

            vardecl.ident.Accept(this);

            if (vardecl.init == null)
            {
                this.Push(StackValue.Null);
            }
            else
            {
                vardecl.init.Accept(this);
            }
        }

        public void Visit(ExpressionStatement exprstmt)
        {
            exprstmt.Expression.Accept(this);
        }

        public void Visit(FunctionCall functionCall)
        {
            this.Push(new StackValue(ValType.Operator, Operator.ExitScope));
            this.Push(StackValue.CreateStackValue(functionCall));
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
        void BinaryRuntimeError(StackValue operand1, StackValue operand2, Operator op, string message = "")
        {
            this.RuntimeError("Undefined behaviour :" + operand1 + " " + op + " " + operand2 + "\n" + message);
        }
        void UnaryRuntimeError(StackValue operand, Operator op, string message = "")
        {
            this.RuntimeError("Undefined behaviour :" + op + " " + operand + "\n" + message);
        }

        #region stack manipulation
        private void ReverseStack()
        {
            Stack<StackValue> temp = new Stack<StackValue>();

            while (this.EvaluationStack.Count > 0)
            {
                temp.Push(this.EvaluationStack.Pop());
            }

            this.EvaluationStack = temp;
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
                    return this.CurrentScope.Get((string)value.Value);
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
                if (!this.CurrentScope.Contain(varname))
                {
                    this.RuntimeError($"{varname} is not defined");
                }
                var value = this.CurrentScope.Get(varname);
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
            this.EvaluationStack.Push(stackValue);
        }

        private void Push(int value)
        {
            this.EvaluationStack.Push(new StackValue(ValType.Integer, value));
        }

        private void Push(float value)
        {
            this.EvaluationStack.Push(new StackValue(ValType.Float, value));
        }

        private void Push(bool value)
        {
            this.EvaluationStack.Push(new StackValue(ValType.Bool, value));
        }

        private void Push(char value)
        {
            this.EvaluationStack.Push(new StackValue(ValType.Char, value));
        }

        private void Push(string value)
        {
            this.EvaluationStack.Push(new StackValue(ValType.String, value));
        }

        private void Push(ValType value)
        {
            this.EvaluationStack.Push(new StackValue(ValType.Type, value));
        }

        #endregion

        private StackValue Evaluate(ASTNode Expression)
        {
            #region operation
            int IntegerOperation(StackValue operand1, StackValue operand2, Operator op)
            {
                int i1 = (int)operand1.Value;
                int i2 = (int)operand2.Value;
                switch (op)
                {
                    case Operator.Plus:
                        return i1 + i2;
                    case Operator.Subtract:
                        return i1 - i2;
                    case Operator.Multiply:
                        return i1 * i2;
                    case Operator.Divide:
                        return i1 / i2;
                    case Operator.Modulo:
                        return i1 % i2;
                }

                return 0;
            }
            float FloatOperation(StackValue operand1, StackValue operand2, Operator op)
            {
                float f1 = operand1.Value is float ? (float)operand1.Value : (int)operand1.Value;
                float f2 = operand2.Value is float ? (float)operand2.Value : (int)operand2.Value;
                switch (op)
                {
                    case Operator.Plus:
                        return f1 + f2;
                    case Operator.Subtract:
                        return f1 - f2;
                    case Operator.Multiply:
                        return f1 * f2;
                    case Operator.Divide:
                        return f1 / f2;
                    case Operator.Exponent:
                        return (float)Math.Pow(f1, f2);
                }

                return 0;
            }
            string StringOperation(StackValue operand1, StackValue operand2, Operator op)
            {
                string s1 = operand1.Value.ToString();
                string s2 = operand2.Value.ToString();

                if (op == Operator.Plus)
                {
                    return s1 + s2;
                }

                return "";
            }
            bool BoolOperation(StackValue operand1, StackValue operand2, Operator op)
            {
                bool b1 = (bool)operand1.Value;
                bool b2 = op == Operator.Not ? true : (bool)operand2.Value;

                switch (op)
                {
                    case Operator.And:
                        return b1 && b2;
                    case Operator.Or:
                        return b1 || b2;
                    case Operator.Xor:
                        return b1 ^ b2;
                    case Operator.Not:
                        return !b1;
                }

                return false;
            }
            bool IntComparison(StackValue operand1, StackValue operand2, Operator op)
            {
                int i1 = (int)operand1.Value;
                int i2 = (int)operand2.Value;
                switch (op)
                {
                    case Operator.Equal:
                        return i1 == i2;
                    case Operator.NotEqual:
                        return i1 != i2;
                    case Operator.Larger:
                        return i1 > i2;
                    case Operator.LargerOrEqual:
                        return i1 >= i2;
                    case Operator.Lesser:
                        return i1 < i2;
                    case Operator.LesserOrEqual:
                        return i1 <= i2;
                }
                return false;
            }
            bool FloatComparison(StackValue operand1, StackValue operand2, Operator op)
            {
                float f1 = (float)operand1.Value;
                float f2 = (float)operand2.Value;
                switch (op)
                {
                    case Operator.Equal:
                        return f1 == f2;
                    case Operator.NotEqual:
                        return f1 != f2;
                    case Operator.Larger:
                        return f1 > f2;
                    case Operator.LargerOrEqual:
                        return f1 >= f2;
                    case Operator.Lesser:
                        return f1 < f2;
                    case Operator.LesserOrEqual:
                        return f1 <= f2;
                }
                return false;
            }
            bool CharComparison(StackValue operand1, StackValue operand2, Operator op)
            {
                char c1 = (char)operand1.Value;
                char c2 = (char)operand2.Value;
                switch (op)
                {
                    case Operator.Equal:
                        return c1 == c2;
                    case Operator.NotEqual:
                        return c1 != c2;
                    case Operator.Larger:
                        return c1 > c2;
                    case Operator.LargerOrEqual:
                        return c1 >= c2;
                    case Operator.Lesser:
                        return c1 < c2;
                    case Operator.LesserOrEqual:
                        return c1 <= c2;
                }
                return false;
            }
            bool StringComparison(StackValue operand1, StackValue operand2, Operator op)
            {
                string s1 = (string)operand1.Value;
                string s2 = (string)operand2.Value;
                var comparison = s1.CompareTo(s2);
                switch (op)
                {
                    case Operator.Equal:
                        return s1.Equals(s2);
                    case Operator.NotEqual:
                        return !s1.Equals(s2);
                    case Operator.Larger:
                        return comparison > 0;
                    case Operator.LargerOrEqual:
                        return comparison >= 0;
                    case Operator.Lesser:
                        return comparison < 0;
                    case Operator.LesserOrEqual:
                        return comparison <= 0;
                }
                return false;
            }
            bool BoolComparison(StackValue operand1, StackValue operand2, Operator op)
            {
                bool b1 = (bool)operand1.Value;
                bool b2 = (bool)operand2.Value;
                switch (op)
                {
                    case Operator.Equal:
                        return b1 == b2;
                    case Operator.NotEqual:
                        return b1 != b2;
                }
                return false;
            }
            bool Comparison(StackValue operand1, StackValue operand2, Operator op)
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
                                case Operator.Equal:
                                    result = i1 == i2;
                                    break;
                                case Operator.NotEqual:
                                    result = i1 != i2;
                                    break;
                                case Operator.Larger:
                                    result = i1 > i2;
                                    break;
                                case Operator.LargerOrEqual:
                                    result = i1 >= i2;
                                    break;
                                case Operator.Lesser:
                                    result = i1 < i2;
                                    break;
                                case Operator.LesserOrEqual:
                                    result = i1 <= i2;
                                    break;
                            }
                        }
                        else if (operand2.Type == ValType.Char)
                        {
                            //do int comparison
                            int i2 = (char)operand2.Value;
                            switch (op)
                            {
                                case Operator.Equal:
                                    result = i1 == i2;
                                    break;
                                case Operator.NotEqual:
                                    result = i1 != i2;
                                    break;
                                case Operator.Larger:
                                    result = i1 > i2;
                                    break;
                                case Operator.LargerOrEqual:
                                    result = i1 >= i2;
                                    break;
                                case Operator.Lesser:
                                    result = i1 < i2;
                                    break;
                                case Operator.LesserOrEqual:
                                    result = i1 <= i2;
                                    break;
                            }
                        }
                        else
                        {
                            this.BinaryRuntimeError(operand1, operand2, op);
                        }
                        break;

                    case ValType.Float:
                        float f1 = (float)operand1.Value;
                        if (operand2.Type == ValType.Integer)
                        {
                            //do flt math
                            float f2 = (int)operand2.Value;
                            switch (op)
                            {
                                case Operator.Equal:
                                    result = f1 == f2;
                                    break;
                                case Operator.NotEqual:
                                    result = f1 != f2;
                                    break;
                                case Operator.Larger:
                                    result = f1 > f2;
                                    break;
                                case Operator.LargerOrEqual:
                                    result = f1 >= f2;
                                    break;
                                case Operator.Lesser:
                                    result = f1 < f2;
                                    break;
                                case Operator.LesserOrEqual:
                                    result = f1 <= f2;
                                    break;
                            }
                        }
                        else
                        {
                            this.BinaryRuntimeError(operand1, operand2, op);
                        }
                        break;
                    case ValType.Char:
                        int c1 = (char)operand1.Value;
                        if (operand2.Type == ValType.Integer)
                        {
                            //do int comparison
                            int c2 = (char)operand2.Value;
                            switch (op)
                            {
                                case Operator.Equal:
                                    result = c1 == c2;
                                    break;
                                case Operator.NotEqual:
                                    result = c1 != c2;
                                    break;
                                case Operator.Larger:
                                    result = c1 > c2;
                                    break;
                                case Operator.LargerOrEqual:
                                    result = c1 >= c2;
                                    break;
                                case Operator.Lesser:
                                    result = c1 < c2;
                                    break;
                                case Operator.LesserOrEqual:
                                    result = c1 <= c2;
                                    break;
                            }
                        }
                        else
                        {
                            this.BinaryRuntimeError(operand1, operand2, op);
                        }
                        break;

                    case ValType.Bool:
                        //cause there is no boolean comparison operator with mixed type
                        this.BinaryRuntimeError(operand1, operand2, op);
                        break;

                    case ValType.String:
                        //cause there is no string comparison operator with mixed type
                        this.BinaryRuntimeError(operand1, operand2, op);
                        break;

                    case ValType.Null:
                        this.BinaryRuntimeError(operand1, operand2, op);
                        break;
                }
                return result;
            }
            bool TypeTesting(StackValue operand1, StackValue operand2, Operator op)
            {
                ValType type = operand2.CastTo<ValType>();

                switch (op)
                {
                    case Operator.Equal:
                    case Operator.NotEqual:
                        if (operand1.Type != ValType.Type)
                        {
                            return false;
                        }
                        else
                        {
                            var type2 = operand1.CastTo<ValType>();
                            return op == Operator.Equal ? type == type2 : !(type == type2);
                        }
                    case Operator.Is:
                        // <value> is <Type>
                        return operand1.Type == type;
                }

                return false;
            }
            #endregion

            #region evaluate expression
            void EvaluateBinaryOperation(StackValue operand1, StackValue operand2, Operator op)
            {
                var v1 = this.ResolveStackValue(operand1);
                var v2 = this.ResolveStackValue(operand2);
                if (v1.Type == v2.Type)
                {
                    switch (v1.Type)
                    {
                        case ValType.Integer:
                            switch (op)
                            {
                                case Operator.Plus:
                                case Operator.Subtract:
                                case Operator.Multiply:
                                case Operator.Divide:
                                case Operator.Modulo:
                                    this.Push(IntegerOperation(v1, v2, op));
                                    break;
                                case Operator.Exponent:
                                    this.Push(FloatOperation(v1, v2, Operator.Exponent));
                                    break;
                                case Operator.Equal:
                                case Operator.NotEqual:
                                case Operator.Larger:
                                case Operator.LargerOrEqual:
                                case Operator.Lesser:
                                case Operator.LesserOrEqual:
                                    this.Push(IntComparison(v1, v2, op));
                                    break;

                                default:
                                    this.BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;

                        case ValType.Float:
                            switch (op)
                            {
                                case Operator.Plus:
                                case Operator.Subtract:
                                case Operator.Multiply:
                                case Operator.Divide:
                                case Operator.Exponent:
                                    this.Push(FloatOperation(v1, v2, op));
                                    break;
                                case Operator.Equal:
                                case Operator.NotEqual:
                                case Operator.Larger:
                                case Operator.LargerOrEqual:
                                case Operator.Lesser:
                                case Operator.LesserOrEqual:
                                    this.Push(FloatComparison(v1, v2, op));
                                    break;

                                default:
                                    this.BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;

                        case ValType.Bool:
                            switch (op)
                            {
                                case Operator.And:
                                case Operator.Or:
                                case Operator.Xor:
                                    this.Push(BoolOperation(v1, v2, op));
                                    break;
                                case Operator.Equal:
                                case Operator.NotEqual:
                                    this.Push(BoolComparison(v1, v2, op));
                                    break;

                                default:
                                    this.BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;

                        case ValType.Char:
                            //concat char
                            switch (op)
                            {
                                case Operator.Plus:
                                    this.Push(StringOperation(v1, v2, Operator.Plus));
                                    break;

                                case Operator.Equal:
                                case Operator.NotEqual:
                                case Operator.Larger:
                                case Operator.LargerOrEqual:
                                case Operator.Lesser:
                                case Operator.LesserOrEqual:
                                    this.Push(CharComparison(v1, v2, op));
                                    break;

                                default:
                                    this.BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;

                        case ValType.String:
                            //concat string
                            switch (op)
                            {
                                case Operator.Plus:
                                    this.Push(StringOperation(v1, v2, Operator.Plus));
                                    break;

                                case Operator.Equal:
                                case Operator.NotEqual:
                                case Operator.Larger:
                                case Operator.LargerOrEqual:
                                case Operator.Lesser:
                                case Operator.LesserOrEqual:
                                    this.Push(StringComparison(v1, v2, op));
                                    break;

                                default:
                                    this.BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;
                        case ValType.Null:
                            this.BinaryRuntimeError(v1, v2, op);
                            break;
                        case ValType.Type:
                            switch (op)
                            {
                                case Operator.Equal:
                                case Operator.NotEqual:
                                    this.Push(TypeTesting(v1, v2, op));
                                    break;
                                default:
                                    this.BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;

                        default:
                            switch (op)
                            {
                                case Operator.Is:
                                    this.Push(TypeTesting(v1, v2, op));
                                    break;
                                default:
                                    this.BinaryRuntimeError(v1, v2, op);
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
                                    case Operator.Plus:
                                    case Operator.Subtract:
                                    case Operator.Multiply:
                                    case Operator.Divide:
                                    case Operator.Exponent:
                                        this.Push(FloatOperation(v1, v2, op));
                                        break;

                                    case Operator.Equal:
                                    case Operator.NotEqual:
                                    case Operator.Larger:
                                    case Operator.LargerOrEqual:
                                    case Operator.Lesser:
                                    case Operator.LesserOrEqual:
                                        this.Push(Comparison(v1, v2, op));
                                        break;
                                }
                            }
                            else if (v2.Type == ValType.Char)
                            {
                                //do int math
                                switch (op)
                                {
                                    case Operator.Plus:
                                    case Operator.Subtract:
                                    case Operator.Multiply:
                                    case Operator.Divide:
                                        this.Push(IntegerOperation(v1, v2, op));
                                        break;
                                    case Operator.Exponent:
                                        this.Push(FloatOperation(v1, v2, Operator.Exponent));
                                        break;

                                    case Operator.Equal:
                                    case Operator.NotEqual:
                                    case Operator.Larger:
                                    case Operator.LargerOrEqual:
                                    case Operator.Lesser:
                                    case Operator.LesserOrEqual:
                                        this.Push(Comparison(v1, v2, op));
                                        break;
                                }
                            }
                            else if (op == Operator.Is)
                            {
                                this.Push(TypeTesting(v1, v2, op));
                            }
                            else
                            {
                                this.BinaryRuntimeError(v1, v2, op);
                            }
                            break;

                        case ValType.Float:
                            if (v2.Type == ValType.Integer)
                            {
                                //do flt math
                                switch (op)
                                {
                                    case Operator.Plus:
                                    case Operator.Subtract:
                                    case Operator.Multiply:
                                    case Operator.Divide:
                                    case Operator.Exponent:
                                        this.Push(FloatOperation(v1, v2, op));
                                        break;

                                    case Operator.Equal:
                                    case Operator.NotEqual:
                                    case Operator.Larger:
                                    case Operator.LargerOrEqual:
                                    case Operator.Lesser:
                                    case Operator.LesserOrEqual:
                                        this.Push(Comparison(v1, v2, op));
                                        break;
                                }
                            }
                            else if (op == Operator.Is)
                            {
                                this.Push(TypeTesting(v1, v2, op));
                            }
                            else
                            {
                                this.BinaryRuntimeError(v1, v2, op);
                            }
                            break;
                        case ValType.Char:
                            switch (op)
                            {

                                case Operator.Equal:
                                case Operator.NotEqual:
                                case Operator.Larger:
                                case Operator.LargerOrEqual:
                                case Operator.Lesser:
                                case Operator.LesserOrEqual:
                                    this.Push(Comparison(v1, v2, op));
                                    break;
                                case Operator.Is:
                                    this.Push(TypeTesting(v1, v2, op));
                                    break;
                                default:
                                    this.BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;

                        case ValType.Bool:
                            if (op == Operator.Is)
                            {
                                this.Push(TypeTesting(v1, v2, op));
                            }
                            else
                            {
                                this.BinaryRuntimeError(v1, v2, op);
                            }
                            break;

                        case ValType.String:
                            //concat string
                            if (op == Operator.Plus)
                            {
                                this.Push(StringOperation(v1, v2, Operator.Plus));
                            }
                            else if (op == Operator.Is)
                            {
                                this.Push(TypeTesting(v1, v2, op));
                            }
                            else if (op == Operator.Is)
                            {
                                this.Push(TypeTesting(v1, v2, op));
                            }
                            else
                            {
                                this.BinaryRuntimeError(v1, v2, op);
                            }
                            break;

                        default:
                            switch (op)
                            {
                                case Operator.Is:
                                    this.Push(TypeTesting(v1, v2, op));
                                    break;
                                default:
                                    this.BinaryRuntimeError(v1, v2, op);
                                    break;
                            }
                            break;
                    }
                }
            }
            void EvaluateUnaryOperation(StackValue operand, Operator op)
            {
                var v = this.ResolveStackValue(operand);

                switch (op)
                {
                    case Operator.Subtract:
                        if (v.Type == ValType.Integer)
                        {
                            int i = v.CastTo<int>();
                            this.Push(-i);
                        }
                        else if (v.Type == ValType.Float)
                        {
                            float f = v.CastTo<float>();
                            this.Push(-f);
                        }
                        else
                        {
                            this.UnaryRuntimeError(operand, op);
                        }
                        break;
                    case Operator.Not:
                        if (v.Type == ValType.Bool)
                        {
                            bool b = v.CastTo<bool>();
                            this.Push(!b);
                        }
                        break;
                    case Operator.TypeOf:
                        this.Push(v.Type);
                        break;
                }
            }
            void EvaluateFunctionCall(FunctionCall functionCall)
            {
                this.isInFunction = true;

                //resolve all parameters
                List<(ValType type, object value)> parameters = new List<(ValType type, object value)>();
                foreach (var parameter in functionCall.Parameters)
                {
                    StackValue temp;
                    switch (parameter)
                    {
                        case Operand operand:
                            {
                                temp = this.ResolveStackValue(StackValue.CreateStackValue(operand.token));
                            }
                            break;

                        case BinaryOperation binop:
                            {
                                temp = this.Evaluate(binop);
                            }
                            break;
                        case UnaryOperation unaop:
                            {
                                temp = this.Evaluate(unaop);
                            }
                            break;
                        case FunctionCall funcCall:
                            {
                                temp = this.Evaluate(funcCall);
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
                this.CurrentScope.Contain(functionCall.FunctionName);
                var function = this.CurrentScope.Get(functionCall.FunctionName) as NativeFunction;

                var localScope = new Scope(this.CurrentScope);

                for (int i = 0; i < parameters.Count; i++)
                {
                    localScope.Define(function.FunctionDeclaration.ParameterNames[i], parameters[i].value);
                }

                this.CurrentScope = localScope;

                this.Execute(function.FunctionDeclaration.Body);

                this.CurrentScope = this.Global;

                this.isInFunction = false;
            }
            #endregion

            #region visit the astnode
            Expression.Accept(this);
            #endregion

            #region process operand
            if (this.EvaluationStack.Count == 0)
            {
                return StackValue.Null;
            }

            //ReverseStack();
            while (this.EvaluationStack.Count > 0)
            {
                StackValue operand1 = this.EvaluationStack.Pop();
                StackValue operand2;
                Operator op;
                if (operand1.Type == ValType.FunctionCall)
                {
                    EvaluateFunctionCall((FunctionCall)operand1.Value);
                }
                else
                {
                    if (this.EvaluationStack.Count > 0)
                    {
                        operand2 = this.EvaluationStack.Pop();
                        if (operand2.Type == ValType.Operator)
                        {
                            //unary opearation
                            op = operand2.CastTo<Operator>();
                            if (op == Operator.ExitScope)
                            {
                                //check if current scope is global
                                //if (!isInFunction)
                                if (!this.CurrentScope.IsGlobal)
                                {
                                    //throw new Exception("can't exit from current scope, current scope is global");
                                    //exit current scope
                                    this.CurrentScope = this.CurrentScope.parent;
                                }
                                this.Push(operand1);
                            }
                            else
                            {
                                EvaluateUnaryOperation(operand1, op);
                            }
                        }
                        else
                        {
                            if (this.EvaluationStack.Count > 0)
                            {
                                op = (Operator)this.EvaluationStack.Pop().Value;

                                if (op == Operator.Assign)
                                {
                                    //do assignment
                                    //todo check type
                                    var varname = (string)operand2.Value;
                                    if (!this.CurrentScope.Contain(varname))
                                    {
                                        this.RuntimeError($"{varname} is Not defined");
                                    }
                                    this.CurrentScope.Assign(varname, this.ResolveStackValue(operand1).Value);
                                    this.Push(this.ResolveStackValue(operand2));
                                }
                                else if (op == Operator.DefineVar)
                                {
                                    //declare a variable in the environment
                                    var varname = operand2.CastTo<string>();
                                    if (this.CurrentScope.Contain(varname))
                                    {
                                        this.RuntimeError($"{varname} is already defined");
                                    }
                                    this.CurrentScope.Define(varname, this.ResolveStackValue(operand1).Value);
                                    this.Push(this.ResolveStackValue(operand2));
                                }
                                else
                                {
                                    EvaluateBinaryOperation(operand1, operand2, op);
                                }
                            }
                            else
                            {
                                return this.ResolveStackValue(operand1);
                            }
                        }
                    }
                    else
                    {
                        return this.ResolveStackValue(operand1);
                    }
                }
            }

            return this.ResolveStackValue(this.EvaluationStack.Pop());
            #endregion
        }

        public void Execute(ASTNode program)
        {
            StackValue v;
            switch (program)
            {
                case VariableDeclareStatement varDecl:
                    {
                        v = this.Evaluate(varDecl);
                        Console.WriteLine($"var {varDecl.ident.token.lexeme} <- {this.Stringify(v)}");
                    }
                    break;
                case ExpressionStatement expr:
                    {
                        v = this.Evaluate(expr);
                        if (expr.Expression is AST.Assignment)
                        {
                            var ass = expr.Expression as Assignment;
                            Console.WriteLine($"{ass.ident.token.lexeme} <- {this.Stringify(v)}");
                        }
                        Console.WriteLine(this.Stringify(v));
                    }
                    break;
                case Block block:
                    {
                        foreach (var stmt in block.Statements)
                        {
                            this.Execute(stmt);
                        }
                    }
                    break;
                case IfStatement ifStmt:
                    {
                        var condition = this.Evaluate(ifStmt.condition);
                        if (this.Truthify(condition))
                        {
                            this.Execute(ifStmt.ifBody);
                        }
                        else if (ifStmt.elseBody != null)
                        {
                            this.Execute(ifStmt.elseBody);
                        }
                    }
                    break;
                case WhileStatement whileStmt:
                    {
                        while (this.Truthify(this.Evaluate(whileStmt.condition)))
                        {
                            this.Execute(whileStmt.body);
                            if (this.breakFlag)
                            {
                                break;
                            }
                        }
                    }
                    break;
                case MatchStatement matchStmt:
                    {
                        var value = this.Evaluate(matchStmt.expression);
                        var matchedCase = matchStmt.matchCases.FirstOrDefault(mc => mc.Type == value.Type);
                        if (matchedCase != null)
                        {
                            this.Execute(matchedCase.Statement);
                        }
                        else
                        {
                            if (matchStmt.defaultCase != null)
                            {
                                this.Execute(matchStmt.defaultCase);
                            }
                        }
                    }
                    break;
                case ReturnStatement retStmt:
                    {
                        //return from main program;
                        //Console.WriteLine()
                        var retValue = this.Evaluate(retStmt.ReturnValue);
                        //Console.WriteLine(Stringify(retValue));
                        this.Push(retValue);

                        if (this.isInFunction)
                        {
                            //exit function or something, idk
                            this.isInFunction = false;
                        }
                    }
                    break;
                case FunctionDeclaration funcDecl:
                    {
                        //if (Functions.FirstOrDefault(f => f.Name == funcDecl.Name) != null)
                        if (this.CurrentScope.Contain(funcDecl.Name))
                        {
                            throw new Exception($"function {funcDecl} is already defined");
                        }
                        NativeFunction natFunc = new NativeFunction(funcDecl.Name, funcDecl);
                        //ns.Add(natFunc);
                        this.CurrentScope.Define(funcDecl.Name, natFunc);
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
                    return this.Truthify(this.ResolveStackValue(value));

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
                    return this.Stringify(this.ResolveStackValue(value));

                default:
                    return value.Value.ToString();
            }
        }
    }
}

