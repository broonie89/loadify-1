using System.Collections.Generic;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class HandleReturnTransformer : IArgumentTransformer
    {
        Dictionary<string, string> iHandlesToClassNames;

        public HandleReturnTransformer(IEnumerable<KeyValuePair<string, string>> aHandlesToClassNames)
        {
            iHandlesToClassNames = aHandlesToClassNames.ToDictionary(x=>x.Key, x=>x.Value);
        }

        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            if (aNativeFunction.CurrentParameter != null) { return false; }
            var pointerType = aNativeFunction.ReturnType as PointerCType;
            if (pointerType == null) return false;
            var namedType = pointerType.BaseType as NamedCType;
            if (namedType == null) return false;
            string className;
            if (!iHandlesToClassNames.TryGetValue(namedType.Name, out className))
            {
                return false;
            }

            aAssembler.InsertAtTop("IntPtr returnValue;");
            aAssembler.SetPInvokeReturn(new CSharpType("IntPtr"), "returnValue");
            aAssembler.SetManagedReturn(new CSharpType(className));
            aAssembler.InsertAtEnd("return (returnValue==IntPtr.Zero ? null : new "+className+"(returnValue));");
            aNativeFunction.ConsumeReturn();
            return true;
        }
    }
}