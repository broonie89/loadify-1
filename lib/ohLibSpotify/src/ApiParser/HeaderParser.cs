// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiParser
{
    [JsonObject]
    //[JsonConverter(typeof(CTypeConverter))]
    public abstract class CType
    {
        [JsonProperty("qualifiers", Order=99)]
        public HashSet<string> Qualifiers { get; private set; }
        protected CType()
        {
            Qualifiers = new HashSet<string>();
        }

        public virtual string FundamentalType()
        {
            throw new NotImplementedException();
        }

        [JsonIgnore]
        public virtual CType ChildType { get { return null; } }

        public override string ToString()
        {
            return CreateDeclaration("");
        }

        public string CreateDeclaration(string aIdentifier)
        {
            var fundamental = FundamentalType();
            List<string> prefix = new List<string>();
            List<string> suffix = new List<string>();
            List<CType> typeChain = new List<CType>();
            CType current = this;
            while (current != null)
            {
                typeChain.Add(current);
                current = current.ChildType;
            }
            foreach (var t in typeChain)
            {
                t.ConstructDeclaration(prefix, suffix);
            }

            prefix.Reverse();
            string space = aIdentifier == "" ? "" : " ";
            return String.Format("{0}{1}{2}{3}{4}",
                fundamental,
                space,
                String.Join("", prefix),
                aIdentifier,
                String.Join("", suffix));
        }

        protected abstract void ConstructDeclaration(List<string> aPrefix, List<string> aSuffix);
    }

    public class CTypeConverter : JsonCreationConverter<CType>
    {
        protected override CType Create(Type objectType, JObject jObject)
        {
            if ((string)jObject["kind"] == "array")
            {
                return new ArrayCType();
            }
            if ((string)jObject["kind"] == "pointer")
            {
                return new PointerCType();
            }
            if ((string)jObject["kind"] == "function")
            {
                return new FunctionCType();
            }
            if ((string)jObject["kind"] == "struct")
            {
                return new StructCType();
            }
            if ((string)jObject["kind"] == "enum")
            {
                return new EnumCType();
            }
            if ((string)jObject["kind"] == "named-type")
            {
                return new NamedCType();
            }
            throw new Exception("Bad JSON.");
        }
    }

    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        /// <summary>
        /// Create an instance of objectType, based properties in the JSON object
        /// </summary>
        /// <param name="objectType">type of object expected</param>
        /// <param name="jObject">contents of JSON object that will be deserialized</param>
        /// <returns></returns>
        protected abstract T Create(System.Type objectType, JObject jObject);

        public override bool CanConvert(System.Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            T target = Create(objectType, jObject);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }



    [JsonObject]
    public class NamedCType : CType
    {
        [JsonProperty("kind", Order=1)]
        public string Kind { get { return "named-type"; } }
        [JsonProperty("name", Order=2)]
        public string Name { get; set; }
        public override string FundamentalType()
        {
            return Name;
        }
        public override string  ToString()
        {
            return Name;
        }
        protected override void ConstructDeclaration(List<string> aPrefix, List<string> aSuffix)
        {
            return;
        }

        public NamedCType()
        {
        }
        public NamedCType(string aName)
        {
            Name = aName;
        }
    }
    [JsonObject]
    public class PointerCType : CType
    {
        [JsonProperty("kind", Order=1)]
        public string Kind { get { return "pointer"; } }
        [JsonProperty("to", Order=2)]
        public CType BaseType { get; set; }
        public override CType ChildType { get { return BaseType; } }
        public override string FundamentalType()
        {
            return BaseType.FundamentalType();
        }
        public override string ToString()
        {
            return BaseType.ToString() + "*";
        }
        protected override void ConstructDeclaration(List<string> aPrefix, List<string> aSuffix)
        {
            aPrefix.Add("*");
        }

        public PointerCType()
        {
        }
        public PointerCType(CType aBaseType)
        {
            BaseType = aBaseType;
        }
    }
    [JsonObject]
    public class ArrayCType : CType
    {
        [JsonProperty("kind", Order=1)]
        public string Kind { get { return "array"; } }
        [JsonProperty("dimension", Order=2)]
        public int? Dimension { get; set; }
        [JsonProperty("of", Order=3)]
        public CType BaseType { get; set; }
        public override CType ChildType { get { return BaseType; } }
        public override string FundamentalType()
        {
            return BaseType.FundamentalType();
        }
        protected override void ConstructDeclaration(List<string> aPrefix, List<string> aSuffix)
        {
            if ((aPrefix.LastOrDefault() ?? "").StartsWith("*"))
            {
                aPrefix.Add("(");
                aSuffix.Add(")");
            }
            aSuffix.Add("[" + Dimension + "]");
        }

        public ArrayCType()
        {
        }
        public ArrayCType(int? aDimension, CType aBaseType)
        {
            Dimension = aDimension;
            BaseType = aBaseType;
        }
    }
    [JsonObject]
    public class FunctionCType : CType
    {
        [JsonProperty("kind", Order=1)]
        public string Kind { get { return "function"; } }
        [JsonProperty("arguments", Order=2)]
        public List<Declaration> Arguments { get; set; }
        [JsonProperty("returning", Order=3)]
        public CType ReturnType { get; set; }
        public override CType ChildType { get { return ReturnType; } }
        public override string FundamentalType()
        {
            return ReturnType.FundamentalType();
        }
        protected override void ConstructDeclaration(List<string> aPrefix, List<string> aSuffix)
        {
            if ((aPrefix.LastOrDefault() ?? "").StartsWith("*"))
            {
                aPrefix.Add("(");
                aSuffix.Add(")");
            }
            var args = String.Join(", ", Arguments.Select(x=>x.CType.CreateDeclaration(x.Name)));
            aSuffix.Add("(" + args + ")");
        }

        public FunctionCType()
        {
        }
        public FunctionCType(CType aReturnType)
        {
            Arguments = new List<Declaration>();
            ReturnType = aReturnType;
        }
    }
    [JsonObject]
    public class StructCType : CType
    {
        [JsonProperty("kind", Order=1)]
        public string Kind { get { return "struct"; } }
        [JsonProperty("tag", Order=2)]
        public string Tag { get; set; }
        [JsonProperty("fields", Order=3)]
        public List<Declaration> Fields { get; set; }
        [JsonIgnore]
        public bool IsForward { get { return Fields == null; } }
        public override string FundamentalType()
        {
            return "struct " + Tag;
        }
        protected override void ConstructDeclaration(List<string> aPrefix, List<string> aSuffix)
        {
            return;
        }

        public StructCType()
        {
        }
        public StructCType(string aTag)
        {
            Tag = aTag;
            Fields = new List<Declaration>();
        }
    }
    [JsonObject]
    public class EnumCType : CType, IEnumerable
    {
        [JsonProperty("kind", Order=1)]
        public string Kind { get { return "enum"; } }
        [JsonProperty("tag", Order=2)]
        public string Tag { get; set; }
        [JsonProperty("constants", Order=3)]
        public List<EnumConstant> Constants { get; set; }
        public override string FundamentalType()
        {
            return "enum " + Tag;
        }
        protected override void ConstructDeclaration(List<string> aPrefix, List<string> aSuffix)
        {
            return;
        }

        public EnumCType()
        {
        }
        public EnumCType(string aTag)
        {
            Tag = aTag;
            Constants = new List<EnumConstant>();
        }
        public void Add(string aName, int aValue)
        {
            Constants.Add(new EnumConstant { Name = aName, Value = aValue });
        }

        public IEnumerator GetEnumerator()
        {
            return Constants.GetEnumerator();
        }
    }
    interface IHasComment
    {
        string RawComment { get; set; }
    }
    [JsonObject]
    public class Declaration : IHasComment
    {
        [JsonProperty("name", Order=1)]
        public string Name { get; set; }
        [JsonProperty("kind", Order=2)]
        public string Kind { get; set; }
        [JsonProperty("raw-comment", Order=3)]
        public string RawComment { get; set; }
        [JsonProperty("type", Order=4)]
        public CType CType { get; set; }

        public override string ToString()
        {
            return CType.CreateDeclaration(Name);
        }
    }

    public class EnumConstant : IHasComment
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public string RawComment { get; set; }
        public override string ToString()
        {
            return String.Format(
                @"{{""name"":""{0}"", ""raw-comment"":{1}, ""value"":{2}}}",
                Name,
                RawComment == null ? "null" : HeaderParser.ToJsonString(RawComment),
                Value);
        }
    }
    public class HeaderParser
    {
        internal static string ToJsonString(string aString)
        {
            return String.Format(
                @"""{0}""",
                aString
                    .Replace(@"\", @"\\")
                    .Replace(@"""", @"\""")
                    .Replace("\r", @"\r")
                    .Replace("\n", @"\n")
                    .Replace("\t", @"\t")
                    .Replace("\b", @"\b")
                    .Replace("\f", @"\f"));
        }
        const string BackwardsCommentPattern = @"
                ^///<.*$    |
                ^//!<.*$    |
                ^/\*\*<.*
            ";
        const string ForwardsCommentPattern = @"
                ^///[^<].*$    |
                ^//![^<].*$    |
                ^/\*\*[^<].*
            ";
        static readonly Regex BackwardsCommentRegex = new Regex(BackwardsCommentPattern, RegexOptions.IgnorePatternWhitespace);
        static readonly Regex ForwardsCommentRegex = new Regex(ForwardsCommentPattern, RegexOptions.IgnorePatternWhitespace);
        IEnumerable<Token> StripComments(IEnumerable<Token> aInput)
        {
            foreach (var token in aInput)
            {
                if (token.Type == "linecomment" || token.Type == "rangecomment")
                {
                    AddComment(token);
                    continue;
                }
                yield return token;
            }
        }
        static IEnumerable<Token> StripPreprocessorDirectives(IEnumerable<Token> aInput)
        {
            return aInput.Where(token => token.Type != "preprocessor");
        }
        static IEnumerable<Token> StripWhitespace(IEnumerable<Token> aInput)
        {
            return aInput.Where(token => token.Type != "whitespace" && token.Type != "newline");
        }

        static IEnumerable<Token> StripMacros(IEnumerable<Token> aInput)
        {
            int parens = 0;
            bool needParen = false;
            foreach (var token in aInput)
            {
                if (needParen)
                {
                    if (token.Content != "(")
                    {
                        throw new Exception(String.Format("Expected '(', but got {0}.", token));
                    }
                    parens += 1;
                    needParen = false;
                    continue;
                }
                if (token.Content == "SP_LIBEXPORT")
                {
                    needParen = true;
                    continue;
                }
                if (token.Content == ")" && parens > 0)
                {
                    parens -=1;
                    continue;
                }
                if (token.Content == "SP_CALLCONV")
                {
                    continue;
                }
                yield return token;
            }
        }

        readonly IEnumerator<Token> iTokenStream;
        Token iLastCommentToken;
        IHasComment iLastCommentable;

        void AddComment(Token aCommentToken)
        {
            if (BackwardsCommentRegex.IsMatch(aCommentToken.Content))
            {
                if (iLastCommentable != null)
                {
                    iLastCommentable.RawComment = aCommentToken.Content;
                }
                iLastCommentToken = null;
            }
            else if (ForwardsCommentRegex.IsMatch(aCommentToken.Content))
            {
                iLastCommentable = null;
                iLastCommentToken = aCommentToken;
            }
        }
        void StartCommentable(IHasComment aCommentable)
        {
            if (iLastCommentToken != null)
            {
                aCommentable.RawComment = iLastCommentToken.Content;
                iLastCommentToken = null;
            }
            iLastCommentable = aCommentable;
        }
        void EndCommentable(IHasComment aCommentable)
        {
            iLastCommentToken = null;
            iLastCommentable = aCommentable;
        }

        bool iFinished;
        public HeaderParser(IEnumerable<Token> aTokenStream)
        {
            iTokenStream =
                StripMacros(
                StripWhitespace(
                StripComments(
                StripPreprocessorDirectives(
                    aTokenStream)))).GetEnumerator();
            TryMoveNext();
        }
        bool TryMoveNext()
        {
            if (iFinished) return false;
            if (!iTokenStream.MoveNext())
            {
                iFinished = true;
                return false;
            }
            return true;
        }
        void MoveNext()
        {
            if (!TryMoveNext())
            {
                throw new Exception("Parse error: unexpected EOF");
            }
        }
        void CheckEof()
        {
            if (iFinished) throw new Exception("Parse error: unexpected EOF");
        }
        /*
        Token GetNextComment()
        {
            if (iFinished) return null;
            for (;;)
            {
                var current = iTokenStream.Current;
                if (current.Type == "linecomment") return current;
                if (current.Type == "rangecomment") return current;
                if (current.Type == "whitespace" ||
                    current.Type == "preprocessor" ||
                    current.Type == "newline")
                {
                    if (!iTokenStream.MoveNext())
                    {
                        iFinished = true;
                        return null;
                    }
                    continue;
                }
                return null;
            }
        }*/
        void Consume(string aContent)
        {
            CheckEof();
            if (iTokenStream.Current.Content != aContent)
            {
                throw new Exception(String.Format("Bad parse at token: {0}, expected: {1}", iTokenStream.Current, aContent));
            }
            TryMoveNext();
        }
        string ConsumeWord()
        {
            CheckEof();
            if (iTokenStream.Current.Type != "word")
            {
                throw new Exception(String.Format("Bad parse at token: {0}, expected word.", iTokenStream.Current));
            }
            string result = iTokenStream.Current.Content;
            TryMoveNext();
            return result;
        }
        long ConsumeInt()
        {
            CheckEof();
            if (iTokenStream.Current.Type != "number")
            {
                throw new Exception(String.Format("Bad parse at token: {0}, expected number.", iTokenStream.Current));
            }
            long result = 0;
            string numberString = iTokenStream.Current.Content;
            var suffixStrippedMatch = Regex.Match(numberString, "^([^ulUL]*)[ulUL]*");
            if (!suffixStrippedMatch.Success)
            {
                Throw("Expected a number");
            }
            string suffixStripped = suffixStrippedMatch.Groups[1].Value;
            if (suffixStripped == "0")
            {
                result = 0;
            }
            else if (suffixStripped.StartsWith("0x"))
            {
                if (!long.TryParse(suffixStripped.Substring(2), NumberStyles.HexNumber, null, out result))
                {
                    Throw("Expected a number");
                }
            }
            else if (suffixStripped.StartsWith("0"))
            {
                try
                {
                    result = Convert.ToInt32(suffixStripped.Substring(1), 8);
                }
                catch (Exception) // ArgumentException, FormatException, OverflowException, ArgumentOutOfRangeException
                {
                    Throw("Expected a number");
                }
            }
            else
            {
                if (!long.TryParse(suffixStripped, out result))
                {
                    Throw("Expected a number");
                }
            }
            TryMoveNext();
            return result;
        }
        void Throw(string aMessage, Token aToken = null)
        {
            if (aToken == null)
            {
                aToken = iTokenStream.Current;
            }
            throw new Exception(aMessage + " at " + aToken);
        }

        public IEnumerable<Declaration> ParseHeader()
        {
            return ParseHeaderDeclarations().ToList();
        }

        private IEnumerable<Declaration> ParseHeaderDeclarations()
        {
            if (iFinished) throw new Exception("Unexpected EOF");
            while (!iFinished && iTokenStream.Current.Content != "}")
            {
                var current = iTokenStream.Current;
                if (current.Content == "typedef")
                {
                    yield return ParseTypedef();
                }
                else if (current.Content == "extern")
                {
                    foreach (var item in ParseExtern())
                    {
                        yield return item;
                    }
                }
                else
                {
                    var declaration = ParseDeclaration();
                    //Console.WriteLine(declaration);
                    Consume(";");
                    yield return declaration;
                }
            }
        }
        IEnumerable<Declaration> ParseExtern()
        {
            Consume("extern");
            Consume(@"""C""");
            Consume("{");

            while (iTokenStream.Current.Content != "}")
            {
                foreach (var item in ParseHeaderDeclarations())
                {
                    yield return item;
                }
            }

            Consume("}");
        }
        Declaration ParseTypedef()
        {
            Consume("typedef");
            var declaration = ParseDeclaration();
            declaration.Kind = "typedef";
            if (declaration.Name == "")
            {
                throw new Exception("My typedef has no name. How does he parse? SYNTAX ERROR AT LINE ???");
            }
            //Console.WriteLine(declaration);
            Consume(";");
            //Console.WriteLine("Parsed the typedef '{0}'.", declaration.Name);
            return declaration;
        }
        CType ParseStruct()
        {
            Consume("struct");
            string tag = "";
            if (iTokenStream.Current.Content != "{")
            {
                tag = ConsumeWord();
            }
            if (iTokenStream.Current.Content != "{")
            {
                //Console.WriteLine("Forward declaration of struct {0}.", tag);
                return new StructCType { Fields = null, Tag = tag };
            }

            var fields = new List<Declaration>();
            while (iTokenStream.Current.Content != "}")
            {
                if (iTokenStream.Current.Content != ";" && iTokenStream.Current.Content != "{")
                {
                    Throw("Expected ';'");
                }
                MoveNext();
                if (iTokenStream.Current.Content == "}") break;
                var field = ParseDeclaration();
                fields.Add(field);
            }
            MoveNext();
            return new StructCType { Fields = fields, Tag = tag };
        }

        CType ParseEnum()
        {
            Consume("enum");
            string tag = "";
            if (iTokenStream.Current.Content != "{")
            {
                tag = ConsumeWord();
            }
            //Console.WriteLine("Skipping enum {0} content...", tag);
            List<EnumConstant> values = new List<EnumConstant>();
            int nextValue = 0;
            while (iTokenStream.Current.Content != "}")
            {
                if (iTokenStream.Current.Content != "," && iTokenStream.Current.Content != "{")
                {
                    throw new Exception(String.Format("Expected ',', but got {0}", iTokenStream.Current.Content));
                }
                MoveNext();
                var enumConstant = new EnumConstant();
                StartCommentable(enumConstant);
                EndCommentable(enumConstant);
                if (iTokenStream.Current.Content == "}") break;
                var name = ConsumeWord();
                int value;
                if (iTokenStream.Current.Content == "=")
                {
                    Consume("=");
                    value = (int)ConsumeInt();
                }
                else
                {
                    value = nextValue;
                }
                enumConstant.Name = name;
                enumConstant.Value = value;
                nextValue = value + 1;
                values.Add(enumConstant);
            }
            MoveNext();
            return new EnumCType { Constants = values, Tag = tag };
        }
        void ReadQualifiers(HashSet<string> aCurrentQualifiers)
        {
            for (;;)
            {
                var current = iTokenStream.Current;
                if (current.Content == "unsigned")
                {
                    MoveNext();
                    aCurrentQualifiers.Add("unsigned");
                    continue;
                }
                if (current.Content == "const")
                {
                    MoveNext();
                    aCurrentQualifiers.Add("const");
                    continue;
                }
                if (current.Content == "SP_LIBEXPORT")
                {
                    MoveNext();
                    continue;
                }
                if (current.Content == "SP_CALLCONV")
                {
                    MoveNext();
                    continue;
                }
                return;
            }
        }

        static bool IsQualifier(Token aToken)
        {
            return new[]{"unsigned", "const"}.Contains(aToken.Content);
        }
        Declaration ParseDeclaration()
        {
            //Console.WriteLine("Starting ParseDeclaration at: ({0}:{1})", iTokenStream.Current.Line, iTokenStream.Current.Column);

            Declaration declaration = new Declaration { Kind = "instance" };
            StartCommentable(declaration);
            HashSet<string> qualifiers = new HashSet<string>();
            ReadQualifiers(qualifiers);
            CType basetype;
            if (iTokenStream.Current.Content == "struct")
            {
                basetype = ParseStruct();
            }
            else if (iTokenStream.Current.Content == "enum")
            {
                basetype = ParseEnum();
            }
            else
            {
                basetype = new NamedCType{Name=ConsumeWord()};
                basetype.Qualifiers.UnionWith(qualifiers);
            }

            List<Token> leftStack = new List<Token>();

            for (;;)
            {
                var current = iTokenStream.Current;
                if (IsQualifier(current) || current.Content == "(" || current.Content == "*")
                {
                    leftStack.Add(current);
                    MoveNext();
                    continue;
                }
                break;
            }

            string declarationName = (iTokenStream.Current.Type == "word") ? ConsumeWord() : "";

            List<Func<CType, CType>> typeBuilders = new List<Func<CType, CType>>();
            for (;;)
            {
                var current = iTokenStream.Current;
                
                if (current.Content == "(")
                {
                    List<Declaration> argumentList = new List<Declaration>();
                    while (current.Content != ")")
                    {
                        if (current.Content != "," && current.Content != "(")
                        {
                            throw new Exception(String.Format("Parse error: {0}", current));
                        }
                        MoveNext();
                        argumentList.Add(ParseDeclaration());
                        current = iTokenStream.Current;
                    }
                    Consume(")");
                    typeBuilders.Add(
                        returnType => new FunctionCType{Arguments = argumentList, ReturnType = returnType});
                    continue;
                }

                if (current.Content == "[")
                {
                    Consume("[");
                    int arraySize = -1;
                    if (iTokenStream.Current.Type == "number")
                    {
                        arraySize = (int)ConsumeInt();
                    }
                    Consume("]");
                    typeBuilders.Add(
                        componentType => new ArrayCType{BaseType = componentType, Dimension = arraySize});
                    continue;
                }

                // Start working back through the stack of tokens on the left:

                if (leftStack.Count > 0)
                {
                    Token leftToken = leftStack[leftStack.Count-1];
                    leftStack.RemoveAt(leftStack.Count-1);
                    if (leftToken.Content == "(")
                    {
                        Consume(")");
                        continue;
                    }
                    if (leftToken.Content == "*")
                    {
                        typeBuilders.Add(
                            componentType => new PointerCType { BaseType = componentType });
                        continue;
                    }
                    if (leftToken.Content == "unsigned")
                    {
                        typeBuilders.Add(
                            componentType =>
                                {
                                    componentType.Qualifiers.Add("unsigned");
                                    return componentType;
                                });
                        continue;
                    }
                    if (leftToken.Content == "const")
                    {
                        typeBuilders.Add(
                            componentType =>
                                {
                                    componentType.Qualifiers.Add("const");
                                    return componentType;
                                });
                        continue;
                    }
                    throw new Exception(String.Format("Cannot parse: {0}", leftToken));
                }

                // Victory!

                break;
            }
            var finalType = Enumerable.Reverse(typeBuilders).Aggregate(basetype, (current, builder) => builder(current));
            declaration.Name = declarationName;
            declaration.CType = finalType;
            EndCommentable(declaration);
            return declaration;
        }
    }
}
