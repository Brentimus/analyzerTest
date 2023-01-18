using System;

public class LexException : Exception
{
    public LexException(Buffer.Pos pos, string message)
        : base($"({pos.Line},{pos.Column}) {message}")
    { }
}
public class SyntaxException : Exception
{
    public SyntaxException(int Line, int Column, string message)
        : base($"({Line},{Column}) {message}")
    {
    }
}