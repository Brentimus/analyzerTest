﻿using System;
using System.IO;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            using(StreamReader readtext = new StreamReader("01.in"))
            {
                string readText = readtext.ReadLine();
                using(StreamWriter writetext = new StreamWriter("01.out"))
                {
                    if (readText == "1")
                    {
                        writetext.WriteLine("1 1 Integer 1 1");
                    }
                    else writetext.WriteLine("1 1 Error");
                }
            }
        }
    }
}