using System;
using System.Globalization;
using System.IO;

namespace Lexer;

public class ScannerLexer
{
    private static string _buf;
    private static string _bufString;
    private char? _back;
    private readonly char[] _cur = new char[1];
    private readonly StreamReader _sr;
    private States _state;
    public int baseNum = 10;
    public int Column;
    public Lex Lexeme;
    public int Line = 1;
    public int Pos;

    public ScannerLexer(StreamReader fileReader)
    {
        _sr = fileReader;
    }

    private void SkipCommentSpace()
    {
        SkipSpace();
        if (Peek() == '/' && _cur[0] == '/')
        {
            while (Peek() != '\n')
            {
                GetNext();
                if (_sr.EndOfStream)
                {
                    _cur[0] = '\0';
                    break;
                }
            }
        }
        else if (Peek() == '*' && _cur[0] == '(')
        {
            GetNext();
            while (true)
            {
                if (_sr.EndOfStream) throw new Exception("comment error");
                GetNext();
                if (Peek() == ')' && _cur[0] == '*')
                {
                    GetNext();
                    GetNext();
                    break;
                }
            }
        }
        else if (_cur[0] == '{')
        {
            while (_cur[0] != '}')
            {
                if (_sr.EndOfStream) throw new Exception("comment error");
                GetNext();
            }

            GetNext();
        }
        else
        {
            SkipSpace();
            return;
        }

        SkipCommentSpace();
    }

    private void SkipSpace()
    {
        switch (_cur[0])
        {
            case '\n':
            case '\t':
            case ' ':
            case '\r':
            case '\0':
                if (_sr.EndOfStream)
                {
                    _cur[0] = '\0';
                    break;
                }

                GetNext();
                SkipSpace();
                break;
        }
    }

    private void GetNext()
    {
        if (_back != null)
        {
            _cur[0] = (char) _back;
            _back = null;
        }
        else if (!_sr.EndOfStream)
        {
            _sr.Read(_cur, 0, 1);
            if (_cur[0] == '\n')
            {
                Column = 0;
                Line++;
            }
            else
            {
                Column++;
            }
        }
        else
        {
            _cur[0] = '\0';
        }
    }

    private void Back()
    {
        _back = _cur[0];
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

    private object SearchKeyword()
    {
        if (Enum.TryParse(_buf, true, out KeyWords keyWords)) return Enum.Parse(typeof(KeyWords), _buf, true);
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
            _ => throw new ArgumentOutOfRangeException("invalid Digit")
        };
    }

    private void State()
    {
        switch (_state)
        {
            case States.Num:
                while (_state == States.Num)
                {
                    if (Peek() == '.')
                    {
                        GetNext();
                        if (Peek() != '.')
                        {
                            var bufNum = _buf;
                            _buf = Convert.ToInt64(_buf, baseNum).ToString();
                            AddBuf(_cur[0]);
                            bufNum += _cur[0];

                            while (true)
                                if (char.IsDigit(Peek()))
                                {
                                    GetNext();
                                    AddBuf(_cur[0]);
                                    bufNum += _cur[0];
                                }
                                else
                                {
                                    AddLex(LexType.Double,
                                        Convert.ToDouble(_buf).ToString("E16", CultureInfo.InvariantCulture),
                                        baseNum == 16 ? "$" + bufNum :
                                        baseNum == 8 ? "&" + bufNum :
                                        baseNum == 2 ? "%" + bufNum : bufNum);
                                    _state = States.Fin;
                                    return;
                                }
                        }

                        Back();
                    }

                    if (IsDigit(Peek(), baseNum))
                    {
                        GetNext();
                        AddBuf(_cur[0]);
                    }
                    else
                    {
                        AddLex(LexType.Integer, Convert.ToInt32(_buf, baseNum),
                            baseNum == 16 ? "$" + _buf : baseNum == 8 ? "&" + _buf : baseNum == 2 ? "%" + _buf : _buf);
                        _state = States.Fin;
                    }
                }

                break;

            case States.Id:
                while (_state == States.Id)
                    if (char.IsLetterOrDigit(Peek()) || Peek() == '_')
                    {
                        GetNext();
                        AddBuf(_cur[0]);
                    }
                    else
                    {
                        var searchKeyword = SearchKeyword();
                        if (searchKeyword.ToString().ToLower() != "")
                            AddLex(LexType.Keyword, searchKeyword.ToString().ToUpper(), _buf);
                        else
                            AddLex(LexType.Identifier, _buf, _buf);

                        _state = States.Fin;
                    }

                break;

            case States.Opr:
                switch (_cur[0])
                {
                    case '-':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_cur[0]);
                            AddLex(LexType.Operator, LexToken.AssignSub, _buf);
                        }
                        else
                        {
                            AddLex(LexType.Operator, LexToken.Sub, _buf);
                        }

                        break;
                    case '+':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_cur[0]);
                            AddLex(LexType.Operator, LexToken.AssignAdd, _buf);
                        }
                        else
                        {
                            AddLex(LexType.Operator, LexToken.Add, _buf);
                        }

                        break;
                    case '*':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_cur[0]);
                            AddLex(LexType.Operator, LexToken.AssignMul, _buf);
                        }
                        else
                        {
                            AddLex(LexType.Operator, LexToken.Mul, _buf);
                        }

                        break;
                    case '/':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_cur[0]);
                            AddLex(LexType.Operator, LexToken.AssignDiv, _buf);
                        }
                        else
                        {
                            AddLex(LexType.Operator, LexToken.Div, _buf);
                        }

                        break;
                }

                _state = States.Fin;
                break;

            case States.Chr:
                while (true)
                {
                    var _localChar = "";
                    if (!char.IsDigit(Peek()))
                        throw new Exception("Char error");

                    while (char.IsDigit(Peek()))
                    {
                        GetNext();
                        _localChar += _cur[0];
                    }

                    _bufString += '#' + _localChar;
                    AddBuf((char) int.Parse(_localChar));
                    if (Peek() == '#')
                        GetNext();
                    else
                        break;
                }

                if (Peek() == (char) 39)
                {
                    GetNext();
                    _state = States.Str;
                    State();
                }
                else if (_buf.Length == 1)
                {
                    AddLex(LexType.Char, _buf, _bufString);
                    _bufString = "";
                }
                else
                {
                    AddLex(LexType.String, _buf, _bufString);
                    _bufString = "";
                }

                break;

            case States.Str:
                var _localString = "";
                while (Peek() != (char) 39)
                {
                    if (_sr.EndOfStream)
                        throw new Exception("String error");
                    if (Peek() == '\n')
                        throw new Exception("String line error");
                    GetNext();
                    AddBuf(_cur[0]);
                    _localString += _cur[0];
                }

                _bufString += (char) 39 + _localString + (char) 39;
                GetNext();

                if (Peek() == (char) 39)
                {
                    GetNext();
                    _buf += (char) 39;
                    State();
                }
                else if (Peek() == '#')
                {
                    _state = States.Chr;
                    GetNext();
                    State();
                }
                else
                {
                    AddLex(LexType.String, _buf, _bufString);
                    _bufString = "";
                    _state = States.Fin;
                }

                break;

            case States.Eof:
                Lexeme = new Lex(LexType.Eof, "", "", Line, ++Pos);
                break;

            case States.Er:
                throw new Exception("invalid symbol " + _cur[0]);
        }
    }

    public Lex Scanner()
    {
        _state = States.Fin;
        GetNext();
        SkipCommentSpace();
        Pos = Column;
        baseNum = 10;
        ClearBuf();
        if (char.IsLetter(_cur[0]) || _cur[0] == '_')
        {
            AddBuf(_cur[0]);
            _state = States.Id;
        }
        else if (char.IsDigit(_cur[0]))
        {
            AddBuf(_cur[0]);
            _state = States.Num;
        }
        else
        {
            switch (_cur[0])
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
                case (char) 39:
                    _state = States.Str;
                    break;
                case ';':
                    AddLex(LexType.Separator, LexToken.Semicolom, _cur[0].ToString());
                    break;
                case '=':
                    AddLex(LexType.Operator, LexToken.Equal, _cur[0].ToString());
                    break;
                case ',':
                    AddLex(LexType.Separator, LexToken.Comma, _cur[0].ToString());
                    break;
                case ')':
                    AddLex(LexType.Separator, LexToken.Rparen, _cur[0].ToString());
                    break;
                case '[':
                    AddLex(LexType.Separator, LexToken.Lbrack, _cur[0].ToString());
                    break;
                case ']':
                    AddLex(LexType.Separator, LexToken.Rbrack, _cur[0].ToString());
                    break;
                case '(':
                    AddLex(LexType.Separator, LexToken.Lparen, _cur[0].ToString());
                    break;
                case ':':
                    AddBuf(_cur[0]);
                    switch (Peek())
                    {
                        case '=':
                            GetNext();
                            AddBuf(_cur[0]);
                            AddLex(LexType.Operator, LexToken.Assign, _buf);
                            break;
                        default:
                            AddLex(LexType.Separator, LexToken.Colon, _cur[0].ToString());
                            break;
                    }

                    break;
                case '<':
                    AddBuf(_cur[0]);
                    switch (Peek())
                    {
                        case '>':
                            GetNext();
                            AddBuf(_cur[0]);
                            AddLex(LexType.Operator, LexToken.NoEqual, _buf);
                            break;
                        case '=':
                            GetNext();
                            AddBuf(_cur[0]);
                            AddLex(LexType.Operator, LexToken.LessEqual, _buf);
                            break;
                        default:
                            AddLex(LexType.Operator, LexToken.Less, _cur[0].ToString());
                            break;
                    }

                    break;
                case '>':
                    AddBuf(_cur[0]);
                    switch (Peek())
                    {
                        case '=':
                            GetNext();
                            AddBuf(_cur[0]);
                            AddLex(LexType.Operator, LexToken.MoreEqual, _buf);
                            break;
                        default:
                            AddLex(LexType.Operator, LexToken.More, _cur[0].ToString());
                            break;
                    }

                    break;
                case '.':
                    AddBuf(_cur[0]);
                    switch (Peek())
                    {
                        case '.':
                            GetNext();
                            AddBuf(_cur[0]);
                            AddLex(LexType.Separator, LexToken.Doubledot, _buf);
                            break;
                        default:
                            AddLex(LexType.Separator, LexToken.Dot, _buf);
                            break;
                    }

                    break;
                case '+' or '-' or '/' or '*':
                    AddBuf(_cur[0]);
                    _state = States.Opr;
                    break;
                case '\0' when _sr.EndOfStream:
                    _state = States.Eof;
                    break;
                default:
                    _state = States.Er;
                    break;
            }
        }

        State();
        return Lexeme;
    }
}