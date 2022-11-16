using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lexer;

public class ScannerLexer
{
    private char[] _sm = new char[1];
    private States _state;
    public Lex lexeme;
    private StreamReader _sr;
    public int _line = 1;
    public int _column = 0;
    public int _pos;
    private static string _buf;
    private void skipCommentSpace()
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
            skipSpace();
            return;
        }
        skipSpace();
        skipCommentSpace();
    }
    private void skipSpace()
    {
        switch (_sm[0])
        {
            case '\n':
            case '\t':
            case ' ':
            case '\r':
            case '\0':
                GetNext();
                skipSpace();
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
                _column = 0;
                _line++;
            }
            else
                _column++;
        }
        else
            _sm = new char[1];

    }
    private char Peek()
    {
        return (char) _sr.Peek();
    }
    
    private void ClearBuf()
    {
        _buf = "";
    }

    private void AddBuf(char symbol)
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
        lexeme = new Lex(id, source, value, _line, _pos);
    }

    private bool IsDigit(char c, int baseNum)
    {
        return baseNum switch
        {
            10 => c is >= '0' and <= '9',
            16 => c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f',
            8 => c is >= '0' and <= '7',
            2 => c is >= '0' and <= '1',
            _ => throw new ArgumentOutOfRangeException("Invalid integer")
        };
    }

    public Lex Scanner(StreamReader fileReader)
    {
        _sr = fileReader;
        GetNext();
        skipSpace();
        skipCommentSpace();
        _pos = _column;
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
                case ';':
                    AddLex(LexType.Separator, LexValue.SEMICOLOM, _sm[0].ToString());
                    break;
                case '=':
                    AddLex(LexType.Operator, LexValue.EQUAL, _sm[0].ToString());
                    break;
                case ',':
                    AddLex(LexType.Separator, LexValue.COMMA, _sm[0].ToString());
                    break;
                case ')':
                    AddLex(LexType.Separator, LexValue.RPAREN, _sm[0].ToString());
                    break;
                case '[':
                    AddLex(LexType.Separator, LexValue.LBRACK, _sm[0].ToString());
                    break;
                case ']':
                    AddLex(LexType.Separator, LexValue.RBRACK, _sm[0].ToString());
                    break;
                case '(':
                    AddLex(LexType.Separator, LexValue.LPAREN, _sm[0].ToString());
                    break;
                case ':':
                    AddBuf(_sm[0]);
                    switch (Peek())
                    {
                        case '=':
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator, LexValue.IMPLICIT, _buf);
                            break;
                        default:
                            AddLex(LexType.Separator, LexValue.COLON, _sm[0].ToString());
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
                            AddLex(LexType.Operator, LexValue.NO_EQUAL, _buf);
                            break;
                        case '=':
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator, LexValue.LESS_EQUAL, _buf);
                            break;
                        default:
                            AddLex(LexType.Operator, LexValue.LESS, _sm[0].ToString());
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
                            AddLex(LexType.Operator, LexValue.MORE_EQUAL, _buf);
                            break;
                        default:
                            AddLex(LexType.Operator, LexValue.MORE, _sm[0].ToString());
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
                            AddLex(LexType.Separator, LexValue.DOUBLEDOT, _buf);
                            break;
                        default:
                            AddLex(LexType.Separator, LexValue.DOT, _buf);
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
                    _state = States.ER;
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
                                    AddLex(LexType.Operator, KeyWords.SHR, _buf);
                                    break;
                                case "shl":
                                    AddLex(LexType.Operator, KeyWords.SHL, _buf);
                                    break;
                                case "div":
                                    AddLex(LexType.Operator, KeyWords.DIV, _buf);
                                    break;
                                case "mod":
                                    AddLex(LexType.Operator, KeyWords.MOD, _buf);
                                    break;
                                case "or":
                                    AddLex(LexType.Operator, KeyWords.OR, _buf);
                                    break;
                                case "xor":
                                    AddLex(LexType.Operator, KeyWords.XOR, _buf);
                                    break;
                                case "not":
                                    AddLex(LexType.Operator, KeyWords.NOT, _buf);
                                    break;
                                case "and":
                                    AddLex(LexType.Operator, KeyWords.AND, _buf);
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
                    try
                    {
                        Convert.ToInt64(_buf + Peek(), baseNum);
                    }
                    catch (Exception e)
                    {
                        AddLex(LexType.Integer, Convert.ToInt64(_buf, baseNum), _buf);
                    }
                    if (IsDigit(Peek(), baseNum))
                    {
                        GetNext();
                        AddBuf(_sm[0]);
                    }
                    else
                    {
                        AddLex(LexType.Integer, Convert.ToInt64(_buf, baseNum), _buf);
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
                            AddLex(LexType.Operator,LexValue.ASSIGN_SUB, _buf);
                        }
                        else
                            AddLex(LexType.Operator,LexValue.SUB, _buf);
                        break;
                    case '+':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator,LexValue.ASSIGN_ADD, _buf);
                        }
                        else
                            AddLex(LexType.Operator,LexValue.ADD, _buf);
                        break;
                    case '*':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator,LexValue.ASSIGN_MUL, _buf);
                        }
                        else
                            AddLex(LexType.Operator,LexValue.MUL, _buf);
                        break;
                    case '/':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(LexType.Operator,LexValue.ASSIGN_DIV, _buf);
                        }
                        else
                            AddLex(LexType.Operator,KeyWords.DIV, _buf);
                        break;
                }
                _state = States.Fin;
                break;
            
            case States.EOF:
                lexeme = new Lex(LexType.EOF, "", "", _line, ++_column);
                break;

            case States.ER:
                AddLex(LexType.Invaild, _sm[0], "error");
                throw new Exception("invaild");
        }

        return lexeme;
    }
}