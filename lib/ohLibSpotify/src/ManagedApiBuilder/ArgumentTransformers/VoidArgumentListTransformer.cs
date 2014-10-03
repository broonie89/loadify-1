// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using ApiParser;

namespace ManagedApiBuilder.ArgumentTransformers
{
    class VoidArgumentListTransformer : IArgumentTransformer
    {
        public bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aAssembler)
        {
            if (aNativeFunction.CurrentParameterIndex != 0) return false;
            if (!aNativeFunction.CurrentParameterType.MatchToPattern(new NamedCType("void")).IsMatch)
            {
                return false;
            }
            aNativeFunction.ConsumeArgument();
            return true;
        }
    }
}