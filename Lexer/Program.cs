using System;
using System.Collections.Generic;
using System.IO;
using Lexer;

public class Lexic
    {
        private string buf = ""; // буфер для хранения лексемы
        private char[] sm = new char[1];
        private string[] Words = {"and", "end", "nil", "set", "array", "file", "not", "then", "begin", "for", "of", "to", "case", "function", "or", "type", "const", "goto", "packed", "until", "div", "if", "procedure", "var", "do", "in", "program", "while", "downto", "label", "record", "with", "else", "mod", "repeat", "AND", "END", "NIL", "SET", "ARRAY", "FILE", "NOT", "THEN", "BEGIN", "FOR", "OF", "TO", "CASE", "FUNCTION", "OR", "TYPE", "CONST", "GOTO", "PACKED", "UNTIL", "DIV", "IF", "PROCEDURE", "VAR", "DO", "IN", "PROGRAM", "WHILE", "DOWNTO", "LABEL", "RECORD", "WITH", "ELSE", "MOD", "REPEAT"};
        private string[] Delimiter = { ".", ";", ",", "(", ")", "+", "-", "*", "/", "=", ">", "<" };
        private int dt = 0;
        private enum States { S, NUM, DLM, FIN, ID, ER, ASGN, COM } // состояния state-машины
        private States _state; // хранит текущее состояние
        
        public readonly List<Lex> Lexemes = new List<Lex>();
        private string[] TID = { "" };
        private string[] TNUM = { "" };
        private StringReader sr; // позволяет посимвольно считывать строку

        private void GetNext()
        {
            sr.Read(sm, 0, 1);
        }

        private void ClearBuf()
        {
            buf = "";
        }

        private void AddBuf(char symbol)
        {
            buf += symbol;
        }

        private (int, string) SearchLex(string[] lexemes)
        {
            var srh = Array.FindIndex(lexemes, s => s.Equals(buf));
            if (srh != -1)
                return (srh, buf);
            return (-1, "");
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

        private void AddLex(List<Lex> lexes, int key, int val, string lex)
        {
            lexes.Add(new Lex(key, val, lex));
        }

        public void Analysis(string text)
        {
            sr = new StringReader(text);
            while (_state != States.FIN)
            {
                switch (_state)
                {

                    case States.S:
                        if (sm[0] == ' ' || sm[0] == '\n' || sm[0] == '\t' || sm[0] == '\0' || sm[0] == '\r')
                            GetNext();
                        else if (Char.IsLetter(sm[0]))
                        {
                            ClearBuf();
                            AddBuf(sm[0]);
                            _state = States.ID;
                            GetNext();
                        }
                        else if (char.IsDigit(sm[0]))
                        {
                            dt = (int)(sm[0] - '0');
                            GetNext();
                            _state = States.NUM;

                        }
                        else if (sm[0] == '{')
                        {
                            _state = States.COM;
                            GetNext();
                        }
                        else if (sm[0] == ':')
                        {
                            _state = States.ASGN;
                            ClearBuf();
                            AddBuf(sm[0]);
                            GetNext();
                        }
                        else if (sm[0] == '.')
                        {
                            AddLex(Lexemes, 2, 0, sm[0].ToString());
                            _state = States.FIN;
                        }
                        else
                        {
                            _state = States.DLM;

                        }

                        break;
                    case States.ID:
                        if (Char.IsLetterOrDigit(sm[0]))
                        {
                            AddBuf(sm[0]);
                            GetNext();
                        }
                        else
                        {
                            var srch = SearchLex(Words);
                            if (srch.Item1 != -1)
                                AddLex(Lexemes, 1, srch.Item1, srch.Item2);
                            else
                            {
                                var j = PushLex(TID, buf);
                                AddLex(Lexemes, 4, j.Item1, j.Item2);
                            }
                            _state = States.S;
                        }
                        break;

                    case States.NUM:
                        if (Char.IsDigit(sm[0]))
                        {
                            dt = dt * 10 + (int)(sm[0] - '0');
                            GetNext();
                        }
                        else
                        {

                            var j = PushLex(TNUM, dt.ToString());
                            AddLex(Lexemes, 3, j.Item1, j.Item2);
                            _state = States.S;
                        }
                        break;
                    case States.DLM:
                        ClearBuf();
                        AddBuf(sm[0]);

                        var r = SearchLex(Delimiter);
                        if (r.Item1 != -1)
                        {
                            AddLex(Lexemes, 2, r.Item1, r.Item2);
                            _state = States.S;
                            GetNext();
                        }
                        else
                            _state = States.ER;
                        break;
                    case States.ASGN:
                        if (sm[0] == '=')
                        {
                            AddBuf(sm[0]);
                            AddLex(Lexemes, 2, 4, buf);
                            ClearBuf();
                            GetNext();
                        }
                        else
                            AddLex(Lexemes, 2, 3, buf);
                        _state = States.S;

                        break;
                    case States.ER:
                        Console.Write("Ошибка в программе");
                        _state = States.FIN;
                        break;
                    case States.FIN:
                        Console.Write("Лексический анализ закончен");
                        break;
                }

            }
        }
    }

class Program
{
    static void Main(string[] args)
    {
        Lexic lexic = new Lexic();
        using (StreamReader readtext = new StreamReader("D:/FEFU/C#/Tester/text.in"))
        {
            string textInput = readtext.ReadToEnd();
            lexic.Analysis(textInput);
            
            foreach (var lex in lexic.Lexemes)
            {
                Console.Write("id: " + lex.Id + " lex: " + lex.Lexeme + " val: " + lex.Value + Environment.NewLine);
            }
        }
        //Console.Write("asd");
    }
}