using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class SimpleStringReturnTransformer : IArgumentTransformer
    {
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aFunctionAssembler)
        {
            if (aNativeFunction.CurrentParameter != null) { return false; }
            if (!aNativeFunction.ReturnType.MatchToPattern(new PointerCType(new NamedCType("char") { Qualifiers = { "const" } })).IsMatch)
            {
                return false;
            }
            aFunctionAssembler.InsertAtTop("IntPtr returnValue;");
            aFunctionAssembler.SetPInvokeReturn(new CSharpType("IntPtr"), "returnValue");
            aFunctionAssembler.SetManagedReturn(new CSharpType("string"));
            aFunctionAssembler.InsertAtEnd("return SpotifyMarshalling.Utf8ToString(returnValue);");
            aNativeFunction.ConsumeReturn();
            return true;
        }
    }
}