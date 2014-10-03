using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class PointerArgumentTransformer : IArgumentTransformer
    {
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            var pointerType = aNativeFunction.CurrentParameterType as PointerCType;
            if (pointerType == null) return false;
            var namedType = pointerType.BaseType as NamedCType;
            if (namedType == null) return false;

            // Accept anything.

            aAssembler.AddPInvokeParameter(new CSharpType("IntPtr"), aNativeFunction.CurrentParameter.Name, null);
            aAssembler.SuppressManagedWrapper();
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}