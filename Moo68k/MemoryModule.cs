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

        public short ReadWord(int address)
        {
            return BitConverter.ToInt16(Bank, address);
        }

        public short ReadWord(uint address)
        {
            return BitConverter.ToInt16(Bank, (int)address);
        }

        public void WriteWord(int address, short value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        public ushort ReadUWord(int address)
        {
            return BitConverter.ToUInt16(Bank, address);
        }

        public void WriteUWord(int address, ushort value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        // Long / Unsigned Long

        public int ReadLong(int address)
        {
            return BitConverter.ToInt32(Bank, address);
        }

        public int ReadLong(uint address)
        {
            return BitConverter.ToInt32(Bank, (int)address);
        }

        public void WriteLong(int address, int value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        public uint ReadULong(int address)
        {
            return BitConverter.ToUInt32(Bank, address);
        }

        public void WriteULong(int address, uint value)
        {
            Write(address, BitConverter.GetBytes(value));
        }

        // Misc.

        void Write(int address, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; ++i)
            {
                Bank[address + i] = bytes[i];
            }
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
