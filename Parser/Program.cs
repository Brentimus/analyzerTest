using System.Globalization;
using Parser;
using Parser.Visitor;

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
                IVisitor visitor = new PrinterVisitor();
                visitor.Visit(parser.Program());
            }
            catch (SyntaxException e)
            {
                Console.Write(e.Message);
            }
        }
    }
}