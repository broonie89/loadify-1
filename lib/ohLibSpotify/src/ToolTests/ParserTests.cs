using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiParser;
using NUnit.Framework;

namespace ToolTests
{
    [TestFixture]
    public class WhenParsingAForwardStructTypedef
    {
        Declaration[] iDeclarations;
        [SetUp]
        public void Parse()
        {
            var parser = new HeaderParser(CHeaderLexer.Lex("typedef struct tagname typename; ///< This is the comment\n"));
            iDeclarations = parser.ParseHeader().ToArray();
        }
        [Test]
        public void ThereShouldBeOneDeclaration()
        {
            Assert.That(iDeclarations.Length, Is.EqualTo(1));
        }
        [Test]
        public void TheDeclarationShouldBeATypedef()
        {
            Assert.That(iDeclarations[0].Kind, Is.EqualTo("typedef"));
        }
        [Test]
        public void TheDeclarationShouldHaveAName()
        {
            Assert.That(iDeclarations[0].Name, Is.EqualTo("typename"));
        }
        [Test]
        public void TheDeclarationShouldHaveAComment()
        {
            Assert.That(iDeclarations[0].RawComment, Is.EqualTo("///< This is the comment\n"));
        }
        [Test]
        public void TheDeclarationTypeShouldBeStruct()
        {
            var ctype = iDeclarations[0].CType;
            var structType = ctype as StructCType;
            Assert.That(structType, Is.Not.Null);
        }
        [Test]
        public void TheStructShouldHaveATag()
        {
            var ctype = iDeclarations[0].CType;
            var structType = (StructCType)ctype;
            Assert.That(structType.Tag, Is.EqualTo("tagname"));
        }
        [Test]
        public void TheStructShouldBeForwardDeclared()
        {
            var ctype = iDeclarations[0].CType;
            var structType = (StructCType)ctype;
            Assert.That(structType.IsForward, Is.True);
        }
    }

    [TestFixture]
    public class WhenParsingAnEnumTypedef
    {
        Declaration[] iDeclarations;
        [SetUp]
        public void Parse()
        {
            var parser = new HeaderParser(CHeaderLexer.Lex(
                "/** Flavors of food */\n" +
                "typedef enum tagflavor {\n" +
                "    FLAVOR_VANILLA = 0,      ///< Universally popular\n" +
                "    FLAVOR_CHEESE_AND_ONION,\n" +
                "    FLAVOR_SPEARMINT,        ///< Soothing\n" +
                "    FLAVOR_COFFEE=99,        ///< Stimulating\n" +
                "    FLAVOR_SMOKY_BACON       ///< Controversial\n" +
                "} flavor;\n"
                ));
            iDeclarations = parser.ParseHeader().ToArray();
        }
        [Test]
        public void ThereShouldBeOneDeclaration()
        {
            Assert.That(iDeclarations.Length, Is.EqualTo(1));
        }
        [Test]
        public void TheDeclarationShouldBeATypedef()
        {
            Assert.That(iDeclarations[0].Kind, Is.EqualTo("typedef"));
        }
        [Test]
        public void TheDeclarationShouldHaveAName()
        {
            Assert.That(iDeclarations[0].Name, Is.EqualTo("flavor"));
        }
        [Test]
        public void TheDeclarationShouldHaveAComment()
        {
            Assert.That(iDeclarations[0].RawComment, Is.EqualTo("/** Flavors of food */"));
        }
        [Test]
        public void TheDeclarationTypeShouldBeEnum()
        {
            var ctype = iDeclarations[0].CType;
            var enumType = ctype as EnumCType;
            Assert.That(enumType, Is.Not.Null);
        }
        [Test]
        public void TheEnumShouldHaveATag()
        {
            var ctype = iDeclarations[0].CType;
            var enumType = (EnumCType)ctype;
            Assert.That(enumType.Tag, Is.EqualTo("tagflavor"));
        }
        [Test]
        public void TheConstantsShouldBeNamed()
        {
            var ctype = iDeclarations[0].CType;
            var enumType = (EnumCType)ctype;
            Assert.That(enumType.Constants.Select(x=>x.Name), Is.EqualTo(new[]{
                "FLAVOR_VANILLA",
                "FLAVOR_CHEESE_AND_ONION",
                "FLAVOR_SPEARMINT",
                "FLAVOR_COFFEE",
                "FLAVOR_SMOKY_BACON"
            }));
        }
        [Test]
        public void TheConstantsShouldHaveValues()
        {
            var ctype = iDeclarations[0].CType;
            var enumType = (EnumCType)ctype;
            Assert.That(enumType.Constants.Select(x=>x.Value), Is.EqualTo(new[]{ 0, 1, 2, 99, 100 }));
        }
        [Test]
        public void TheConstantsShouldHaveComments()
        {
            var ctype = iDeclarations[0].CType;
            var enumType = (EnumCType)ctype;
            Assert.That(enumType.Constants.Select(x=>x.RawComment), Is.EqualTo(new[]{
                "///< Universally popular\n",
                null,
                "///< Soothing\n",
                "///< Stimulating\n",
                "///< Controversial\n"
            }));
        }
    }

    [TestFixture]
    public class WhenParsingAStructTypedef
    {
        Declaration[] iDeclarations;
        [SetUp]
        public void Parse()
        {
            var parser = new HeaderParser(CHeaderLexer.Lex(
                "/** Brave explorer */\n" +
                "typedef struct tagexplorer {\n" +
                "    int x;                           ///< X coordinate\n" +
                "    int y;                           ///< Y coordinate\n" +
                "    char name[32];                   ///< Name\n" +
                "    struct taghero * bestfriend;     ///< Friend\n" +
                "} explorer;\n"
                ));
            iDeclarations = parser.ParseHeader().ToArray();
        }
        [Test]
        public void ThereShouldBeOneDeclaration()
        {
            Assert.That(iDeclarations.Length, Is.EqualTo(1));
        }
        [Test]
        public void TheDeclarationShouldBeATypedef()
        {
            Assert.That(iDeclarations[0].Kind, Is.EqualTo("typedef"));
        }
        [Test]
        public void TheDeclarationShouldHaveAName()
        {
            Assert.That(iDeclarations[0].Name, Is.EqualTo("explorer"));
        }
        [Test]
        public void TheDeclarationShouldHaveAComment()
        {
            Assert.That(iDeclarations[0].RawComment, Is.EqualTo("/** Brave explorer */"));
        }
        [Test]
        public void TheDeclarationTypeShouldBeStruct()
        {
            var ctype = iDeclarations[0].CType;
            var structType = ctype as StructCType;
            Assert.That(structType, Is.Not.Null);
        }
        [Test]
        public void ThestructShouldHaveATag()
        {
            var ctype = iDeclarations[0].CType;
            var structType = (StructCType)ctype;
            Assert.That(structType.Tag, Is.EqualTo("tagexplorer"));
        }
        [Test]
        public void TheFieldsShouldBeNamed()
        {
            var ctype = iDeclarations[0].CType;
            var structType = (StructCType)ctype;
            Assert.That(structType.Fields.Select(x=>x.Name), Is.EqualTo(new[]{
                "x", "y", "name", "bestfriend"
            }));
        }
        [Test]
        public void TheFieldsShouldHaveValues()
        {
            var ctype = iDeclarations[0].CType;
            var structType = (StructCType)ctype;
            Assert.That(structType.Fields.Select(x=>x.CType.ToString()), Is.EqualTo(new[]{ "int", "int", "char[32]", "struct taghero*" }));
        }
        [Test]
        public void TheFieldsShouldHaveComments()
        {
            var ctype = iDeclarations[0].CType;
            var structType = (StructCType)ctype;
            Assert.That(structType.Fields.Select(x=>x.RawComment), Is.EqualTo(new[]{
                "///< X coordinate\n",
                "///< Y coordinate\n",
                "///< Name\n",
                "///< Friend\n"
            }));
        }
    }

    [TestFixture]
    public class WhenParsingAFunctionDeclaration
    {
        Declaration[] iDeclarations;
        [SetUp]
        public void Parse()
        {
            var parser = new HeaderParser(CHeaderLexer.Lex(
                "/** Some function */\n" +
                "void frob_widgets(widget *widgets, int num_widgets);"));
            iDeclarations = parser.ParseHeader().ToArray();
        }
        [Test]
        public void ThereShouldBeOneDeclaration()
        {
            Assert.That(iDeclarations.Length, Is.EqualTo(1));
        }
        [Test]
        public void TheDeclarationShouldBeAnInstance()
        {
            Assert.That(iDeclarations[0].Kind, Is.EqualTo("instance"));
        }
        [Test]
        public void TheDeclarationShouldHaveAName()
        {
            Assert.That(iDeclarations[0].Name, Is.EqualTo("frob_widgets"));
        }
        [Test]
        public void TheDeclarationShouldHaveAComment()
        {
            Assert.That(iDeclarations[0].RawComment, Is.EqualTo("/** Some function */"));
        }
        [Test]
        public void TheDeclarationTypeShouldBeFunction()
        {
            var ctype = iDeclarations[0].CType;
            var funcType = ctype as FunctionCType;
            Assert.That(funcType, Is.Not.Null);
        }
        [Test]
        public void TheReturnTypeShouldBeVoid()
        {
            var ctype = iDeclarations[0].CType;
            var funcType = (FunctionCType)ctype;
            Assert.That(funcType.ReturnType.ToString(), Is.EqualTo("void"));
        }
        [Test]
        public void TheArgumentsShouldBeNamed()
        {
            var ctype = iDeclarations[0].CType;
            var funcType = (FunctionCType)ctype;
            Assert.That(funcType.Arguments.Select(x=>x.Name), Is.EqualTo(new[]{
                "widgets", "num_widgets"
            }));
        }
        [Test]
        public void TheArgumentsShouldHaveTypes()
        {
            var ctype = iDeclarations[0].CType;
            var funcType = (FunctionCType)ctype;
            Assert.That(funcType.Arguments.Select(x=>x.CType.ToString()), Is.EqualTo(new[]{ "widget*", "int" }));
        }
    }

    [TestFixture]
    public class QualifierTests
    {
        [Test]
        public void WhenParsingUnsignedInt_TheUnsignedQualifierIsApplied()
        {
            var parser = new HeaderParser(CHeaderLexer.Lex(
                "unsigned int foo;"));
            var declaration = parser.ParseHeader().Single();
            Assert.That(declaration.CType.Qualifiers, Is.EqualTo(new[] { "unsigned" }));
        }

        [Test]
        public void WhenParsingInt_TheUnsignedQualifierIsNotApplied()
        {
            var parser = new HeaderParser(CHeaderLexer.Lex(
                "int foo;"));
            var declaration = parser.ParseHeader().Single();
            Assert.That(declaration.CType.Qualifiers, Is.Empty);
        }

        [Test]
        public void WhenParsingConstIntStar_TheConstQualifierIsAppliedToTheInt()
        {
            var parser = new HeaderParser(CHeaderLexer.Lex(
                "const int *foo;"));
            var declaration = parser.ParseHeader().Single();
            var ptrType = (PointerCType)declaration.CType;
            Assert.That(ptrType.BaseType.Qualifiers, Is.EqualTo(new[] { "const" }));
            Assert.That(ptrType.Qualifiers, Is.Empty);
        }

        [Test]
        public void WhenParsingIntConstStar_TheConstQualifierIsAppliedToTheInt()
        {
            var parser = new HeaderParser(CHeaderLexer.Lex(
                "int const *foo;"));
            var declaration = parser.ParseHeader().Single();
            var ptrType = (PointerCType)declaration.CType;
            Assert.That(ptrType.BaseType.Qualifiers, Is.EqualTo(new[] { "const" }));
            Assert.That(ptrType.Qualifiers, Is.Empty);
        }

        [Test]
        public void WhenParsingIntStarConst_TheConstQualifierIsAppliedToThePointer()
        {
            var parser = new HeaderParser(CHeaderLexer.Lex(
                "int * const foo;"));
            var declaration = parser.ParseHeader().Single();
            var ptrType = (PointerCType)declaration.CType;
            Assert.That(ptrType.Qualifiers, Is.EqualTo(new[] { "const" }));
            Assert.That(ptrType.BaseType.Qualifiers, Is.Empty);
        }

        [Test]
        public void WhenParsingConstIntStarConst_TheConstQualifierIsAppliedToBoth()
        {
            var parser = new HeaderParser(CHeaderLexer.Lex(
                "const int * const foo;"));
            var declaration = parser.ParseHeader().Single();
            var ptrType = (PointerCType)declaration.CType;
            Assert.That(ptrType.Qualifiers, Is.EqualTo(new[] { "const" }));
            Assert.That(ptrType.BaseType.Qualifiers, Is.EqualTo(new[] { "const" }));
        }
    }
}
