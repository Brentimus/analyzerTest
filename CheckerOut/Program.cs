using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Lexer;

namespace ChekerOut;

internal class Program
{
    public static void ParserTest(string pathFile)
    {
        var fileReaderIn = new StreamReader(pathFile + ".in");
        var parser = new Parser.Parser(fileReaderIn);
        var fileReaderOut = new StreamReader(pathFile + ".out");
        var line = fileReaderOut.ReadToEnd();
        var oldOut = Console.Out;
        try
        {
            var sw = new StringWriter();
            Console.SetOut(sw);
            parser.Expression().PrintTree("");
            /*var originOutput = new StreamWriter(Console.OpenStandardOutput());
            originOutput.AutoFlush = true;*/
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

        Console.WriteLine(pathFile + ".in\tOK");
    }

    public static void LexerTest(string pathFile)
    {
        var fileReaderIn = new StreamReader(pathFile + ".in");
        var scan = new Scanner(fileReaderIn);
        var fileReaderOut = new StreamReader(pathFile + ".out");
        while (!fileReaderIn.EndOfStream)
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
                    Console.Write("found:\n" + found);
                    Environment.Exit(0);
                }
            }
            catch (LexException e)
            {
                var found = e.Message;
                if (line != found)
                {
                    Console.WriteLine(pathFile + '\t' + "WA");
                    Console.WriteLine("expected:\n" + line);
                    Console.Write("found:\n" + found);
                    Environment.Exit(0);
                }
            }
        }

        Console.WriteLine(pathFile + ".in\tOK");
    }

    private static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

        var files = Directory.GetFiles("../../../../Tests/Lexer/", "*.in")
            .Select(f => Path.GetFileName(f)[..^3]).ToList();

        Console.WriteLine("Lexer Test:");
        foreach (var file in files) LexerTest("../../../../Tests/Lexer/" + file);

        /*var filesP = Directory.GetFiles("../../../../Tests/Parser/", "*.in")
            .Select(f => Path.GetFileName(f)[..^3]).ToList();

        Console.WriteLine("\nParser Test:");
        foreach (var file in filesP) ParserTest("../../../../Tests/Parser/" + file);*/
    }
}