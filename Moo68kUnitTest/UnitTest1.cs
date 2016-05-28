using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moo68k;

namespace Moo86kUnitTest
{
    [TestClass]
    public class Moo86kUnitTest
    {
        [TestMethod]
        public void DataRegisters()
        {
            MC86000 m68k = new MC86000();

            m68k.Execute(0x203C, 5);
            Assert.AreEqual(m68k.D0, 5);
            m68k.Execute(0x223C, 5);
            Assert.AreEqual(m68k.D1, 5);
            m68k.Execute(0x243C, 5);
            Assert.AreEqual(m68k.D2, 5);
            m68k.Execute(0x263C, 5);
            Assert.AreEqual(m68k.D3, 5);
            m68k.Execute(0x283C, 5);
            Assert.AreEqual(m68k.D4, 5);
            m68k.Execute(0x2A3C, 5);
            Assert.AreEqual(m68k.D5, 5);
            m68k.Execute(0x2C3C, 5);
            Assert.AreEqual(m68k.D6, 5);
            m68k.Execute(0x2E3C, 5);
            Assert.AreEqual(m68k.D7, 5);

            // Those two instructions is from a manual (mbsd_l2.pdf) from
            // Ricardo Gutierrez-Osuna, Wright State University

            // MOVE.L #$12,d0 | 00 10 000 000 111 100 | 203C 00000012
            // self note: puts 0x12 into register d0
            m68k.Execute(0x203C, 0x12);
            Assert.AreEqual(m68k.D0, 0x12);

            // MOVE.B data,d1 | 00 01 001 000 111 001 | 1239 00002000
            // self note: This goes at the address 2000 stored in data (a variable?) and
            //            loads "24" into d1 (manually?)
            //            Is this due to a higher language?
            m68k.Execute(0x1239, 0x2000);
            Assert.AreEqual(m68k.D1, 24);
        }
    }

    [TestClass]
    public class Moo86kToolsUnitTest
    {
        [TestMethod]
        public void Compiler()
        {

        }
    }
}