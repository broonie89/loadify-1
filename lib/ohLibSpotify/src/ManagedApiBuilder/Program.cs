// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ApiParser;
using Newtonsoft.Json;

namespace ManagedApiBuilder
{

    [JsonObject]
    public class ApiBuilderConfiguration
    {
        [JsonProperty("ignore")]
        public List<string> DeclarationsToIgnore { get; set; }
        [JsonProperty("namespace")]
        public string RootNamespace { get; set; }
        [JsonProperty("structs")]
        public List<ApiStructConfiguration> Structs { get; set; }
        [JsonProperty("enums")]
        public List<ApiEnumConfiguration> Enums { get; set; }
    }

    [JsonObject]
    public class ApiStructConfiguration
    {
        [JsonProperty("native-name")]
        public string NativeName { get; set; }
        [JsonProperty("managed-name")]
        public string ManagedName { get; set; }
        [JsonProperty("force-public")]
        public bool ForcePublic { get; set; }
        [JsonProperty("suppress-functions")]
        public List<string> SuppressFunctions { get; set; }

        public ApiStructConfiguration()
        {
            SuppressFunctions = new List<string>();
        }
    }

    [JsonObject]
    public class ApiEnumConfiguration
    {
        [JsonProperty("native-name")]
        public string NativeName { get; set; }
        [JsonProperty("managed-name")]
        public string ManagedName { get; set; }
        [JsonProperty("native-constant-prefix")]
        public string NativeConstantPrefix { get; set; }
        [JsonProperty("managed-constant-prefix")]
        public string ManagedConstantPrefix { get; set; }
    }

    class Program
    {
        /*static void Main2(string[] args)
        {
            var transformers = new List<IArgumentTransformer>{
                new StringReturnTransformer(),
                new StringArgumentTransformer(),
                new TrivialArgumentTransformer(),
                new RefArgumentTransformer() };
            Declaration arg1 = new Declaration { Name = "inputString", Kind = "instance", CType = new PointerCType(new NamedCType("char")) };
            Declaration arg2 = new Declaration { Name = "flag1", Kind = "instance", CType = new NamedCType("bool") };
            Declaration arg3 = new Declaration { Name = "ptrToInt", Kind = "instance", CType = new PointerCType(new NamedCType("int")) };
            //Console.WriteLine(stringTransformer.CanApply(arg1, null, null));
            Declaration arg4 = new Declaration { Name = "outputString", Kind = "instance", CType = new PointerCType(new NamedCType("char")) };
            Declaration arg5 = new Declaration { Name = "bufferLength", Kind = "instance", CType = new NamedCType("size_t") };
            CType retType = new NamedCType("int");
            List<Declaration> arguments = new List<Declaration> { arg1, arg2, arg3, arg4, arg5 };
            var assembler = new FunctionAssembler("my_function", "MyFunction");
            var nativeFunction = new FunctionSpecificationAnalyser(arguments, retType);
            while (nativeFunction.CurrentParameter != null)
            {
                var transformer = transformers.FirstOrDefault(x=>x.Apply(nativeFunction, assembler));
                if (transformer == null)
                {
                    throw new Exception("Could not handle all arguments.");
                }
                assembler.NextArgument();
            }
            Console.WriteLine(assembler.GenerateWrapperMethod("    "));
        }*/
        static void Main(string[] args)
        {
            var text = File.ReadAllText(args[0]);
            var declarations =
                JsonConvert.DeserializeObject<List<Declaration>>(text, new CTypeConverter());

            var configurationJson = File.ReadAllText(args[1]);
            var configuration = JsonConvert.DeserializeObject<ApiBuilderConfiguration>(configurationJson);

            var categorizedDeclarations = new CategorizedDeclarations(configuration.DeclarationsToIgnore);
            categorizedDeclarations.AddDeclarations(declarations);
            OrderedDictionary<string, SpotifyClass> classes;
            OrderedDictionary<string, FunctionCType> functions;
            categorizedDeclarations.CategorizeFunctions(out classes, out functions);
            var anonymousDelegates = categorizedDeclarations.FindAnonymousDelegates();
            CSharpGenerator gen = new CSharpGenerator(
                categorizedDeclarations.EnumTable.Keys,
                categorizedDeclarations.StructTable.Keys,
                categorizedDeclarations.HandleTable,
                categorizedDeclarations.FunctionTypedefTable.Keys,
                configuration.Structs,
                configuration.Enums,
                new DefaultFunctionFactory());
            Console.WriteLine("using System;");
            Console.WriteLine("using System.Linq;");
            Console.WriteLine("using System.Runtime.InteropServices;");
            Console.WriteLine("");
            Console.WriteLine("namespace "+configuration.RootNamespace);
            Console.WriteLine("{");
            Console.WriteLine("");
            Console.WriteLine("    // Enums");
            foreach (var kvpEnum in categorizedDeclarations.EnumTable)
            {
                Console.Write(gen.GenerateEnumDeclaration("    ", kvpEnum.Key, kvpEnum.Value));
            }
            Console.WriteLine("");
            Console.WriteLine("    // Named delegates");
            foreach (var kvpDelegate in categorizedDeclarations.FunctionTypedefTable)
            {
                Console.Write(gen.GenerateRawDelegate("    ", kvpDelegate.Key, kvpDelegate.Value));
            }
            Console.WriteLine("");
            Console.WriteLine("    // Un-named delegates");
            foreach (var kvpDelegate in anonymousDelegates)
            {
                Console.Write(gen.GenerateRawDelegate("    ", kvpDelegate.Key, kvpDelegate.Value));
            }

            Console.WriteLine("");
            Console.WriteLine("    // Structs");
            foreach (var kvpStruct in categorizedDeclarations.StructTable)
            {
                var structName = kvpStruct.Key;
                var structType = kvpStruct.Value;
                Console.WriteLine(gen.GenerateStruct("    ", structName, structType));
            }
            Console.WriteLine("    class NativeMethods");
            Console.WriteLine("    {");
            foreach (var kvpFunction in categorizedDeclarations.FunctionTable)
            {
                var functionName = kvpFunction.Key;
                var functionSignature = kvpFunction.Value;
                Console.Write(gen.GenerateDllImportFunction("        ", functionName, functionSignature));
            }
            Console.WriteLine("    }");
            Console.WriteLine("");
            Console.WriteLine("    // Classes");
            foreach (var kvpClass in classes)
            {
                string name = kvpClass.Key;
                var spotifyClass = kvpClass.Value;
                Console.Write(gen.GenerateCSharpClass("    ", name, spotifyClass.NativeFunctions));
                /*
                Console.WriteLine("    public partial class {0}", name);
                Console.WriteLine("    {");
                Console.WriteLine("        IntPtr _handle;");
                Console.WriteLine("        internal {0}(IntPtr handle)", name);
                Console.WriteLine("        {");
                Console.WriteLine("            this._handle = handle;");
                Console.WriteLine("        }");
                Console.WriteLine("");
                foreach (var kvpFunction in spotifyClass.NativeFunctions)
                {
                    Console.WriteLine(gen.GenerateCSharpWrappingMethod("        ", kvpFunction.Key, name, kvpFunction.Value));
                }
                Console.WriteLine("    }");*/
            }
            Console.WriteLine("}");
        }
    }
}
