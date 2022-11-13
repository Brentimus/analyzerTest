using System;
using System.Collections.Generic;
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
        private int _column = 1;
        private static string _buf;

        private void GetNext()
        {
            if (_sr.EndOfStream)
            {
                _sm = new char[1];
            } else
                _sr.Read(_sm, 0, 1);

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

        public void SkipSpace()
        {
            switch (_sm[0])
            {
                case ' ':
                case '\n':
                case '\t':
                case '\r':
                case '\0':
                    if (_sm[0] == ' ')
                        _column++;
                    else if (_sm[0] == '\t')
                        _column += 4;
                    else if (_sm[0] == '\n')
                    {
                        _column = 1;
                        _line++;
                    }
                    GetNext();
                    if (!_sr.EndOfStream)
                        SkipSpace();
                    break;
            }
            _state = States.S;
        }
        private void AddLex(List<Lex> lexes, LexType id, object value, string source)
        {
            _column += source.Length;
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
                        SkipSpace();
                        _pos = _column;
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
                                case ';':
                                    AddLex(Lexemes, LexType.Separator, LexValue.SEMICOLOM, _sm[0].ToString());
                                    GetNext();
                                    break;
                                case ',':
                                    AddLex(Lexemes, LexType.Separator, LexValue.COMMA, _sm[0].ToString());
                                    GetNext();
                                    break;
                                case '(':
                                    AddLex(Lexemes, LexType.Separator, LexValue.LPAREN, _sm[0].ToString());
                                    GetNext();
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
                                    _state = States.ASGN;
                                    AddBuf(_sm[0]);
                                    GetNext();
                                    break;
                                case '<':
                                    _state = States.ASGN;
                                    AddBuf(_sm[0]);
                                    GetNext();
                                    break;
                                case '>':
                                    _state = States.ASGN;
                                    AddBuf(_sm[0]);
                                    GetNext();
                                    break;
                                case '.':
                                    _state = States.Dot;
                                    AddBuf(_sm[0]);
                                    GetNext();
                                    break;
                                case '+':
                                case '-':
                                case '/':
                                case '*':
                                    _state = States.Opr;
                                    AddBuf(_sm[0]);
                                    GetNext();
                                    break;
                                case '=':
                                    AddLex(Lexemes, LexType.Operator, LexValue.EQUAL, _sm[0].ToString());
                                    GetNext();
                                    break;
                                default:
                                    if (_sr.EndOfStream)
                                    {
                                        _state = States.EOF;
                                    }
                                    else
                                        _state = States.ER;
                                    break;
                            }
                        }
                        break;
                    
                    case States.Dot:
                        if (_sm[0] == '.')
                        {
                            AddBuf(_sm[0]);
                            AddLex(Lexemes, LexType.Separator, LexValue.DOUBLEDOT, _buf);
                            GetNext();
                        }
                        else
                            AddLex(Lexemes, LexType.Separator, LexValue.DOT, _buf);
                        ClearBuf();
                        _state = States.S;
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
                                        AddLex(Lexemes, LexType.Operator, KeyWords.SHR, searchLex);
                                        break;
                                    case "shl":
                                        AddLex(Lexemes, LexType.Operator, KeyWords.SHL, searchLex);
                                        break;
                                    case "div":
                                        AddLex(Lexemes, LexType.Operator, KeyWords.DIV, searchLex);
                                        break;
                                    case "mod":
                                        AddLex(Lexemes, LexType.Operator, KeyWords.MOD, searchLex);
                                        break;
                                    case "or":
                                        AddLex(Lexemes, LexType.Operator, KeyWords.OR, searchLex);
                                        break;
                                    case "xor":
                                        AddLex(Lexemes, LexType.Operator, KeyWords.XOR, searchLex);
                                        break;
                                    case "not":
                                        AddLex(Lexemes, LexType.Operator, KeyWords.NOT, searchLex);
                                        break;
                                    case "and":
                                        AddLex(Lexemes, LexType.Operator, KeyWords.AND, searchLex);
                                        break;
                                    default:
                                        AddLex(Lexemes, LexType.Keyword, searchLex.ToUpper() , searchLex);
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
                            //_column++;
                            GetNext();
                        }
                        else
                        {
                            //_column++;
                            AddLex(Lexemes, LexType.Integer, _dt, _dt.ToString());
                            _state = States.S;
                        }
                        break;

                    case States.ASGN:
                        switch (_sm[0])
                        {
                            case '=':
                                AddBuf(_sm[0]);
                                AddLex(Lexemes, LexType.Operator, LexValue.IMPLICIT , _buf);
                                ClearBuf();
                                GetNext();
                                break;
                            default:
                                AddLex(Lexemes, LexType.Separator, LexValue.COLON, _buf);
                                break;
                        }
                        _state = States.S;
                        break;
                        
                    case States.Opr:
                        if (_sm[0] == '=')
                        {
                            AddBuf(_sm[0]);
                            switch (_buf.ToCharArray()[0])
                            {
                                case '-':
                                    AddLex(Lexemes, LexType.Operator,LexValue.ASSIGN_SUB, _buf);
                                    break;
                                case '+':
                                    AddLex(Lexemes, LexType.Operator,LexValue.ASSIGN_ADD, _buf);
                                    break;
                                case '*':
                                    AddLex(Lexemes, LexType.Operator,LexValue.ASSIGN_MUL, _buf);
                                    break;
                                case '/':
                                    AddLex(Lexemes, LexType.Operator,LexValue.ASSIGN_DIV, _buf);
                                    break;
                            }
                            ClearBuf();
                            GetNext();
                        }
                        else
                            switch (_buf.ToCharArray()[0])
                            {
                                case '-':
                                    AddLex(Lexemes, LexType.Operator,LexValue.SUB, _buf);
                                    break;
                                case '+':
                                    AddLex(Lexemes, LexType.Operator,LexValue.ADD, _buf);
                                    break;
                                case '*':
                                    AddLex(Lexemes, LexType.Operator,LexValue.MUL, _buf);
                                    break;
                                case '/':
                                    AddLex(Lexemes, LexType.Operator,KeyWords.DIV, _buf);
                                    break;
                                default: 
                                    AddLex(Lexemes, LexType.Operator,LexValue.EQUAL, _buf);
                                    break;
                            }
                            
                        _state = States.S;
                        break;
                    
                    case States.Com:
                        while (true)
                        {
                            
                        }
                    
                    case States.EOF:
                        _state = States.Fin;
                        Lexemes.Add(new Lex(LexType.EOF, "", "", _line, _column));
                        break;

                    case States.ER:
                        AddLex(Lexemes, LexType.EOF, "error", _sm.ToString());
                        _state = States.Fin;
                        return;
                }

            }
        }
    }
}