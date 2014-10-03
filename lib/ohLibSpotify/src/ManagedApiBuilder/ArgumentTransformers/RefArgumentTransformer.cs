using System.Collections.Generic;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class RefArgumentTransformer : IArgumentTransformer
    {
        readonly Dictionary<string, string> iEnumNativeToManagedMappings;
        public RefArgumentTransformer(
            IEnumerable<KeyValuePair<string, string>> aEnumNativeToManagedMappings
            )
        {
            iEnumNativeToManagedMappings = aEnumNativeToManagedMappings.ToDictionary(x => x.Key, x => x.Value);
        }

        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {

            PointerCType pointerType = aNativeFunction.CurrentParameterType as PointerCType;
            if (pointerType == null) return false;
            NamedCType nativeType = pointerType.BaseType as NamedCType;
            if (nativeType == null) return false;


            CSharpType pinvokeArgType;
            CSharpType managedArgType;
            switch (nativeType.Name)
            {
                case "bool":
                    pinvokeArgType = new CSharpType("bool") { IsRef = true, Attributes = { "MarshalAs(UnmanagedType.I1)" } };
                    managedArgType = new CSharpType("bool") { IsRef = true };
                    break;
                case "int":
                    pinvokeArgType = managedArgType = new CSharpType("int") { IsRef = true };
                    break;
                case "size_t":
                    pinvokeArgType = managedArgType = new CSharpType("UIntPtr") { IsRef = true };
                    break;
                case "sp_uint64":
                    pinvokeArgType = managedArgType = new CSharpType("ulong") { IsRef = true };
                    break;
                default:
                    string managedEnumName;
                    if (!iEnumNativeToManagedMappings.TryGetValue(nativeType.Name, out managedEnumName))
                    {
                        return false;
                    }
                    pinvokeArgType = managedArgType = new CSharpType(managedEnumName) { IsRef = true };
                    break;
            }

            aAssembler.AddPInvokeParameter(pinvokeArgType, aNativeFunction.CurrentParameter.Name, "ref @" + aNativeFunction.CurrentParameter.Name);
            aAssembler.AddManagedParameter(aNativeFunction.CurrentParameter.Name, managedArgType);
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}