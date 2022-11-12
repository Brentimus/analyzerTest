using System;
using System.IO;
using Lexer;
class Program
{
    static void Main(string[] args)
    {
        Scanner scanner = new Scanner();
        using (StreamReader fileReader = new StreamReader("D:/FEFU/C#/Tester/text.in"))
        {
            scanner.Analysis(fileReader);
            foreach (var lex in scanner.Lexemes)
            {
                // lex.ToString();
                Console.WriteLine(lex);
            }
        }
    }
}