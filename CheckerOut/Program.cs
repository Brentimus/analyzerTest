using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Lexer;

namespace ChekerOut
{
    class Program
    {
        private static string pathFile;
        
        public static void ParserTest(StreamReader fileReaderIn)
        {
            var parser = new global::Parser.Parser(fileReaderIn);
            var fileReaderOut = new StreamReader(pathFile+".out");
            var line = fileReaderOut.ReadToEnd();
            var oldOut = Console.Out;
            try
            {
                var sw = new StringWriter();
                Console.SetOut(sw);
                parser.ParseExpression().PrintTree();
                /*var originOutput = new StreamWriter(Console.OpenStandardOutput());
                originOutput.AutoFlush = true;*/
                Console.SetOut(oldOut);
                var found = sw.ToString();
                if (line+"\r\n" != found)
                {
                    Console.WriteLine(pathFile+'\t'+"WA");
                    Console.WriteLine("expected:\n"+line);
                    Console.Write("found:\n"+found);
                    Environment.Exit(0);
                }
            }
            catch (Exception e)
            {
                Console.SetOut(oldOut);
                var found = e.Message;
                if (line != found)
                {
                    Console.WriteLine(pathFile+'\t'+"WA");
                    Console.WriteLine("expected:\n"+line);
                    Console.Write("found:\n"+found);
                    Environment.Exit(0);
                }
            }
            Console.WriteLine(pathFile+".in\tOK");
        }
        public static void LexerTest(StreamReader fileReaderIn)
        {
            var scan = new ScannerLexer(fileReaderIn);
            var fileReaderOut = new StreamReader(pathFile+".out");
            while (!fileReaderIn.EndOfStream)
            {
                string line;
                    line = fileReaderOut.ReadLine();
                try
                {
                    var found = scan.Scanner().ToString();
                    if (line != found)
                    {
                        Console.WriteLine(pathFile+'\t'+"WA");
                        Console.WriteLine("expected:\n"+line);
                        Console.Write("found:\n"+found);
                        Environment.Exit(0);
                    }
                }
                catch (Exception e)
                {
                    var found = scan.Line+"  "+scan.Pos+"  "+LexType.Invaild+"  "+e.Message;
                    if (line != found)
                    {
                        Console.WriteLine(pathFile+'\t'+"WA");
                        Console.WriteLine("expected:\n"+line);
                        Console.Write("found:\n"+found);
                        Environment.Exit(0);
                    }
                }
            }
            Console.WriteLine(pathFile +".in\tOK");
        }
        
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Console.WriteLine("Lexer Test:");
            for (int i = 1; i <= 12; i++)
            {
                pathFile = "../../../../Tests/Lexer/"+i;
                LexerTest(new StreamReader(pathFile+".in"));
            }
            Console.WriteLine("\nParser Test:");
            for (int i = 1; i <= 6; i++)
            {
                pathFile = "../../../../Tests/Parser/"+i;
                ParserTest(new StreamReader(pathFile+".in"));
            }
        }
    }
}