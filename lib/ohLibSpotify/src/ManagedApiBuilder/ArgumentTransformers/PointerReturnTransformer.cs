using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class PointerReturnTransformer : IArgumentTransformer
    {
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            if (aNativeFunction.CurrentParameter != null) return false;
            if (!(aNativeFunction.ReturnType is PointerCType)) return false;

            aAssembler.SetPInvokeReturn(new CSharpType("IntPtr"), null);
            aAssembler.SuppressManagedWrapper();
            aNativeFunction.ConsumeReturn();
            return true;
        }
    }
}