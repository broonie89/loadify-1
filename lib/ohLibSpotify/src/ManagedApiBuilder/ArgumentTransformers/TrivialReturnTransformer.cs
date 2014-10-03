using System.Collections.Generic;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class TrivialReturnTransformer : IArgumentTransformer
    {
        Dictionary<string, string> iEnumNativeToManagedMappings;
        public TrivialReturnTransformer(IEnumerable<KeyValuePair<string, string>> aEnumNativeToManagedMappings)
        {
            iEnumNativeToManagedMappings = aEnumNativeToManagedMappings.ToDictionary(x => x.Key, x => x.Value);
        }
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aFunctionAssembler)
        {
            if (aNativeFunction.CurrentParameter != null) { return false; }
            var namedType = aNativeFunction.ReturnType as NamedCType;
            if (namedType == null) { return false; }
            CSharpType pinvokeArgType;
            CSharpType managedArgType;
            switch (namedType.Name)
            {
                case "bool":
                    pinvokeArgType = new CSharpType("bool"){ Attributes = { "MarshalAs(UnmanagedType.I1)" } };
                    managedArgType = new CSharpType("bool");
                    break;
                case "int":
                    pinvokeArgType = managedArgType = new CSharpType("int");
                    break;
                case "sp_uint64":
                    pinvokeArgType = managedArgType = new CSharpType("ulong");
                    break;
                default:
                    string managedEnumName;
                    if (!iEnumNativeToManagedMappings.TryGetValue(namedType.Name, out managedEnumName))
                    {
                        return false;
                    }
                    pinvokeArgType = managedArgType = new CSharpType(managedEnumName);
                    break;
            }
            aFunctionAssembler.InsertAtTop(managedArgType.Name + " returnValue;");
            aFunctionAssembler.SetPInvokeReturn(pinvokeArgType, "returnValue");
            aFunctionAssembler.SetManagedReturn(managedArgType);
            aFunctionAssembler.InsertAtEnd("return returnValue;");
            aNativeFunction.ConsumeReturn();
            return true;
        }
    }
}