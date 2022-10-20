using System;
using System.IO;

namespace CheckerOut
{
    class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader readtext = new StreamReader("D:/FEFU/C#/Tester/Tester/bin/Debug/net5.0/01.out"))
            {
                string readText = readtext.ReadLine();
                if (readText == "1 1 Integer 1 1")
                {
                    Console.Write("OK");
                }
                else 
                    Console.Write("WA");
            }
        }
    }
}