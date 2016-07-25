using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moo68k.Tools;

namespace Moo68kUnitTest
{
    [TestClass]
    public class Moo86kToolsUnitTest
    {
        [TestMethod]
        [TestCategory("Tools")]
        public void TestCompiler()
        {

        }

        [TestMethod]
        [TestCategory("Parser")]
        public void TestHexParser()
        {
            // Int

            Assert.AreEqual(15, "0x_F".HexStringToInt());
            Assert.AreEqual(291, "0x123".HexStringToInt());
            Assert.AreEqual(32848, "8050".HexStringToInt());

            // Long

            Assert.AreEqual(4294967296, "0x1_0000_0000ul".HexStringToLong());

            // ULong

            Assert.AreEqual(4294967296u, "0x1_0000_0000ul".HexStringToULong());

            // Formatted hex strings longer than 16 characters are trimmed off.
            Assert.AreEqual(ulong.MaxValue, "0xF_FFFF_FFFF_FFFF_FFFFul".HexStringToULong());
        }
    }
}
