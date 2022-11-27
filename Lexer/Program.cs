using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Lexer;

class Program
{
    static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        using (StreamReader fileReader = new StreamReader("../../../../test.in"))
        {
            ScannerLexer scan = new ScannerLexer(fileReader);
            while (!fileReader.EndOfStream)
            {
                try
                {
                    Console.WriteLine(scan.Scanner());
                }
                catch (Exception e)
                {
                    Console.Write(scan.Line +"  "+scan.Pos+"  "+LexType.Invaild+"  "+e.Message);
                    return;
                }
            } 
            if (scan.Lexeme.LexType != LexType.Eof)
                Console.Write(scan.Scanner());
        }
    }
}