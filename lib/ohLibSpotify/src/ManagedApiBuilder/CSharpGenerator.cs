// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiParser;
using ManagedApiBuilder.ArgumentTransformers;

namespace ManagedApiBuilder
{
    public class CSharpGenerator
    {
        readonly HashSet<string> iEnumNames;
        readonly HashSet<string> iStructNames;
        readonly HashSet<string> iHandleNames;
        readonly HashSet<string> iDelegateNames;
        readonly Dictionary<string, ApiStructConfiguration> iStructConfigurations;
        readonly Dictionary<string, ApiEnumConfiguration> iEnumConfigurations;
        IFunctionFactory iFunctionFactory;

        public CSharpGenerator(
            IEnumerable<string> aEnumNames,
            IEnumerable<string> aStructNames,
            IEnumerable<string> aHandleNames,
            IEnumerable<string> aDelegateNames,
            IEnumerable<ApiStructConfiguration> aStructConfigurations,
            IEnumerable<ApiEnumConfiguration> aEnumConfigurations,
            IFunctionFactory aFunctionFactory)
        {
            iEnumNames = new HashSet<string>(aEnumNames);
            iStructNames = new HashSet<string>(aStructNames);
            iHandleNames = new HashSet<string>(aHandleNames);
            iDelegateNames = new HashSet<string>(aDelegateNames);
            iStructConfigurations = aStructConfigurations.ToDictionary(x => x.NativeName);
            iEnumConfigurations = aEnumConfigurations.ToDictionary(x => x.NativeName);
            iFunctionFactory = aFunctionFactory;
        }

        string ManagedNameForNativeType(string aNativeTypeName)
        {
            ApiStructConfiguration structConfig;
            if (iStructConfigurations.TryGetValue(aNativeTypeName, out structConfig))
            {
                return structConfig.ManagedName ?? aNativeTypeName;
            }
            ApiEnumConfiguration enumConfig;
            if (iEnumConfigurations.TryGetValue(aNativeTypeName, out enumConfig))
            {
                return enumConfig.ManagedName ?? aNativeTypeName;
            }
            // Delegate??
            if (iEnumNames.Contains(aNativeTypeName))
            {
                return DefaultManagedEnumName(aNativeTypeName);
            }
            if (iHandleNames.Contains(aNativeTypeName))
            {
                return DefaultManagedClassName(aNativeTypeName);
            }
            return aNativeTypeName;
        }

        CSharpType GetCSharpMarshalType(CType aType, string aName, bool aAsParameter)
        {
            var pointerType = aType as PointerCType;
            var namedType = aType as NamedCType;
            var arrayType = aType as ArrayCType;
            if (pointerType != null)
            {
                var functionType = pointerType.BaseType as FunctionCType;
                if (functionType != null)
                {
                    // Delegate
                    return new CSharpType(aName);
                }
                if (!aAsParameter)
                {
                    return new CSharpType("IntPtr");
                }
                var pointerPointerType = pointerType.BaseType as PointerCType;
                if (pointerPointerType != null)
                {
                    return new CSharpType("IntPtr") { IsRef = true };
                }
                var pointedToType = pointerType.BaseType as NamedCType;
                if (pointedToType != null)
                {
                    string typeName = pointedToType.Name;
                    if (iStructNames.Contains(typeName))
                    {
                        return new CSharpType(ManagedNameForNativeType(pointedToType.Name)) { IsRef = true };
                    }
                    if (iEnumNames.Contains(typeName))
                    {
                        return new CSharpType(ManagedNameForNativeType(pointedToType.Name)) { IsRef = true };
                    }
                    if (iHandleNames.Contains(typeName))
                    {
                        return new CSharpType("IntPtr");
                    }
                    if (iDelegateNames.Contains(typeName))
                    {
                        return new CSharpType(ManagedNameForNativeType(typeName));
                    }
                    if (typeName == "void") return new CSharpType("IntPtr");
                    if (typeName == "int") return new CSharpType("int") { IsRef = true };
                    if (typeName == "bool") return new CSharpType("bool") { IsRef = true, Attributes = { "MarshalAs(UnmanagedType.I1)" } };
                    if (typeName == "char") return new CSharpType("IntPtr");
                    if (typeName == "byte") return new CSharpType("IntPtr");
                    if (typeName == "size_t") return new CSharpType("UIntPtr") { IsRef = true };
                    return new CSharpType("/* ??? */ IntPtr"); // TODO: Out of band warning
                }
                return new CSharpType("IntPtr");
            }
            if (namedType != null)
            {
                switch (namedType.Name)
                {
                    case "void":
                        return new CSharpType("void");
                    case "int":
                        return new CSharpType("int");
                    case "bool":
                        return new CSharpType("bool") { Attributes = { "MarshalAs(UnmanagedType.I1)" } };
                    case "size_t":
                        return new CSharpType("UIntPtr");
                    case "sp_uint64":
                        return new CSharpType("ulong");
                }
                if (iEnumNames.Contains(namedType.Name))
                {
                    return new CSharpType(ManagedNameForNativeType(namedType.Name));
                }
                throw new Exception("Don't know how to marshal type: " + namedType.Name);
            }
            if (arrayType != null)
            {
                return new CSharpType("IntPtr");
            }
            throw new Exception("Don't know how to marshal type: " + aType);
        }

        public string GenerateDllImportFunction(string aIndent, string aFunctionName, FunctionCType aFunctionType)
        {
            var assembler = AssembleFunction(aFunctionName, aFunctionType, null, null);
            if (assembler == null)
            {
                return aIndent +"// Failed to generate: " + aFunctionName;
            }
            string newResult = assembler.GeneratePInvokeDeclaration(aIndent);
            return newResult.Replace("\n", Environment.NewLine);
        }

        public string GenerateRawDelegate(string aIndent, string aFunctionName, FunctionCType aFunctionType)
        {
            var assembler = AssembleFunction(aFunctionName, aFunctionType, null, null);
            if (assembler == null)
            {
                return aIndent +"// Failed to generate: " + aFunctionName;
            }
            return assembler.GenerateNativeDelegateDeclaration(aIndent).Replace("\n", Environment.NewLine);
        }

        static IEnumerable<string> SplitName(string aName)
        {
            return aName.Split('_');
        }

        static string PascalCase(string aFragment)
        {
            if (aFragment.Length == 0)
                return "";
            return aFragment.Substring(0, 1).ToUpperInvariant() + aFragment.Substring(1).ToLowerInvariant();
        }

        static string PascalCase(IEnumerable<string> aFragments)
        {
            return String.Join("", aFragments.Select(PascalCase));
        }

        static string PascalCaseMemberName(string aMemberName)
        {
            string trimmedName = aMemberName;
            return PascalCase(SplitName(trimmedName));
        }

        public string GenerateCSharpClass(string aIndent, string aHandleName, IEnumerable<KeyValuePair<string, FunctionCType>> aFunctions)
        {
            ApiStructConfiguration config;

            if (!iStructConfigurations.TryGetValue(aHandleName, out config))
            {
                config = new ApiStructConfiguration{
                    ManagedName = ManagedNameForNativeType(aHandleName),
                    NativeName = aHandleName
                };
            }

            StringBuilder methodBuilder = new StringBuilder();

            methodBuilder.AppendFormat("{0}public partial class {1}\n", aIndent, config.ManagedName);
            methodBuilder.AppendFormat("{0}{{\n", aIndent);
            methodBuilder.AppendFormat("{0}    internal IntPtr _handle;\n", aIndent);
            methodBuilder.AppendFormat("{0}    internal {1}(IntPtr handle)\n", aIndent, config.ManagedName);
            methodBuilder.AppendFormat("{0}    {{\n", aIndent);
            methodBuilder.AppendFormat("{0}        this._handle = handle;\n", aIndent);
            methodBuilder.AppendFormat("{0}    }}\n", aIndent);
            methodBuilder.AppendFormat("\n");
            HashSet<string> suppressedFunctions = new HashSet<string>(config.SuppressFunctions);
            foreach (var kvpFunction in aFunctions)
            {
                if (suppressedFunctions.Contains(kvpFunction.Key))
                {
                    methodBuilder.Append(aIndent + String.Format("    // Suppressed function '{0}'.\n", kvpFunction.Key));
                    continue;
                }
                methodBuilder.Append(GenerateCSharpWrappingMethod(aIndent+"    ", kvpFunction.Key, aHandleName, kvpFunction.Value));
            }
            methodBuilder.Append(aIndent + "}\n");
            return methodBuilder.ToString().Replace("\n", Environment.NewLine);
        }

        IFunctionGenerator AssembleFunction(string aFunctionName, FunctionCType aFunctionType, string aHandleName, string aMethodName)
        {
            var enumNames = iEnumNames.ToDictionary(x => x, ManagedNameForNativeType);
            var structNames = iStructNames.ToDictionary(x => x, ManagedNameForNativeType);
            var classNames = iHandleNames.ToDictionary(x => x, ManagedNameForNativeType);
            var delegateNames = iDelegateNames.ToDictionary(x => x, ManagedNameForNativeType);

            // TODO: Generate transformers once only.
            // This will require a different approach for ThisPointerArgumentTransformer.

            var transformers = new List<IArgumentTransformer>{
                // This-pointer must be handled first to avoid it getting
                // stolen as a normal handle argument.
                aHandleName == null ? null : new ThisPointerArgumentTransformer{HandleType = aHandleName},

                // C-style empty argument list can go in any order.
                new VoidArgumentListTransformer(),

                // These consume multiple arguments. They need to happen
                // early enough that single-argument mappings don't steal
                // their arguments.
                new StringReturnTransformer(),
                new UnknownLengthStringReturnTransformer(),
                new StringArgumentTransformer(),
                new HandleArrayArgumentTransformer(classNames),
                new TrivialArrayArgumentTransformer(),

                // Normal arguments.
                new FunctionPointerArgumentTransformer(delegateNames),
                new VoidStarArgumentTransformer(),
                new HandleArgumentTransformer(classNames),
                // Callback structs get special handling, they need to precede
                // normal structs which can be passed as ref arguments.
                new CallbackStructArgumentTransformer(structNames),
                new RefStructArgumentTransformer(structNames),
                new ByteArrayArgumentTransformer(),
                new TrivialArgumentTransformer(enumNames),
                new RefHandleArgumentTransformer(iStructNames.Concat(iHandleNames)),
                new RefArgumentTransformer(enumNames),
                // Pointer-argument comes last because it would match against
                // other more specific arguments, like handles and ref arguments.
                new PointerArgumentTransformer(),

                // Return types.
                // Error-return has to come first out of the return handlers,
                // otherwise the error will end up being treated as a regular
                // enum.
                new SpotifyErrorReturnTransformer(),
                new TrivialReturnTransformer(enumNames),
                new HandleReturnTransformer(classNames),
                new SimpleStringReturnTransformer(),
                // Pointer-return must come later than handle-return.
                new PointerReturnTransformer(),
                new VoidReturnTransformer()}.Where(x=>x!=null).ToList();

            var assembler = iFunctionFactory.CreateAssembler(aFunctionName, aMethodName);
            var nativeFunction = iFunctionFactory.CreateAnalyser(aFunctionType.Arguments, aFunctionType.ReturnType);
            while (nativeFunction.CurrentParameter != null || nativeFunction.ReturnType != null)
            {
                var transformer = transformers.FirstOrDefault(x=>x.Apply(nativeFunction, assembler));
                if (transformer == null)
                {
                    //Console.WriteLine("/* FAILED AT ARG {0} */", nativeFunction.CurrentParameter != null ? nativeFunction.CurrentParameter.Name : "none");
                    return null;
                }
                assembler.NextArgument();
            }
            return assembler;
        }

        string GenerateCSharpWrappingMethod(string aIndent, string aFunctionName, string aHandleName, FunctionCType aFunctionType)
        {
            if (!aFunctionName.StartsWith(aHandleName+"_"))
            {
                return aIndent + "// Bad method " + aFunctionName + "\n";
            }
            var methodName = PascalCase(SplitName(aFunctionName.Substring(aHandleName.Length+1)));
            var assembler = AssembleFunction(aFunctionName, aFunctionType, aHandleName, methodName);

            if (assembler == null)
            {
                return aIndent + String.Format("// Skipped function '{0}'.\n", aFunctionName);
            }

            return assembler.GenerateWrapperMethod(aIndent);
        }

        const string EnumTemplate =
            "{0}public enum {1}\n" +
                "{0}{{\n" +
                    "{2}" +
                        "{0}}}\n";
        const string EnumConstantTemplate =
            "{0}{1} = {2},\n";

        const string SingleIndent = "    ";

        static string DropPrefix(string aString, string aPrefix)
        {
            if (!aString.StartsWith(aPrefix))
            {
                throw new Exception(String.Format("Expected '{0}' to start with '{1}'.", aString, aPrefix));
            }
            var retval = aString.Substring(aPrefix.Length);
            return retval;
        }

        static string DefaultManagedClassName(string aNativeName)
        {
            return DefaultManagedTypeName(aNativeName);
        }

        static string DefaultManagedEnumName(string aNativeName)
        {
            return DefaultManagedTypeName(aNativeName);
        }

        static string DefaultManagedTypeName(string aNativeName)
        {
            if (aNativeName.StartsWith("sp_"))
            {
                aNativeName = aNativeName.Substring(3);
            }
            return PascalCase(SplitName(aNativeName));
        }

        static string DefaultNativeConstantPrefix(string aNativeName)
        {
            return aNativeName.ToUpperInvariant() + "_";
        }

        public string GenerateEnumDeclaration(string aIndent, string aEnumName, EnumCType aEnumType)
        {
            ApiEnumConfiguration configuration;
            iEnumConfigurations.TryGetValue(aEnumName, out configuration);
            string managedName = configuration != null ? configuration.ManagedName : null;
            managedName = managedName ?? DefaultManagedEnumName(aEnumName);
            string memberPrefix = configuration != null ? configuration.NativeConstantPrefix : null;
            memberPrefix = memberPrefix ?? DefaultNativeConstantPrefix(aEnumName);
            string managedMemberPrefix = configuration != null ? configuration.ManagedConstantPrefix : null;
            managedMemberPrefix = managedMemberPrefix ?? "";
            var constantStrings = aEnumType.Constants.Select(x =>
                String.Format(
                    EnumConstantTemplate,
                    aIndent + SingleIndent,
                    managedMemberPrefix + PascalCaseMemberName(DropPrefix(x.Name, memberPrefix)),
                    x.Value));
            var joinedConstantStrings = String.Join("", constantStrings);
            return String.Format(EnumTemplate, aIndent, managedName, joinedConstantStrings).Replace("\n", Environment.NewLine);
        }

        const string StructTemplate =
            "{0}{3} struct {1}\n" +
            "{0}{{\n" +
            "{2}" +
            "{0}}}\n";
        const string StructFieldTemplate =
            "{2}" +
            "{0}public {1};\n";

        public string GenerateStruct(string aIndent, string aStructName, StructCType aStructType)
        {
            var fieldStrings = aStructType.Fields.Select(x =>
                {
                    var csharpType = GetCSharpMarshalType(x.CType, x.Name, false);
                    var attribute = csharpType.CreateFieldAttribute();
                    if (attribute != "")
                        attribute = aIndent + SingleIndent + attribute + "\n";
                    return String.Format(
                        StructFieldTemplate,
                        aIndent + SingleIndent,
                        csharpType.CreateFieldDeclaration(x.Name),
                        attribute);
                });
            var joinedFieldStrings = String.Join("", fieldStrings);
            ApiStructConfiguration config;
            if (!iStructConfigurations.TryGetValue(aStructName, out config))
            {
                config = new ApiStructConfiguration{
                    ManagedName = ManagedNameForNativeType(aStructName),
                    NativeName = aStructName,
                    ForcePublic = false
                };
            }
            string visibility = config.ForcePublic ? "public" : "internal";
            return String.Format(StructTemplate, aIndent, config.ManagedName, joinedFieldStrings, visibility).Replace("\n", Environment.NewLine);
        }
    }

    public interface IFunctionFactory
    {
        IFunctionGenerator CreateAssembler(string aFunctionName, string aMethodName);
        IFunctionSpecificationAnalyser CreateAnalyser(List<Declaration> aArguments, CType aReturnType);
    }

    public class DefaultFunctionFactory : IFunctionFactory
    {
        public IFunctionGenerator CreateAssembler(string aFunctionName, string aMethodName)
        {
            return new FunctionAssembler(aFunctionName, aMethodName);
        }

        public IFunctionSpecificationAnalyser CreateAnalyser(List<Declaration> aArguments, CType aReturnType)
        {
            return new FunctionSpecificationAnalyser(aArguments, aReturnType);
        }
    }
}