using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiParser;
using ManagedApiBuilder;
using NUnit.Framework;

namespace ToolTests
{
    public class PatternMatchTests
    {
        public CType Void { get { return new NamedCType("void"); } }
        public CType Int { get { return new NamedCType("int"); } }
        public CType Char { get { return new NamedCType("char"); } }
        public CType ConstChar { get { return new NamedCType("char"){Qualifiers={"const"}}; } }
        public CType VoidPtr { get { return new PointerCType(Void); } }
        public CType IntPtr { get { return new PointerCType(Int); } }
        public CType CharPtr { get { return new PointerCType(Char); } }
        public CType ConstCharPtr { get { return new PointerCType(ConstChar); } }
        public CType IntArray { get { return new ArrayCType(null, Int); } }
        public CType IntArrayFive { get { return new ArrayCType(5, Int); } }
        public CType IntArrayTen { get { return new ArrayCType(10, Int); } }
        public CType CharArray { get { return new ArrayCType(null, Char); } }
        public CType CharArrayTen { get { return new ArrayCType(10, Char); } }
        public CType CharArraySixty { get { return new ArrayCType(60, Char); } }

        public IEnumerable<TestCaseData> PatternMatchTestCases()
        {
            Func<CType, CType, bool, string, TestCaseData> tc =
                (t1, t2, expected, name) => new TestCaseData(t1, t2, expected).SetName(name);
            yield return tc(Void, Void, true, "Void_ShouldMatch_Void");
            yield return tc(Void, Int, false, "Void_ShouldNotMatch_Int");
            yield return tc(VoidPtr, VoidPtr, true, "VoidPtr_ShouldMatch_VoidPtr");
            yield return tc(VoidPtr, IntPtr, false, "VoidPtr_ShouldNotMatch_IntPtr");
            yield return tc(IntPtr, VoidPtr, false, "IntPtr_ShouldNotMatch_VoidPtr");
            yield return tc(IntPtr, CharPtr, false, "IntPtr_ShouldNotMatch_CharPtr");
            yield return tc(IntArray, IntArray, true, "IntArray_ShouldMatch_IntArray");
            yield return tc(IntArray, CharArray, false, "IntArray_ShouldNotMatch_CharArray");
            yield return tc(IntArray, IntArrayTen, false, "IntArray_ShouldNotMatch_IntArrayTen");
            yield return tc(IntArray, Void, false, "IntArray_ShouldNotMatch_Void");
            yield return tc(IntArray, IntArrayFive, false, "IntArray_ShouldNotMatch_IntArrayFive");
            yield return tc(IntArrayFive, IntArray, false, "IntArrayFive_ShouldNotMatch_IntArray");
            yield return tc(IntArrayFive, IntArrayFive, true, "IntArrayFive_ShouldMatch_IntArrayFive");
            yield return tc(ConstCharPtr, ConstCharPtr, true, "ConstCharPtr_ShouldMatch_ConstCharPtr");
            yield return tc(CharPtr, ConstCharPtr, false, "ConstPtr_ShouldNotMatch_ConstCharPtr");
            yield return tc(ConstCharPtr, CharPtr, false, "ConstCharPtr_ShouldNotMatch_ConstPtr");
        }

        [Test]
        [TestCaseSource("PatternMatchTestCases")]
        public void TestMatch(CType aType, CType aPattern, bool aExpected)
        {
            var result = aType.MatchToPattern(aPattern);
            Assert.That(result.IsMatch, Is.EqualTo(aExpected));
        }

    }
}
