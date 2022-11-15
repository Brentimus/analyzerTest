using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Lexer;

public class ScannerLexer
{
    private char[] _sm = new char[1];
    private int _pos;
    private States _state;

    public List<Lex> Lexemes = new List<Lex>();
    private StreamReader _sr;
    public int _line = 1;
    public int _column = 0;
    private static string _buf;

    public int BaseNum(char basenum)
    {
        switch (basenum)
        {
            case '$':
                return 16;
            case '%':
                return 2;
            case '&':
                return 8;
        }

        return 10;
    }

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

    private void AddLex(List<Lex> lexes, LexType id, object value, string source)
    {
        lexes.Add(new Lex(id, source, value, _line, _pos));
    }

    public void Scanner(StreamReader fileReader)
    {
        _sr = fileReader;
        GetNext();
        skipSpace();
        skipCommentSpace();
        _pos = _column;
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
                case '$':
                case '&':
                    GetNext();
                    _state = States.Num;
                    break;
                case ';':
                    AddLex(Lexemes, LexType.Separator, LexValue.SEMICOLOM, _sm[0].ToString());
                    break;
                case '=':
                    AddLex(Lexemes, LexType.Operator, LexValue.EQUAL, _sm[0].ToString());
                    break;
                case ',':
                    AddLex(Lexemes, LexType.Separator, LexValue.COMMA, _sm[0].ToString());
                    break;
                case ')':
                    AddLex(Lexemes, LexType.Separator, LexValue.RPAREN, _sm[0].ToString());
                    break;
                case '[':
                    AddLex(Lexemes, LexType.Separator, LexValue.LBRACK, _sm[0].ToString());
                    break;
                case ']':
                    AddLex(Lexemes, LexType.Separator, LexValue.RBRACK, _sm[0].ToString());
                    break;
                case '(':
                    AddLex(Lexemes, LexType.Separator, LexValue.LPAREN, _sm[0].ToString());
                    break;
                case ':':
                    AddBuf(_sm[0]);
                    switch (Peek())
                    {
                        case '=':
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(Lexemes, LexType.Operator, LexValue.IMPLICIT, _buf);
                            break;
                        default:
                            AddLex(Lexemes, LexType.Separator, LexValue.COLON, _sm[0].ToString());
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
                            AddLex(Lexemes, LexType.Operator, LexValue.NO_EQUAL, _buf);
                            break;
                        case '=':
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(Lexemes, LexType.Operator, LexValue.LESS_EQUAL, _buf);
                            break;
                        default:
                            AddLex(Lexemes, LexType.Operator, LexValue.LESS, _sm[0].ToString());
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
                            AddLex(Lexemes, LexType.Operator, LexValue.MORE_EQUAL, _buf);
                            break;
                        default:
                            AddLex(Lexemes, LexType.Operator, LexValue.MORE, _sm[0].ToString());
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
                            AddLex(Lexemes, LexType.Separator, LexValue.DOUBLEDOT, _buf);
                            break;
                        default:
                            AddLex(Lexemes, LexType.Separator, LexValue.DOT, _buf);
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
                                    AddLex(Lexemes, LexType.Operator, KeyWords.SHR, _buf);
                                    break;
                                case "shl":
                                    AddLex(Lexemes, LexType.Operator, KeyWords.SHL, _buf);
                                    break;
                                case "div":
                                    AddLex(Lexemes, LexType.Operator, KeyWords.DIV, _buf);
                                    break;
                                case "mod":
                                    AddLex(Lexemes, LexType.Operator, KeyWords.MOD, _buf);
                                    break;
                                case "or":
                                    AddLex(Lexemes, LexType.Operator, KeyWords.OR, _buf);
                                    break;
                                case "xor":
                                    AddLex(Lexemes, LexType.Operator, KeyWords.XOR, _buf);
                                    break;
                                case "not":
                                    AddLex(Lexemes, LexType.Operator, KeyWords.NOT, _buf);
                                    break;
                                case "and":
                                    AddLex(Lexemes, LexType.Operator, KeyWords.AND, _buf);
                                    break;
                                default:
                                    AddLex(Lexemes, LexType.Keyword, searchLex.ToUpper() , _buf);
                                    break;
                            }
                        else
                        {
                            AddLex(Lexemes, LexType.Indificator, _buf, _buf);
                        }
                        _state = States.Fin;
                    }
                }
                break;
            
            case States.Num:
                while (_state == States.Num)
                {
                    if (Char.IsDigit(Peek()))
                    {
                        GetNext();
                        AddBuf(_sm[0]);
                    }
                    else
                    {
                        AddLex(Lexemes, LexType.Integer, Convert.ToInt64(_buf, 10), _buf);
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
                            AddLex(Lexemes, LexType.Operator,LexValue.ASSIGN_SUB, _buf);
                        }
                        else
                            AddLex(Lexemes, LexType.Operator,LexValue.SUB, _buf);
                        break;
                    case '+':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(Lexemes, LexType.Operator,LexValue.ASSIGN_ADD, _buf);
                        }
                        else
                            AddLex(Lexemes, LexType.Operator,LexValue.ADD, _buf);
                        break;
                    case '*':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(Lexemes, LexType.Operator,LexValue.ASSIGN_MUL, _buf);
                        }
                        else
                            AddLex(Lexemes, LexType.Operator,LexValue.MUL, _buf);
                        break;
                    case '/':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_sm[0]);
                            AddLex(Lexemes, LexType.Operator,LexValue.ASSIGN_DIV, _buf);
                        }
                        else
                            AddLex(Lexemes, LexType.Operator,KeyWords.DIV, _buf);
                        break;
                }
                _state = States.Fin;
                break;
            
            case States.EOF:
                Lexemes.Add(new Lex(LexType.EOF, "", "", _line, ++_column));
                break;

            case States.ER:
                AddLex(Lexemes, LexType.Invaild, _sm[0], "error");
                throw new Exception("invaild");
        }
    }
}