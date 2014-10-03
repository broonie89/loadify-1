using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class SpotifyErrorReturnTransformer : IArgumentTransformer
    {
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aFunctionAssembler)
        {
            if (aNativeFunction.CurrentParameter != null) { return false; }
            var namedType = aNativeFunction.ReturnType as NamedCType;
            if (namedType == null) { return false; }
            if (namedType.Name != "sp_error") { return false; }
            aFunctionAssembler.InsertAtTop("SpotifyError errorValue;");
            aFunctionAssembler.SetPInvokeReturn(new CSharpType("SpotifyError"), "errorValue");
            aFunctionAssembler.InsertAtEnd("SpotifyMarshalling.CheckError(errorValue);");
            aNativeFunction.ConsumeReturn();
            return true;
        }
    }
}