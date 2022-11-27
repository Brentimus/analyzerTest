using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        using (StreamReader fileReader = new StreamReader("../../../../test.in"))
        {
            Parser.Parser parser = new Parser.Parser(fileReader);
            try
            {
                parser.ParseExpression().PrintTree();
                //Console.Write(parser.ParseExpression().Calc());
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }
    }
}