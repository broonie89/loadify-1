// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagedApiBuilder
{
    public interface IFunctionAssembler
    {
        void AddPInvokeParameter(CSharpType aType, string aPInvokeName, string aExpression);
        void SetPInvokeReturn(CSharpType aType, string aVariableName);
        void AddManagedParameter(string aName, CSharpType aType);
        void SetManagedReturn(CSharpType aType);
        void SuppressManagedWrapper();
        void InsertAtTop(string aCode);
        void InsertAtEnd(string aCode);
        void InsertBeforeCall(string aCode);
        void InsertAfterCall(string aCode);
        void InsertPreCall(string aVariableName);
        void IncreaseIndent();
        void DecreaseIndent();
        bool IsStatic { get; set; }
    }

    public interface IFunctionGenerator : IFunctionAssembler
    {
        string GenerateWrapperMethod(string aIndent);
        string GenerateNativeDelegateDeclaration(string aIndent);
        string GeneratePInvokeDeclaration(string aIndent);
        void NextArgument();
    }

    public class FunctionAssembler : IFunctionGenerator
    {
        int iIndentLevelAbove = 0;
        int iIndentLevelBelow = 0;
        CSharpType iPInvokeReturnType = new CSharpType("void");
        string iPInvokeReturnVariable = null;
        /// <summary>
        /// The C# type of each native argument.
        /// </summary>
        readonly List<KeyValuePair<string, CSharpType>> iPInvokeArguments = new List<KeyValuePair<string, CSharpType>>();
        /// <summary>
        /// The actual text of the C# expressions to use for each
        /// argument to the native function.
        /// </summary>
        readonly List<string> iArgumentExpressions = new List<string>();

        readonly List<KeyValuePair<string, CSharpType>> iManagedArguments = new List<KeyValuePair<string, CSharpType>>();
        readonly List<string> iTopCode = new List<string>();
        readonly List<List<string>> iBottomCode = new List<List<string>> { new List<string>() };
        readonly List<string> iAboveCode = new List<string>();
        readonly List<List<string>> iBelowCode = new List<List<string>> { new List<string>() };
        public string NativeFunctionName { get; set; }
        public string ManagedFunctionName { get; set; }
        CSharpType iManagedReturnType = new CSharpType("void");
        public bool IsStatic { get; set; }
        public bool HasManagedWrapper { get; set; }

        public FunctionAssembler(string aNativeFunctionName, string aManagedFunctionName)
        {
            NativeFunctionName = aNativeFunctionName;
            ManagedFunctionName = aManagedFunctionName;
            IsStatic = true;
            HasManagedWrapper = true;
        }

        public void AddPInvokeParameter(CSharpType aType, string aPInvokeName, string aExpression)
        {
            iPInvokeArguments.Add(new KeyValuePair<string, CSharpType>(aPInvokeName, aType));
            iArgumentExpressions.Add(aExpression);
        }

        public void SetPInvokeReturn(CSharpType aType, string aVariableName)
        {
            iPInvokeReturnType = aType;
            iPInvokeReturnVariable = aVariableName;
        }

        public void AddManagedParameter(string aName, CSharpType aType)
        {
            iManagedArguments.Add(new KeyValuePair<string, CSharpType>(aName, aType));
        }

        public void SetManagedReturn(CSharpType aType)
        {
            if (iManagedReturnType.ToString() != "void")
            {
                throw new Exception(String.Format("Return type collision. Already had: {0}, tried to add: {1}", iManagedReturnType, aType));
            }
            iManagedReturnType = aType;
        }

        public void SuppressManagedWrapper()
        {
            HasManagedWrapper = false;
        }

        public void InsertAtTop(string aCode)
        {
            iTopCode.Add(aCode);
        }

        public void InsertAtEnd(string aCode)
        {
            iBottomCode.Last().Add(aCode);
        }

        public void InsertBeforeCall(string aCode)
        {
            iAboveCode.Add(new String(' ', iIndentLevelAbove * 4) + aCode);
        }

        public void InsertAfterCall(string aCode)
        {
            iBelowCode.Last().Add(new String(' ', iIndentLevelBelow * 4) + aCode);
        }

        public void NextArgument()
        {
            iBottomCode.Add(new List<string>());
            iBelowCode.Add(new List<string>());
            iIndentLevelBelow = iIndentLevelAbove;
        }

        public string GetCallExpression()
        {
            return String.Format("NativeMethods.{0}({1})", NativeFunctionName, String.Join(", ", iArgumentExpressions));
        }

        public void InsertPreCall(string aVariableName)
        {
            InsertBeforeCall(String.Format("{0} = {1};", aVariableName, GetCallExpression()));
        }

        public void IncreaseIndent()
        {
            iIndentLevelAbove += 1;
            iIndentLevelBelow = iIndentLevelAbove;
        }

        public void DecreaseIndent()
        {
            iIndentLevelBelow -= 1;
        }

        public string GenerateWrapperMethod(string aIndent)
        {
            if (!HasManagedWrapper)
            {
                return String.Format("{0}// Skipped {1}\n",aIndent, NativeFunctionName);
            }
            const string template =
                "{0}" +
                "{1}public {2}{3} {4}({5})\n" +
                "{1}{{\n" +
                "{6}" +
                "{1}}}\n";
            string argString = String.Join(", ", iManagedArguments.Select(x => x.Value.CreateParameterDeclaration(x.Key)));

            var returnAttribute = iManagedReturnType.CreateReturnTypeAttribute();
            if (returnAttribute != "")
                returnAttribute = aIndent + returnAttribute + "\n";
            var returnTypeDeclaration = iManagedReturnType.CreateReturnTypeDeclaration();

            string bodyIndent = aIndent + "    ";
            StringBuilder bodyBuilder = new StringBuilder(); // *groan*
            foreach (string s in iTopCode)
            {
                bodyBuilder.Append(bodyIndent + s + "\n");
            }
            foreach (string s in iAboveCode)
            {
                bodyBuilder.Append(bodyIndent + s + "\n");
            }
            string assignmentLeft = iPInvokeReturnVariable == null ? "" : (iPInvokeReturnVariable + " = ");
            bodyBuilder.Append(bodyIndent + new string(' ', 4*iIndentLevelAbove) + assignmentLeft + GetCallExpression() + ";\n");
            foreach (string s in Enumerable.Reverse(iBelowCode).SelectMany(x=>x))
            {
                bodyBuilder.Append(bodyIndent + s + "\n");
            }
            foreach (string s in Enumerable.Reverse(iBottomCode).SelectMany(x=>x))
            {
                bodyBuilder.Append(bodyIndent + s + "\n");
            }

            string staticString = IsStatic ? "static " : "";

            return String.Format(template, returnAttribute, aIndent, staticString, returnTypeDeclaration, ManagedFunctionName, argString, bodyBuilder);
        }


        public string GenerateNativeDelegateDeclaration(string aIndent)
        {
            const string template =
                "{4}" +
                "{0}internal delegate {1} {2}({3});\n";
            return GenerateNativeFunctionDeclaration(aIndent, template, NativeFunctionName);
        }
        public string GeneratePInvokeDeclaration(string aIndent)
        {
            const string template =
                "{0}[DllImport(\"libspotify\")]\n" +
                "{4}" +
                "{0}internal static extern {1} {2}({3});\n";
            return GenerateNativeFunctionDeclaration(aIndent, template, NativeFunctionName);
        }
        string GenerateNativeFunctionDeclaration(string aIndent, string aTemplate, string aFunctionName)
        {
            string argString;
            if (iPInvokeArguments.Count == 0)
            {
                argString = "";
            }
            else
            {
                var args = iPInvokeArguments.Select(x=>x.Value.CreateParameterDeclaration(x.Key));
                argString = String.Join(", ", args.ToArray());
            }
            var returnType = iPInvokeReturnType;
            var returnAttribute = returnType.CreateReturnTypeAttribute();
            if (returnAttribute != "")
            {
                returnAttribute = aIndent + returnAttribute + "\n";
            }
            var returnTypeDeclaration = returnType.CreateReturnTypeDeclaration();
            return String.Format(aTemplate, aIndent, returnTypeDeclaration, aFunctionName, argString, returnAttribute);
        }

    }
}