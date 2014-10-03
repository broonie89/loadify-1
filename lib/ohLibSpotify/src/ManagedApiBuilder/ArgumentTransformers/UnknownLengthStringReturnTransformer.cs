using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    /// <summary>
    /// Handles (... char *buffer, int buffer_size ...)
    /// The returned string might be truncated to fit in the buffer. Since the function
    /// doesn't tell us how long the full string is, we just have to live with the
    /// truncation. (We use a fixed buffer size of 256. That seems to be the longest
    /// string the Spotify app will allow in such places.)
    /// </summary>
    class UnknownLengthStringReturnTransformer : IArgumentTransformer
    {
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            var matcher = Matcher.CType(
                new TupleCType(aNativeFunction.CurrentParameterType, aNativeFunction.NextParameterType));
            if (!matcher.Match(new TupleCType(
                new PointerCType(new NamedCType("char")),
                new NamedCType("int")
                )))
            {
                return false;
            }
            if (aNativeFunction.CurrentParameter.Name != "buffer") return false;
            if (aNativeFunction.NextParameter.Name != "buffer_size") return false;

            string parameterName = aNativeFunction.CurrentParameter.Name;
            string utf8StringName = "utf8_"+parameterName;
            aAssembler.AddPInvokeParameter(new CSharpType("IntPtr"), aNativeFunction.CurrentParameter.Name, utf8StringName + ".IntPtr");
            aAssembler.AddPInvokeParameter(new CSharpType("int"), aNativeFunction.NextParameter.Name, utf8StringName + ".BufferLength");
            aAssembler.SetManagedReturn(new CSharpType("string"));
            aAssembler.InsertAtTop(      "string returnValue;");

            aAssembler.InsertBeforeCall("using (Utf8String " + utf8StringName + " = SpotifyMarshalling.AllocBuffer(256))");
            aAssembler.InsertBeforeCall("{");
            aAssembler.IncreaseIndent();
            aAssembler.InsertAfterCall("returnValue = " + utf8StringName + ".Value;");
            aAssembler.DecreaseIndent();
            aAssembler.InsertAfterCall("}");

            aAssembler.InsertAtEnd("return returnValue;");
            aNativeFunction.ConsumeArgument();
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}