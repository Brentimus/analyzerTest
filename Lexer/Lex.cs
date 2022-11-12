namespace Lexer
{
    public class Lex
    {
        public lexType Id;
        //public object Value;
        public string Source;
        public int Line;
        public int Column;
    
        public Lex(lexType id, string source, int line, int column)
        {
            this.Id = id;
            //this.Value = value;
            this.Source = source;
            this.Line = line;
            this.Column = column;
        }

        public override string ToString()
        {
            return Line+"\t"+ Column +"\t"+Id +"   \t"+Source;
        }
    }
}