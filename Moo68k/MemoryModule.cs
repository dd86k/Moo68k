using System;

//TODO: Manual memory mapping instead of using BitConverter

namespace Moo68k
{
    /// <summary>
    /// Represents a memory module.
    /// </summary>
    /// <remarks>
    /// Remember, Big Endianess:
    /// Highest byte is stored first, then going up in memory,
    /// the lower bytes will rest.
    /// </remarks>
    public class MemoryModule
    {
        public MemoryModule(int capacity = 0xFFFF)
        {
            Bank = new byte[capacity];
        }

        /// <summary>
        /// Memory bank, containing the data.
        /// </summary>
        public byte[] Bank { get; private set; }
        //TODO: Consider: Use "smart" Dictionary<int, int>?

        // Byte

        public byte ReadByte(int address)
        {
            return Bank[address];
        }
        
        public void WriteByte(int address, byte value)
        {
            Bank[address] = value;
        }

        // Word / Unsigned Word

        public unsafe short ReadWord(int address)
        {
            fixed (byte* p = Bank)
                return *((short*)p + (address / sizeof(short)));
        }

        public unsafe short ReadWord(uint address)
        {
            fixed (byte* p = Bank)
                return *((short*)p + (address / sizeof(short)));
        }

        public unsafe ushort ReadUWord(int address)
        {
            fixed (byte* p = Bank)
                return *((ushort*)p + (address / sizeof(ushort)));
        }

        public unsafe void WriteWord(int address, short value)
        {
            fixed (byte* p = Bank)
                *((short*)p + (address / sizeof(short))) = value;
        }

        public unsafe void WriteUWord(int address, ushort value)
        {
            fixed (byte* p = Bank)
                *((ushort*)p + (address / sizeof(ushort))) = value;
        }

        // Long / Unsigned Long

        public unsafe int ReadLong(int address)
        {
            fixed (byte* p = Bank)
                return *((int*)p + (address / sizeof(int)));
        }

        public unsafe int ReadLong(uint address)
        {
            fixed (byte* p = Bank)
                return *((int*)p + (address / sizeof(int)));
        }

        public unsafe uint ReadULong(int address)
        {
            fixed (byte* p = Bank)
                return *((uint*)p + (address / sizeof(uint)));
        }

        public unsafe void WriteLong(int address, int value)
        {
            fixed (byte* p = Bank)
                *((int*)p + (address / sizeof(int))) = value;
        }

        public unsafe void WriteULong(int address, uint value)
        {
            fixed (byte* p = Bank)
                *((uint*)p + (address / sizeof(uint))) = value;
        }

        // Indexers

        public byte this[int address]
        {
            get
            {
                return Bank[address];
            }
            set
            {
                Bank[address] = value;
            }
        }
    }
}
