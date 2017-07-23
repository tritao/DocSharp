-- This is the starting point of the build scripts for the project.
-- It defines the common build settings that all the projects share
-- and calls the build scripts of all the sub-projects.

-- This module checks for the all the project dependencies.

examplesdir = path.getabsolute("../examples");

function managed_project(name)
  local proj = project(name)

  filter { "action:vs*" }
    location "."

  filter {}

  return proj
end

function include_dir(dir)
  local deps = os.matchdirs(dir .. "/*")
  
  for i,dep in ipairs(deps) do
    local fp = path.join(dep, "premake4.lua")
    fp = path.join(os.getcwd(), fp)
    
    if os.isfile(fp) then
      print(string.format(" including %s", dep))
      include(dep)
    end
  end
end

solution "DocSharp"

  configurations { "Debug", "Release" }

  characterset "Unicode"
  symbols "On"

  local action = _OPTIONS["outdir"] or _ACTION
  location (".")

  objdir (path.join("./", action, "obj"))
  targetdir (path.join("./", action, "lib", "%{cfg.buildcfg}"))

  startproject "DocSharp"
  
  filter { "configurations:Release" }
    optimize "On"

  filter {}

  managed_project("DocSharp")

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
      location ("../external/NRefactory/ICSharpCode.NRefactory")
      uuid "3B2A5653-EC97-4001-BB9B-D90F1AF2C371"
      language "C#"
      kind "SharedLib"

    external "ICSharpCode.NRefactory.Cecil"
      location ("../external/NRefactory/ICSharpCode.NRefactory.Cecil")
      uuid "2B8F4F83-C2B3-4E84-A27B-8DEE1BE0E006"
      language "C#"
      kind "SharedLib"      

    external "ICSharpCode.NRefactory.CSharp"
      location ("../external/NRefactory/ICSharpCode.NRefactory.CSharp")
      uuid "53DCA265-3C3C-42F9-B647-F72BA678122B"
      language "C#"
      kind "SharedLib"

    external "ICSharpCode.NRefactory.Xml"
      location ("../external/NRefactory/ICSharpCode.NRefactory.Xml")
      uuid "DC393B66-92ED-4CAD-AB25-CFEF23F3D7C6"
      language "C#"
      kind "SharedLib"

    external "Mono.Cecil"
      location ("../external/cecil")
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
    include_dir(examplesdir)