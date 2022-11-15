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
                    scan.Scanner(fileReader);
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }

                if (fileReader.EndOfStream)
                {
                    scan.Lexemes.Add(new Lex(LexType.EOF, "", "", scan._line, ++scan._column));
                    break;
                }
            }
            foreach (var lex in scan.Lexemes)
            {
                // lex.ToString();
                Console.WriteLine(lex);
            }
        }
    }
}