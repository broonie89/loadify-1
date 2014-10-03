using System.Collections.Generic;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class TrivialArgumentTransformer : IArgumentTransformer
    {
        Dictionary<string, string> iEnumNativeToManagedMappings;
        public TrivialArgumentTransformer(IEnumerable<KeyValuePair<string, string>> aEnumNativeToManagedMappings)
        {
            iEnumNativeToManagedMappings = aEnumNativeToManagedMappings.ToDictionary(x => x.Key, x => x.Value);
        }
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            NamedCType nativeType = aNativeFunction.CurrentParameterType as NamedCType;
            if (nativeType == null) return false;
            CSharpType pinvokeArgType;
            CSharpType managedArgType;
            switch (nativeType.Name)
            {
                case "bool":
                    pinvokeArgType = new CSharpType("bool"){ Attributes = { "MarshalAs(UnmanagedType.I1)" } };
                    managedArgType = new CSharpType("bool");
                    break;
                case "int":
                    pinvokeArgType = managedArgType = new CSharpType("int");
                    break;
                case "size_t":
                    pinvokeArgType = managedArgType = new CSharpType("UIntPtr");
                    break;
                default:
                    string managedEnumName;
                    if (!iEnumNativeToManagedMappings.TryGetValue(nativeType.Name, out managedEnumName))
                    {
                        return false;
                    }
                    pinvokeArgType = managedArgType = new CSharpType(managedEnumName);
                    break;
            }
            aAssembler.AddPInvokeParameter(pinvokeArgType, aNativeFunction.CurrentParameter.Name, aNativeFunction.CurrentParameter.Name);
            aAssembler.AddManagedParameter(aNativeFunction.CurrentParameter.Name, managedArgType);
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}