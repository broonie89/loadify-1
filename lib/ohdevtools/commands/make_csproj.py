import sys
import uuid
import optparse
import os

description = "Create an empty, properly configured .csproj file."
command_group = "Developer tools"
command_synonyms = ["mkprj","make-csproj"]
command_name = "make-csproj"

TEMPLATE = """<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" DefaultTargets="WafBuild" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProductVersion>8.0.30703</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{{{projectguid}}}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>{rootnamespace}</RootNamespace>
    <AssemblyName>{assemblyname}</AssemblyName>
    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <TargetFrameworkProfile>Client</TargetFrameworkProfile>
  </PropertyGroup>
  <Import Project="..\\SharedSettings.target" />
  <PropertyGroup>
    <StartupObject />
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="System.Xml.Linq" />
    <Reference Include="System.Data.DataSetExtensions" />
    <Reference Include="System.Data" />
    <Reference Include="System.Xml" />
  </ItemGroup>
  <ItemGroup>
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\\Microsoft.CSharp.targets" />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name="BeforeBuild">
  </Target>
  <Target Name="AfterBuild">
  </Target>
  -->
</Project>"""

HELP_SYNONYMS = ["--help", "-h", "/h", "/help", "/?", "-?", "h", "help"]

USAGE="""%prog [options] [PROJECT_NAME [ASSEMBLY_NAME [ROOT_NAMESPACE [DIRECTORY_NAME]]]

Create a new project called PROJECT_NAME, producing an assembly called
ASSEMBLY_NAME with the given root namespace in subdirectory DIRECTORY_NAME
under the src directory. Unspecified elements default to the project name.
If no project name is specified, the script runs in interactive mode."""

def main():
    parser = optparse.OptionParser(usage=USAGE)
    parser.add_option("--stdout", action="store_true", default=False, help="Print .csproj to stdout.")
    parser.add_option("--output-type", default="Library", help="Set output type, can be Library, Exe or WinExe.")
    opts, args = parser.parse_args()
    if len(args) == 0:
        print "Creating a .csproj file."
        print ""
        print "1. Project name"
        print "The project name determines the .csproj filename and the"
        print "appearance in the Solution Explorer."
        print ""
        project_name = raw_input("Project name (e.g. WeebleCorps.Widgets)? ").strip()
        if project_name=="":
            return
        print ""
        print "2. Assembly name"
        print "The assembly name determines the .dll or .exe filename."
        print "(This should match the name in the wscript.)"
        print ""
        assembly_name = raw_input("Assembly name (default '{0}')? ".format(project_name)).strip()
        if assembly_name=="":
            assembly_name = project_name
            print assembly_name
        print ""
        print "3. Root namespace"
        print "The root namespace is used by Visual Studio and Resharper"
        print "when automatically generating or fixing namespace blocks."
        print ""
        root_namespace = raw_input("Root namespace (default '{0}')? ".format(project_name)).strip()
        if root_namespace == "":
            root_namespace = project_name
            print root_namespace
        print ""
        print "4. Directory name"
        print "Determines the subdirectory of 'src' to put the .csproj file."
        print ""
        directory_name = raw_input("Directory name (default '{0}')? ".format(project_name)).strip()
        if directory_name == "":
            directory_name = project_name
            print directory_name
        output_type = None
        while output_type not in ["Library", "Exe", "WinExe"]:
            print "Output type (Library, Exe, WinExe: default {0})? ".format(opts.output_type),
            output_type = raw_input().strip()
            if output_type == "":
                output_type = opts.output_type
                print output_type
            if "LIBRARY".startswith(output_type.upper()):
                output_type = "Library"
            elif "EXE".startswith(output_type.upper()):
                output_type = "Exe"
            elif "WINEXE".startswith(output_type.upper()):
                output_type = "WinExe"
    else:
        project_name = args[0]
        assembly_name = project_name
        root_namespace = project_name
        directory_name = project_name
        if len(args)>=2:
            assembly_name = args[1]
        if len(args)>=3:
            directory_name = args[2]
        if len(args)>=4:
            project_name = args[3]
        output_type = opts.output_type
    UUID = str(uuid.uuid4()).upper()
    if opts.stdout:
        f=sys.stdout
    else:
        if not os.path.isdir(os.path.join("src", directory_name)):
            os.makedirs(os.path.join("src", directory_name))
        fname = os.path.join("src", directory_name, assembly_name + ".csproj")
        f = open(fname, "w")
    f.write(TEMPLATE.format(
            projectguid=UUID,
            rootnamespace=root_namespace,
            assemblyname=assembly_name))

if __name__ == "__main__":
    main()
