using System.Collections.Generic;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class RefStructArgumentTransformer : IArgumentTransformer
    {
        readonly Dictionary<string, string> iHandlesToStructNames;

        public RefStructArgumentTransformer(IEnumerable<KeyValuePair<string, string>> aHandlesToStructNames)
        {
            iHandlesToStructNames = aHandlesToStructNames.ToDictionary(x=>x.Key, x=>x.Value);
        }

        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            var pointerType = aNativeFunction.CurrentParameterType as PointerCType;
            if (pointerType == null) return false;
            var namedType = pointerType.BaseType as NamedCType;
            if (namedType == null) return false;
            string structName;
            if (!iHandlesToStructNames.TryGetValue(namedType.Name, out structName))
            {
                return false;
            }

            aAssembler.AddPInvokeParameter(new CSharpType(structName) { IsRef = true }, aNativeFunction.CurrentParameter.Name, null);
            aAssembler.SuppressManagedWrapper();
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}