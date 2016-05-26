using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
using System.Timers;
using Moo68k;

namespace Emuumuu
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteLine("Creating MC86000...");
            MC86000 m68k = new MC86000();

            WriteLine("Enabling tracing...");
            m68k.FlagIsSupervisor = true;
            m68k.FlagTracingEnabled = true;
            
            // 1A3C - [00 01] [101][0 00][11 1][100]
            // Operator code: Move byte (0001)
            // Destination register: 5 (101)
            // Destination mode: Data register (000)
            // Source mode: #<data> (111)
            // Source register: #<data> (100)
            m68k.Execute(0x1A3C, 5);
            WriteLine($"1A3C 00000005 -> D5={m68k.D5:X8}"); // move.b #5,d5

            // Those instructions is from a manual from 
            // MOVE.L #$12,d0 | 00 10 000 000 111 100 | 203C 00000012
            m68k.Execute(0x203C, 0x12);
            WriteLine($"203C 00000012 -> D0={m68k.D0:X8}"); // MOVE.L #$12,d0

            WriteLine();
            WriteLine("Press RETURN to start random test.");
            ReadLine();

            WriteLine("Running random stuff...");
            Random r = new Random();
            int max = ushort.MaxValue;
            while (true)
            {
                System.Threading.Thread.Sleep(500);

                ushort op = (ushort)r.Next(max);

                m68k.Execute(op, 0x69);

                Clear();

                WriteLine($" D0={m68k.D0:X4}  D1={m68k.D1:X4}  D2={m68k.D2:X4}  D3={m68k.D3:X4}");
                WriteLine($" D4={m68k.D4:X4}  D5={m68k.D5:X4}  D6={m68k.D6:X4}  D7={m68k.D7:X4}");
                WriteLine($" A0={m68k.A0:X4}  A1={m68k.A1:X4}  A2={m68k.A2:X4}  A3={m68k.A3:X4}");
                WriteLine($" A4={m68k.A4:X4}  A5={m68k.A5:X4}  A6={m68k.A6:X4}  A7={m68k.A7:X4}");
                WriteLine($"SSP={m68k.SSP:X8}  USP={m68k.USP:X8}  SR={m68k.SR:X4}");
                WriteLine();
                WriteLine($"{m68k.PC:X8}  {op:X4}");
            }
        }
    }
}
