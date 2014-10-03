// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System.Collections.Generic;
using System.Linq;
using ApiParser;

namespace ManagedApiBuilder
{
    class CategorizedDeclarations
    {
        public OrderedDictionary<string, FunctionCType> FunctionTable { get; private set; }
        public OrderedDictionary<string, StructCType> StructTable { get; private set; }
        public OrderedDictionary<string, EnumCType> EnumTable { get; private set; }
        public OrderedDictionary<string, FunctionCType> FunctionTypedefTable { get; private set; }
        public HashSet<string> HandleTable { get; private set; }
        public HashSet<string> DeclarationsToIgnore { get; private set; }

        public CategorizedDeclarations(IEnumerable<string> aDeclarationsToIgnore)
        {
            FunctionTable = new OrderedDictionary<string, FunctionCType>();
            StructTable = new OrderedDictionary<string, StructCType>();
            EnumTable = new OrderedDictionary<string, EnumCType>();
            FunctionTypedefTable = new OrderedDictionary<string, FunctionCType>();
            HandleTable = new HashSet<string>();
            DeclarationsToIgnore = new HashSet<string>(aDeclarationsToIgnore);
        }

        public void AddDeclarations(IEnumerable<Declaration> aDeclarations)
        {
            foreach (var decl in aDeclarations)
            {
                if (DeclarationsToIgnore.Contains(decl.Name)) continue;
                //== "sp_uint64" || decl.Name == "bool" || decl.Name == "byte") continue;
                if (decl.Kind == "typedef")
                {
                    StructCType structType = decl.CType as StructCType;
                    EnumCType enumType = decl.CType as EnumCType;
                    FunctionCType functionType = decl.CType as FunctionCType;
                    if (structType != null)
                    {
                        if (structType.Fields == null)
                        {
                            HandleTable.Add(decl.Name);
                        }
                        else
                        {
                            StructTable.Add(decl.Name, structType);
                        }
                    }
                    else if (enumType != null)
                    {
                        EnumTable.Add(decl.Name, enumType);
                    }
                    else if (functionType != null)
                    {
                        FunctionTypedefTable.Add(decl.Name, functionType);
                    }
                }
                else if (decl.Kind == "instance")
                {
                    FunctionCType funcType = decl.CType as FunctionCType;
                    if (funcType == null) continue;
                    FunctionTable.Add(decl.Name, funcType);
                }
            }
        }

        public OrderedDictionary<string, FunctionCType> FindAnonymousDelegates()
        {
            OrderedDictionary<string, FunctionCType> results = new OrderedDictionary<string, FunctionCType>();
            foreach (var kvp in StructTable)
            {
                var structName = kvp.Key;
                var structType = kvp.Value;
                foreach (var fieldDeclaration in structType.Fields)
                {
                    var fieldName = fieldDeclaration.Name;
                    var fieldType = fieldDeclaration.CType;
                    var pointerType = fieldType as PointerCType;
                    if (pointerType != null)
                    {
                        var pointedType = pointerType.BaseType;
                        var functionType = pointedType as FunctionCType;
                        if (functionType != null)
                        {
                            // What if there are duplicate names!?
                            results.Add(fieldName, functionType);
                        }
                    }
                }
            }
            return results;
        }

        public void CategorizeFunctions(
            out OrderedDictionary<string, SpotifyClass> aClasses,
            out OrderedDictionary<string, FunctionCType> aFunctions)
        {
            var classTable = new OrderedDictionary<string, SpotifyClass>();
            var unclaimedFunctions = new HashSet<string>(FunctionTable.Keys);
            foreach (string handleName in HandleTable)
            {
                var spotifyClass = new SpotifyClass(handleName);
                foreach (var kvp in FunctionTable)
                {
                    string name = kvp.Key;
                    var function = kvp.Value;
                    if (name.StartsWith(handleName + "_"))
                    {
                        spotifyClass.AddFunction(name, function);
                        unclaimedFunctions.Remove(name);
                    }
                }
                classTable.Add(handleName, spotifyClass);
            }
            aClasses = classTable;
            aFunctions = new OrderedDictionary<string, FunctionCType>(FunctionTable.Where(x => unclaimedFunctions.Contains(x.Key)));
        }
    }

    class SpotifyClass
    {
        public string HandleName { get; private set; }
        public OrderedDictionary<string, FunctionCType> NativeFunctions { get; private set; }

        public SpotifyClass(string aHandleName)
        {
            HandleName = aHandleName;
            NativeFunctions = new OrderedDictionary<string, FunctionCType>();
        }

        public void AddFunction(string aName, FunctionCType aFunction)
        {
            NativeFunctions.Add(aName, aFunction);
        }
    }
}