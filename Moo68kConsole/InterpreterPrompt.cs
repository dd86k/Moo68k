using Moo68k;
using System;
using static System.Console;
using Moo68k.Tools;

namespace Moo68kConsole
{
    public static class InterpreterPrompt
    {
        public static void Enter()
        {
            MC68000 m68k = new MC68000();
            string n;
            bool c = true; // Continue

            while (c)
            {
                n = null;
                do
                {
                    Write(">");
                    n = ReadLine();
                } while (n == null || n.Length == 0);

                if (!n.StartsWith("#")) // Remark/Comment
                    switch (n[0])
                    {
                        case 'p': // Print
                            {
                                string[] s = n.Split(
                                    new[] { ' ' },
                                    StringSplitOptions.RemoveEmptyEntries
                                );

                                switch (s.Length)
                                {
                                    case 1:
                                        WriteLine("No idea what to print.");
                                        break;

                                    case 2:
                                        switch (s[1]) // Lazy..
                                        {
                                            case "d0":
                                                WriteLine($"{m68k.D0:X8}");
                                                break;
                                            case "d1":
                                                WriteLine($"{m68k.D1:X8}");
                                                break;
                                            case "d2":
                                                WriteLine($"{m68k.D2:X8}");
                                                break;
                                            case "d3":
                                                WriteLine($"{m68k.D3:X8}");
                                                break;
                                            case "d4":
                                                WriteLine($"{m68k.D4:X8}");
                                                break;
                                            case "d5":
                                                WriteLine($"{m68k.D5:X8}");
                                                break;
                                            case "d6":
                                                WriteLine($"{m68k.D6:X8}");
                                                break;
                                            case "d7":
                                                WriteLine($"{m68k.D7:X8}");
                                                break;
                                            case "a0":
                                                WriteLine($"{m68k.A0:X8}");
                                                break;
                                            case "a1":
                                                WriteLine($"{m68k.A1:X8}");
                                                break;
                                            case "a2":
                                                WriteLine($"{m68k.A2:X8}");
                                                break;
                                            case "a3":
                                                WriteLine($"{m68k.A3:X8}");
                                                break;
                                            case "a4":
                                                WriteLine($"{m68k.A4:X8}");
                                                break;
                                            case "a5":
                                                WriteLine($"{m68k.A5:X8}");
                                                break;
                                            case "a6":
                                                WriteLine($"{m68k.A6:X8}");
                                                break;
                                            /*case "a7": // Already a TODO
                                            WriteLine($"{m68k.A7:X8}");
                                                break;*/
                                            case "sr":
                                                WriteLine($"{m68k.SR:X4}");
                                                break;
                                            case "ssp":
                                                WriteLine($"{m68k.SSP:X6}");
                                                break;
                                            case "usp":
                                                WriteLine($"{m68k.USP:X6}");
                                                break;
                                            case "pc":
                                                WriteLine($"{m68k.PC:X6}");
                                                break;
                                            /* MSP, IRP already a TODO */

                                            default:
                                                WriteLine("Unknown p argument.");
                                                break;
                                        }
                                        break;

                                    default:
                                        WriteLine("Unsupported number of p arguments.");
                                        break;
                                }
                            }
                            break;

                        case 't': // Trace
                            WriteLine($" D0={m68k.D0:X8}  D1={m68k.D1:X8}  D2={m68k.D2:X8}  D3={m68k.D3:X8}");
                            WriteLine($" D4={m68k.D4:X8}  D5={m68k.D5:X8}  D6={m68k.D6:X8}  D7={m68k.D7:X8}");
                            WriteLine($" A0={m68k.A0:X8}  A1={m68k.A1:X8}  A2={m68k.A2:X8}  A3={m68k.A3:X8}");
                            WriteLine($" A4={m68k.A4:X8}  A5={m68k.A5:X8}  A6={m68k.A6:X8}");
                            WriteLine($" USP={m68k.USP:X6}  SSP={m68k.SSP:X6}  SR={m68k.SR:X4}");
                            WriteLine();
                            WriteLine($" PC={m68k.PC:X6}");
                            break;

                        case 'q': // Quit
                            c = false;
                            break;

                        case 'h': // Help
                        case '?':
                            WriteLine(" Usage:");
                            WriteLine("  <COMMAND> or <OPCODE> [<OPERAND>]");
                            WriteLine(" Note: Hexadecimal numbers must have the 0x prefix.");
                            WriteLine();
                            WriteLine(" COMMANDS:");
                            WriteLine("p <REGISTER>...Print register.");
                            WriteLine("t..............Print all registers.");
                            WriteLine();
                            WriteLine("?.........Brings this screen.");
                            WriteLine("q.........Quits.");
                            break;

                        default:
                            {
                                string[] s = n.Split(
                                    new[] { ' ' },
                                    StringSplitOptions.RemoveEmptyEntries
                                );

                                switch (s.Length)
                                {
                                    // Case 0 is handled earlier.

                                    case 1:
                                        try
                                        {
                                            m68k.Execute(s[0].HexStringToUInt());
                                        }
                                        catch
                                        {
                                            WriteLine("Couldn't parse operation code.");
                                        }
                                        break;

                                    case 2:
                                        {
                                            try
                                            {
                                                m68k.Execute(
                                                    s[0].HexStringToUInt(),
                                                    s[1].HexStringToUInt()
                                                );
                                            }
                                            catch
                                            {
                                                WriteLine("Couldn't parse operation code or operand.");
                                            }
                                        }
                                        break;

                                    default:
                                        WriteLine("Too many operands");
                                        break;
                                }
                            }
                            break;
                    }

            }
        }
    }
}
