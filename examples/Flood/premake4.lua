project "Flood"

  SetupManagedProject()

  kind "ConsoleApp"
  language "C#"

  files   { "**.cs" }

  links { "DocSharp" }