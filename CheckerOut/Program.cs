using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Lexer;
using Parser;
using Parser.Visitor;

namespace ChekerOut;

internal class Program
{
    private static int total = 0;
    private static int test = 0;
    public static bool SimpleParser(string pathFile)
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
                Console.WriteLine("found:\n" + found);
                return false;
            }
        }
        catch (SyntaxException e)
        {
            Console.SetOut(oldOut);
            var found = e.Message;
            if (line != found)
            {
                Console.WriteLine(pathFile + '\t' + "WA");
                Console.WriteLine("expected:\n" + line);
                Console.WriteLine("found:\n" + found);
                return false;
            }
        }

        Console.WriteLine($"TEST {test}\tOK");
        return true;
    }
    public static bool Parser(string pathFile)
    {
        test++;
        var fileReaderIn = new StreamReader(pathFile + ".in");
        var parser = new Parser.Parser(fileReaderIn);
        var fileReaderOut = new StreamReader(pathFile + ".out");
        var line = fileReaderOut.ReadToEnd();
        var oldOut = Console.Out;
        try
        {
            var sw = new StringWriter();
            Console.SetOut(sw);
            IVisitor visitor = new PrinterVisitor();
            visitor.Visit(parser.Program());
            Console.SetOut(oldOut);
            var found = sw.ToString();
            if (line != found)
            {
                Console.WriteLine(pathFile + '\t' + "WA");
                Console.WriteLine("expected:\n" + line);
                Console.WriteLine("found:\n" + found);
                return false;
            }
        }
        catch (SyntaxException e)
        {
            Console.SetOut(oldOut);
            var found = e.Message;
            if (line != found)
            {
                Console.WriteLine(pathFile + '\t' + "WA");
                Console.WriteLine("expected:\n" + line);
                Console.WriteLine("found:\n" + found);
                return false;
            }
        }

        Console.WriteLine($"TEST {test}\tOK");
        return true;
    }
    public static bool Lexer(string pathFile)
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
                    return false;
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
                    return false;
                }
                break;
            }
        } while (!fileReaderIn.EndOfStream);
        Console.WriteLine($"TEST {test}\tOK");
        return true;
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
    public static void MakeOutTestParser()
    {
        var files = Directory.GetFiles("../../../../Tests/Parser/", "*.in")
            .Select(f => Path.GetFileName(f)[..^3]).ToList();
        foreach (var nameFile in files)
        {
            var path = $"../../../../Tests/Parser/{nameFile}.out";
            try
            {
                // Create the file, or overwrite if the file exists.
                using (FileStream fs = File.Create(path))
                {
                    var fileReaderIn = new StreamReader($"../../../../Tests/Parser/{nameFile}.in");
                    var parser = new Parser.Parser(fileReaderIn);
                    string text ="";
                    
                    var oldOut = Console.Out;
                    try
                    {
                        var sw = new StringWriter();
                        Console.SetOut(sw);
                        IVisitor visitor = new PrinterVisitor();
                        visitor.Visit(parser.Program());
                        Console.SetOut(oldOut);
                        var found = sw.ToString();
                        text += found;
                        //text = text.TrimEnd('\n');
                    }
                    catch (SyntaxException e)
                    {
                        Console.SetOut(oldOut);
                        var found = e.Message;
                        text += found;
                    }
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
    public static void StartTest(string program)
    {
        var files = Directory.GetFiles($"../../../../Tests/{program}/", "*.in")
            .Select(f => Path.GetFileName(f)[..^3]).ToList();

        Console.WriteLine($"{program} Test:");
        foreach (var file in files)
        {
            var check = false;
            if (program == "Lexer")
                check = Lexer($"../../../../Tests/{program}/" + file);
            else if (program == "SimpleParser")
                check = SimpleParser($"../../../../Tests/{program}/" + file);
            else if (program == "Parser")
                check = Parser($"../../../../Tests/{program}/" + file);
            
            if (check) total++;
        }
    }
    private static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        //MakeOutTestParser();
        StartTest("Lexer");
        StartTest("SimpleParser");
        StartTest("Parser");
        Console.Out.Write($"TOTAL: {total}/{test}");
    }
}