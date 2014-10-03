using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class VoidReturnTransformer : IArgumentTransformer
    {
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aFunctionAssembler)
        {
            if (aNativeFunction.CurrentParameter != null) { return false; }
            if (!aNativeFunction.ReturnType.MatchToPattern(new NamedCType("void")).IsMatch)
            {
                return false;
            }
            if (aNativeFunction.ReturnType == null)
            {
                return false;
            }
            // Leave it at the default.
            // aFunctionAssembler.SetManagedReturn(new CSharpType("void"));
            aNativeFunction.ConsumeReturn();
            return true;
        }
    }
}