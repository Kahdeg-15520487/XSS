namespace XSS.Visitor.InterpreterEngine
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
}
