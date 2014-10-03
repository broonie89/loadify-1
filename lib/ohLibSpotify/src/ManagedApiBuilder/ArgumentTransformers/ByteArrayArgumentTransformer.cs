using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    public class ByteArrayArgumentTransformer : IArgumentTransformer
    {
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            var arrayType = aNativeFunction.CurrentParameterType as ArrayCType;
            if (arrayType == null) return false;
            var namedType = arrayType.BaseType as NamedCType;
            if (namedType == null) return false;

            switch (namedType.Name)
            {
                case "byte":
                    break;
                default:
                    return false;
            }

            aAssembler.AddPInvokeParameter(new CSharpType("IntPtr"), aNativeFunction.CurrentParameter.Name, null);
            aAssembler.SuppressManagedWrapper();
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}