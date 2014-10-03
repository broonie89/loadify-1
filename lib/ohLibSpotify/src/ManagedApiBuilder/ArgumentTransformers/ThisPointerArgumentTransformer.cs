using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class ThisPointerArgumentTransformer : IArgumentTransformer
    {
        public string HandleType { get; set; }
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            if (aNativeFunction.CurrentParameterIndex != 0) return false;
            if (!aNativeFunction.CurrentParameterType.MatchToPattern(new PointerCType(new NamedCType(HandleType))).IsMatch)
            {
                return false;
            }
            aAssembler.AddPInvokeParameter(new CSharpType("IntPtr"), aNativeFunction.CurrentParameter.Name, "this._handle");
            aAssembler.IsStatic = false;
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}