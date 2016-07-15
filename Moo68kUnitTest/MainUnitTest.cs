using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moo68k;

namespace Moo86kUnitTest
{
    [TestClass]
    public class Moo86kUnitTest
    {
        [TestMethod]
        [TestCategory("I/O")]
        public void TestInput()
        {

        }

        [TestMethod]
        [TestCategory("Registers")]
        public void TestRegisters()
        {
            MC86000 m68k = new MC86000();

            // Data
            m68k.Execute(0x203C, 1);
            Assert.AreEqual(m68k.D0, 1u);
            m68k.Execute(0x223C, 2);
            Assert.AreEqual(m68k.D1, 2u);
            m68k.Execute(0x243C, 3);
            Assert.AreEqual(m68k.D2, 3u);
            m68k.Execute(0x263C, 4);
            Assert.AreEqual(m68k.D3, 4u);
            m68k.Execute(0x283C, 5);
            Assert.AreEqual(m68k.D4, 5u);
            m68k.Execute(0x2A3C, 6);
            Assert.AreEqual(m68k.D5, 6u);
            m68k.Execute(0x2C3C, 7);
            Assert.AreEqual(m68k.D6, 7u);
            m68k.Execute(0x2E3C, 8);
            Assert.AreEqual(m68k.D7, 8u);

            // Address
            m68k.Execute(0x20BC, 11);
            Assert.AreEqual(m68k.A0, 11u);
            m68k.Execute(0x22BC, 12);
            Assert.AreEqual(m68k.A1, 12u);
            m68k.Execute(0x24BC, 13);
            Assert.AreEqual(m68k.A2, 13u);
            m68k.Execute(0x26BC, 14);
            Assert.AreEqual(m68k.A3, 14u);
            m68k.Execute(0x28BC, 15);
            Assert.AreEqual(m68k.A4, 15u);
            m68k.Execute(0x2ABC, 16);
            Assert.AreEqual(m68k.A5, 16u);
            m68k.Execute(0x2CBC, 17);
            Assert.AreEqual(m68k.A6, 17u);
            m68k.Execute(0x2EBC, 18);
            Assert.AreEqual(m68k.A7, 18u);

            // Those two instructions is from a manual (mbsd_l2.pdf) from
            // Ricardo Gutierrez-Osuna, Wright State University

            // MOVE.L #$12,d0 | 00 10 000 000 111 100 | 203C 00000012
            m68k.Execute(0x203C, 0x12);
            Assert.AreEqual(m68k.D0, 0x12u);

            // MOVE.B data,d1 | 00 01 001 000 111 001 | 1239 00002000
            // self note: This goes at the address 2000 stored in data (a variable?) and
            //            loads 24h into D1 (manually?)
            //            Is this due to a higher language?
            //m68k.Execute(0x1239, 0x2000);
            //Assert.AreEqual(m68k.D1, 24u);
        }

        [TestMethod]
        [TestCategory("Memory")]
        public void TestMemory()
        {

        }

        [TestMethod]
        [TestCategory("Processor")]
        public void TestAddition()
        {

        }

        [TestMethod]
        [TestCategory("Processor")]
        public void TestSubtraction()
        {
            MC86000 m68k = new MC86000();

            // Move immidiate value long 73 into register D0
            m68k.Execute(0x203C, 73);
            // Subtract immidiate value long 4 from register D0
            m68k.Execute(0x90BC, 4);

            Assert.AreEqual(m68k.D0, 69u);

            // Move immidiate value long 63 into register D1 and
            // Move immidiate value long 21 into register D2
            m68k.Execute(0x223C, 63);
            m68k.Execute(0x243C, 21);
            // Subtract D1 with D2 (long)
            m68k.Execute(0x9282);

            Assert.AreEqual(m68k.D1, 42u);
        }
    }

    [TestClass]
    public class Moo86kToolsUnitTest
    {
        [TestMethod]
        [TestCategory("Compiler")]
        public void TestCompiler()
        {

        }
    }
}