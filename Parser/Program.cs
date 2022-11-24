using System.Globalization;
using Lexer;

class Program
{
    static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        using (StreamReader fileReader = new StreamReader("../../../../test.in"))
        {
            ScannerLexer scan = new ScannerLexer();
            Parser.Parser parser = new Parser.Parser(); 
            
            while (!fileReader.EndOfStream)
            {
                try
                {
                    parser.ParserExp(fileReader);
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                    return;
                }
            }

            Console.WriteLine(parser.ParserExp(fileReader));
        }
    }
}