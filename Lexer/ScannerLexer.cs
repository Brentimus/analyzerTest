using System;
using System.IO;

namespace Lexer;

public class ScannerLexer
{
    private char[] _sm = new char[1];
    private States _state;
    public Lex Lexeme;
    private StreamReader _sr;
    public int Line = 1;
    public int Column = 0;
    public int Pos;
    private static string _buf;
    private void SkipCommentSpace()
    {
        if (Peek() == '/' && _sm[0] == '/')
        {
            while (_sm[0] != '\n')
            {
                if (_sr.EndOfStream)
                {
                    throw new Exception("comment error");
                }
                GetNext();
            }
        } else if (Peek() == '*' && _sm[0] == '(')
        {
            while (true)
            {
                if (_sr.EndOfStream)
                {
                    throw new Exception("comment error");
                }
                GetNext();
                if (Peek() == ')' && _sm[0] == '*')
                {
                    GetNext();
                    GetNext();
                    break;
                }
            }
        } else if (_sm[0] == '{')
        {
            while (_sm[0] != '}')
            {
                if (_sr.EndOfStream)
                {
                    throw new Exception("comment error");
                }
                GetNext();
            }
            GetNext();
        }
        else
        {
            SkipSpace();
            return;
        }
        SkipSpace();
        SkipCommentSpace();
    }
    private void SkipSpace()
    {
        switch (_sm[0])
        {
            case '\n':
            case '\t':
            case ' ':
            case '\r':
            case '\0':
                GetNext();
                SkipSpace();
                break;
        }
    }
    private void GetNext()
    {
        if (!_sr.EndOfStream)
        {
            _sr.Read(_sm, 0, 1);
            if (_sm[0] == '\n')
            {
                Column = 0;
                Line++;
            }
            else
                Column++;
        }
        else
            _sm = new char[1];

    }
    private char Peek()
    {
        return (char) _sr.Peek();
    }
    
    private static void ClearBuf()
    {
        _buf = "";
    }

    private static void AddBuf(char symbol)
    {
        _buf += symbol;
    }

    private string SearchLex()
    {
        if (Enum.TryParse(_buf, true, out KeyWords keyWords))
        {
            return Enum.Parse(typeof(KeyWords), _buf, true).ToString().ToLower();
        }
        return "";
    }

    private void AddLex(LexType id, object value, string source)
    {
        Lexeme = new Lex(id, source, value, Line, Pos);
    }

    private bool IsDigit(char c, int baseNum)
    {
        return baseNum switch
        {
            10 => c is >= '0' and <= '9',
            16 => c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f',
            8 => c is >= '0' and <= '7',
            2 => c is >= '0' and <= '1',
            _ => throw new ArgumentOutOfRangeException("invalid integer")
        };
    }

    public Lex Scanner(StreamReader fileReader)
    {
        _sr = fileReader;
        GetNext();
        SkipSpace();
        SkipCommentSpace();
        Pos = Column;
        int baseNum = 10;
        ClearBuf();
        if (Char.IsLetter(_sm[0]))
        {
            AddBuf(_sm[0]);
            _state = States.Id;
        }
        else if (char.IsDigit(_sm[0]))
        {
            AddBuf(_sm[0]);
            _state = States.Num;
        }
        else
        {
            switch (_sm[0])
            {
                case '%':
                    baseNum = 2;
                    IsDigit(Peek(), baseNum);
                    _state = States.Num;
                    break;
                case '$':
                    baseNum = 16;
                    IsDigit(Peek(), baseNum);
                    _state = States.Num;
                    break;
                case '&':
                    baseNum = 8;
                    IsDigit(Peek(), baseNum);
                    _state = States.Num;
                    break;
                case '#':
                    _state = States.Chr;
                    break;
                case (char)39:
                    _state = States.Str;
                    break;
                case ';':
                    AddLex(LexType.Separator, LexValue.Semicolom, _sm[0].ToString());
                    break;
                case '=':
                    AddLex(LexType.Operator, LexValue.Equal, _sm[0].ToString());
                    break;
                case ',':
                    AddLex(LexType.Separator, LexValue.Comma, _sm[0].ToString());
                    break;
                case ')':
                    AddLex(LexType.Separator, LexValue.Rparen, _sm[0].ToString());
                    break;
                case '[':
                    AddLex(LexType.Separator, LexValue.Lbrack, _sm[0].ToString());
                    break;
                case ']':
                    AddLex(LexType.Separator, LexValue.Rbrack, _sm[0].ToString());
                    break;
                case '(':
                    AddLex(LexType.Separator, LexValue.Lparen, _sm[0].ToString());
                    break;
                case ':':
                    AddBuf(_sm[0]);
                    switch (Peek())
                    {
                        case '=':
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator, LexValue.Implicit, _buf);
                            break;
                        default:
                            AddLex(LexType.Separator, LexValue.Colon, _sm[0].ToString());
                            break;
                    }
                    break;
                case '<':
                    AddBuf(_sm[0]);
                    switch (Peek())
                    {
                        case '>':
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator, LexValue.NoEqual, _buf);
                            break;
                        case '=':
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator, LexValue.LessEqual, _buf);
                            break;
                        default:
                            AddLex(LexType.Operator, LexValue.Less, _sm[0].ToString());
                            break;
                    }
                    break;
                case '>':
                    AddBuf(_sm[0]);
                    switch (Peek())
                    {
                        case '=':
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator, LexValue.MoreEqual, _buf);
                            break;
                        default:
                            AddLex(LexType.Operator, LexValue.More, _sm[0].ToString());
                            break;
                    }
                    break;
                case '.':
                    AddBuf(_sm[0]);
                    switch (Peek())
                    {
                        case '.':
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Separator, LexValue.Doubledot, _buf);
                            break;
                        default:
                            AddLex(LexType.Separator, LexValue.Dot, _buf);
                            break;
                    }
                    break;
                case '+':
                case '-':
                case '/':
                case '*':
                    AddBuf(_sm[0]);
                    _state = States.Opr;
                    break;
                
                default:
                    _state = States.Er;
                    break;
            }
        }

        switch (_state)
        {
            case States.Id:
                while (_state == States.Id)
                {
                    if (Char.IsLetterOrDigit(Peek())) 
                    {
                        GetNext();
                        AddBuf(_sm[0]);
                    }
                    else
                    {
                        var searchLex = SearchLex();
                        
                        if (searchLex != "")
                            switch (searchLex)
                            {
                                case "shr":
                                    AddLex(LexType.Operator, KeyWords.Shr, _buf);
                                    break;
                                case "shl":
                                    AddLex(LexType.Operator, KeyWords.Shl, _buf);
                                    break;
                                case "div":
                                    AddLex(LexType.Operator, KeyWords.Div, _buf);
                                    break;
                                case "mod":
                                    AddLex(LexType.Operator, KeyWords.Mod, _buf);
                                    break;
                                case "or":
                                    AddLex(LexType.Operator, KeyWords.Or, _buf);
                                    break;
                                case "xor":
                                    AddLex(LexType.Operator, KeyWords.Xor, _buf);
                                    break;
                                case "not":
                                    AddLex(LexType.Operator, KeyWords.Not, _buf);
                                    break;
                                case "and":
                                    AddLex(LexType.Operator, KeyWords.And, _buf);
                                    break;
                                default:
                                    AddLex(LexType.Keyword, searchLex.ToUpper() , _buf);
                                    break;
                            }
                        else
                        {
                            AddLex(LexType.Indificator, _buf, _buf);
                        }
                        _state = States.Fin;
                    }
                }
                break;
            
            case States.Num:
                while (_state == States.Num)
                {
                    if (Peek() == '.')
                    {
                        string bufNum = _buf;
                        _buf = Convert.ToInt64(_buf, baseNum).ToString();
                        GetNext();
                        AddBuf(_sm[0]);
                        bufNum += _sm[0];
                        if (!char.IsDigit(Peek()))
                            throw new Exception("invalid Double");
                        while (true)
                        {
                            if (char.IsDigit(Peek()))
                            {
                                GetNext();
                                AddBuf(_sm[0]);
                                bufNum += _sm[0];
                            }
                            else
                            {
                                AddLex(LexType.Double, Convert.ToDouble(_buf), baseNum == 16 ? "$"+bufNum : baseNum == 8 ? "&"+bufNum : baseNum == 2 ? "%"+bufNum : bufNum);
                                _state = States.Fin;
                                return Lexeme;
                            }
                        }
                    }
                    if (IsDigit(Peek(), baseNum))
                    {
                        GetNext();
                        AddBuf(_sm[0]);
                    }
                    else
                    {
                        AddLex(LexType.Integer, Convert.ToInt64(_buf, baseNum), baseNum == 16 ? "$"+_buf : baseNum == 8 ? "&"+_buf : baseNum == 2 ? "%"+_buf : _buf);
                        _state = States.Fin;
                    }
                }
                break;

            case States.Opr:
                switch (_sm[0])
                {
                    case '-':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator,LexValue.AssignSub, _buf);
                        }
                        else
                            AddLex(LexType.Operator,LexValue.Sub, _buf);
                        break;
                    case '+':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator,LexValue.AssignAdd, _buf);
                        }
                        else
                            AddLex(LexType.Operator,LexValue.Add, _buf);
                        break;
                    case '*':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator,LexValue.AssignMul, _buf);
                        }
                        else
                            AddLex(LexType.Operator,LexValue.Mul, _buf);
                        break;
                    case '/':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator,LexValue.AssignDiv, _buf);
                        }
                        else
                            AddLex(LexType.Operator,KeyWords.Div, _buf);
                        break;
                }
                _state = States.Fin;
                break;
            
            case States.Chr:
                if (!char.IsDigit(Peek()))
                    throw new Exception("Char error");
                
                while (char.IsDigit(Peek()))
                {
                    GetNext();
                    AddBuf(_sm[0]);
                }
                AddLex(LexType.Char, (char)int.Parse(_buf), '#'+_buf);
                break;
            
            case States.Str:
                    while (Peek() != (char)39)
                    {
                        if (_sr.EndOfStream)
                            throw new Exception("String error");
                        if (Peek() == '\n')
                            throw new Exception("String line error");
                        GetNext();
                        AddBuf(_sm[0]);
                    }
                    AddLex(LexType.String, _buf, (char)39+_buf+(char)39);
                    GetNext();
                    _state = States.Fin;
                break;
            
            case States.Eof:
                Lexeme = new Lex(LexType.Eof, "", "", Line, ++Column);
                break;

            case States.Er:
                throw new Exception("invalid symbol" +'\t' + _sm[0]);
        }

        return Lexeme;
    }
}