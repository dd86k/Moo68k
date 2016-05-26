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
 * Where to increase Program Counter?
 *     
 * Decide what to make as public property.
 *     
 * Base class or interface?
 *     Probably class.
 */

/* set it with OR
 * toggle with XOR
 * unset with AND NOT */

namespace Moo68k
{
    [Flags]
    public enum CCRFlags : byte
    {
        C = 1, V = 2, Z = 4, N = 8, X = 16 // Am I even going to use these?
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
    /// Represents a Motorola 68000 microprocessor.
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
        const int MEM_MAX = 0xFFFFFF; // 0x00FFFFFF, ‭16777215‬, 16 MB

        // Properties ヽ(•̀ω•́ )ゝ

        /* Data registers */

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
        
        /* Address registers */

        public uint A0
        {
            get { return addressRegisters[0]; }
            private set { addressRegisters[0] = value; }
        }
        public uint A1
        {
            get { return addressRegisters[1]; }
            private set { addressRegisters[1] = value; }
        }
        public uint A2
        {
            get { return addressRegisters[2]; }
            private set { addressRegisters[2] = value; }
        }
        public uint A3
        {
            get { return addressRegisters[3]; }
            private set { addressRegisters[3] = value; }
        }
        public uint A4
        {
            get { return addressRegisters[4]; }
            private set { addressRegisters[4] = value; }
        }
        public uint A5
        {
            get { return addressRegisters[5]; }
            private set { addressRegisters[5] = value; }
        }
        public uint A6
        {
            get { return addressRegisters[6]; }
            private set { addressRegisters[6] = value; }
        }
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
        /// System Stack Pointer (A7") (ISP and/or MSP?)
        /// </summary>
        public uint SSP { get; private set; }
        /// <summary>
        /// Program Counter (24-bit)
        /// </summary>
        public uint PC { get; private set; }

        /// <summary>
        /// Status Register.
        /// </summary>
        /// <remarks>
        /// Page 1-11, 1.3.2
        /// 15 14 13 12 11 10  9  8  7  6  5  4  3  2  1  0
        /// T1 T0  S  M  0 I2 I1 I0  0  0  0  X  N  Z  V  C
        ///  0  0  1  0  0  1  1  1  0  0  0  0  0  0  0  0 - Reset (0x2700)
        /// |------------------------|--------------------|
        ///        System byte           User byte (CCR)
        /// --
        /// T1, T0 - TRACE MODE
        ///  0   0 - NO TRACE
        ///  1   0 - TRACE ON ANY INSTRUCTION
        ///  0   1 - TRACE ON CHANGE OF FLOW
        ///  1   1 - UNDEFINED
        /// NOTE: MC68000, MC68EC000, MC68008, MC68010, MC68HC000, MC68HC001, and CPU32
        ///       Only one trace mode supported, where the T0-bit is always zero, and
        ///       only one system stack where the M-bit is always zero.
        /// --
        /// S - Supervisor mode
        /// M - Master/Interupt state
        /// S, M - ACTIVE STACK
        /// 0, x - USP (x being 0 or 1)
        /// 1, 0 - ISP
        /// 1, 1 - MSP
        /// --
        /// I1, I2, I3 - Interrupt mask level.
        /// </remarks>
        public ushort SR { get; private set; }
        /// <summary>
        /// Condition Code Register byte.
        /// </summary>
        public byte CCR { get { return (byte)(SR & 0xFF); } }
        /* System byte */
        public bool FlagTracingEnabled
        {
            get { return (SR & 0x8000) != 0; }
            set
            {
                //TODO: Tracing is only with supervisor powers!

                //SR = value ? (ushort)(SR | 0x8000) : (ushort)(SR & ~0x8000); could be cool

                if (value)
                    SR |= 0x8000;
                else
                    SR = (ushort)(SR & ~0x8000);
            }
        }
        public bool FlagIsSupervisor
        {
            get { return (SR & 0x2000) != 0; }
            set
            {
                if (value)
                    SR |= 0x2000;
                else
                    SR = (ushort)(SR & ~0x2000);
            }
        }
        //TODO: Interrupt mask level property
        /* User byte */
        public bool FlagIsExtend
        {
            get { return (SR & 0x10) != 0; }
            private set
            {
                if (value)
                    SR |= 0x10;
                else
                    SR = (ushort)(SR & ~0x10);
            }
        }
        public bool FlagIsNegative
        {
            get { return (SR & 0x8) != 0; }
            private set
            {
                if (value)
                    SR |= 0x8;
                else
                    SR = (ushort)(SR & ~0x8);
            }
        }
        public bool FlagIsZero
        {
            get { return (SR & 0x4) != 0; }
            private set
            {
                if (value)
                    SR |= 0x4;
                else
                    SR = (ushort)(SR & ~0x4);
            }
        }
        public bool FlagIsOverflow
        {
            get { return (SR & 0x2) != 0; }
            private set
            {
                if (value)
                    SR |= 0x2;
                else
                    SR = (ushort)(SR & ~0x2);
            }
        }
        public bool FlagIsCarry
        {
            get { return (SR & 0x1) != 0; }
            private set
            {
                if (value)
                    SR |= 0x1;
                else
                    SR = (ushort)(SR & ~0x1);
            }
        }

        /* In later models... (MC68040 and the MC68881/MC68882)
        float FP0, FP1, FP2, FP3, FP4, FP5, FP6, FP7; // Floating-Point Data Registers (80 bits)
        ushort FPCR; // Floating-Point Control Register (IEEE 754)

        uint FPSR; // Floating-Point Status Register <-+
                                                       |
        //byte FPCC; // FPSR Condition Code Byte   ----+
        //byte QB;   // FPSR Quotient byte   ----------+
        //byte EXC;  // FPSR Exception Status Byte   --+
        //byte AEXC; // FPSR Accrued Exception Byte   -+

        uint FPIAR; // Floating-Point Instruction Address Register
        */
        
        public MemoryModule Memory;

        // Constructors ✧(≖ ◡ ≖✿)

        public MC86000()
        {
            dataRegisters = new uint[8];
            addressRegisters = new uint[8];

            Memory = new MemoryModule(0xFFFF); //MEM_MAX

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

        /// <summary>
        /// Compile instructions and execute.
        /// </summary>
        /// <param name="input">Assembly.</param>
        public void Interpret(string input)
        {
            //TODO: Interpret(string)
        }

        public void Execute(ushort opcode, uint operand = 0) // public for now..?
        {
            //TODO: Clean up (at some point)

            if (FlagTracingEnabled)
                Trace.WriteLine($"{PC:X8}  {opcode:X4}  {operand:X8}");

            //[0000]0000 0000 0000
            // Operation Code
            int oc = (opcode >> 12) & 0xF;

            // Page 8-4, 8.2 OPERATION CODE MAP
            switch (oc)
            {
                // 0000 - Bit manipulation
                case 0:
                    {

                    }
                    break;

                // 0001~0011
                case 1: // Move byte
                case 2: // Move long
                case 3: // Move word
                    {
                        // Size, Page 4-116
                        int sz = (opcode >> 12) & 3; // 00[00]000 000 000 000

                        // Desition, Page 4-117
                        // Destination register
                        int dr = (opcode >> 9) & 7; //  00 00[000]000 000 000
                        // Destination mode
                        int dm = (opcode >> 6) & 7; //  00 00 000[000]000 000
                        
                        // Source, Page 4-118
                        // Source mode
                        int sm = (opcode >> 3) & 7; //  00 00 000 000[000]000
                        // Source register
                        int sr = opcode & 7; //         00 00 000 000 000[000]

                        if (FlagTracingEnabled)
                            Trace.WriteLine($"sz={sz} dr={dr} dm={dm} sm={sm} sr={sr}");

                        switch (sm) // Source Effective Address field
                        {
                            case 0: // 000 Dn
                                switch (dm) // Destination Effective Address field
                                {
                                    // Where to put An/#<DATA>/(d16,PC)/(d8,PC,Xn)? Probably ILLEGAL (default)

                                    case 0: // 000 Dn
                                        dataRegisters[dr] = dataRegisters[sr];

                                        FlagIsNegative = dataRegisters[sr] < 0; //..I doubt that will work.
                                        FlagIsZero = dataRegisters[sr] == 0; // Also, is the source the data specified?
                                        break;
                                    case 2: // 010 (An)

                                        break;
                                    case 3: // 011 (An)+

                                        break;
                                    case 4: // 100 -(An)

                                        break;
                                    case 5: // 101 (d16, An)

                                        break;
                                    case 6: // 110 (d8, An, Xn) 

                                        break;
                                    case 7: // 111
                                        switch (dr)
                                        {
                                            case 0: // Word

                                                break;
                                            case 1: // Long

                                                break;
                                        }
                                        break;
                                }
                                break;

                            case 1: // 001 An
                                switch (dm)
                                {
                                    case 0: // 000 Dn
                                        dataRegisters[dr] = addressRegisters[sr];

                                        FlagIsNegative = dataRegisters[sr] < 0;
                                        FlagIsZero = dataRegisters[sr] == 0;
                                        break;
                                    case 2: // 010 (An)

                                        break;
                                    case 3: // 011 (An)+

                                        break;
                                    case 4: // 100 -(An)

                                        break;
                                    case 5: // 101 (d16, An)

                                        break;
                                    case 6: // 110 (d8, An, Xn) 

                                        break;
                                    case 7: // 111
                                        switch (dr)
                                        {
                                            case 0: // Word

                                                break;
                                            case 1: // Long

                                                break;
                                        }
                                        break;
                                }
                                break;
                            case 2: // 010 (An)

                                break;
                            case 3: // 011 (An)+

                                break;
                            case 4: // 100 -(An)

                                break;
                            case 5: // 101 (d16, An)

                                break;
                            case 6: // 110 (d8, An, Xn)

                                break;
                            case 7: // 111

                                break;
                        }

                        if (dm != 2)
                        {
                            // X N Z V C
                            // - * * 0 0
                            FlagIsOverflow = false;
                            FlagIsCarry = false;
                        }
                    }
                    break;

                // 0100 - Miscellaneous
                case 4:
                    {
                        // Some constant opcodes
                        switch (opcode)
                        {
                            case 0x4AFC: //TODO: ILLEGAL
                                break;
                            case 0x4E70: // RESET
                                if (FlagIsSupervisor)
                                    Reset();
                                //TODO: else TRAP
                                return;
                            case 0x4E71: // NOP
                                return;
                            case 0x4E72: //TODO: STOP

                                return;
                            case 0x4E73: //TODO: RTE

                                break;
                            case 0x4E75: //TODO: RTS

                                break;
                            case 0x4E76: //TODO: TRAPV

                                break;
                            case 0x4E77: //TODO: RTR

                                break;

                            default:
                                {

                                }
                                break;
                        }
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

                // 1010 - Unassigned, Reserved
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

                // 1111 - Coprocessor Interface/MC68040 and CPU32 Extensions
                case 15:
                    {

                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Compilers, decompilers, S-Record file maker, etc.
    /// </summary>
    public static class M68000Tools
    {
        /* Notes
         * - Abuse << to push in bits while compiling
         */

        const int DATA_BUS_WIDTH = 16;
        
        public enum FileFormat : byte { S19 = 16, S28 = 24, S37 = 32 }

        static class SRecord
        {
            public static string Compile(ref byte[] Memory, string path)
            {
                //TODO: Compile(string)

                return "";
            }

            public static void CompileToFile(ref byte[] memory, string path)
            {
                CompileToFile(ref memory, path, FileFormat.S28, Encoding.ASCII);
            }

            public static void CompileToFile(ref byte[] memory, string path, FileFormat format)
            {
                CompileToFile(ref memory, path, format, Encoding.ASCII);
            }

            public static void CompileToFile(ref byte[] memory, string path, FileFormat format, Encoding encoding, bool capitalized = true)
            {
                //TODO: CompileToFile(string)

                using (StreamWriter sw = new StreamWriter(path, false, encoding))
                {



                }
            }
        }


    }

    /// <summary>
    /// Represents a memory module.
    /// </summary>
    /// <remarks>
    /// Not to worry about endianness! BitConverter takes care of that for us.
    /// Also never stackalloc the memory.. it's supposed to be on the heap.
    /// </remarks>
    public class MemoryModule // static class for now
    {
        public MemoryModule()
        {
            Bank = new byte[0xFFFF];
        }

        public MemoryModule(int capacity)
        {
            Bank = new byte[capacity];
        }

        /// <summary>
        /// Memory bank, which enables you to do some memory mapping!
        /// </summary>
        public byte[] Bank { get; set; } // Maybe do private set?

        // Byte

        public byte ReadByte(int address)
        {
            return Bank[address];
        }

        public void WriteByte(int address, byte value)
        {
            Bank[address] = value;
        }

        // Word

        public short ReadWord(int address)
        {
            return BitConverter.ToInt16(Bank, address);
        }

        public void WriteWord(int address, short value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        // Unsigned word

        public ushort ReadUWord(int address)
        {
            return BitConverter.ToUInt16(Bank, address);
        }

        public void WriteUWord(int address, ushort value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        // Long

        public int ReadLong(int address)
        {
            return BitConverter.ToInt32(Bank, address);
        }

        public void WriteLong(int address, int value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        // Unsigned long

        public uint ReadULong(int address)
        {
            return BitConverter.ToUInt32(Bank, address);
        }

        public void WriteULong(int address, uint value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        // Misc.

        void Write(int address, byte[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                Bank[address + i] = values[i];
            }
        }
    }
}
