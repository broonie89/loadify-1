// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder
{

    public static class Matcher
    {
        public static PatternMatcher<CType, CType> CType(CType aItem)
        {
            return new PatternMatcher<CType, CType>(new CTypeTreeWalker(), aItem);
        }
    }

    public interface IPatternTreeWalker<TPatternNode, TTreeNode>
    {
        /// <summary>
        /// Get the children of a node in the pattern we're using to match.
        /// </summary>
        /// <param name="aNode"></param>
        /// <returns></returns>
        IEnumerable<TPatternNode> PatternChildren(TPatternNode aNode);
        /// <summary>
        /// Get the children of a node in the tree we're trying to match.
        /// </summary>
        /// <param name="aNode"></param>
        /// <returns></returns>
        IEnumerable<TTreeNode> TreeChildren(TTreeNode aNode);
        /// <summary>
        /// If the node is a variable, return its name, otherwise return null.
        /// </summary>
        /// <param name="aNode"></param>
        /// <returns></returns>
        string Variable(TPatternNode aNode);
        /// <summary>
        /// Check whether the nodes are equivalent, ignoring their children.
        /// </summary>
        /// <param name="aPatternNode"></param>
        /// <param name="aSecond"></param>
        /// <returns></returns>
        bool NodeMatch(TPatternNode aPatternNode, TTreeNode aSecond);
    }

    // Used during pattern matching. If CType is null it matches anything,
    // while if CType is non-null it matches as CType. Regardless, when it
    // matches, it binds the match to the given variable name.
    public class VariableCType : CType
    {
        public string Name { get; set; }
        public CType CType { get; set; }
        public override string FundamentalType()
        {
            return Name;
        }
        public override string ToString()
        {
            return Name;
        }
        protected override void ConstructDeclaration(List<string> aPrefix, List<string> aSuffix)
        {
            return;
        }

        public VariableCType(string aName)
        {
            Name = aName;
        }
        public VariableCType(string aName, CType aCType)
        {
            Name = aName;
            CType = aCType;
        }
    }

    // Convenience for pattern matching. Allows us to construct an expression
    // of a pair of CTypes and match both at once.
    public class TupleCType : CType
    {
        public CType First { get; set; }
        public CType Second { get; set; }
        public CType[] Items { get; private set; }
        public override string ToString()
        {
            return String.Format("({0})", String.Join(", ", Items.Select(x => x.ToString())));
        }
        protected override void ConstructDeclaration(List<string> aPrefix, List<string> aSuffix)
        {
            throw new NotImplementedException();
        }

        public TupleCType(params CType[] aItems)
        {
            Items = aItems;
        }
    }

    class CTypeTreeWalker : IPatternTreeWalker<CType, CType>
    {
        public IEnumerable<CType> PatternChildren(CType aNode)
        {
            var pointerType = aNode as PointerCType;
            var arrayType = aNode as ArrayCType;
            var namedType = aNode as NamedCType;
            var functionType = aNode as FunctionCType;
            var structType = aNode as StructCType;
            var enumType = aNode as EnumCType;
            var variableType = aNode as VariableCType;
            var tupleType = aNode as TupleCType;

            if (pointerType != null)
            {
                yield return pointerType.BaseType;
                yield break;
            }
            if (arrayType != null)
            {
                yield return arrayType.BaseType;
                yield break;
            }
            if (namedType != null)
            {
                yield break;
            }
            if (functionType != null)
            {
                foreach (var arg in functionType.Arguments)
                {
                    yield return arg.CType;
                }
                yield return functionType.ReturnType;
                yield break;
            }
            if (structType != null)
            {
                foreach (var field in structType.Fields)
                {
                    yield return field.CType;
                }
                yield break;
            }
            if (enumType != null)
            {
                yield break;
            }
            if (variableType != null)
            {
                if (variableType.CType != null)
                {
                    foreach (var item in PatternChildren(variableType.CType))
                    {
                        yield return item;
                    }
                }
                yield break;
            }
            if (tupleType != null)
            {
                foreach (var item in tupleType.Items)
                {
                    yield return item;
                }
                yield break;
            }
        }

        public IEnumerable<CType> TreeChildren(CType aNode)
        {
            return PatternChildren(aNode);
        }

        public string Variable(CType aNode)
        {
            VariableCType variableType = aNode as VariableCType;
            return variableType != null ? variableType.Name : null;
        }

        public bool NodeMatch(CType aPatternNode, CType aTreeNode)
        {
            if (aPatternNode == null && aTreeNode == null) return true;
            if (aPatternNode == null || aTreeNode == null) return false;
            VariableCType variableType = aPatternNode as VariableCType;
            if (variableType != null)
            {
                if (variableType.CType == null) return true;
                return NodeMatch(variableType.CType, aTreeNode);
            }
            if (aPatternNode.GetType() != aTreeNode.GetType())
            {
                return false;
            }
            NamedCType namedPatternType = aPatternNode as NamedCType;
            NamedCType namedTreeType = aTreeNode as NamedCType;
            if (namedPatternType != null)
            {
                Debug.Assert(namedTreeType != null); // Cannot be null as we already checked they have the same type.
                if (namedPatternType.Name != namedTreeType.Name)
                {
                    return false;
                }
            }
            ArrayCType arrayPatternType = aPatternNode as ArrayCType;
            ArrayCType arrayTreeType = aTreeNode as ArrayCType;
            if (arrayPatternType != null)
            {
                Debug.Assert(arrayTreeType != null);
                if (arrayPatternType.Dimension != arrayTreeType.Dimension)
                {
                    return false;
                }
            }
            if (!aPatternNode.Qualifiers.SetEquals(aTreeNode.Qualifiers))
            {
                return false;
            }
            return true;
        }
    }

    public class CTypeFactory
    {
        public CType Ptr(CType aBaseType) { return new PointerCType(aBaseType); }
        public CType Type(string aTypeName) { return new NamedCType(aTypeName); }
        public CType Array(int aDimension, CType aBaseType) { return new ArrayCType(aDimension, aBaseType); }
        public CType Array(CType aBaseType) { return new ArrayCType(null, aBaseType); }
        public CType Variable(string aVariableName) { return new VariableCType(aVariableName); }
        public CType Pair(CType aFirst, CType aSecond) { return new TupleCType(aFirst, aSecond); }
        public CType Tuple(params CType[] aItems) { return new TupleCType(aItems); }
    }

    public static class ExtensionMethods
    {
        public static PatternMatcher<CType, CType> MatchToPattern(this CType aCType, CType aPattern)
        {
            PatternMatcher<CType, CType> matcher = new PatternMatcher<CType, CType>(new CTypeTreeWalker(), aCType);
            matcher.Match(aPattern);
            return matcher;
        }
    }

    public class PatternMatcher<TPatternNode, TTreeNode>
    {
        readonly IPatternTreeWalker<TPatternNode, TTreeNode> iWalker;
        TTreeNode Value { get; set; }
        public Dictionary<string, TTreeNode> BoundVariables { get; private set; }
        public bool IsMatch { get; set; }
        public PatternMatcher(IPatternTreeWalker<TPatternNode, TTreeNode> aWalker, TTreeNode aValue)
        {
            iWalker = aWalker;
            Value = aValue;
        }

        public bool Match(TPatternNode aPattern)
        {
            Dictionary<string, TTreeNode> boundVariables;
            if (TryMatch(aPattern, Value, out boundVariables))
            {
                BoundVariables = boundVariables;
                return IsMatch = true;
            }
            BoundVariables = null;
            return IsMatch = false;
        }

        public int FirstMatch(params TPatternNode[] aPatterns)
        {
            for (int i = 0; i != aPatterns.Length; ++i)
            {
                if (Match(aPatterns[i])) return i;
            }
            return -1;
        }

        bool TryMatch(TPatternNode aPattern, TTreeNode aTree, out Dictionary<string, TTreeNode> aBoundVariables)
        {
            //Dictionary<string, TTreeNode> boundVariables;
            string variable = iWalker.Variable(aPattern);
            if (!iWalker.NodeMatch(aPattern, aTree))
            {
                aBoundVariables = null;
                return false;
            }
            var patternChildren = iWalker.PatternChildren(aPattern).ToList();
            var treeChildren = iWalker.TreeChildren(aTree).ToList();
            if (patternChildren.Count != treeChildren.Count)
            {
                aBoundVariables = null;
                return false;
            }
            var boundVariables = new Dictionary<string, TTreeNode>();
            if (variable != null)
            {
                boundVariables.Add(variable, aTree);
            }
            for (int i = 0; i != patternChildren.Count; ++i)
            {
                Dictionary<string, TTreeNode> recursiveVariables;
                bool recursiveResult = TryMatch(patternChildren[i], treeChildren[i], out recursiveVariables);
                if (!recursiveResult)
                {
                    aBoundVariables = null;
                    return false;
                }
                foreach (var kvp in recursiveVariables)
                {
                    boundVariables[kvp.Key] = kvp.Value;
                }
            }
            aBoundVariables = boundVariables;
            return true;
        }
    }
}