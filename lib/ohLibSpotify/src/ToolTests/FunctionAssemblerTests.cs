using System;
using ManagedApiBuilder;
using NUnit.Framework;

namespace ToolTests
{
    public class FunctionAssemblerContext
    {
        protected IFunctionGenerator iFunctionAssembler;

        public void MakeAssembler()
        {
            iFunctionAssembler = new FunctionAssembler("my_native_function", "MyManagedMethod");
        }
        public string GeneratePInvokeDeclaration()
        {
            return iFunctionAssembler.GeneratePInvokeDeclaration("");
        }
        public string GenerateWrapperMethod()
        {
            return iFunctionAssembler.GenerateWrapperMethod("");
        }
        public string GenerateNativeDelegateDeclaration()
        {
            return iFunctionAssembler.GenerateNativeDelegateDeclaration("");
        }
        public void AddManagedParameter(string aName, CSharpType aType)
        {
            iFunctionAssembler.AddManagedParameter(aName, aType);
        }
        public void AddNativeParameter(CSharpType aType, string aPInvokeName, string aExpression)
        {
            iFunctionAssembler.AddPInvokeParameter(aType, aPInvokeName, aExpression);
        }
        public void IncreaseIndent()
        {
            iFunctionAssembler.IncreaseIndent();
        }
        public void DecreaseIndent()
        {
            iFunctionAssembler.DecreaseIndent();
        }
        public void InsertAtTop(string aCode)
        {
            iFunctionAssembler.InsertAtTop(aCode);
        }
        public void InsertAtEnd(string aCode)
        {
            iFunctionAssembler.InsertAtEnd(aCode);
        }
        public void InsertBeforeCall(string aCode)
        {
            iFunctionAssembler.InsertBeforeCall(aCode);
        }
        public void InsertAfterCall(string aCode)
        {
            iFunctionAssembler.InsertAfterCall(aCode);
        }
        public void NextArgument()
        {
            iFunctionAssembler.NextArgument();
        }
        public bool IsStatic { get { return iFunctionAssembler.IsStatic; } }
        //public bool HasManagedWrapper { get { return iFunctionAssembler.HasManagedWrapper; } }
        //public string ManagedFunctionName { get { return iFunctionAssembler.ManagedFunctionName; } }
        //public string NativeFunctionName { get { return iFunctionAssembler.NativeFunctionName; } }
    }

    public class WhenBuildingAnEmptyMethodDeclaration : FunctionAssemblerContext
    {
        [SetUp]
        public void SetUp()
        {
            MakeAssembler();
        }

        [Test]
        public void InvokeDeclarationShouldReturnACorrectDeclaration()
        {
            string pinvokeDecl = GeneratePInvokeDeclaration();
            Assert.That(pinvokeDecl, Is.EqualTo(
                "[DllImport(\"libspotify\")]\n" +
                "internal static extern void my_native_function();\n"));
        }

        [Test]
        public void WrapperMethodShouldReturnACorrectMethod()
        {
            string wrapperMethod = GenerateWrapperMethod();
            Assert.That(wrapperMethod, Is.EqualTo(
                "public static void MyManagedMethod()\n" +
                "{\n" +
                "    NativeMethods.my_native_function();\n" +
                "}\n"));
        }

        [Test]
        public void GenerateNativeDelegateDeclarationShouldReturnACorrectDeclaration()
        {
            string delegateDecl = GenerateNativeDelegateDeclaration();
            Assert.That(delegateDecl, Is.EqualTo(
                "internal delegate void my_native_function();\n"));
        }

        /*[Test]
        public void ManagedFunctionNameShouldBeCorrect()
        {
            Assert.That(ManagedFunctionName, Is.EqualTo("MyManagedMethod"));
        }
        [Test]
        public void NativeFunctionNameShouldBeCorrect()
        {
            Assert.That(NativeFunctionName, Is.EqualTo("my_native_function"));
        }*/

        [Test]
        public void IsStaticReturnsTrue()
        {
            Assert.That(IsStatic, Is.True);
        }
    }

    public class WhenBuildingAFunctionWithOneManagedArgument : FunctionAssemblerContext
    {
        [SetUp]
        public void SetUp()
        {
            MakeAssembler();
            AddManagedParameter("numbers", new CSharpType("int[]"));
        }

        [Test]
        public void TheWrapperMethodHasTheManagedParameter()
        {
            string wrapperMethod = GenerateWrapperMethod();
            Assert.That(wrapperMethod, Is.StringMatching(
                @".*MyManagedMethod\(int\[\] @?numbers\).*")); // Allow but don't require '@' sign
        }
    }

    public class WhenBuildingAFunctionWithOneNativeArgument : FunctionAssemblerContext
    {
        [SetUp]
        public void SetUp()
        {
            MakeAssembler();
            AddNativeParameter(new CSharpType("IntPtr"), "session", "this._handle");
        }

        [Test]
        public void TheWrapperMethodUsesTheArgumentExpression()
        {
            string wrapperMethod = GenerateWrapperMethod();
            Assert.That(wrapperMethod, Is.StringMatching(
                @".*my_native_function\(this._handle\);.*"));
        }

        [Test]
        public void ThePInvokeDeclarationHasTheParameter()
        {
            string pinvokeDecl = GeneratePInvokeDeclaration();
            Assert.That(pinvokeDecl, Is.StringMatching(
                @".*my_native_function\(IntPtr @?session\);.*")); // Allow but don't require '@' sign
        }
    }

    public class WhenBuildingAFunctionWithCodeForTwoArguments : FunctionAssemblerContext
    {
        protected string iResult;
        [SetUp]
        public void SetUp()
        {
            MakeAssembler();
            InsertAtTop("Alpha.Top(1);");
            InsertAtTop("Alpha.Top(2);");
            InsertBeforeCall("Alpha.Before(1);");
            InsertBeforeCall("Alpha.Before(2);");
            IncreaseIndent();
            InsertBeforeCall("Alpha.Before(3);");
            InsertAfterCall("Alpha.After(1);");
            DecreaseIndent();
            InsertAfterCall("Alpha.After(2);");
            InsertAfterCall("Alpha.After(3);");
            InsertAtEnd("Alpha.End(1);");
            InsertAtEnd("Alpha.End(2);");

            NextArgument();

            InsertAtTop("Beta.Top(1);");
            InsertAtTop("Beta.Top(2);");
            InsertBeforeCall("Beta.Before(1);");
            InsertBeforeCall("Beta.Before(2);");
            IncreaseIndent();
            InsertBeforeCall("Beta.Before(3);");
            InsertAfterCall("Beta.After(1);");
            DecreaseIndent();
            InsertAfterCall("Beta.After(2);");
            InsertAfterCall("Beta.After(3);");
            InsertAtEnd("Beta.End(1);");
            InsertAtEnd("Beta.End(2);");

            iResult = GenerateWrapperMethod();
        }

        [Test]
        public void InsertAtTopCodeShouldBeInOrder()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)");
            Assert.That(iResult, Is.StringMatching(
                "(?s)" +
                anything + esc("Alpha.Top(1);") +
                anything + esc("Alpha.Top(2);") +
                anything + esc("Beta.Top(1);") +
                anything + esc("Beta.Top(2);") +
                anything));
        }
        [Test]
        public void InsertAtEndCodeShouldBeInOrder()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)");
            Assert.That(iResult, Is.StringMatching(
                "(?s)" +
                anything + esc("Beta.End(1);") +
                anything + esc("Beta.End(2);") +
                anything + esc("Alpha.End(1);") +
                anything + esc("Alpha.End(2);") +
                anything));
        }
        [Test]
        public void CodeBlocksShouldBeInOrder()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)");
            Assert.That(iResult, Is.StringMatching(
                "(?s)" +
                anything + esc("Alpha.Before(1);") +
                anything + esc("Beta.Before(1);") +
                anything + esc("my_native_function") +
                anything + esc("Beta.After(1);") +
                anything + esc("Alpha.After(1);") +
                anything));
        }
        [Test]
        public void BeforeCodeShouldBeInOrder()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)");
            Assert.That(iResult, Is.StringMatching(
                "(?s)" +
                anything + esc("Alpha.Before(1);") +
                anything + esc("Alpha.Before(2);") +
                anything + esc("Alpha.Before(3);") +
                anything));
        }
        [Test]
        public void AfterCodeShouldBeInOrder()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)");

            Assert.That(iResult, Is.StringMatching(
                "(?s)" +
                anything + esc("Alpha.After(1);") +
                anything + esc("Alpha.After(2);") +
                anything + esc("Alpha.After(3);") +
                anything));
        }
        [Test]
        public void FirstArgumentTopCodeShouldNotBeIndented()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)");
            Assert.That(iResult, Is.StringMatching(
                "(?sm)" +
                anything + @"^    " + esc("Alpha.Top(1);") +
                anything));
        }
        [Test]
        public void SecondArgumentTopCodeShouldNotBeIndented()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)");
            Assert.That(iResult, Is.StringMatching(
                "(?sm)" +
                anything + @"^    " + esc("Beta.Top(1);") +
                anything));
        }
        [Test]
        public void BeforeCodeShouldBeCorrectlyIndented()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)");
            Assert.That(iResult, Is.StringMatching(
                "(?sm)" +
                anything + @"^    " + esc("Alpha.Before(1);") +
                anything + @"^    " + esc("Alpha.Before(2);") +
                anything + @"^        " + esc("Alpha.Before(3);") +
                anything));
        }
        [Test]
        public void IndentShouldContinueIntoSecondArgumentBeforeCode()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)");
            Assert.That(iResult, Is.StringMatching(
                "(?sm)" +
                anything + @"^        " + esc("Beta.Before(1);") +
                anything + @"^        " + esc("Beta.Before(2);") +
                anything + @"^            " + esc("Beta.Before(3);") +
                anything));
        }
        [Test]
        public void AfterCodeShouldBeCorrectlyIndented()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)");
            Assert.That(iResult, Is.StringMatching(
                "(?sm)" +
                anything + @"^        " + esc("Alpha.After(1);") +
                anything + @"^    " + esc("Alpha.After(2);") +
                anything + @"^    " + esc("Alpha.After(3);") +
                anything));
        }
        [Test]
        public void IndentShouldContinueIntoSecondArgumentAfterCode()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)");
            Assert.That(iResult, Is.StringMatching(
                "(?sm)" +
                anything + @"\n            " + esc("Beta.After(1);") +
                anything + @"\n        " + esc("Beta.After(2);") +
                anything + @"\n        " + esc("Beta.After(3);") +
                anything));
        }
    }
}
