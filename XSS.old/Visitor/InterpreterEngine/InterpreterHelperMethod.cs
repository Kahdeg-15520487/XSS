namespace XSS.Visitor.InterpreterEngine
{
    static class InterpreterHelperMethod
    {
        public static T CastTo<T>(this IValue value)
        {
            return (T)value.Value;
        }
    }
}
