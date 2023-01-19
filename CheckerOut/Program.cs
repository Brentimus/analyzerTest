using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Lexer;
using SimpleParser = SimpleParser.SimpleParser;

namespace ChekerOut;

internal class Program
{
    private static int total = 0;
    private static int test = 0;
    public static void ParserTest(string pathFile)
    {
        test++;
        var fileReaderIn = new StreamReader(pathFile + ".in");
        var parser = new global::SimpleParser.SimpleParser(fileReaderIn);
        var fileReaderOut = new StreamReader(pathFile + ".out");
        var line = fileReaderOut.ReadToEnd();
        var oldOut = Console.Out;
        try
        {
            var sw = new StringWriter();
            Console.SetOut(sw);
            parser.ParseExpression().PrintTree("");
            Console.SetOut(oldOut);
            var found = sw.ToString();
            if (line + "\r\n" != found)
            {
                Console.WriteLine(pathFile + '\t' + "WA");
                Console.WriteLine("expected:\n" + line);
                Console.Write("found:\n" + found);
                Environment.Exit(0);
            }
        }
        catch (Exception e)
        {
            Console.SetOut(oldOut);
            var found = e.Message;
            if (line != found)
            {
                Console.WriteLine(pathFile + '\t' + "WA");
                Console.WriteLine("expected:\n" + line);
                Console.Write("found:\n" + found);
                Environment.Exit(0);
            }
        }

        Console.WriteLine($"TEST {test}\tOK");
        total++;
    }

    public static void LexerTest(string pathFile)
    {
        test++;
        var fileReaderIn = new StreamReader(pathFile + ".in");
        var scan = new Scanner(fileReaderIn);
        var fileReaderOut = new StreamReader(pathFile + ".out");
        do
        {
            string line;
            line = fileReaderOut.ReadLine();
            try
            {
                var found = scan.ScannerLex().ToString();
                if (line != found)
                {
                    Console.WriteLine(pathFile + '\t' + "WA");
                    Console.WriteLine("expected:\n" + line);
                    Console.WriteLine("found:\n" + found + "\n");
                    //Environment.Exit(0);
                }
            }
            catch (LexException e)
            {
                var found = e.Message;
                if (line != found)
                {
                    Console.WriteLine(pathFile + '\t' + "WA");
                    Console.WriteLine("expected:\n" + line);
                    Console.WriteLine("found:\n" + found + "\n");
                    //Environment.Exit(0);
                }
                break;
            }
        } while (!fileReaderIn.EndOfStream);
        Console.WriteLine($"TEST {test}\tOK");
        total++;
    }

    public static void MakeOutTest()
    {
        var files = Directory.GetFiles("../../../../Tests/Lexer/", "*.in")
            .Select(f => Path.GetFileName(f)[..^3]).ToList();
        foreach (var nameFile in files)
        {
            var path = $"../../../../Tests/Lexer/{nameFile}.out";
            try
            {
                // Create the file, or overwrite if the file exists.
                using (FileStream fs = File.Create(path))
                {
                    var fileReaderIn = new StreamReader($"../../../../Tests/Lexer/{nameFile}.in");
                    var scan = new Scanner(fileReaderIn);
                    string text ="";
                    do
                    {
                        try
                        {
                            var found = scan.ScannerLex().ToString();
                            text += $"{found}\n";
                        }
                        catch (LexException e)
                        {
                            var found = e.Message;
                            text += $"{found}";
                            break;
                        }
                    } while (!fileReaderIn.EndOfStream);
                    byte[] info = new UTF8Encoding(true).GetBytes(text);
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public static void StartLexerTest()
    {
        var files = Directory.GetFiles("../../../../Tests/Lexer/", "*.in")
            .Select(f => Path.GetFileName(f)[..^3]).ToList();

        Console.WriteLine("Lexer Test:");
        foreach (var file in files) LexerTest("../../../../Tests/Lexer/" + file);

    }
    public static void StartParserTest()
    {
        var filesP = Directory.GetFiles("../../../../Tests/SimpleParser/", "*.in")
            .Select(f => Path.GetFileName(f)[..^3]).ToList();

        Console.WriteLine("SimpleParser Test:");
        foreach (var file in filesP) ParserTest("../../../../Tests/SimpleParser/" + file);
    }
    private static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        StartLexerTest();
        StartParserTest();
        Console.Out.Write($"TOTAL: {test}/{total}");
    }
}