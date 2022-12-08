using System;

namespace Lexer;

public class LexException : Exception
{
    public LexException(Buffer.Pos pos, string message)
        : base(pos.Line+"  "+pos.Column+"  "+message)
    { }

    public LexException(LexException exception)
        : base(string.Empty, exception)
    {
        throw new Exception(exception.Message);
    }
}

public class LexemeOverflowException : OverflowException
{
    public LexemeOverflowException(Buffer.Pos pos)
        : base(pos.Line+"  "+pos.Column+"  Type overflow")
    { }
}