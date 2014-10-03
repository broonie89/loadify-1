using System;
using System.Collections.Generic;
using System.Linq;
using ApiParser;
using ManagedApiBuilder;
using Moq;
using NUnit.Framework;

namespace ToolTests
{
    public abstract class CSharpGeneratorContext
    {
        protected CSharpGenerator iGenerator;

        [SetUp]
        public void SetUp()
        {
            iGenerator = new CSharpGenerator(
                    new[]{"sp_color", "sp_flavor", "sp_speed"},
                    new[]{"sp_person", "sp_room", "sp_item", "sp_room_callbacks"},
                    new[]{"sp_file", "sp_device"},
                    new[]{"sp_ready_cb", "sp_finished_cb"},
                    new[]{
                        new ApiStructConfiguration{
                            NativeName="sp_person",
                            ManagedName="SpotifyPerson",
                            ForcePublic=false,
                            SuppressFunctions={"sp_person_difficult_function"}
                        }
                    },
                    new[]{
                        new ApiEnumConfiguration{
                            NativeName = "sp_speed",
                            ManagedName = "SpotifySpeed",
                            NativeConstantPrefix = "SP_SPEED_",
                            ManagedConstantPrefix = "_"
                        },
                        new ApiEnumConfiguration{
                            NativeName = "sp_flavor",
                            ManagedName = "Flavor",
                            NativeConstantPrefix = "SP_FLAVOR_",
                            ManagedConstantPrefix = ""
                        },
                    },
                    MakeFunctionFactory()
                );
        }

        protected virtual IFunctionFactory MakeFunctionFactory()
        {
            return new DefaultFunctionFactory();
        }
    }

    public class WhenGeneratingAnEmptyStruct : CSharpGeneratorContext
    {
        protected string iResult;

        [SetUp]
        public void GenerateStruct()
        {
            iResult = iGenerator.GenerateStruct("", "sp_room", new StructCType("room_tag"));
        }

        [Test]
        public void TheResultShouldNotBePublic()
        {
            Assert.That(iResult, Is.Not.StringContaining("public"));
        }

        [Test]
        public void TheResultShouldKeepTheSameName()
        {
            Assert.That(iResult, Is.StringContaining("sp_room"));
        }

        [Test]
        public void TheResultShouldHaveNoContents()
        {
            Assert.That(iResult, Is.StringMatching(@"(?s).*\{\s*\}"));
        }
    }

    public class WhenGeneratingAStructWithSomeMembers : CSharpGeneratorContext
    {
        protected string iResult;

        [SetUp]
        public void GenerateStruct()
        {
            iResult = iGenerator.GenerateStruct("", "sp_room",
                new StructCType("room_tag")
                {
                    Fields = {
                        new Declaration{Name="height", CType=new NamedCType("int")},
                        new Declaration{Name="name", CType=new PointerCType(new NamedCType("char"){Qualifiers={"const"}})},
                        new Declaration{Name="capacity", CType=new NamedCType("size_t")},
                        new Declaration{Name="lit", CType=new NamedCType("bool")},
                        new Declaration{Name="blob", CType=new PointerCType(new NamedCType("void"))},
                    }
                });
        }

        [Test]
        public void TheFieldsShouldBeDeclaredInOrder()
        {
            const string anything = ".*";
            Assert.That(iResult, Is.StringMatching(
                "(?s)" +
                anything + "height;" +
                anything + "name;" +
                anything + "capacity;" +
                anything + "lit;" +
                anything + "blob;" +
                anything));
        }

        [Test]
        public void AnIntNativeField_ShouldBe_AnIntManagedField()
        {
            const string anything = ".*";
            Assert.That(iResult, Is.StringMatching(
                "(?s)" + anything + "public int @height;" + anything));
        }

        [Test]
        public void AConstCharStarNativeField_ShouldBe_AnIntPtrManagedField()
        {
            const string anything = ".*";
            Assert.That(iResult, Is.StringMatching(
                "(?s)" + anything + "public IntPtr @name;" + anything));
        }

        [Test]
        public void ASizeTNativeField_ShouldBe_AUIntPtrManagedField()
        {
            const string anything = ".*";
            Assert.That(iResult, Is.StringMatching(
                "(?s)" + anything + "public UIntPtr @capacity;" + anything));
        }

        [Test]
        public void ABoolNativeField_ShouldBe_ABoolManagedFieldWithByteMarshalling()
        {
            const string anything = ".*";
            Func<string, string> esc = x=>x.Replace(".", "\\.").Replace("(", "\\(").Replace(")", "\\)").Replace("[", "\\[").Replace("]","\\]");
            Assert.That(iResult, Is.StringMatching(
                "(?s)" + anything + esc("[MarshalAs(UnmanagedType.I1)]") + @"\s*" + "public bool @lit;" + anything));
        }
    }

    public class WhenGeneratingAnEnumWithSomeConstants : CSharpGeneratorContext
    {
        protected string iResult;

        [SetUp]
        public void GenerateStruct()
        {
            iResult = iGenerator.GenerateEnumDeclaration("", "sp_flavor",
                new EnumCType("flavor_tag")
                {
                    Constants = {
                        new EnumConstant{Name="SP_FLAVOR_MINT", Value=6},
                        new EnumConstant{Name="SP_FLAVOR_STRAWBERRY_AND_ONION", Value=34},
                        new EnumConstant{Name="SP_FLAVOR_BURNT_POTATO", Value=77},
                        new EnumConstant{Name="SP_FLAVOR_TARMAC", Value=4},
                    }
                });
        }

        [Test]
        public void TheEnumShouldBePublic()
        {
            Assert.That(iResult, Is.StringMatching(@"(?s)\s*public.*"));
        }

        [Test]
        public void TheEnumShouldHaveTheCorrectName()
        {
            Assert.That(iResult, Is.StringMatching(@"(?s).* Flavor\s*{.*"));
        }

        [Test]
        public void TheEnumShouldHaveTheConstantsInOrder()
        {
            Assert.That(iResult, Is.StringMatching(@"(?s).*\{.*Mint.*StrawberryAndOnion.*BurntPotato.*Tarmac.*\}"));
        }

        [TestCase("Mint", "6")]
        [TestCase("StrawberryAndOnion", "34")]
        [TestCase("BurntPotato", "77")]
        [TestCase("Tarmac", "4")]
        public void TheEnumShouldHaveTheCorrectConstantValues(string aName, string aValue)
        {
            const string singleLineMode = "(?s)";
            const string sp = @"\s*";
            const string any = @"\.*";
            Action<string> chk = s => Assert.That(iResult, Is.StringMatching(s));
            chk(singleLineMode + @".*" + aName + sp + "=" + sp + aValue +  sp + "," + any);
        }
    }

    public class ClassGenerationTests : CSharpGeneratorContext
    {
        public CType Void { get { return new NamedCType("void"); } }
        public CType Int { get { return new NamedCType("int"); } }
        public CType Byte { get { return new NamedCType("byte"); } }
        public CType SizeT { get { return new NamedCType("size_t"); } }
        public CType Char { get { return new NamedCType("char"); } }
        public CType Bool { get { return new NamedCType("bool"); } }
        public CType SpUint64 { get { return new NamedCType("sp_uint64"); } }
        public CType ConstChar { get { return new NamedCType("char"){Qualifiers={"const"}}; } }
        public CType VoidPtr { get { return new PointerCType(Void); } }
        public CType IntPtr { get { return new PointerCType(Int); } }
        public CType CharPtr { get { return new PointerCType(Char); } }
        public CType ConstCharPtr { get { return new PointerCType(ConstChar); } }
        public CType BoolPtr { get { return new PointerCType(Bool); } }
        public CType SizeTPtr { get { return new PointerCType(SizeT); } }
        public CType SpUint64Ptr { get { return new PointerCType(SpUint64); } }
        public CType IntArray { get { return new ArrayCType(null, Int); } }
        public CType IntArrayFive { get { return new ArrayCType(5, Int); } }
        public CType IntArrayTen { get { return new ArrayCType(10, Int); } }
        public CType ByteArray { get { return new ArrayCType(null, Byte); } }
        public CType CharArray { get { return new ArrayCType(null, Char); } }
        public CType CharArrayTen { get { return new ArrayCType(10, Char); } }
        public CType CharArraySixty { get { return new ArrayCType(60, Char); } }

        // These tests are rather unusual because of the need to be strongly
        // data-driven. GetTestData defines one entry for each native function
        // spec that we want to generate a class from. It also specifies the
        // expected C# specification of both the P/Invoke declaration and the
        // managed wrapper method. These terse entries are expanded into several
        // tests each by GenerateTestCases below. The test cases are all
        // attached to the RunTests "test", which demultiplexes them back to
        // the appropriate test methods.

        IEnumerable<TestCase> GetTestData()
        {
            var data = new []{
                new TestCase{
                    Name = "WhenMapping('void x(byte[])')",
                    // ByteArrayArgumentTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("alpha", ByteArray) },
                    Suppressed = true,
                    PInvokeArgs = { new ArgInfo {Name="alpha", Type="IntPtr"} },
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMappingCallbackStruct",
                    // void xxx(sp_room_callbacks * callbacks);
                    // CallbackStructArgumentTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("callbacks", new PointerCType(new NamedCType("sp_room_callbacks"))) },
                    Suppressed = true,
                    PInvokeArgs = { new ArgInfo {Name="callbacks", Type="IntPtr"} },
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMappingFunctionPointer",
                    // void xxx(sp_ready_cb * ready_callback);
                    // FunctionPointerArgumentTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("ready_callback", new PointerCType(new NamedCType("sp_ready_cb"))) },
                    Suppressed = true,
                    PInvokeArgs = { new ArgInfo {Name="ready_callback", Type="sp_ready_cb"} },
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMappingHandleArgument",
                    // void xxx(sp_device * device);
                    // HandleArgumentTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("device", new PointerCType(new NamedCType("sp_device"))) },
                    ManagedArgs = { new ArgInfo {Name="device", Type="Device"} },
                    PInvokeArgs = { new ArgInfo {Name="device", Type="IntPtr"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMapping('void x(handle **, int)')",
                    // void xxx(sp_device ** devices, int num_devices);
                    // HandleArrayArgumentTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("devices", new PointerCType(new PointerCType(new NamedCType("sp_device")))), Decl("num_devices", Int) },
                    ManagedArgs = { new ArgInfo {Name="devices", Type="Device[]"} },
                    PInvokeArgs = { new ArgInfo {Name="devices", Type="IntPtr"}, new ArgInfo {Name="num_devices", Type="int"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMapping('handle *x()')",
                    // sp_device * xxx();
                    // HandleReturnTransformer
                    NativeRet = new PointerCType(new NamedCType("sp_device")),
                    NativeArgs = { },
                    ManagedArgs = { },
                    PInvokeArgs = { },
                    ManagedRet = "Device",
                    PInvokeRet = "IntPtr",
                },
                new TestCase{
                    // void xxx(int * alpha)
                    // RefArgumentTransformer
                    Name = "WhenMapping('void x(int *)')",
                    NativeRet = Void,
                    NativeArgs = { Decl("alpha", IntPtr ) },
                    ManagedArgs = { new ArgInfo {Name="alpha", Type="ref int"} },
                    PInvokeArgs = { new ArgInfo {Name="alpha", Type="ref int"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    // void xxx(bool * beta)
                    // RefArgumentTransformer
                    Name = "WhenMapping('void x(bool *)')",
                    NativeRet = Void,
                    NativeArgs = { Decl("beta", BoolPtr ) },
                    ManagedArgs = { new ArgInfo {Name="beta", Type="ref bool"} },
                    PInvokeArgs = { new ArgInfo {Name="beta", Type="[MarshalAs(UnmanagedType.I1)]ref bool"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    // void xxx(size_t * gamma)
                    // RefArgumentTransformer
                    Name = "WhenMapping('void x(size_t *)')",
                    NativeRet = Void,
                    NativeArgs = { Decl("gamma", SizeTPtr ) },
                    ManagedArgs = { new ArgInfo {Name="gamma", Type="ref UIntPtr"} },
                    PInvokeArgs = { new ArgInfo {Name="gamma", Type="ref UIntPtr"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMapping('void x(sp_uint64 *)')",
                    // void xxx(sp_uint64 * delta)
                    // RefArgumentTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("delta", SpUint64Ptr ) },
                    ManagedArgs = { new ArgInfo {Name="delta", Type="ref ulong"} },
                    PInvokeArgs = { new ArgInfo {Name="delta", Type="ref ulong"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMappingRefEnum",
                    // void xxx(sp_flavor * epsilon)
                    // RefArgumentTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("epsilon", new PointerCType(new NamedCType("sp_flavor"))) },
                    ManagedArgs = { new ArgInfo {Name="epsilon", Type="ref Flavor"} },
                    PInvokeArgs = { new ArgInfo {Name="epsilon", Type="ref Flavor"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMappingRefHandle",
                    // void xxx(sp_device ** device)
                    // RefHandleArgumentTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("device", new PointerCType(new PointerCType(new NamedCType("sp_device")))) },
                    Suppressed = true,
                    PInvokeArgs = { new ArgInfo {Name="device", Type="ref IntPtr"} },
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMappingRefStruct",
                    // void xxx(sp_room * room)
                    // RefStructTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("room", new PointerCType(new NamedCType("sp_room"))) },
                    PInvokeArgs = { new ArgInfo {Name="room", Type="ref sp_room"} },
                    Suppressed = true, // There are few enough such functions that we write them by hand.
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMapping('const char * x()')",
                    // SimpleStringReturnTransformer
                    NativeRet = ConstCharPtr,
                    NativeArgs = { },
                    ManagedArgs = { },
                    ManagedRet = "string",
                    PInvokeArgs = { },
                    PInvokeRet = "IntPtr",
                },
                new TestCase{
                    Name = "WhenMapping('sp_error x()')",
                    // SpotifyErrorReturnTransformer
                    NativeRet = new NamedCType("sp_error"),
                    NativeArgs = { },
                    ManagedArgs = { },
                    ManagedRet = "void",
                    PInvokeArgs = { },
                    PInvokeRet = "SpotifyError", // Note that in the overrides we explicitly renamed sp_error to SpotifyError.
                },
                new TestCase{
                    Name = "WhenMapping('void x(const char *)')",
                    // StringArgumentTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("alpha", ConstCharPtr) },
                    ManagedArgs = { new ArgInfo {Name="alpha", Type="string"} },
                    PInvokeArgs = { new ArgInfo {Name="alpha", Type="IntPtr"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMapping('int x(char *, size_t)')",
                    // StringReturnTransformer
                    // Yes, we're testing with buffer_len as size_t but the return type int.
                    // This is how sp_session_remembered_user is declared.
                    NativeRet = Int,
                    NativeArgs = { Decl("buffer", CharPtr), Decl("buffer_len", SizeT) },
                    ManagedArgs = { },
                    PInvokeArgs = { new ArgInfo {Name="buffer", Type="IntPtr"}, new ArgInfo{Name="buffer_len", Type="UIntPtr"} },
                    ManagedRet = "string",
                    PInvokeRet = "int",
                },
                new TestCase{
                    Name = "WhenMappingThisPointer",
                    // void xxx(sp_file * file);
                    // ThisPointerArgumentTransformer
                    // (Note: all the methods we generate during these tests are on the 'File'
                    // class. Most don't start with an sp_file * argument, so they are generated
                    // as static methods. This one should generate as an instance method.)
                    // TODO: Assert that the generator sets IsStatic=false
                    NativeRet = Void,
                    NativeArgs = { Decl("file", new PointerCType(new NamedCType("sp_file"))) },
                    ManagedArgs = { },
                    PInvokeArgs = { new ArgInfo {Name="file", Type="IntPtr"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    // void xxx(int alpha)
                    // TrivialArgumentTransformer
                    Name = "WhenMapping('void x(int)')",
                    NativeRet = Void,
                    NativeArgs = { Decl("alpha", Int ) },
                    ManagedArgs = { new ArgInfo {Name="alpha", Type="int"} },
                    PInvokeArgs = { new ArgInfo {Name="alpha", Type="int"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    // void xxx(bool beta)
                    // TrivialArgumentTransformer
                    Name = "WhenMapping('void x(bool)')",
                    NativeRet = Void,
                    NativeArgs = { Decl("beta", Bool ) },
                    ManagedArgs = { new ArgInfo {Name="beta", Type="bool"} },
                    PInvokeArgs = { new ArgInfo {Name="beta", Type="[MarshalAs(UnmanagedType.I1)]bool"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    // void xxx(size_t gamma)
                    // TrivialArgumentTransformer
                    Name = "WhenMapping('void x(size_t)')",
                    NativeRet = Void,
                    NativeArgs = { Decl("gamma", SizeT ) },
                    ManagedArgs = { new ArgInfo {Name="gamma", Type="UIntPtr"} },
                    PInvokeArgs = { new ArgInfo {Name="gamma", Type="UIntPtr"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMappingEnum",
                    // void xxx(sp_flavor epsilon)
                    // TrivialArgumentTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("epsilon", new NamedCType("sp_flavor")) },
                    ManagedArgs = { new ArgInfo {Name="epsilon", Type="Flavor"} },
                    PInvokeArgs = { new ArgInfo {Name="epsilon", Type="Flavor"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    // void xxx(int* alpha, int num_alpha)
                    // TrivialArrayArgumentTransformer
                    Name = "WhenMapping('void x(int *, int)')",
                    NativeRet = Void,
                    NativeArgs = { Decl("alpha", IntPtr ), Decl("num_alpha", Int) },
                    ManagedArgs = { new ArgInfo {Name="alpha", Type="int[]"} },
                    PInvokeArgs = { new ArgInfo {Name="alpha", Type="IntPtr"}, new ArgInfo {Name="num_alpha", Type="int"} },
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    // TrivialReturnTransformer
                    Name = "WhenMapping('int x()')",
                    NativeRet = Int,
                    NativeArgs = {},
                    ManagedArgs = {},
                    PInvokeArgs = {},
                    ManagedRet = "int",
                    PInvokeRet = "int",
                },
                new TestCase{
                    // TrivialReturnTransformer
                    Name = "WhenMapping('bool x()')",
                    NativeRet = Bool,
                    NativeArgs = {},
                    ManagedArgs = {},
                    PInvokeArgs = {},
                    ManagedRet = "bool",
                    PInvokeRet = "[MarshalAs(UnmanagedType.I1)]bool",
                },
                new TestCase{
                    Name = "WhenMapping('sp_uint64 x()')",
                    // TrivialReturnTransformer
                    NativeRet = SpUint64,
                    NativeArgs = {},
                    ManagedArgs = {},
                    PInvokeArgs = {},
                    ManagedRet = "ulong",
                    PInvokeRet = "ulong",
                },
                new TestCase{
                    Name = "WhenMappingEnumReturn",
                    // sp_flavor xxx()
                    // TrivialReturnTransformer
                    NativeRet = new NamedCType("sp_flavor"),
                    NativeArgs = {},
                    ManagedArgs = {},
                    PInvokeArgs = {},
                    ManagedRet = "Flavor",
                    PInvokeRet = "Flavor",
                },
                new TestCase{
                    Name = "WhenMapping('void x(char *, int)')",
                    // UnknownLengthStringReturnTransformer
                    // Note: This transformation is very narrowly targeted to avoid
                    // the risk of it misfiring. It only applies when the char* arg
                    // is called 'buffer' and the int arg is called 'buffer_size'.
                    NativeRet = Void,
                    NativeArgs = { Decl("buffer", CharPtr), Decl("buffer_size", Int) },
                    ManagedArgs = { },
                    PInvokeArgs = { new ArgInfo {Name="buffer", Type="IntPtr"}, new ArgInfo{Name="buffer_size", Type="int"} },
                    ManagedRet = "string",
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMapping('void x(void)')",
                    // VoidArgumentListTransformer
                    // Note: This is only here because our parser is pretty simple
                    // and doesn't know that (void) is C syntax for an empty
                    // argument list.
                    NativeRet = Void,
                    NativeArgs = { Decl("", Void) },
                    ManagedArgs = { },
                    PInvokeArgs = {},
                    ManagedRet = "void",
                    PInvokeRet = "void",
                },
                new TestCase{
                    Name = "WhenMapping('void x(void *)')",
                    // VoidStarArgumentTransformer
                    NativeRet = Void,
                    NativeArgs = { Decl("alpha", VoidPtr) },
                    Suppressed = true,
                    PInvokeArgs = { new ArgInfo {Name="alpha", Type="IntPtr"} },
                    PInvokeRet = "void",
                },
            };
            return data;
        }

        public KeyValuePair<string, FunctionCType> Function(string aName, CType aReturnType, Declaration[] aArgTypes)
        {
            return new KeyValuePair<string, FunctionCType>(aName, FuncType(aReturnType, aArgTypes));
        }
        public FunctionCType FuncType(CType aReturnType, Declaration[] aArgTypes)
        {
            var func = new FunctionCType(aReturnType);
            func.Arguments.AddRange(aArgTypes);
            return func;
        }
        public Declaration Decl(string aName, CType aType)
        {
            return new Declaration{Name = aName, CType = aType};
        }

        protected Mock<IFunctionGenerator> iAssemblerMock;
        protected IFunctionGenerator iAssembler;
        protected List<string> iManagedParameterOrder;
        protected List<string> iPInvokeParameterOrder;

        [SetUp]
        public void ResetParameterOrder()
        {
            iManagedParameterOrder = new List<string>();
            iPInvokeParameterOrder = new List<string>();
        }

        protected override IFunctionFactory MakeFunctionFactory()
        {
            iAssemblerMock = new Mock<IFunctionGenerator>();
            iAssembler = iAssemblerMock.Object;
            var factory = new Mock<IFunctionFactory>();
            factory.Setup(x => x.CreateAssembler(It.IsAny<string>(), It.IsAny<string>())).Returns((string nativeName, string managedName)=>iAssembler);
            factory.Setup(x => x.CreateAnalyser(It.IsAny<List<Declaration>>(), It.IsAny<CType>())).Returns((
                List<Declaration> decls, CType retType)=>new FunctionSpecificationAnalyser(decls, retType));
            iAssemblerMock
                .Setup(x=>x.AddManagedParameter(It.IsAny<string>(), It.IsAny<CSharpType>()))
                .Callback((string name, CSharpType t) => iManagedParameterOrder.Add(name));
            iAssemblerMock
                .Setup(x=>x.AddPInvokeParameter(It.IsAny<CSharpType>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback((CSharpType t, string name, string expr) => iPInvokeParameterOrder.Add(name));
            return factory.Object;
        }

        [TestCaseSource("GenerateTestCases")]
        public void RunTestCases(Action<ClassGenerationTests> aAction)
        {
            aAction(this);
        }

        void GenerateClass(CType aRetType, Declaration[] aParameters)
        {
            iGenerator.GenerateCSharpClass("", "sp_file", new[]{
                Function("sp_file_do_something", aRetType, aParameters)
            });
        }

        void CheckManagedParameterWasGenerated(CType aRetType, Declaration[] aParameters, string aExpectedParameterName, string aExpectedCSharpType)
        {
            GenerateClass(aRetType, aParameters);
            iAssemblerMock.Verify(x => x.AddManagedParameter(aExpectedParameterName, It.Is<CSharpType>(y => y.ToString() == aExpectedCSharpType)));
        }

        void CheckManagedParameterOrder(CType aRetType, Declaration[] aParameters, string[] aOrder)
        {
            GenerateClass(aRetType, aParameters);
            Assert.That(iManagedParameterOrder, Is.EqualTo(aOrder));
        }

        void CheckManagedReturnType(CType aRetType, Declaration[] aParameters, string aExpectedReturnType)
        {
            GenerateClass(aRetType, aParameters);
            if (aExpectedReturnType == "void")
            {
                // void is the default, so we won't actually need to have called SetManagedReturn.
                iAssemblerMock.Verify(x => x.SetManagedReturn(It.Is<CSharpType>(y => y.ToString() == "void")), Times.AtMostOnce());
                iAssemblerMock.Verify(x => x.SetManagedReturn(It.Is<CSharpType>(y => y.ToString() != "void")), Times.Never());
            }
            else
            {
                iAssemblerMock.Verify(x => x.SetManagedReturn(It.Is<CSharpType>(y => y.ToString() == aExpectedReturnType)));
                iAssemblerMock.Verify(x => x.SetManagedReturn(It.Is<CSharpType>(y => y.ToString() != aExpectedReturnType)), Times.Never());
            }
        }

        void CheckPInvokeParameterWasGenerated(CType aRetType, Declaration[] aParameters, string aExpectedParameterName, string aExpectedCSharpType)
        {
            GenerateClass(aRetType, aParameters);
            iAssemblerMock.Verify(x => x.AddPInvokeParameter(It.Is<CSharpType>(y => y.ToString() == aExpectedCSharpType), aExpectedParameterName, It.IsAny<string>()));
        }

        void CheckPInvokeParameterOrder(CType aRetType, Declaration[] aParameters, string[] aOrder)
        {
            GenerateClass(aRetType, aParameters);
            Assert.That(iPInvokeParameterOrder, Is.EqualTo(aOrder));
        }

        void CheckPInvokeReturnType(CType aRetType, Declaration[] aParameters, string aExpectedReturnType)
        {
            GenerateClass(aRetType, aParameters);
            if (aExpectedReturnType == "void")
            {
                iAssemblerMock.Verify(x => x.SetPInvokeReturn(It.IsAny<CSharpType>(), It.IsAny<string>()), Times.Never());
            }
            else
            {
                iAssemblerMock.Verify(x => x.SetPInvokeReturn(It.Is<CSharpType>(y => y.ToString() == aExpectedReturnType), It.IsAny<string>()));
            }
        }

        void CheckWhetherManagedWrapperWasSuppressed(CType aRetType, Declaration[] aParameters, bool aSuppressed)
        {
            GenerateClass(aRetType, aParameters);
            iAssemblerMock.Verify(x => x.SuppressManagedWrapper(), aSuppressed ? Times.Once() : Times.Never());
        }

        static Action<ClassGenerationTests> MkAct(Action<ClassGenerationTests> aAction)
        {
            return aAction;
        }


        class ArgInfo
        {
            public string Name;
            public string Type;
        }
        class TestCase
        {
            public List<Declaration> NativeArgs = new List<Declaration>();
            public List<ArgInfo> ManagedArgs = new List<ArgInfo>();
            public List<ArgInfo> PInvokeArgs = new List<ArgInfo>();
            public CType NativeRet;
            public string ManagedRet;
            public string PInvokeRet;
            public bool Suppressed;
            public string Name;
        }


        public IEnumerable<TestCaseData> GenerateTestCases()
        {
            // Note: This used to be a generator (yield-return) method, but
            // Mono's C# compiler before version 2.11.2 is broken and makes
            // a mess of the results.
            // See Xamarin bug 4641.
            List<TestCaseData> testcases = new List<TestCaseData>();
            var data = GetTestData();
            foreach (var item in data)
            {
                var nativeRetType = item.NativeRet;
                var nativeArgs = item.NativeArgs.ToArray();
                var testName = item.Name;
                Console.WriteLine("nativeArgs: {0}", String.Join(", ", nativeArgs.Select(x=>x.ToString())));

                bool suppressed = item.Suppressed;
                testcases.Add(new TestCaseData(MkAct(x=>x.CheckWhetherManagedWrapperWasSuppressed(nativeRetType, nativeArgs, suppressed)))
                    .SetName(String.Format("{0}_CheckWhetherManagedWrapperWasSuppressed({1})", testName, suppressed)));
                if (!suppressed)
                {
                    // Check each managed argument was present with the correct type.
                    foreach (var managedArg in item.ManagedArgs)
                    {
                        var managedArgName = managedArg.Name;
                        var managedArgType = managedArg.Type;
                        testcases.Add(new TestCaseData(MkAct(x => x.CheckManagedParameterWasGenerated(nativeRetType,
                            nativeArgs, managedArgName, managedArgType)))
                            .SetName(String.Format("{0}_CheckManagedParameterWasGenerated({1}, {2}, {3}, {4})", testName, nativeRetType, nativeArgs, managedArgName, managedArgType)));
                    }

                    // Check all the managed arguments were in the right order.
                    var managedArgOrder = item.ManagedArgs.Select(x => x.Name).ToArray();
                    testcases.Add(new TestCaseData(MkAct(x => x.CheckManagedParameterOrder(nativeRetType, nativeArgs, managedArgOrder)))
                        .SetName(String.Format("{0}_CheckManagedParameterOrder({1}, {2}, {3})", testName, nativeRetType, nativeArgs, managedArgOrder)));

                    // Check the managed return type was correct.
                    var managedRet = item.ManagedRet;
                    testcases.Add(new TestCaseData(MkAct(x => x.CheckManagedReturnType(nativeRetType, nativeArgs, managedRet)))
                        .SetName(String.Format("{0}_CheckManagedReturnType({1}, {2}, {3})", testName, nativeRetType, nativeArgs, managedRet)));
                }

                // Check each pinvoke argument was present with the correct type.
                foreach (var pinvokeArg in item.PInvokeArgs)
                {
                    var pinvokeArgName = pinvokeArg.Name;
                    var pinvokeArgType = pinvokeArg.Type;
                    testcases.Add(new TestCaseData(
                        MkAct(x=>x.CheckPInvokeParameterWasGenerated(nativeRetType,nativeArgs, pinvokeArgName, pinvokeArgType)))
                        .SetName(String.Format("{0}_CheckPInvokeParameterWasGenerated({1}, {2}, {3}, {4})",testName, nativeRetType, nativeArgs, pinvokeArgName, pinvokeArgType)));
                }

                // Check all the P/Invoke arguments were in the right order.
                var pinvokeArgOrder = item.PInvokeArgs.Select(x=>x.Name).ToArray();
                testcases.Add(new TestCaseData(MkAct(x=>x.CheckPInvokeParameterOrder(nativeRetType, nativeArgs, pinvokeArgOrder)))
                    .SetName(String.Format("{0}_CheckPInvokeParameterOrder({1}, {2}, {3})", testName, nativeRetType, nativeArgs, pinvokeArgOrder)));

                // Check the P/Invoke return type was correct.
                var pinvokeRet = item.PInvokeRet;
                testcases.Add(new TestCaseData(MkAct(x=>x.CheckPInvokeReturnType(nativeRetType, nativeArgs, pinvokeRet)))
                    .SetName(String.Format("{0}_CheckPInvokeReturnType({1}, {2}, {3})", testName, nativeRetType, nativeArgs, pinvokeRet)));
            }
            return testcases;
        }
    }
}
