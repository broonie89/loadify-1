using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    /// <summary>
    /// Handles   int (... char *buffer, size_t buffer_size)
    /// </summary>
    class StringReturnTransformer : IArgumentTransformer
    {
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            if (!aNativeFunction.CurrentParameterType.MatchToPattern(
                new PointerCType(new NamedCType("char"))).IsMatch)
            {
                return false;
            }
            if (-1 != Matcher.CType(aNativeFunction.CurrentParameterType).FirstMatch(
                new NamedCType("size_t"),
                new NamedCType("int")
                ))
            {
                return false;
            }
            if (!aNativeFunction.ReturnType.MatchToPattern(
                new NamedCType("int")).IsMatch)
            {
                return false;
            }
            if (aNativeFunction.CurrentParameterIndex != aNativeFunction.ParameterCount - 2)
            {
                return false;
            }
            string lengthNativeType = ((NamedCType)aNativeFunction.NextParameterType).Name;
            string lengthManagedType = lengthNativeType == "size_t" ? "UIntPtr" : "int";
            string parameterName = aNativeFunction.CurrentParameter.Name;
            string utf8StringName = "utf8_"+parameterName;
            aAssembler.AddPInvokeParameter(new CSharpType("IntPtr"), aNativeFunction.CurrentParameter.Name, utf8StringName + ".IntPtr");
            aAssembler.AddPInvokeParameter(new CSharpType(lengthManagedType), aNativeFunction.NextParameter.Name, "(" + lengthManagedType + ")(" + utf8StringName + ".BufferLength)");
            aAssembler.SetPInvokeReturn(new CSharpType("int"), "stringLength_"+parameterName);
            aAssembler.SetManagedReturn(new CSharpType("string"));
            aAssembler.InsertAtTop(      "string returnValue;");
            aAssembler.InsertAtTop("int stringLength_" + parameterName + " = 256;");

            aAssembler.InsertBeforeCall("using (Utf8String " + utf8StringName + " = SpotifyMarshalling.AllocBuffer(stringLength_" + parameterName + "))");
            aAssembler.InsertBeforeCall("{");
            aAssembler.IncreaseIndent();
            aAssembler.InsertPreCall("stringLength_" + parameterName);
            aAssembler.InsertBeforeCall(utf8StringName + ".ReallocIfSmaller(stringLength_" + parameterName + " + 1);");

            aAssembler.InsertAfterCall("returnValue = " + utf8StringName + ".GetString(stringLength_" + parameterName + ");");
            aAssembler.DecreaseIndent();
            aAssembler.InsertAfterCall("}");

            aAssembler.InsertAtEnd("return returnValue;");
            aNativeFunction.ConsumeArgument();
            aNativeFunction.ConsumeArgument();
            aNativeFunction.ConsumeReturn();
            return true;
        }
    }
}