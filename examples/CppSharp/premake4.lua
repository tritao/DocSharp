project "CppSharp"

  SetupManagedProject()

  kind "ConsoleApp"
  language "C#"

  files   { "**.cs" }

  links { "DocSharp" }