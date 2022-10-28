using System;
using System.IO;
using Lexer;
class Program
{
    static void Main(string[] args)
    {
        Scanner scanner = new Scanner();
        using (StreamReader readtext = new StreamReader("D:/FEFU/C#/Tester/text.in"))
        {
            while (!readtext.EndOfStream)
            {
                scanner.Analysis(readtext.ReadToEnd()); // так плохо, я знаю, позже исправлю
            }
            foreach (var lex in scanner.Lexemes)
            {
                Console.WriteLine(lex.Column + "\t" + lex.Line + "\t" +  lex.Id + '\t'+ '\t' + lex.Source);
            }
        }
    }
}