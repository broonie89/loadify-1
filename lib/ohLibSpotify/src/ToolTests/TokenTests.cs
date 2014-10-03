using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiParser;
using NUnit.Framework;

namespace ToolTests
{
    [TestFixture]
    public class TokenTests
    {
        [Test]
        [TestCase("abcdefgh\n12345678\n\nxyz", 0, 1)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 1, 2)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 7, 8)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 8, 9)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 9, 1)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 16, 8)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 17, 9)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 18, 1)]
        public void TestColumn(string aOriginalString, int aIndex, int aExpectedColumn)
        {
            Assert.That(new Token("kind", "", aOriginalString, aIndex).Column, Is.EqualTo(aExpectedColumn));
        }

        [Test]
        [TestCase("abcdefgh\n12345678\n\nxyz", 0, 1)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 1, 1)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 7, 1)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 8, 1)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 9, 2)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 16, 2)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 17, 2)]
        [TestCase("abcdefgh\n12345678\n\nxyz", 18, 3)]
        [TestCase("\n\n\n\n\nx", 5, 6)]
        public void TestLine(string aOriginalString, int aIndex, int aExpectedLine)
        {
            Assert.That(new Token("kind", "", aOriginalString, aIndex).Line, Is.EqualTo(aExpectedLine));
        }

        [TestCase("word", "xxx", "aaa (xxx)", 5, @"(word,""xxx""):1:6")]
        [TestCase("symbol", "(", "aaa (xxx)", 4, @"(symbol,""(""):1:5")]
        [TestCase("symbol", "\"", "\"xxx\"", 0, @"(symbol,""\""""):1:1")]
        public void TestToString(string aKind, string aContent, string aOriginalString, int aIndex, string aExpectedString)
        {
            Assert.That(new Token(aKind, aContent, aOriginalString, aIndex).ToString(), Is.EqualTo(aExpectedString));
        }
    }
}
