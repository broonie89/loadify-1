using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiParser;
using NUnit.Framework;

namespace ToolTests
{
    [TestFixture]
    public class LexerTests
    {
        [TestCase("    ",     new[]{@"(whitespace,""    ""):1:1"})]
        [TestCase("\t",       new[]{@"(whitespace,""\t""):1:1"})]
        [TestCase("    \t  ", new[]{@"(whitespace,""    \t  ""):1:1"})]
        public void TestWhitespace(string aInput, IEnumerable<string> aExpected)
        {
            TestExpected(aInput, aExpected);
        }

        [TestCase("\n",     new[]{@"(newline,""\n""):1:1"})]
        [TestCase("\r\n",   new[]{@"(newline,""\r\n""):1:1"})]
        public void TestNewline(string aInput, IEnumerable<string> aExpected)
        {
            TestExpected(aInput, aExpected);
        }

        [TestCase(" \n ",     new[]{
            @"(whitespace,"" ""):1:1",
            @"(newline,""\n""):1:2",
            @"(whitespace,"" ""):2:1"})]
        [TestCase("    \r\n\t   ",     new[]{
            @"(whitespace,""    ""):1:1",
            @"(newline,""\r\n""):1:5",
            @"(whitespace,""\t   ""):2:1"})]
        public void TestWhitespaceAndNewline(string aInput, IEnumerable<string> aExpected)
        {
            TestExpected(aInput, aExpected);
        }

        [TestCase("identifier",        new[]{@"(word,""identifier""):1:1"})]
        [TestCase("_underscore",       new[]{@"(word,""_underscore""):1:1"})]
        [TestCase("underscore_",       new[]{@"(word,""underscore_""):1:1"})]
        [TestCase("abc123",            new[]{@"(word,""abc123""):1:1"})]
        public void TestWords(string aInput, IEnumerable<string> aExpected)
        {
            TestExpected(aInput, aExpected);
        }

        [TestCase("[",       new[]{@"(symbol,""[""):1:1"})]
        [TestCase("]",       new[]{@"(symbol,""]""):1:1"})]
        [TestCase("*",       new[]{@"(symbol,""*""):1:1"})]
        [TestCase("(*)",     new[]{@"(symbol,""(""):1:1", @"(symbol,""*""):1:2", @"(symbol,"")""):1:3"})]
        public void TestSymbols(string aInput, IEnumerable<string> aExpected)
        {
            TestExpected(aInput, aExpected);
        }


        [TestCase("// Anything !* 44[\n",      new[]{@"(linecomment,""// Anything !* 44[\n""):1:1"})]
        [TestCase("foo // Anything !* 44[\nnextline", new[]{
            @"(word,""foo""):1:1",
            @"(whitespace,"" ""):1:4",
            @"(linecomment,""// Anything !* 44[\n""):1:5",
            @"(word,""nextline""):2:1"})]
        public void TestLineComment(string aInput, IEnumerable<string> aExpected)
        {
            TestExpected(aInput, aExpected);
        }

        [TestCase("/* stuff */ ",       new[]{@"(rangecomment,""/* stuff */""):1:1",@"(whitespace,"" ""):1:12"})]
        [TestCase("/*********/ ",       new[]{@"(rangecomment,""/*********/""):1:1",@"(whitespace,"" ""):1:12"})]
        [TestCase("/****** **/ ",       new[]{@"(rangecomment,""/****** **/""):1:1",@"(whitespace,"" ""):1:12"})]
        [TestCase("/***** ***/ ",       new[]{@"(rangecomment,""/***** ***/""):1:1",@"(whitespace,"" ""):1:12"})]
        [TestCase("/*/      */ ",       new[]{@"(rangecomment,""/*/      */""):1:1",@"(whitespace,"" ""):1:12"})]
        public void TestRangeComment(string aInput, IEnumerable<string> aExpected)
        {
            TestExpected(aInput, aExpected);
        }

        [TestCase("#define x\n ",       new[]{@"(preprocessor,""#define x\n""):1:1",@"(whitespace,"" ""):2:1"},
            Ignored=true, Reason="Lexer can't yet spot a directive on the first line")]
        [TestCase("\n#define x\n ",     new[]{
            @"(newline,""\n""):1:1",
            @"(preprocessor,""#define x\n""):2:1",
            @"(whitespace,"" ""):3:1"})]
        public void TestPreprocessor(string aInput, IEnumerable<string> aExpected)
        {
            TestExpected(aInput, aExpected);
        }

        [TestCase("10",         new[]{@"(number,""10""):1:1"})]
        [TestCase("0x10",       new[]{@"(number,""0x10""):1:1"})]
        [TestCase("0xAF",       new[]{@"(number,""0xAF""):1:1"})]
        [TestCase("0xdb",       new[]{@"(number,""0xdb""):1:1"})]
        public void TestNumber(string aInput, IEnumerable<string> aExpected)
        {
            TestExpected(aInput, aExpected);
        }

        [TestCase("\"foo\"",       new[]{@"(string,""\""foo\""""):1:1"})]
        [TestCase("\"foo\\t\"",    new[]{@"(string,""\""foo\\t\""""):1:1"})]
        [TestCase("\"foo\\\\\"",     new[]{@"(string,""\""foo\\\\\""""):1:1"})]
        [TestCase("\"foo\" ",      new[]{@"(string,""\""foo\""""):1:1", @"(whitespace,"" ""):1:6"})]
        public void TestString(string aInput, IEnumerable<string> aExpected)
        {
            TestExpected(aInput, aExpected);
        }

        void TestExpected(string aInput, IEnumerable<string> aExpected)
        {
            var tokens = CHeaderLexer.Lex(aInput).Select(t=>t.ToString()).ToList();
            Assert.That(tokens, Is.EqualTo(aExpected.ToList()));
        }
    }
}
