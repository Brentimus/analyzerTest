using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Lexer;
class Program
{
    static void Main(string[] args)
    {
        ScannerLexer scan = new ScannerLexer();
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        using (StreamReader fileReader = new StreamReader("../../../../test.in"))
        {
            while (!fileReader.EndOfStream)
            {
                try
                {
                    Console.WriteLine(scan.Scanner(fileReader));
                }
                catch (Exception e)
                {
                    Console.Write(scan.Line +" "+scan.Pos+" "+LexType.Invaild+" "+ e.Message);
                    return;
                }
            } 
            Console.Write(scan.Scanner(fileReader));
        }
    }
}