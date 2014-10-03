using System.Collections.Generic;
using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class RefHandleArgumentTransformer : IArgumentTransformer
    {
        readonly HashSet<string> iHandleNames;

        public RefHandleArgumentTransformer(IEnumerable<string> aHandleNames)
        {
            iHandleNames = new HashSet<string>(aHandleNames);
        }
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            PointerCType pointer1Type = aNativeFunction.CurrentParameterType as PointerCType;
            if (pointer1Type == null) return false;
            PointerCType pointer2Type = pointer1Type.BaseType as PointerCType;
            if (pointer2Type == null) return false;
            NamedCType nativeType = pointer2Type.BaseType as NamedCType;
            if (nativeType == null) return false;
            if (!iHandleNames.Contains(nativeType.Name)) return false;

            aAssembler.AddPInvokeParameter(
                new CSharpType("IntPtr") { IsRef = true },
                aNativeFunction.CurrentParameter.Name,
                null);
            aAssembler.SuppressManagedWrapper();
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}