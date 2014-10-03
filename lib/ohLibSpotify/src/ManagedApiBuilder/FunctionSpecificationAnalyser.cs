// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System.Collections.Generic;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder
{
    public interface IFunctionSpecificationAnalyser
    {
        int CurrentParameterIndex { get; }
        Declaration CurrentParameter { get; }
        Declaration NextParameter { get; }
        int ParameterCount { get; }
        CType CurrentParameterType { get; }
        CType NextParameterType { get; }
        CType ReturnType { get; }
        void ConsumeArgument();
        void ConsumeReturn();
    }

    public class FunctionSpecificationAnalyser : IFunctionSpecificationAnalyser
    {
        int iIndex = 0;
        readonly List<Declaration> iParameters;
        CType iReturnType;

        public FunctionSpecificationAnalyser(List<Declaration> aParameters, CType aReturnType)
        {
            iParameters = aParameters;
            iReturnType = aReturnType;
        }

        public int CurrentParameterIndex { get { return iIndex; } }

        public Declaration CurrentParameter { get { return iParameters.ElementAtOrDefault(iIndex); } }

        public Declaration NextParameter { get { return iParameters.ElementAtOrDefault(iIndex+1); } }

        public int ParameterCount { get { return iParameters.Count; } }

        public CType CurrentParameterType
        {
            get
            {
                var parameter = CurrentParameter;
                return parameter == null ? null : parameter.CType;
            }
        }

        public CType NextParameterType
        {
            get
            {
                var parameter = NextParameter;
                return parameter == null ? null : parameter.CType;
            }
        }

        public CType ReturnType { get { return iReturnType; } }

        public void ConsumeArgument()
        {
            if (iIndex < iParameters.Count)
            {
                iIndex += 1;
            }
        }
        public void ConsumeReturn()
        {
            iReturnType = null;
        }
    }
}