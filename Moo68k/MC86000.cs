using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
 *     
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
        const int S0_SHIFT = 8;
        const int S1_SHIFT = 6;

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
        /// <remarks>
        /// 15 14 13 12 11 10  9  8  7  6  5  4  3  2  1  0
        ///  T     S  M  0     I     0  0  0  X  N  Z  V  C
        ///                         User mode |-----------|
        /// </remarks>
        public byte CCR { get { return (byte)(SR & 5); } }

        /// <summary>
        /// Status Register
        /// </summary>
        public ushort SR { get; private set; }


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

            //TODO: Power-up state
        }

        // Methods ヾ(｡>﹏<｡)ﾉﾞ

        /// <summary>
        /// Asserts the RSTO signal for 512 (124 for MC68000, MC68EC000, 
        /// MC68HC000, MC68HC001, MC68008, MC68010, and MC68302) clock periods,
        /// resetting all external devices.The processor state, other than the program counter, is 
        /// unaffected, and execution continues with the next instruction.
        /// </summary>
        public void Reset()
        {
            A7 = SSP = Memory.ReadULong(0);

            PC = Memory.ReadULong(4);

            SR = SR_INIT;
        }

        public void Step()
        {

        }
        
        public void Insert(ushort op) // into the stack??
        {

        }

        public void Interpret(string input)
        {
            //TODO: Interpret(string)
        }

        public void Execute(ushort op, uint arg = 0) // public for now..?
        {
            //TODO: Clean up (at some point)
            
            //TODO: tracing defined in SR register
            Debug.Write($"{PC:X8}  {op:X4}");
            if (arg > 0)
                Debug.WriteLine($"  {arg:X8}");

            switch (op)
            {
                /*case 0x4AFC: //TODO: ILLEGAL
                 * 
                 * ILLEGAL is not in an MC68000
                 * But in MC68EC000, MC68010, MC68020, MC68030, MC68040, CPU32

                    break;*/
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
            //           s0 s1 ...
            int s0 = op >> S0_SHIFT;
            int s1 = op >> S1_SHIFT;

            switch (s0)
            {
                case 0: // 00
                    {
                        if (s1 > 0) // MOVE, MOVEA
                        {
                            // 00 00 000 000 000 000
                            //       |-------------|
                            int m0 = (op >> 9) & 7;
                            int m1 = (op >> 6) & 7;
                            int m2 = (op >> 3) & 7;
                            int m3 = op & 7;


                        }
                        else // 00
                        {

                        }
                    }
                    break;

                case 1: // 01
                    {
                        switch (s1)
                        {
                            case 0: // 00
                                {

                                }
                                break;
                        }
                    }
                    break;

                case 2: // 10

                    break;

                case 3: // 11

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
    static class M68000Tools
    {

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
