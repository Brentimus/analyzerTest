namespace Lexer
{
    public class Lex
    {
        public readonly int Id;
        public readonly int Lexeme;
        public readonly string Value;

        public Lex(int id, int lexeme, string value)
        {
            this.Id = id;
            this.Lexeme = lexeme;
            this.Value = value;
        }
    }
}