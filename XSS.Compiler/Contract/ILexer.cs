namespace XSS.Compiler.Contract
{
    interface ILexer
    {
        int CurrentLine { get; }
        string CurrentLineSource { get; }
        int CurrentPosInLine { get; }

        void Error();
        Token GetNextToken();
        Token PeekNextToken();
    }
}