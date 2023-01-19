using System.Globalization;
using SimpleParser = SimpleParser.SimpleParser;

internal class program
{
    private static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        using (var fileReader = new StreamReader("../../../../test.in"))
        {
            var parser = new global::SimpleParser.SimpleParser(fileReader);
            try
            {
                parser.ParseExpression().PrintTree("");
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }
    }
}