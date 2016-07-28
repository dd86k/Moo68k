using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Moo68k.Tools
{
    /// <summary>
    /// Compilers, decompilers, S-Record file maker, etc.
    /// </summary>
    public static class Moo68kTools
    {
        /* Notes
         * - Abuse << to push in bits while compiling
         */

        const int DATA_BUS_WIDTH = 16;

        public enum FileFormat : byte { S19 = 16, S28 = 24, S37 = 32 }

        static class SRecord
        {
            public static string Compile(string source, string path)
            {
                //TODO: Compile(string)

                throw new NotImplementedException();
            }

            public static void CompileToSRecord(string source, string path)
            {
                CompileToSRecord(source, path, FileFormat.S28, Encoding.ASCII);
            }

            public static void CompileToSRecord(string source, string path,
                FileFormat format)
            {
                CompileToSRecord(source, path, format, Encoding.ASCII);
            }

            public static void CompileToSRecord(string source, string path,
                FileFormat format, Encoding encoding, bool capitalized = true)
            {
                //TODO: CompileToFile(string)

                throw new NotImplementedException();

                using (StreamWriter sw = new StreamWriter(path, false, encoding))
                {

                }
            }
        }

        public static int HexStringToInt(this string s)
        {
            s = PrepareHexString(s);

            return (int)HexToULong(s.Length > 8 ? s.Substring(s.Length - 8) : s);
        }

        public static uint HexStringToUInt(this string s)
        {
            s = PrepareHexString(s);

            return (uint)HexToULong(s.Length > 8 ? s.Substring(s.Length - 8) : s);
        }

        public static long HexStringToLong(this string s)
        {
            s = PrepareHexString(s);

            return (long)HexToULong(s.Length > 16 ? s.Substring(s.Length - 16) : s);
        }

        public static ulong HexStringToULong(this string s)
        {
            s = PrepareHexString(s);

            return HexToULong(s.Length > 16 ? s.Substring(s.Length - 16) : s);
        }

        static Regex HexRegex = new Regex(@"[^\dA-Fa-f]",
                RegexOptions.ECMAScript | RegexOptions.Compiled);
        static string PrepareHexString(string s) => HexRegex.Replace(s, "");

        static ulong HexToULong(string s)
        {
            ulong o = 0;

            for (int h = 0, i = s.Length - 1; i >= 0; --i, ++h)
            {
                switch (s[i])
                {
                    case '0': break; // Ignore
                    case '1': o |= (ulong)0x1 << (h * 4); break;
                    case '2': o |= (ulong)0x2 << (h * 4); break;
                    case '3': o |= (ulong)0x3 << (h * 4); break;
                    case '4': o |= (ulong)0x4 << (h * 4); break;
                    case '5': o |= (ulong)0x5 << (h * 4); break;
                    case '6': o |= (ulong)0x6 << (h * 4); break;
                    case '7': o |= (ulong)0x7 << (h * 4); break;
                    case '8': o |= (ulong)0x8 << (h * 4); break;
                    case '9': o |= (ulong)0x9 << (h * 4); break;
                    case 'A':
                    case 'a': o |= (ulong)0xA << (h * 4); break;
                    case 'B':
                    case 'b': o |= (ulong)0xB << (h * 4); break;
                    case 'C':
                    case 'c': o |= (ulong)0xC << (h * 4); break;
                    case 'D':
                    case 'd': o |= (ulong)0xD << (h * 4); break;
                    case 'E':
                    case 'e': o |= (ulong)0xE << (h * 4); break;
                    case 'F':
                    case 'f': o |= (ulong)0xF << (h * 4); break;

                    default: // Should never happen, as the Regex takes everything else out.
                        throw new ArgumentException($"'{s[i]}' is not a valid hexadecimal character.");
                }
            }

            return o;
        }
    }
}
