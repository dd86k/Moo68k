using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Design TODOs
 * 
 * MC86000 -- Struct or class?
 *     Class -- Let's not overflow the stack, okay? Poor thing.
 * Incorporate FPCC, QB, EXC, AEXC into FPSR?
 *     Yes and no -- Those bytes are into FPSR, but we can do getters.
 * Use unsafe code?
 *     Probably later when this will work. (in 4 years of my lazy time)
 * Where to increase Program Counter?
 *     
 * Decide what to make as public property.
 *     
 * Base class or interface?
 *     Probably class.
 */

namespace Moo68k
{
    [Flags]
    public enum CCRFlags : byte
    {
        C = 1, V = 2, Z = 4, N = 8, X = 16
    }

    [Flags]
    public enum CCRFlagsSimplified : byte
    {
        Carry = 1, Overflow = 2, Zero = 4, Negative = 8, Extend = 16
    }

    /// <summary>
    /// Interface for the M68000 series.
    /// </summary>
    /*interface IM68000
    {
        
    }*/

    /// <summary>
    /// Represents a Motorola 68000 microprocessor
    /// </summary>
    /// <remarks>
    /// Generation: First
    /// Endianness: Big
    /// Introduced: 1979
    /// Design: CISC
    /// Branching: Condition code
    /// Data width: 16 b
    /// Address width: 24 b
    /// </remarks>
    public class MC86000
    {
        // Constants ⁄(⁄ ⁄•⁄ω⁄•⁄ ⁄)⁄

        const ushort SR_INIT = 0x2700;

        // Properties ヽ(•̀ω•́ )ゝ

        public uint D0
        {
            get { return dataRegisters[0]; }
            private set { dataRegisters[0] = value; }
        }
        public uint D1
        {
            get { return dataRegisters[1]; }
            private set { dataRegisters[1] = value; }
        }
        public uint D2
        {
            get { return dataRegisters[2]; }
            private set { dataRegisters[2] = value; }
        }
        public uint D3
        {
            get { return dataRegisters[3]; }
            private set { dataRegisters[3] = value; }
        }
        public uint D4
        {
            get { return dataRegisters[4]; }
            private set { dataRegisters[4] = value; }
        }
        public uint D5
        {
            get { return dataRegisters[5]; }
            private set { dataRegisters[5] = value; }
        }
        public uint D6
        {
            get { return dataRegisters[6]; }
            private set { dataRegisters[6] = value; }
        }
        public uint D7
        {
            get { return dataRegisters[7]; }
            private set { dataRegisters[7] = value; }
        }
        uint[] dataRegisters;

        /// <summary>
        /// Address register A0
        /// </summary>
        public uint A0
        {
            get { return addressRegisters[0]; }
            private set { addressRegisters[0] = value; }
        }
        /// <summary>
        /// Address register A1
        /// </summary>
        public uint A1
        {
            get { return addressRegisters[1]; }
            private set { addressRegisters[1] = value; }
        }
        /// <summary>
        /// Address register A2
        /// </summary>
        public uint A2
        {
            get { return addressRegisters[2]; }
            private set { addressRegisters[2] = value; }
        }
        /// <summary>
        /// Address register A3
        /// </summary>
        public uint A3
        {
            get { return addressRegisters[3]; }
            private set { addressRegisters[3] = value; }
        }
        /// <summary>
        /// Address register A4
        /// </summary>
        public uint A4
        {
            get { return addressRegisters[4]; }
            private set { addressRegisters[4] = value; }
        }
        /// <summary>
        /// Address register A5
        /// </summary>
        public uint A5
        {
            get { return addressRegisters[5]; }
            private set { addressRegisters[5] = value; }
        }
        /// <summary>
        /// Address register A6
        /// </summary>
        public uint A6
        {
            get { return addressRegisters[6]; }
            private set { addressRegisters[6] = value; }
        }
        /// <summary>
        /// Address register A7
        /// </summary>
        public uint A7
        {
            get { return addressRegisters[7]; }
            private set { addressRegisters[7] = value; }
        }
        uint[] addressRegisters;

        /// <summary>
        /// User Stack Pointer (A7')
        /// </summary>
        public uint USP { get; private set; }
        /// <summary>
        /// System Stack Pointer (A7")
        /// </summary>
        public uint SSP { get; private set; }
        /// <summary>
        /// Program Counter (24-bit)
        /// </summary>
        public uint PC { get; private set; }

        /// <summary>
        /// Condition Code Register
        /// </summary>
        public byte CCR { get { return (byte)(SR & 0x1F); } }

        /// <summary>
        /// Status Register
        /// </summary>
        /// <remarks>
        /// 15 14 13 12 11 10  9  8  7  6  5  4  3  2  1  0
        ///  T     S  M  0     I     0  0  0  X  N  Z  V  C
        ///  0  0  1  0  0  1  1  1  0  0  0  0  0  0  0  0 - Reset (0x2700)
        ///                         User mode |-----------|
        /// </remarks>
        public ushort SR { get; private set; }
        public bool TracingEnabled
        {
            get { return (SR & 0x8000) != 0; }
            set
            {
                if (value)
                    SR |= 0x8000;
                else
                    SR = (ushort)(SR & ~0x8000);
            }
        }
        
        /* In later models... (MC68040 and the MC68881/MC68882)
        float FP0, FP1, FP2, FP3, FP4, FP5, FP6, FP7; // Floating-Point Data Registers (80 bits?)
        ushort FPCR; // Floating-Point Control Register (IEEE 754)

        uint FPSR; // Floating-Point Status Register
        //byte FPCC; // FPSR Condition Code Byte
        //byte QB;   // FPSR Quotient byte
        //byte EXC;  // FPSR Exception Status Byte
        //byte AEXC; // FPSR Accrued Exception Byte

        uint FPIAR; // Floating-Point Instruction Address Register
        */

        const byte BYTE_MAX = byte.MaxValue;
        const ushort WORD_MAX = ushort.MaxValue;
        const uint LONG_MAX = uint.MaxValue;

        const uint MEM_MAX = 0xFFFFFF; // 0x00FFFFFF, ‭16777215‬, 16 MB

        public MemoryModule Memory;

        // Constructors ✧(≖ ◡ ≖✿)

        public MC86000()
        {
            dataRegisters = new uint[8];
            addressRegisters = new uint[8];

            Memory = new MemoryModule();

            Memory.Bank = new byte[MEM_MAX];

            Reset();
        }

        // Methods ヾ(｡>﹏<｡)ﾉﾞ

        /// <summary>
        /// Asserts the RSTO signal for 124 clock periods,
        /// resetting all external devices. The processor state,
        /// other than the program counter, is unaffected,
        /// and execution continues with the next instruction.
        /// </summary>
        public void Reset()
        {
            A7 = SSP = Memory.ReadULong(0);

            PC = Memory.ReadULong(4);

            SR = SR_INIT;
        }

        public void Step()
        {
            //TODO: Step(void)
        }
        
        public void Insert(ushort op) // into the stack??
        {
            //TODO: Insert(ushort)
        }

        public void Interpret(string input)
        {
            //TODO: Interpret(string)
        }

        public void Execute(ushort op, uint arg = 0) // public for now..?
        {
            //TODO: Clean up (at some point)

            if (TracingEnabled)
            {
                Debug.WriteLine($"{PC:X8}  {op:X4}  {arg:X8}");
            }

            switch (op)
            {
                case 0x4AFC: //TODO: ILLEGAL
                    break;
                case 0x4E70: // RESET
                    //TODO: RESET IF Supervisor State
                    /*
                    If Supervisor State
                        Then Assert RESET (RSTO, MC68040 Only) Line
                    Else TRAP
                    */
                    Reset();
                    return;
                case 0x4E71: // NOP
                    return;
                case 0x4E72: //TODO: STOP

                    return;
            }
            
            // Grouping: 00 00 000 000 000 000
            //           |---| ...
            //            op
            int s = op & 0xF000;

            // Page 8-4, 8.2 OPERATION CODE MAP
            switch (s)
            {
                // 0000 - Bit manipulation
                case 0:
                    {

                    }
                    break;

                // 0001~0011 - MOVE, MOVEA
                case 1: // Move byte
                case 2: // Move long
                case 3: // Move word
                    {
                        // Page 4-116
                        // Size (s1)            - 00[00]000 000 000 000
                        // Destination register - 00 00[000]000 000 000
                        int m0 = op & 0xE00; //(op >> 9) & 7;
                        // Destination mode     - 00 00 000[000]000 000
                        int m1 = op & 0x1C0; //(op >> 6) & 7;
                        // Source mode          - 00 00 000 000[000]000
                        int m2 = op & 0x38;  //(op >> 3) & 7;
                        // Source register      - 00 00 000 000 000[000]
                        int m3 = op & 7;


                    }
                    break;

                // 0100 - Miscellaneous
                case 4:
                    {

                    }
                    break;

                // 0101 - ADDQ/SUBQ/Scc/DBcc/TRAPc c
                case 5:
                    {

                    }
                    break;

                // 0110 - Bcc/BSR/BRA
                case 6:
                    {

                    }
                    break;

                // 0111 - MOVEQ
                case 7:
                    {

                    }
                    break;

                // 1000 - OR/DIV/SBCD
                case 8:
                    {

                    }
                    break;

                // 1001 - SUB/SUBX
                case 9:
                    {

                    }
                    break;

                // 1010 - (Unassigned, Reserved)
                case 10:
                    {

                    }
                    break;

                // 1011 - CMP/EOR
                case 11:
                    {

                    }
                    break;

                // 1100 - AND/MUL/ABCD/EXG
                case 12:
                    {

                    }
                    break;

                // 1101 - ADD/ADDX
                case 13:
                    {

                    }
                    break;

                // 1110 - Shift/Rotate/Bit Field
                case 14:
                    {

                    }
                    break;

                // 1111 - Coprocessor Interface/ MC68040 and CPU32 Extensions
                case 15:
                    {

                    }
                    break;
            }
        }

        // Instructions ლ(•̀ _ •́ ლ)

        public void JMP(uint address)
        {
            PC = address;
        }
    }

    /// <summary>
    /// Compilers, decompilers, S-Record file maker, etc.
    /// </summary>
    public static class M68000Tools
    {
        const int DATA_BUS_WIDTH = 16;

        public enum FileFormat : byte { S19 = 16, S28 = 24, S37 = 32 }

        static class SRecord
        {
            public static void CompileToFile(string path, FileFormat format)
            {
                CompileToFile(path, format, Encoding.ASCII);
            }

            public static void CompileToFile(string path, FileFormat format, Encoding encoding, bool capitalized = true)
            {
                //TODO: CompileToFile(string)

                using (StreamWriter sw = new StreamWriter(path, false, encoding))
                {



                }
            }
        }


    }

    public class MemoryModule // static class for now
    {
        public MemoryModule() { }

        public readonly bool IsLE = BitConverter.IsLittleEndian;

        public byte[] Bank { get; set; }

        public uint ReadULong(int address)
        {
            // Increase PC here?

            return BitConverter.ToUInt32(Bank, address);
        }

        public int ReadLong(int address)
        {


            return BitConverter.ToInt32(Bank, address);
        }
    }
}
