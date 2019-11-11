using XSS.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSS.Visitor.InterpreterEngine;

namespace XSS
{
    static class ExtensionMethod
    {
        public static bool IsIdent(this char c)
        {
            // Return true if the character is between 0 or 9 inclusive or is an uppercase or
            // lowercase letter or underscore

            return ((c >= '0' && c <= '9') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= 'a' && c <= 'z') ||
                     c == '_' ||
                     c == '$' ||
                     c == ':');
        }

        public static bool IsNumeric(this char c)
        {
            return (c >= '0' && c <= '9');
        }

        public static bool IsHexNumeric(this char c)
        {
            return IsNumeric(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
        }

        public static bool IsWhiteSpace(this char c)
        {
            return c == '\t' || c == ' ';
        }

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static ValType ToValType(this string lexeme)
        {
            switch (lexeme)
            {
                case "INT":
                case "int":
                    return ValType.Integer;
                case "FLT":
                case "flt":
                    return ValType.Float;
                case "CHR":
                case "chr":
                    return ValType.Char;
                case "STR":
                case "str":
                    return ValType.String;
                case "BOOL":
                case "bool":
                    return ValType.Bool;
                default:
                    return ValType.Null;
            }
        }

        public static Operator ToOperator(this TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.PLUS:
                    return Operator.Plus;
                case TokenType.MINUS:
                    return Operator.Subtract;
                case TokenType.MULTIPLY:
                    return Operator.Multiply;
                case TokenType.DIVIDE:
                    return Operator.Divide;
                case TokenType.MODULO:
                    return Operator.Modulo;
                case TokenType.EXPONENT:
                    return Operator.Exponent;
                case TokenType.ASSIGN:
                    return Operator.Assign;
                case TokenType.AND:
                    return Operator.And;
                case TokenType.OR:
                    return Operator.Or;
                case TokenType.XOR:
                    return Operator.Xor;
                case TokenType.NOT:
                    return Operator.Not;
                case TokenType.EQUAL:
                    return Operator.Equal;
                case TokenType.NOTEQUAL:
                    return Operator.NotEqual;
                case TokenType.LARGER:
                    return Operator.Larger;
                case TokenType.LARGEREQUAL:
                    return Operator.LargerOrEqual;
                case TokenType.LESSER:
                    return Operator.Lesser;
                case TokenType.LESSEREQUAL:
                    return Operator.LesserOrEqual;
                case TokenType.IS:
                    return Operator.Is;
                case TokenType.TYPEOF:
                    return Operator.TypeOf;
                case TokenType.VAR:
                    return Operator.DefineVar;

                default:
                    throw new Exception($"unknown operator {tokenType}");
            }
        }
    }
}
