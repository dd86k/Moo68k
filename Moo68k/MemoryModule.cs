using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moo68k
{
    /// <summary>
    /// Represents a memory module.
    /// </summary>
    /// <remarks>
    /// Not to worry about endianness! BitConverter takes care of that for us.
    /// Also never stackalloc the memory.. it's supposed to be on the heap.
    /// </remarks>
    public class MemoryModule // static class for now
    {
        public MemoryModule(int capacity = 0xFFFF)
        {
            Bank = new byte[capacity];
        }

        /// <summary>
        /// Memory bank, which enables you to do some memory mapping!
        /// </summary>
        //TODO: Consider using a Dictionary<int, byte>(capacity); <address, data>
        //   .. Even though writing and reading times may be slower
        public byte[] Bank { get; private set; }

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
