using System;
using System.IO;

public class Buffer
{
    private Pos _pos;
    public StreamReader file;
    protected char? _back;
    protected char _cur;

    private Pos _curPos = new Pos();
    
    public Pos Position { get => _pos; set => _pos = value; }
    public Pos PositionCur { get => _curPos; set => _curPos = value; }

    protected char Peek()
    {
        return (char) file.Peek();
    }
    
    protected void Back()
    {
        _back = _cur;
    }
    
    protected void GetNext()
    {
        if (_back != null)
        {
            _cur = (char) _back;
            _back = null;
        }
        else if (!file.EndOfStream)
        {
            _cur = (char)file.Read();
            if (_cur == '\n')
            {
                _curPos.Column = 0;
                _curPos.Line++;
            }
            else
            {
                _curPos.Column++;
            }
        }
        else
        {
            _cur = '\0';
        }
    }
    
    public struct Pos
    {
        public int Line = 1;
        public int Column = 0;

        public Pos()
        {
        }
    }
}