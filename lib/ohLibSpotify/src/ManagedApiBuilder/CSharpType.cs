using System;
using System.Collections.Generic;

namespace ManagedApiBuilder
{
    public class CSharpType
    {
        public List<string> Attributes { get; private set; }
        public bool IsRef { get; set; }
        public string Name { get; private set; }
        public CSharpType(string aName)
        {
            Name = aName;
            Attributes = new List<string>();
        }
        string GetTypeString()
        {
            return (IsRef ? "ref " : "") + Name;
        }
        string GetAttributeString(string aPrefix = "")
        {
            if (Attributes.Count == 0)
                return "";
            return String.Format(
                "[{0}{1}]",
                aPrefix,
                String.Join(
                    ", ",
                    Attributes));
        }
        public string CreateParameterDeclaration(string aParameterName)
        {
            return GetAttributeString() + GetTypeString() + " @" + aParameterName;
        }
        public string CreateReturnTypeDeclaration()
        {
            return GetTypeString();
        }
        public string CreateReturnTypeAttribute()
        {
            if (IsRef) throw new Exception("Cannot use ref type as return type.");
            return GetAttributeString("return:");
        }
        public string CreateFieldAttribute()
        {
            if (IsRef) throw new Exception("Cannot use ref type as return type.");
            return GetAttributeString();
        }
        public string CreateFieldDeclaration(string aFieldName)
        {
            if (IsRef) throw new Exception("Cannot use ref type as field type.");
            return GetTypeString() + " @" + aFieldName;
        }
        public override string ToString()
        {
            return GetAttributeString() + GetTypeString();
        }
    }
}