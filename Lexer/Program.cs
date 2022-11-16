using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Lexer;
class Program
{
    static void Main(string[] args)
    {
        //Scanner scan = new Scanner();
        ScannerLexer scan = new ScannerLexer();
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        using (StreamReader fileReader = new StreamReader("D:/FEFU/C#/Tester/text.in"))
        {
            while (true)
            {
                try
                {
                    Console.WriteLine(scan.Scanner(fileReader));
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(scan.Line+"\t"+scan.Pos+"\t"+LexType.Invaild+"\t"+e.Message);
                    break;
                }

                if (fileReader.EndOfStream)
                {
                    scan.Lexeme = new Lex(LexType.Eof, "", "", scan.Line, ++scan.Column);
                    Console.WriteLine(scan.Lexeme);
                    break;
                }
            }
        }
    }
}