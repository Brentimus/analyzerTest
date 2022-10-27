namespace Lexer
{
    public class Lex
    {
        public int Id;
        public int Lexeme;
        public string Value;
        /*public int Line;
        public int Column;*/
        

        public Lex(int id, int lexeme, string value)
        {
            this.Id = id;
            this.Lexeme = lexeme;
            this.Value = value;/*
            this.Line = line;
            this.Column = column;*/

        }
    }
}