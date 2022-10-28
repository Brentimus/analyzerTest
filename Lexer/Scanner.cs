using System;
using System.Collections.Generic;
using System.IO;

namespace Lexer
{
    public class Scanner
    {
        private readonly char[] _sm = new char[1];

        readonly string[] _delimiter = { ".", ";", ",", "(", ")", "+", "-", "*", "/", "=", ">", "<" }; // Это я тоже в Enum переделаю
        private int _dt = 0;
        private States _state;

        public List<Lex> Lexemes = new List<Lex>();
        readonly string[] _tid = { "" };
        readonly string[] _tnum = { "" };
        private StringReader _sr;
        private int _line = 1;
        private int _column = 1;
        public static string _buf;

        private void GetNext()
        {
            _sr.Read(_sm, 0, 1);
            
            if (_sm[0] == ' ')
                _line++;
            else if (_sm[0] == '\t')
                _line+=4;
            else if (_sm[0] == '\n')
            {
                _line = 1;
                _column++;
            }

            _line++;
        }
        private void ClearBuf()
        {
            _buf = "";
        }

        private void AddBuf(char symbol)
        {
            _buf += symbol;
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

        private void AddLex(List<Lex> lexes, lexType id, string source, int line, int column)
        {
            lexes.Add(new Lex(id, source, line, column));
        }

        public void Analysis(string text)
        {
            _sr = new StringReader(text);
            while (_state != States.Fin)
            {
                switch (_state)
                {
                    case States.S:
                        if (_sm[0] == ' ' || _sm[0] == '\n' || _sm[0] == '\t' || _sm[0] == '\0' || _sm[0] == '\r')
                            GetNext();
                        else if (Char.IsLetter(_sm[0]))
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
                        else if (_sm[0] == '{')
                        {
                            _state = States.Com;
                            GetNext();
                        }
                        else if (_sm[0] == ':')
                        {
                            _state = States.ASGN;
                            ClearBuf();
                            AddBuf(_sm[0]);
                            GetNext();
                        }
                        else if (_sm[0] == '.')
                        {
                            AddLex(Lexemes, lexType.Separator,_sm[0].ToString(), _line, _column);
                            _state = States.Fin;
                        }
                        else if (_sm[0] == '+' || _sm[0] == '-' || _sm[0] == '/' || _sm[0] == '*' )
                        {
                            _state = States.Opr;
                            ClearBuf();
                            AddBuf(_sm[0]);
                            GetNext();
                        }
                        else if (_sm[0] == '\0')
                        {
                            _state = States.EOF;
                        }
                        else
                        {
                            _state = States.Dlm;
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
                                AddLex(Lexemes, lexType.Keyword, srch.Item2, _line, _column);
                            else
                            {
                                var j = PushLex(_tid, _buf.ToString());
                                AddLex(Lexemes, lexType.Indificator, j.Item2, _line, _column);
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
                            var j = PushLex(_tnum, _dt.ToString());
                            AddLex(Lexemes, lexType.Integer, j.Item2, _line, _column);
                            _state = States.S;
                        }
                        break;
                    case States.Dlm:
                        ClearBuf();
                        AddBuf(_sm[0]);

                        var r = SearchLex(_delimiter);
                        if (r.Item1 != -1)
                        {
                            AddLex(Lexemes, lexType.Separator, r.Item2, _line, _column);
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
                            AddLex(Lexemes, lexType.Operator, _buf, _line, _column);
                            ClearBuf();
                            GetNext();
                        }
                        else
                            AddLex(Lexemes, lexType.Operator, _buf, _line, _column);
                        _state = States.S;
                        break;
                        
                    case States.Opr:
                        if (_sm[0] == '=')
                        {
                            AddBuf(_sm[0]);
                            AddLex(Lexemes, lexType.Operator, _buf, _line, _column);
                            ClearBuf();
                            GetNext();
                        }
                        else
                            AddLex(Lexemes, lexType.Operator, _buf, _line, _column);
                        _state = States.S;

                        break;
                    case States.EOF:
                        AddLex(Lexemes, lexType.EOF, "eof", _line, _column);
                        ClearBuf();
                        GetNext();
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