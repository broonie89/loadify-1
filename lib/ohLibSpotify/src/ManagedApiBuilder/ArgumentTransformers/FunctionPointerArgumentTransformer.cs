using System.Collections.Generic;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class FunctionPointerArgumentTransformer : IArgumentTransformer
    {
        Dictionary<string, string> iFunctionTypedefsToDelegates;

        public FunctionPointerArgumentTransformer(IEnumerable<KeyValuePair<string, string>> aFunctionTypedefsToDelegates)
        {
            iFunctionTypedefsToDelegates = aFunctionTypedefsToDelegates.ToDictionary(x=>x.Key, x=>x.Value);
        }

        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            var pointerType = aNativeFunction.CurrentParameterType as PointerCType;
            if (pointerType == null) return false;
            var namedType = pointerType.BaseType as NamedCType;
            if (namedType == null) return false;
            string className;
            if (!iFunctionTypedefsToDelegates.TryGetValue(namedType.Name, out className))
            {
                return false;
            }

            aAssembler.AddPInvokeParameter(new CSharpType(className), aNativeFunction.CurrentParameter.Name, null);
            aAssembler.SuppressManagedWrapper();
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}