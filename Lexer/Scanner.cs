using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Lexer
{
    public class Scanner
    {
        private char[] _sm = new char[1];
        private int _dt;
        private int _pos;
        private States _state;

        public List<Lex> Lexemes = new List<Lex>();
        private StreamReader _sr;
        private int _line = 1;
        private int _column = 0;
        private static string _buf;

        private void GetNext()
        {
            if (_sr.EndOfStream)
                _sm = new char[1];
            else
            {
                _sr.Read(_sm, 0, 1);
                _column++;
            }
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
        
        private string SearchLex ()
        {
            if(Enum.TryParse(_buf, true, out KeyWords keyWords))
            {
                return Enum.Parse(typeof(KeyWords), _buf, true).ToString().ToLower();
            }
            return "";
        }
        private void AddLex(List<Lex> lexes, LexType id, object value, string source)
        {
            lexes.Add(new Lex(id, source, value, _line, _pos));
        }

        public void Analysis(StreamReader fileReader)
        {
            _sr = fileReader;
            while (_state != States.Fin)
            {
                switch (_state)
                {
                    case States.S:
                        _pos = _column;
                        if (_sr.EndOfStream)
                        {
                            _state = States.EOF;
                            break;
                        }
                        if (Char.IsLetter(_sm[0]))
                        {
                            ClearBuf();
                            AddBuf(_sm[0]);
                            _state = States.Id;
                            GetNext();
                        }
                        else if (char.IsDigit(_sm[0]))
                        {
                            _dt = (int)(_sm[0] - '0');
                            GetNext();
                            _state = States.Num;
                        }
                        else
                        {
                            ClearBuf();
                            switch (_sm[0])
                            {
                                case '\n':
                                    _column = 0;
                                    _line++;
                                    GetNext();
                                    break;
                                case '\t':
                                    _column += 3;
                                    GetNext();
                                    break;
                                case ' ' :
                                case '\r':
                                case '\0':
                                    GetNext();
                                    break;
                                case ';':
                                    AddLex(Lexemes, LexType.Separator, LexValue.SEMICOLOM, _sm[0].ToString());
                                    GetNext();
                                    break;
                                case ',':
                                    AddLex(Lexemes, LexType.Separator, LexValue.COMMA, _sm[0].ToString());
                                    GetNext();
                                    break;
                                case '{':
                                    _state = States.Com;
                                    break;
                                case '(':
                                    if (Peek() == '*')
                                        _state = States.Com;
                                    else
                                    {
                                        AddLex(Lexemes, LexType.Separator, LexValue.LPAREN, _sm[0].ToString());
                                        GetNext();
                                    }
                                    break;
                                case ')':
                                    AddLex(Lexemes, LexType.Separator, LexValue.RPAREN, _sm[0].ToString());
                                    GetNext();
                                    break;
                                case '[':
                                    AddLex(Lexemes, LexType.Separator, LexValue.LBRACK, _sm[0].ToString());
                                    GetNext();
                                    break;
                                case ']':
                                    AddLex(Lexemes, LexType.Separator, LexValue.RBRACK, _sm[0].ToString());
                                    GetNext();
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
                                    ClearBuf();
                                    GetNext();
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
                                    ClearBuf();
                                    GetNext();
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
                                    ClearBuf();
                                    GetNext();
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
                                    ClearBuf();
                                    GetNext();
                                    break;
                                case '+':
                                case '-':
                                case '/':
                                case '*':
                                    AddBuf(_sm[0]);
                                    _state = States.Opr;
                                    if (Peek() == '/' && _sm[0] == '/')
                                        _state = States.Com;
                                    break;
                                case '=':
                                    AddLex(Lexemes, LexType.Operator, LexValue.EQUAL, _sm[0].ToString());
                                    GetNext();
                                    break;
                                default:
                                    _state = States.ER;
                                    break;
                            }
                        }
                        break;

                    case States.Id:
                        if (Char.IsLetterOrDigit(_sm[0]))
                        {
                            AddBuf(_sm[0]);
                            GetNext();
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
                            _state = States.S;
                        }
                        break;

                    case States.Num:
                        if (Char.IsDigit(_sm[0]))
                        {
                            _dt = _dt * 10 + (int)(_sm[0] - '0');
                            GetNext();
                        }
                        else
                        {
                            AddLex(Lexemes, LexType.Integer, _dt, _dt.ToString());
                            _state = States.S;
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
                        ClearBuf();
                        GetNext();
                        _state = States.S;
                        break;
                    
                    case States.Com:
                        _state = States.S;
                        switch (Peek())
                            {
                                case '*':
                                    while (true)
                                    {
                                        if (_sr.EndOfStream)
                                        {
                                            _state = States.EOF;
                                            break;
                                        }
                                        GetNext();
                                        if (_sm[0] == '\n')
                                        {
                                            _column = 0;
                                            _line++;
                                        }
                                        char smm = Peek();
                                        if (Peek() == ')' && _sm[0] == '*')
                                        {
                                            GetNext();
                                            GetNext();
                                            break;
                                        }
                                    }
                                    break;
                                case '/':
                                    while (_sm[0] != '\n')
                                    {
                                        if (_sr.EndOfStream)
                                        {
                                            _state = States.EOF;
                                            break;
                                        }
                                        GetNext();
                                    }
                                    break;
                                default:
                                    while (_sm[0] != '}')
                                    {
                                        if (_sr.EndOfStream)
                                        {
                                            _state = States.EOF;
                                            break;
                                        }
                                        GetNext();
                                        if (_sm[0] == '\n')
                                        {
                                            _column = 0;
                                            _line++;
                                        }
                                    }
                                    GetNext();
                                    break;
                            }
                        break;
                    
                    case States.EOF:
                        _state = States.Fin;
                        Lexemes.Add(new Lex(LexType.EOF, "", "", _line, ++_column));
                        break;

                    case States.ER:
                        AddLex(Lexemes, LexType.EOF, _sm[0], "error");
                        _state = States.Fin;
                        return;
                }

            }
        }
    }
}