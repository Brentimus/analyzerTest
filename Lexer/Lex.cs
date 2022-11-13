namespace Lexer
{
    public class Lex
    {
        public LexType lexType;
        public object Value;
        public string Source;
        public int Line;
        public int Column;
    
        public Lex(LexType lexType, string source, object value, int line, int column)
        {
            this.lexType = lexType;
            this.Value = value;
            this.Source = source;
            this.Line = line;
            this.Column = column;
        }

        public override string ToString()
        {
            return Line+"\t"+ Column +"\t  "+lexType +"\t    "+Value+"   \t"+Source;
        }
    }
}