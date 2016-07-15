using System;
using System.Diagnostics;
using System.IO;
using System.Text;

/* set it with OR
 * toggle with XOR
 * unset with AND NOT */

namespace Moo68k
{
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

        /* What to do with those three?
         * Only one register is active, but..
         * Eh
         */
        /// <summary>
        /// Stack Pointer
        /// </summary>
        public uint SP { get; private set; }
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
        /// |---------------------|  |--------------------|
        ///       System byte           User byte (CCR)
        /// --
        /// T1, T0 - TRACE MODE
        ///  0   0 - NO TRACE
        ///  1   0 - TRACE ON ANY INSTRUCTION
        ///  0   1 - TRACE ON CHANGE OF FLOW
        ///  1   1 - UNDEFINED
        /// NOTE: MC68000, MC68EC000, MC68008, MC68010, MC68HC000, MC68HC001, and CPU32
        ///       only has one trace mode supported, where the T0-bit is always zero, and
        ///       only one system stack where the M-bit is always zero.
        /// --
        /// S - Supervisor mode
        /// M - Master/Interupt state
        /// S, M - ACTIVE STACK
        /// 0, x - USP (x being 0 or 1)
        /// 1, 0 - ISP (Read note)
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
        //byte FPCC; // FPSR Condition Code Byte ------+
        //byte QB;   // FPSR Quotient byte ------------+
        //byte EXC;  // FPSR Exception Status Byte ----+
        //byte AEXC; // FPSR Accrued Exception Byte ---+

        uint FPIAR; // Floating-Point Instruction Address Register
        */
        
        /// <summary>
        /// The installed <see cref="MemoryModule"/>.
        /// </summary>
        public MemoryModule Memory;

        // Constructors ✧(≖ ◡ ≖✿)

        /// <summary>
        /// Constructs a new MC86000.
        /// </summary>
        /// <param name="memorysize">Optional memory size in bytes.</param>
        public MC86000(int memorysize = 0xFFFF)
        {
            Trace.AutoFlush = true;

            dataRegisters = new uint[8];
            addressRegisters = new uint[8];

            Memory = new MemoryModule(memorysize);

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
            A7 = SP = Memory.ReadULong(0);

            PC = Memory.ReadULong(4);

            SR = SR_INIT;
        }

        public void Step()
        {
            //TODO: Step(void)
        }

        /// <summary>
        /// Compile from mnemonic instructions and execute.
        /// </summary>
        /// <param name="input">Mnemonic instructions.</param>
        public void InterpretAssembly(string input)
        {
            //TODO: Interpret(string)
        }

        /// <summary>
        /// Compile from a S-Record line and execute.
        /// </summary>
        /// <param name="input">S-Record line.</param>
        public void InterpretSRecord(string input)
        {
            //TODO: InterpretSRecord(string)
        }

        /// <summary>
        /// Seperate the operation code from the operand and execute.
        /// </summary>
        /// <param name="instruction">Combined instruction.</param>
        public void Execute(ulong instruction)
        {
            Execute((uint)(instruction >> 32), (uint)(instruction & 0xFFFFFFFF));
        }

        /// <summary>
        /// Seperate the operation code from the operand and execute.
        /// </summary>
        /// <param name="instruction">Combined instruction.</param>
        public void Execute(long instruction)
        {
            Execute((uint)(instruction >> 32), (uint)(instruction & 0xFFFFFFFF));
        }

        /// <summary>
        /// Execute an operation code with an optional operand.
        /// </summary>
        /// <param name="opcode">Operation code.</param>
        /// <param name="operand">Operand.</param>
        public void Execute(uint opcode, uint operand = 0)
        {
            //TODO: Clean up (at some point)

            if (FlagTracingEnabled)
                Trace.WriteLine($"{PC:X8}  {opcode:X4}  {operand:X8}");

            // [0000]0000 0000 0000
            // Operation Code
            uint oc = (opcode >> 12) & 0xF;

            // Page 8-4, 8.2 OPERATION CODE MAP
            switch (oc)
            {
                #region 0000 - Bit manipulation
                case 0:
                    {

                    }
                    break;
                #endregion
                
                #region 0001~0011 - Move
                case 1: // Move byte
                case 2: // Move long
                case 3: // Move word
                    {
                        // Size, Page 4-116
                        uint sz = (opcode >> 12) & 3; // 00[00]000 000 000 000

                        // Desition, Page 4-117
                        // Destination register
                        uint dr = (opcode >> 9) & 7; //  00 00[000]000 000 000
                        // Destination mode
                        uint dm = (opcode >> 6) & 7; //  00 00 000[000]000 000
                        
                        // Source, Page 4-118
                        // Source mode
                        uint sm = (opcode >> 3) & 7; //  00 00 000 000[000]000
                        // Source register
                        uint sr = opcode & 7; //         00 00 000 000 000[000]

                        if (FlagTracingEnabled)
                            Trace.WriteLine($"MOVE sz={sz} dr={dr} dm={dm} sm={sm} sr={sr}");
                        
                        //TODO: Re-structure
                        // Structure: sm -> dm -> dr .. sr (for now)
                        
                        switch (sm) // Source Effective Address field
                        {
                            case 0: // 000 Dn
                                switch (dm) // Destination Effective Address field
                                {
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
                                            case 0: // 000 Word

                                                break;
                                            case 1: // 001 Long

                                                break;
                                            case 2: // 010 (d16, PC)

                                                break;
                                            case 3: // 011 (d8, PC, Xn) 

                                                break;
                                            case 4: // 100 #<data>
                                                dataRegisters[dr] = operand;

                                                FlagIsNegative = operand < 0;
                                                FlagIsZero = operand == 0;
                                                break;
                                        }
                                        break;
                                } // Destination Effective Address field
                                break;

                            case 1: // 001 An
                                switch (dm)
                                {
                                    case 0: // 000 Dn
                                        dataRegisters[dr] = addressRegisters[sr];

                                        FlagIsNegative = addressRegisters[sr] < 0;
                                        FlagIsZero = addressRegisters[sr] == 0;
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
                                    case 7: // 111 Immidiate
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
                                switch (dm) // Destination mode
                                {
                                    case 0: // 000 Dn
                                        switch (sr)
                                        {
                                            case 0: // 000 (xxx).W
                                            case 1: // 001 (xxx).L
                                                dataRegisters[dr] = operand;
                                                //TODO: else what if "size" is byte (01)
                                                break;
                                            case 2: // 010 (d16, PC)

                                                break;
                                            case 3: // 011 (d8, PC, Xn)

                                                break;
                                            case 4: // 100 #<data>
                                                dataRegisters[dr] = operand;
                                                break;
                                        }
                                        break;

                                    case 2: // 010 (An)
                                        switch (sr)
                                        {
                                            case 0: // 000 (xxx).W
                                                addressRegisters[dr] = (uint)Memory.ReadWord(operand);
                                                break;
                                            case 1: // 001 (xxx).L
                                                addressRegisters[dr] = (uint)Memory.ReadLong(operand);
                                                //TODO: else what if "size" is byte (01)
                                                break;
                                            case 2: // 010 (d16, PC)

                                                break;
                                            case 3: // 011 (d8, PC, Xn)

                                                break;
                                            case 4: // 100 #<data>
                                                addressRegisters[dr] = operand; // I think?
                                                break;
                                        }
                                        break;
                                }
                                break;
                        } // Source Effective Address field

                        if (dm != 2)
                        {
                            // X N Z V C
                            // - * * 0 0
                            FlagIsOverflow = false;
                            FlagIsCarry = false;
                        }
                    }
                    break;
                #endregion 0001~0011
                
                #region 0100 - Miscellaneous
                case 4:
                    {
                        // Some constant opcodes
                        switch (opcode)
                        {
                            case 0x4AFC: //TODO: ILLEGAL
                                /* Operation:
                                *SSP – 2 → SSP; Vector Offset → (SSP);
                                SSP – 4 → SSP; PC → (SSP);
                                SSP – 2 → SSP; SR → (SSP);
                                Illegal Instruction Vector Address → PC

                                * The MC68000 and MC68008 cannot write the vector
                                * offset and format code to the system stack. */



                                break;
                            case 0x4E70: // RESET
                                if (FlagIsSupervisor)
                                    Reset();
                                //TODO: else TRAP -- But what vector?
                                return;
                            case 0x4E71: // NOP Page 4-147
                                //TODO: Page 4-147
                                return;
                            case 0x4E72: //TODO: STOP

                                return;
                            case 0x4E73: //TODO: RTE

                                break;
                            case 0x4E75: //TODO: RTS

                                break;
                            case 0x4E76: // TRAPV Page 4-191
                                if (FlagIsOverflow)
                                {
                                    //TODO: TRAPV exception with a vector number 7
                                    // See Page B-2, table B-1

                                    TRAP(7);
                                } // Or else do nothing
                                return;
                            case 0x4E77: //TODO: RTR

                                break;

                            default:
                                {

                                }
                                break;
                        }
                    }
                    break;
                #endregion
                
                #region 0101 - ADDQ/SUBQ/Scc/DBcc/TRAPcc
                case 5:
                    {

                    }
                    break;
                #endregion
                
                #region 0110 - Bcc/BSR/BRA
                case 6:
                    {

                    }
                    break;
                #endregion
                
                #region 0111 - MOVEQ
                case 7:
                    {
                        // Page 4-134
                        // Register 0111 nnn0 0000 0000
                        // Data     0111 0000 nnnn nnnn
                        uint data = opcode & 0xFF;

                        dataRegisters[(opcode >> 9) & 7] = data;

                        FlagIsNegative = data < 0;
                        FlagIsZero = data == 0;
                        FlagIsCarry = false;
                        FlagIsOverflow = false;
                    }
                    break;
                #endregion
                
                #region 1000 - OR/DIV/SBCD
                case 8:
                    {

                    }
                    break;
                #endregion
                
                #region 1001 - SUB/SUBX/SUBA
                case 9:
                    {
                        // Page 4-174
                        // Register                   1001 nnn 000 000 000
                        uint reg = (opcode >> 9) & 7;

                        // Opmode *                   1001 000 nnn 000 000
                        uint mode = (opcode >> 6) & 7;
                        
                        // Effective Address Mode     1001 000 000 nnn 000
                        uint eamode = (opcode >> 3) & 7;

                        // Effective Address Register 1001 000 000 000 nnn
                        uint eareg = opcode & 7;

                        /*
                            * Opmode field
                            Byte  Word  Long  Operation
                            000   001   010   Dn – <ea> = Dn
                            100   101   110   <ea> – Dn = <ea>
                        */

                        switch (mode)
                        {
                            // Dn – <ea> = Dn
                            case 0:
                            case 1:
                            case 2:
                                switch (eamode) // <ea>
                                {
                                    case 0: // 000 Dn
                                        dataRegisters[reg] -= dataRegisters[eareg];
                                        break;
                                    case 1: // 001 An
                                        // For byte-sized operation, address register direct is not allowed.
                                        if (mode != 0)
                                            addressRegisters[eareg] -= operand;
                                        break;
                                    case 2: // 010 (An)
                                        {
                                            uint address = addressRegisters[eareg];


                                        }
                                        break;
                                    case 3: // 011 (An)+

                                        break;
                                    case 4: // 100 -(An)

                                        break;
                                    case 5: // 101 (d16, An)

                                        break;
                                    case 6: // 110 (d8, An, Xn)

                                        break;
                                    case 7: // Immidiate
                                        switch (eareg)
                                        {
                                            case 0: // 000 (xxx).W

                                                break;
                                            case 1: // 001 (xxx).L

                                                break;
                                            case 2: // 010 (d16, PC)

                                                break;
                                            case 3: // 011 (d8, PC, Xn)

                                                break;
                                            case 4: // 100 #<data>
                                                dataRegisters[reg] -= operand;
                                                break;
                                        }
                                        break;
                                }
                                break;

                            // <ea> – Dn = <ea>
                            case 4:
                            case 5:
                            case 6:
                                switch (eamode)
                                {
                                    case 2: // 010 (An)
                                        {
                                            int address = (int)addressRegisters[eareg];

                                            switch (mode)
                                            {
                                                case 0:
                                                    Memory.WriteByte(address, (byte)(Memory.ReadByte(address) - operand));
                                                    break;
                                                case 1:
                                                    Memory.WriteWord(address, (short)(Memory.ReadWord(address) - operand));
                                                    break;
                                                case 2:
                                                    Memory.WriteLong(address, (int)(Memory.ReadLong(address) - operand));
                                                    break;
                                            }
                                        }
                                        break;
                                    case 3: // 011 (An)+

                                        break;
                                    case 4: // 100 -(An)

                                        break;
                                    case 5: // 101 (d16, An)

                                        break;
                                    case 6: // 110 (d8, An, Xn)

                                        break;
                                    case 7:
                                        switch (eareg)
                                        {
                                            case 0: // 000 (xxx).W

                                                break;
                                            case 1: // 001 (xxx).L

                                                break;
                                            case 2: // 010 (d16, PC)

                                                break;
                                            case 3: // 011 (d8, PC, Xn)

                                                break;
                                            case 4: // 100 #<data>

                                                break;
                                        }
                                        break;
                                }
                                break;
                        }
                        
                        /*
                            X — Set to the value of the carry bit.
                            N — Set if the result is negative; cleared otherwise.
                            Z — Set if the result is zero; cleared otherwise.
                            V — Set if an overflow is generated; cleared otherwise.
                            C — Set if a borrow is generated; cleared otherwise.
                        */
                    }
                    break;
                #endregion
                
                #region 1010 - Unassigned, Reserved
                case 10:
                    {

                    }
                    break;
                #endregion
                
                #region 1011 - CMP/EOR
                case 11:
                    {

                    }
                    break;
                #endregion
                
                #region 1100 - AND/MUL/ABCD/EXG
                case 12:
                    {

                    }
                    break;
                #endregion
                
                #region 1101 - ADD/ADDX
                case 13:
                    {

                    }
                    break;
                #endregion
                
                #region 1110 - Shift/Rotate/Bit Field
                case 14:
                    {

                    }
                    break;
                #endregion
                
                #region 1111 - Coprocessor Interface/MC68040 and CPU32 Extensions
                /* Does the MC86000 even use this? I doubt. */
                case 15:
                    {
                        
                    }
                    break;
                #endregion
            }
        }

        /// <summary>
        /// Causes a TRAP #<vector> exception.
        /// </summary>
        /// <param name="vector">Vector ranging from 0 to 255.</param>
        /// <remarks>
        /// The MC68000 and MC68008 do not write vector offset or format code to the system stack.
        /// </remarks>
        public void TRAP(byte vector) // Page 4-188
        {
            // consider publicness?!

            //TODO: TRAP(byte);
            /*
             Operation:
                1 → S-Bit of SR
                *SSP – 2 → SSP; Format/Offset → (SSP); (see <remarks>)
                SSP – 4 → SSP; PC → (SSP); SSP – 2 → SSP;
                SR → (SSP); Vector Address → PC
             */
        }
    }
}
