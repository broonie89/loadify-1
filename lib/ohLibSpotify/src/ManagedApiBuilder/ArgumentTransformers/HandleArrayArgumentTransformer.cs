using System.Collections.Generic;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class HandleArrayArgumentTransformer : IArgumentTransformer
    {
        readonly Dictionary<string, string> iHandlesToClassNames;

        public HandleArrayArgumentTransformer(IEnumerable<KeyValuePair<string, string>> aHandlesToClassNames)
        {
            iHandlesToClassNames = aHandlesToClassNames.ToDictionary(x=>x.Key, x=>x.Value);
        }
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            var firstArgType = aNativeFunction.CurrentParameterType;
            var secondArgType = aNativeFunction.NextParameterType;
            bool secondArgIsInt = secondArgType.MatchToPattern(new NamedCType("int")).IsMatch;
            if (!secondArgIsInt) { return false; }

            bool firstArgIsPointer = firstArgType is PointerCType;
            if (!firstArgIsPointer) { return false; }
            var derefOnceType = firstArgType.ChildType;

            bool firstArgIsPointerToPointer = derefOnceType is PointerCType;
            if (!firstArgIsPointerToPointer) { return false; }
            var derefTwiceType = derefOnceType.ChildType;

            bool firstArgIsPointerToPointerToNamedType =  derefTwiceType is NamedCType;
            if (!firstArgIsPointerToPointerToNamedType) { return false; }
            var elementType = (NamedCType)derefTwiceType;

            bool arrayIsMutable = !derefOnceType.Qualifiers.Contains("const");

            string arg1name = aNativeFunction.CurrentParameter.Name;
            string arg2name = aNativeFunction.NextParameter.Name;
            if (arg2name != "num_" + arg1name) { return false; }
            string className;
            if (!iHandlesToClassNames.TryGetValue(elementType.Name, out className)) { return false; }

            // Finally, we are sure that this is an array of handles.
            // Now we need to marshal the cursed thing!
            string paramName = aNativeFunction.CurrentParameter.Name;

            aAssembler.AddPInvokeParameter(
                new CSharpType("IntPtr"),
                paramName,
                "array_" + paramName + ".IntPtr");
            aAssembler.AddPInvokeParameter(
                new CSharpType("int"),
                "num_" + paramName,
                "array_" + paramName + ".Length");
            aAssembler.AddManagedParameter(
                paramName,
                new CSharpType(className + "[]"));

            aAssembler.InsertBeforeCall("using (var array_" + paramName + " = SpotifyMarshalling.ArrayToNativeArray(" + paramName + ", x=>x._handle))");
            aAssembler.InsertBeforeCall("{");
            aAssembler.IncreaseIndent();
            if (arrayIsMutable)
            {
                // Unless we pass in a 'handle * const *', the function might have changed
                // the content of the array, so copy it back.
                aAssembler.InsertAfterCall("array_" + paramName + ".CopyTo(" + paramName + ", ptr => ptr == IntPtr.Zero ? null : new " + className + "(ptr));");
            }
            aAssembler.DecreaseIndent();
            aAssembler.InsertAfterCall( "}");

            aNativeFunction.ConsumeArgument();
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}