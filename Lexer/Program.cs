using System;
using System.IO;
using Lexer;
class Program
{
    static void Main(string[] args)
    {
        //Scanner scan = new Scanner();
        ScannerLexer scan = new ScannerLexer();
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
                    Console.WriteLine(scan._line+"\t"+scan._pos+"\t"+e.Message);
                    break;
                }

                if (fileReader.EndOfStream)
                {
                    scan.lexeme = new Lex(LexType.EOF, "", "", scan._line, ++scan._column);
                    Console.WriteLine(scan.lexeme);
                    break;
                }
            }
        }
    }
}