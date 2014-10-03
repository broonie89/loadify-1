namespace ManagedApiBuilder
{
    interface IArgumentTransformer
    {
        bool Apply(IFunctionSpecificationAnalyser aNativeFunction, IFunctionAssembler aFunctionAssembler);
    }
}