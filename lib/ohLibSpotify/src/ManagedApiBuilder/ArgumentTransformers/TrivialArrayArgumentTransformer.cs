using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class TrivialArrayArgumentTransformer : IArgumentTransformer
    {
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            var matcher = Matcher.CType(new TupleCType(
                aNativeFunction.CurrentParameterType,
                aNativeFunction.NextParameterType));
            if (!matcher.Match(new TupleCType(
                new PointerCType(new VariableCType("element-type")),
                new NamedCType("int"))))
            {
                return false;
            }
            var elementType = matcher.BoundVariables["element-type"] as NamedCType;
            if (elementType == null) { return false; }
            string arg1name = aNativeFunction.CurrentParameter.Name;
            string arg2name = aNativeFunction.NextParameter.Name;
            if (arg2name != "num_" + arg1name) { return false; }

            string managedType;

            switch (elementType.Name)
            {
                case "int":
                    managedType = "int";
                    break;
                default:
                    return false;
            }

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
                new CSharpType(managedType + "[]"));

            aAssembler.InsertBeforeCall("using (var array_" + paramName + " = SpotifyMarshalling.ArrayToNativeArray(" + paramName + "))");
            aAssembler.InsertBeforeCall("{");
            aAssembler.IncreaseIndent();
            aAssembler.InsertAfterCall(     "Array.Copy(array_"+paramName+".Value(), "+paramName+", "+paramName+".Length);");
            aAssembler.DecreaseIndent();
            aAssembler.InsertAfterCall("}");

            aNativeFunction.ConsumeArgument();
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}