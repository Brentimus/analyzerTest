using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Lexer;

namespace ChekerOut
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            for (int i = 1; i <=5; i++)
            {
                ScannerLexer scan = new ScannerLexer();
                using (StreamReader fileReaderOut = new StreamReader("../../../../Tests/"+i+".out"))
                {
                    using (StreamReader fileReaderIn = new StreamReader("../../../../Tests/"+i+".in"))
                    {
                        while (!fileReaderIn.EndOfStream)
                        {
                            var line = fileReaderOut.ReadLine();
                            try
                            {
                                var lexeme = scan.Scanner(fileReaderIn);
                                if (line != lexeme.ToString())
                                {
                                    Console.WriteLine("test "+i+'\t'+"WA");
                                    Console.WriteLine("expected:\n"+line);
                                    Console.WriteLine("found:\n"+lexeme);
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                var found = scan.Line+" "+scan.Pos+" "+LexType.Invaild+" "+e.Message;
                                if (line != found)
                                {
                                    Console.WriteLine("test "+i+'\t'+"WA");
                                    Console.WriteLine("expected:\n"+line);
                                    Console.WriteLine("found:\n"+found);
                                    return;
                                }
                                break;
                            }
                        }
                        Console.WriteLine("test "+ i +"\tOK");
                    }
                }
            }
            
        }
    }
}