using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Lexer
{
    public class Scanner
    {
        private char[] _sm = new char[1];

        readonly string[] _delimiter = { ".", ";", ",", "(", ")", "+", "-", "*", "/", "=", ">", "<" }; // Это я тоже в Enum переделаю
        private int _dt = 0;
        private int pos = 0;
        private States _state;

        public List<Lex> Lexemes = new List<Lex>();
        readonly string[] _tid = { "" };
        readonly string[] _tnum = { "" };
        private StreamReader _sr;
        private int _line = 1;
        private int _column = 1;
        public static string _buf;

        private void GetNext()
        {
            _sr.Read(_sm, 0, 1);
        }
        private void ClearBuf()
        {
            _buf = "";
        }

        private void AddBuf(char symbol)
        {
            _buf += symbol;
            _column++;
        }

        private (int, string) SearchLex (string[] lexemes)
        {
            var srh = Array.FindIndex(lexemes, s => s.Equals(_buf));
            if (srh != -1)
                return (srh, _buf);
            return (-1, "");
        }
        private (int, string) SearchLex2 ()
        {
            if(Enum.TryParse(_buf, true, out KeyWords keyWords))
            {
                return (0, Enum.Parse(typeof(KeyWords), _buf, true).ToString().ToLower() );
            }
            else
            {
                return (-1, "");
            }
        }

        private (int, string) PushLex(string[] lexemes, string buf)
        {
            var srh = Array.FindIndex(lexemes, s => s.Equals(buf));
            if (srh != -1)
                return (-1, "");
            Array.Resize(ref lexemes, lexemes.Length + 1);
            lexemes[lexemes.Length - 1] = buf;
            return (lexemes.Length - 1, buf);
        }

        private void AddLex(List<Lex> lexes, lexType id, string source)
        {
            lexes.Add(new Lex(id, source, _line, pos));
        }

        public void Analysis(StreamReader fileReader)
        {
            _sr = fileReader;
            while (_state != States.Fin)
            {
                switch (_state)
                {
                    case States.S:
                        pos = _column;
                        if (_sr.EndOfStream)
                        {
                            _state = States.EOF;
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
                            switch (_sm[0])
                        {
                            case '{':
                                _state = States.Com;
                                GetNext();
                                break;
                            case ':':
                                _state = States.ASGN;
                                ClearBuf();
                                AddBuf(_sm[0]);
                                GetNext();
                                break;
                            case '.':
                                AddLex(Lexemes, lexType.Separator,_sm[0].ToString());
                                ClearBuf();
                                GetNext();
                                break;
                            case '+': case '-': case '/': case '*':
                                _state = States.Opr;
                                ClearBuf();
                                AddBuf(_sm[0]);
                                GetNext();
                                break;
                            case ' ': case '\n': case '\t': case '\r': case '\0':
                                if (_sm[0] == ' ')
                                    _column++;
                                else if (_sm[0] == '\t')
                                    _column+=4;
                                else if (_sm[0] == '\n')
                                {
                                    _column = 1;
                                    _line++;
                                }
                                GetNext();
                                break;
                            default:
                                _state = States.Dlm;
                                break;
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
                            var srch = SearchLex2();
                            
                            if (srch.Item1 != -1)
                                AddLex(Lexemes, lexType.Keyword, srch.Item2);
                            else
                            {
                                var j = PushLex(_tid, _buf.ToString());
                                AddLex(Lexemes, lexType.Indificator, j.Item2);
                            }
                            _state = States.S;
                        }
                        break;

                    case States.Num:
                        if (Char.IsDigit(_sm[0]))
                        {
                            _dt = _dt * 10 + (int)(_sm[0] - '0');
                            _column++;
                            GetNext();
                        }
                        else
                        {
                            _column++;
                            var j = PushLex(_tnum, _dt.ToString());
                            AddLex(Lexemes, lexType.Integer, j.Item2);
                            _state = States.S;
                        }
                        break;
                    case States.Dlm:
                        ClearBuf();
                        AddBuf(_sm[0]);

                        var r = SearchLex(_delimiter);
                        if (r.Item1 != -1)
                        {
                            AddLex(Lexemes, lexType.Separator, r.Item2);
                            _state = States.S;
                            GetNext();
                        }
                        else
                            _state = States.ER;
                        break;
                    
                    case States.ASGN:
                        if (_sm[0] == '=')
                        {
                            AddBuf(_sm[0]);
                            AddLex(Lexemes, lexType.Operator, _buf);
                            ClearBuf();
                            GetNext();
                        }
                        else
                            AddLex(Lexemes, lexType.Operator, _buf);
                        _state = States.S;
                        break;
                        
                    case States.Opr:
                        if (_sm[0] == '=')
                        {
                            AddBuf(_sm[0]);
                            AddLex(Lexemes, lexType.Operator, _buf);
                            ClearBuf();
                            GetNext();
                        }
                        else
                            AddLex(Lexemes, lexType.Operator, _buf);
                        _state = States.S;
                        break;
                    
                    case States.EOF:
                        _state = States.Fin;
                        AddLex(Lexemes, lexType.EOF, "eof");
                        break;

                    case States.ER:
                        Console.Write("Error");
                        _state = States.Fin;
                        return;
                }

            }
        }
    }
}