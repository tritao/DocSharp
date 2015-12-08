-- This is the starting point of the build scripts for the project.
-- It defines the common build settings that all the projects share
-- and calls the build scripts of all the sub-projects.

dofile "Helpers.lua"

solution "DocSharp"

  configurations { "Debug", "Release" }
  platforms { "x32", "x64" }
  flags { "Unicode", "Symbols" }
  
  location (builddir)
  objdir (path.join(builddir, "obj"))
  targetdir (libdir)
  libdirs { libdir }
  debugdir (bindir)

  startproject "DocSharp"
  
  configuration "Release"
    flags { "Optimize" }

  configuration "vs2012"
    framework "4.5"

  configuration {}

  project "DocSharp"
    SetupManagedProject()

    kind "SharedLib"
    language "C#"

    location "../DocSharp"
    files   { "../DocSharp/**.cs" }
  
    links
    {
      "System",
      "System.Core",
      "ICSharpCode.NRefactory",
      "ICSharpCode.NRefactory.Cecil",
      "ICSharpCode.NRefactory.CSharp",
      "ICSharpCode.NRefactory.Xml",
      "Mono.Cecil",
      "ColorCode",
      "MarkdownDeep"
  }

  group "Dependencies"

    external "ICSharpCode.NRefactory"
      location ("../NRefactory/ICSharpCode.NRefactory")
      uuid "3B2A5653-EC97-4001-BB9B-D90F1AF2C371"
      language "C#"
      kind "SharedLib"

    external "ICSharpCode.NRefactory.Cecil"
      location ("../NRefactory/ICSharpCode.NRefactory.Cecil")
      uuid "2B8F4F83-C2B3-4E84-A27B-8DEE1BE0E006"
      language "C#"
      kind "SharedLib"      

    external "ICSharpCode.NRefactory.CSharp"
      location ("../NRefactory/ICSharpCode.NRefactory.CSharp")
      uuid "53DCA265-3C3C-42F9-B647-F72BA678122B"
      language "C#"
      kind "SharedLib"

    external "ICSharpCode.NRefactory.Xml"
      location ("../NRefactory/ICSharpCode.NRefactory.Xml")
      uuid "DC393B66-92ED-4CAD-AB25-CFEF23F3D7C6"
      language "C#"
      kind "SharedLib"

    external "Mono.Cecil"
      location ("../cecil")
      uuid "D68133BD-1E63-496E-9EDE-4FBDBF77B486"
      language "C#"
      kind "SharedLib"

    external "ColorCode"
      location ("../colorcode/ColorCode")
      uuid "37438935-D221-4FD8-A10E-4EC5356B0F94"
      language "C#"
      kind "SharedLib"

    external "MarkdownDeep"
      location ("../markdowndeep/MarkdownDeep")
      uuid "1569ED47-C7C9-4261-B6F4-7445BD0F2C95"
      language "C#"
      kind "SharedLib"

  group "Examples"

    print("Searching for example projects...")
    IncludeDir(examplesdir)