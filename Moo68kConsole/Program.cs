using Moo68k;
using Moo68k.Tools;
using System;
using static System.Console;

namespace Moo68kConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Write("Creating MC86000... ");
            MC68000 m68k = new MC68000();
            WriteLine("OK");

            m68k.FlagIsSupervisor = true;
            //m68k.FlagTracingEnabled = true;

            WriteLine();
            WriteLine("What would you like to do?");
            WriteLine();
            WriteLine("1. Generate instructions randomly.");
            WriteLine("2. Interpret machine code.");
            WriteLine();

            string n = null;
            while (n == null || n.Length == 0)
            {
                Write("Input [1-2]: ");
                n = ReadLine();
            }

            switch (n[0])
            {
                case '1':
                    RandomTester.Run();
                    break;

                case '2':
                    InterpreterPrompt.Enter();
                    break;
            }
        }
    }
}
