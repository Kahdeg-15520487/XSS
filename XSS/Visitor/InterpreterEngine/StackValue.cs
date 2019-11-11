using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSS.AST;

namespace XSS.Visitor.InterpreterEngine
{
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
}
