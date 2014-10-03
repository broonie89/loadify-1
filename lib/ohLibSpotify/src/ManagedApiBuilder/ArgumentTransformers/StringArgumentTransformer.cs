using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class StringArgumentTransformer : IArgumentTransformer
    {
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            var matcher = Matcher.CType(aNativeFunction.CurrentParameterType);
            if (!matcher.Match(new PointerCType(new NamedCType("char") { Qualifiers = { "const" } })))
            {
                return false;
            }
            string utf8StringName = "utf8_" + aNativeFunction.CurrentParameter.Name;
            aAssembler.AddPInvokeParameter(new CSharpType("IntPtr"), aNativeFunction.CurrentParameter.Name, utf8StringName + ".IntPtr");
            aAssembler.AddManagedParameter(aNativeFunction.CurrentParameter.Name, new CSharpType("string"));
            aAssembler.InsertBeforeCall("using (Utf8String " + utf8StringName + " = SpotifyMarshalling.StringToUtf8(" + aNativeFunction.CurrentParameter.Name + "))");
            aAssembler.InsertBeforeCall("{");
            aAssembler.IncreaseIndent();
            aAssembler.DecreaseIndent();
            aAssembler.InsertAfterCall("}");
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}