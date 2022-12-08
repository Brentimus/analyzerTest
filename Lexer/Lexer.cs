using System;
using System.Globalization;
using System.IO;

namespace Lexer;

public class Scanner : Buffer
{
    private static string _buf;
    private static string _bufString;
    private States _state;
    public int baseNum = 10;
    public Lex Lexeme;

    public Scanner(StreamReader fileReader)
    {
        file = fileReader;
    }

    private void SkipCommentSpace()
    {
        SkipSpace();
        if (Peek() == '/' && _cur == '/')
        {
            while (Peek() != '\n')
            {
                GetNext();
                if (file.EndOfStream)
                {
                    _cur = '\0';
                    break;
                }
            }
        }
        else if (Peek() == '*' && _cur == '(')
        {
            GetNext();
            while (true)
            {
                if (file.EndOfStream) throw new LexException(PositionCur, "comment error");
                GetNext();
                if (Peek() == ')' && _cur == '*')
                {
                    GetNext();
                    GetNext();
                    break;
                }
            }
        }
        else if (_cur == '{')
        {
            while (_cur != '}')
            {
                if (file.EndOfStream) throw new LexException(PositionCur, "comment error");
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
        switch (_cur)
        {
            case '\n':
            case '\t':
            case ' ':
            case '\r':
            case '\0':
                if (file.EndOfStream)
                {
                    _cur = '\0';
                    break;
                }
                GetNext();
                SkipSpace();
                break;
        }
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
        Lexeme = new Lex(id, source, value, Position.Line, Position.Column);
    }

    private bool IsDigit(char c, int baseNum)
    {
        return baseNum switch
        {
            10 => c is >= '0' and <= '9',
            16 => c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f',
            8 => c is >= '0' and <= '7',
            2 => c is >= '0' and <= '1'
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
                            AddBuf(_cur);
                            bufNum += _cur;

                            while (true)
                                if (char.IsDigit(Peek()))
                                {
                                    GetNext();
                                    AddBuf(_cur);
                                    bufNum += _cur;
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
                        AddBuf(_cur);
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
                        AddBuf(_cur);
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
                switch (_cur)
                {
                    case '-':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_cur);
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
                            AddBuf(_cur);
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
                            AddBuf(_cur);
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
                            AddBuf(_cur);
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
                        throw new LexException(PositionCur, "Char error");

                    while (char.IsDigit(Peek()))
                    {
                        GetNext();
                        _localChar += _cur;
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
                    if (file.EndOfStream)
                        throw new LexException(PositionCur, "String error");
                    if (Peek() == '\n')
                        throw new LexException(PositionCur, "String line error");
                    GetNext();
                    AddBuf(_cur);
                    _localString += _cur;
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
                Lexeme = new Lex(LexType.Eof, "", "", Position.Line, (Position.Column + 1));
                break;

            case States.Er:
                throw new LexException(PositionCur, "invalid symbol " +_cur);
        }
    }

    public Lex ScannerLex()
    {
        _state = States.Fin;
        GetNext();
        SkipCommentSpace();
        Position = PositionCur;
        baseNum = 10;
        ClearBuf();
        if (char.IsLetter(_cur) || _cur == '_')
        {
            AddBuf(_cur);
            _state = States.Id;
        }
        else if (char.IsDigit(_cur))
        {
            AddBuf(_cur);
            _state = States.Num;
        }
        else
        {
            switch (_cur)
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
                    AddLex(LexType.Separator, LexToken.Semicolom, _cur.ToString());
                    break;
                case '=':
                    AddLex(LexType.Operator, LexToken.Equal, _cur.ToString());
                    break;
                case ',':
                    AddLex(LexType.Separator, LexToken.Comma, _cur.ToString());
                    break;
                case ')':
                    AddLex(LexType.Separator, LexToken.Rparen, _cur.ToString());
                    break;
                case '[':
                    AddLex(LexType.Separator, LexToken.Lbrack, _cur.ToString());
                    break;
                case ']':
                    AddLex(LexType.Separator, LexToken.Rbrack, _cur.ToString());
                    break;
                case '(':
                    AddLex(LexType.Separator, LexToken.Lparen, _cur.ToString());
                    break;
                case ':':
                    AddBuf(_cur);
                    switch (Peek())
                    {
                        case '=':
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexToken.Assign, _buf);
                            break;
                        default:
                            AddLex(LexType.Separator, LexToken.Colon, _cur.ToString());
                            break;
                    }

                    break;
                case '<':
                    AddBuf(_cur);
                    switch (Peek())
                    {
                        case '>':
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexToken.NoEqual, _buf);
                            break;
                        case '=':
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexToken.LessEqual, _buf);
                            break;
                        default:
                            AddLex(LexType.Operator, LexToken.Less, _cur.ToString());
                            break;
                    }

                    break;
                case '>':
                    AddBuf(_cur);
                    switch (Peek())
                    {
                        case '=':
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexToken.MoreEqual, _buf);
                            break;
                        default:
                            AddLex(LexType.Operator, LexToken.More, _cur.ToString());
                            break;
                    }

                    break;
                case '.':
                    AddBuf(_cur);
                    switch (Peek())
                    {
                        case '.':
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Separator, LexToken.Doubledot, _buf);
                            break;
                        default:
                            AddLex(LexType.Separator, LexToken.Dot, _buf);
                            break;
                    }

                    break;
                case '+' or '-' or '/' or '*':
                    AddBuf(_cur);
                    _state = States.Opr;
                    break;
                case '\0' when file.EndOfStream:
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