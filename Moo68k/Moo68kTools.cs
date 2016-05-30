using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moo68k
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

            public static void CompileToFile(string source, string path)
            {
                CompileToFile(source, path, FileFormat.S28, Encoding.ASCII);
            }

            public static void CompileToFile(string source, string path, FileFormat format)
            {
                CompileToFile(source, path, format, Encoding.ASCII);
            }

            public static void CompileToFile(string source, string path, FileFormat format, Encoding encoding, bool capitalized = true)
            {
                //TODO: CompileToFile(string)

                throw new NotImplementedException();

                using (StreamWriter sw = new StreamWriter(path, false, encoding))
                {



                }
            }
        }


    }
}
