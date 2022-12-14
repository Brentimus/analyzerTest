using System.Globalization;

internal class program
{
    private static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        using (var fileReader = new StreamReader("../../../../test.in"))
        {
            var parser = new Parser.Parser(fileReader);
            try
            {
                parser.ParseExpression().PrintTree("");
                //Console.Write(parser.ParseExpression().Calc());
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }
    }
}