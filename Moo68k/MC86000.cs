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

namespace Emuumuu
{
    [Flags]
    enum CCRFlags : byte
    {
        C = 1, V = 2, Z = 4, N = 8, X = 16
    }

    [Flags]
    enum CCRFlagsSimplified : byte
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
    class MC86000
    {
        // Constants ⁄(⁄ ⁄•⁄ω⁄•⁄ ⁄)⁄

        const ushort SR_INIT = 0x2700;

        // Properties ヽ(•̀ω•́ )ゝ

        /// <summary>
        /// Data register.
        /// </summary>
        public uint D0, D1, D2, D3, D4, D5, D6, D7;
        /// <summary>
        /// Address register.
        /// </summary>
        public uint A0, A1, A2, A3, A4, A5, A6, A7;
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
            Memory = new MemoryModule();

            Memory.Bank = new byte[MEM_MAX];

            //TODO: Power-up state
        }

        // Methods ヾ(｡>﹏<｡)ﾉﾞ

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

        public void Execute(ushort op)
        {
            //TODO: Clean up (at some point)

            // Big Endian
            byte higher = (byte)(op >> 8);
            byte lower = (byte)(op);

            // Half nyblets (2 bits)
            byte h0 = (byte)(higher >> 6);
            byte h1 = (byte)((higher >> 4) & 3);
            byte h2 = (byte)((higher >> 2) & 3);
            byte h4 = (byte)(higher & 3);
            byte l0 = (byte)(lower >> 6);
            byte l1 = (byte)((lower >> 4) & 3);
            byte l2 = (byte)((lower >> 2) & 3);
            byte l4 = (byte)(lower & 3);

            Debug.WriteLine($"{PC:X8}  {op:X4}");

            switch (h0)
            {
                case 0:

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
