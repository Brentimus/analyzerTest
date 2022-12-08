using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Lexer;

internal class Program : Buffer
{
    private static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        using (var fileReader = new StreamReader("../../../../test.in"))
        {
            var scan = new Scanner(fileReader);
            while (!fileReader.EndOfStream)
                try
                {
                    Console.WriteLine(scan.ScannerLex());
                }
                catch (LexException e)
                {
                    Console.Write(e.Message);
                    return;
                }

            if (scan.Lexeme.LexType != LexType.Eof)
                Console.Write(scan.ScannerLex());
        }
    }
}