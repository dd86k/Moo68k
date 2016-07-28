using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moo68k;

namespace Moo86kUnitTest
{
    [TestClass]
    public class Moo86kUnitTest
    {
        [TestMethod]
        [TestCategory("Processor")]
        public void TestInput()
        {
            MC68000 m68k = new MC68000();

            Assert.AreEqual(0x2700u, m68k.SR);
        }

        [TestMethod]
        [TestCategory("Processor")]
        public void TestRegisters()
        {
            MC68000 m68k = new MC68000();

            // Data
            m68k.Execute(0x203C, 1);
            Assert.AreEqual(1u, m68k.D0);
            m68k.Execute(0x223C, 2);
            Assert.AreEqual(2u, m68k.D1);
            m68k.Execute(0x243C, 3);
            Assert.AreEqual(3u, m68k.D2);
            m68k.Execute(0x263C, 4);
            Assert.AreEqual(4u, m68k.D3);
            m68k.Execute(0x283C, 5);
            Assert.AreEqual(5u, m68k.D4);
            m68k.Execute(0x2A3C, 6);
            Assert.AreEqual(6u, m68k.D5);
            m68k.Execute(0x2C3C, 7);
            Assert.AreEqual(7u, m68k.D6);
            m68k.Execute(0x2E3C, 8);
            Assert.AreEqual(8u, m68k.D7);

            // Address
            m68k.Execute(0x20BC, 11);
            Assert.AreEqual(11u, m68k.A0);
            m68k.Execute(0x22BC, 12);
            Assert.AreEqual(12u, m68k.A1);
            m68k.Execute(0x24BC, 13);
            Assert.AreEqual(13u, m68k.A2);
            m68k.Execute(0x26BC, 14);
            Assert.AreEqual(14u, m68k.A3);
            m68k.Execute(0x28BC, 15);
            Assert.AreEqual(15u, m68k.A4);
            m68k.Execute(0x2ABC, 16);
            Assert.AreEqual(16u, m68k.A5);
            m68k.Execute(0x2CBC, 17);
            Assert.AreEqual(17u, m68k.A6);
            /*m68k.Execute(0x2CBC, 18);
            Assert.AreEqual(17u, m68k.A7);*/

            // The next two instructions is from a manual (mbsd_l2.pdf) from
            // Ricardo Gutierrez-Osuna, Wright State University

            // MOVE.L #$12,d0 | 00 10 000 000 111 100 | 203C 00000012
            m68k.Execute(0x203C, 0x12);
            Assert.AreEqual(0x12u, m68k.D0);

            // MOVE.B data,d1 | 00 01 001 000 111 001 | 1239 00002000
            // self note: This goes at the address 2000 stored in data (a variable?) and
            //            loads 24h into D1 (manually?)
            //            Is this due to a higher language?
            //m68k.Execute(0x1239, 0x2000);
            //Assert.AreEqual(24u, m68k.D1);
        }

        [TestMethod]
        [TestCategory("Processor")]
        public void TestMemory()
        {

        }

        [TestMethod]
        [TestCategory("Processor")]
        public void TestAddition()
        {
            MC68000 m68k = new MC68000();

            // Move immidiate value long 4 into register D0
            m68k.Execute(0x203C, 4);
            // Add immidiate value long 8 with register D0
            // ADD.L #8,D0
            m68k.Execute(0xD0BC, 8);

            Assert.AreEqual(12u, m68k.D0);

            // Move immidiate value long 10 into register D1 and
            // Move immidiate value long 11 into register D2
            m68k.Execute(0x223C, 10);
            m68k.Execute(0x243C, 11);
            // Add D1 with the value of D2 (long)
            // ADD.L D2,D1
            m68k.Execute(0xD382);

            Assert.AreEqual(21u, m68k.D1);

            // ADDQ.L 4,D1
            m68k.Execute(0x5881);

            Assert.AreEqual(25u, m68k.D1);

            // ADDQ.L 8,D0
            m68k.Execute(0x5080);

            Assert.AreEqual(20u, m68k.D0);
        }

        [TestMethod]
        [TestCategory("Processor")]
        public void TestSubtraction()
        {
            MC68000 m68k = new MC68000();

            // Move immidiate value long 73 into register D0
            m68k.Execute(0x203C, 73);
            // Subtract immidiate value long 4 with register D0
            m68k.Execute(0x90BC, 4);

            Assert.AreEqual(69u, m68k.D0);

            // Move immidiate value long 63 into register D1 and
            // Move immidiate value long 21 into register D2
            m68k.Execute(0x223C, 63);
            m68k.Execute(0x243C, 21);
            // Subtract D1 with the value of D2 (long)
            m68k.Execute(0x9282);

            Assert.AreEqual(42u, m68k.D1);

            // SUBQ.L 4,D1
            m68k.Execute(0x5981);

            Assert.AreEqual(38u, m68k.D1);

            // SUBQ.L 8,D1
            m68k.Execute(0x5181);

            Assert.AreEqual(30u, m68k.D1);
        }

        [TestMethod]
        [TestCategory("Processor")]
        public void TestBitwiseOperations()
        {
            // AND, OR, etc.
            MC68000 m68k = new MC68000();
            
            m68k.Execute(0x203C, 0x50); // D0: $50

            // ORI.L #$10,D0
            m68k.Execute(0x80, 0x12);

            Assert.AreEqual(0x52u, m68k.D0);
        }

        [TestMethod]
        [TestCategory("Processor")]
        public void TestBitShifting()
        {
            MC68000 m68k = new MC68000();

            // Move immidiate value long 4 into register D0
            m68k.Execute(0x203C, 4);

            // Rotate long D0 to the left for 4 bits immidiately
            // ROXL 4,D0
            m68k.Execute(0xE990);

            Assert.AreEqual(64u, m68k.D0); //  4 << 4 = 64

            m68k.Execute(0x223C, 2); // 2 -> D1

            // Rotate long D0 to the right from D1 (2 bits)
            // ROXL D1,D0
            m68k.Execute(0xE2B0);

            Assert.AreEqual(16u, m68k.D0); // 64 >> 2 = 16
        }
    }
}