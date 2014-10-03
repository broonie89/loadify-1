using System.Collections.Generic;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    /// <summary>
    /// Handle arguments that are pointers to structures of callbacks.
    /// </summary>
    /// <remarks>
    /// Callback structs are expected to live beyond the call to the native function,
    /// so they can't be marshalled as ref parameters. Instead use IntPtr and the
    /// caller will need to wrangle Marshal.StructureToPtr appropriately.
    /// We identify callback structs simply by their suffix, "_callbacks".
    /// </remarks>
    class CallbackStructArgumentTransformer : IArgumentTransformer
    {
        readonly Dictionary<string, string> iHandlesToStructNames;

        public CallbackStructArgumentTransformer(IEnumerable<KeyValuePair<string, string>> aHandlesToStructNames)
        {
            iHandlesToStructNames = aHandlesToStructNames.Where(x=>x.Key.EndsWith("_callbacks")).ToDictionary(x=>x.Key, x=>x.Value);
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

            aAssembler.AddPInvokeParameter(new CSharpType("IntPtr"), aNativeFunction.CurrentParameter.Name, null);
            aAssembler.SuppressManagedWrapper();
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}