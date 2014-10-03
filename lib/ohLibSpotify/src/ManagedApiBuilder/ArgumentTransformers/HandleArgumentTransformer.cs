using System.Collections.Generic;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class HandleArgumentTransformer : IArgumentTransformer
    {
        Dictionary<string, string> iHandlesToClassNames;

        public HandleArgumentTransformer(IEnumerable<KeyValuePair<string, string>> aHandlesToClassNames)
        {
            iHandlesToClassNames = aHandlesToClassNames.ToDictionary(x=>x.Key, x=>x.Value);
        }

        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            var pointerType = aNativeFunction.CurrentParameterType as PointerCType;
            if (pointerType == null) return false;
            var namedType = pointerType.BaseType as NamedCType;
            if (namedType == null) return false;
            string className;
            if (!iHandlesToClassNames.TryGetValue(namedType.Name, out className))
            {
                return false;
            }

            aAssembler.AddPInvokeParameter(new CSharpType("IntPtr"), aNativeFunction.CurrentParameter.Name, aNativeFunction.CurrentParameter.Name + "._handle");
            aAssembler.AddManagedParameter(aNativeFunction.CurrentParameter.Name, new CSharpType(className));
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}