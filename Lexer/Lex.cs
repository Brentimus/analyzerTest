namespace Lexer
{
    public class Lex
    {
        public LexType LexType;
        public object Value;
        public string Source;
        public int Line;
        public int Column;
    
        public Lex(LexType lexType, string source, object value, int line, int column)
        {
            LexType = lexType;
            Value = value;
            Source = source;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return Line+"  "+ Column +"  "+LexType +"  "+Value+"  "+Source;
        }
    }
}