using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using System.Timers;

namespace Emuumuu
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteLine("Creating MC86000...");
            MC86000 m68k = new MC86000();

            WriteLine("Resetting...");
            m68k.Reset();

            WriteLine("Running random stuff...");

            Random r = new Random();

            int max = ushort.MaxValue;
            while (true)
            {
                System.Threading.Thread.Sleep(500);

                ushort op = (ushort)r.Next(max);

                m68k.Execute(op);

                Clear();

                WriteLine($"A0 ={m68k.A0:X4}  A1={m68k.A1:X4}  A2={m68k.A2:X4}  A3={m68k.A3:X4}");
                WriteLine($"A4 ={m68k.A4:X4}  A5={m68k.A5:X4}  A6={m68k.A6:X4}  A7={m68k.A7:X4}");
                WriteLine($"D0 ={m68k.D0:X4}  D1={m68k.D1:X4}  D2={m68k.D2:X4}  D3={m68k.D3:X4}");
                WriteLine($"D4 ={m68k.D4:X4}  D5={m68k.D5:X4}  D6={m68k.D6:X4}  D7={m68k.D7:X4}");
                WriteLine($"SSP={m68k.SSP:X8}  USP={m68k.USP:X8}  SR={m68k.SR:X4}");
                WriteLine();
                WriteLine($"{m68k.PC:X8}  {op:X4}");
            }
        }
    }
}
