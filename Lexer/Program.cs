using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Lexer;

internal class Program
{
    private static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        using (var fileReader = new StreamReader("../../../../test.in"))
        {
            var scan = new ScannerLexer(fileReader);
            while (!fileReader.EndOfStream)
                try
                {
                    Console.WriteLine(scan.Scanner());
                }
                catch (Exception e)
                {
                    Console.Write(scan.Line + "  " + scan.Pos + "  " + e.Message);
                    return;
                }

            if (scan.Lexeme.LexType != LexType.Eof)
                Console.Write(scan.Scanner());
        }
    }
}