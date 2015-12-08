using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DocSharp
{
    /// <summary>
    /// Represents an input file in the documentation project.
    /// </summary>
    public class ProjectInput
    {
        /// <summary>
        /// Full path to the input file.
        /// </summary>
        public string FullPath;

        /// <summary>
        /// Base path to the input file.
        /// </summary>
        public string BasePath;

        /// <summary>
        /// Stream to the the input file.
        /// </summary>
        public Stream Stream;
    }

    /// <summary>
    /// Represents the output generated for the documentation project.
    /// </summary>
    public class ProjectOutput
    {
        public Dictionary<string, Stream> Files;

        public ProjectOutput()
        {
            Files = new Dictionary<string, Stream>();
        }

        public Stream GetOutput(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new NotSupportedException("Invalid path");

            var stream = new MemoryStream();
            Files[path] = stream;

            return stream;
        }

        public Stream WriteOutput(string path, string content)
        {
            var stream = GetOutput(path);

            var bytes = Encoding.UTF8.GetBytes(content);
            stream.Write(bytes, 0, bytes.Length);

            return stream;
        }
    }

    /// <summary>
    /// Represents a documentation project.
    /// </summary>
    public class Project
    {
        public string Name;
        public string OutputPath;

        public List<string> SourceDirs;
        public List<string> AssemblyDirs;
        public List<string> DocumentDirs;

        public List<string> Sources;
        public List<string> Assemblies;
        public List<string> Documents;

        internal List<ProjectInput> SourceInputs;
        internal List<ProjectInput> AssemblyInputs;
        internal List<ProjectInput> DocumentInputs;

        public Project()
        {
            SourceDirs = new List<string>();
            AssemblyDirs = new List<string>();
            DocumentDirs = new List<string>();

            Sources = new List<string>();
            Assemblies = new List<string>();
            Documents = new List<string>();

            SourceInputs = new List<ProjectInput>();
            AssemblyInputs = new List<ProjectInput>();
            DocumentInputs = new List<ProjectInput>();
        }

        public void BuildInputs()
        {
            foreach (var path in SourceDirs)
            {
                if (!Directory.Exists(path)) continue;

                var files = Directory.EnumerateFiles(path, "*.cs",
                                                     SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var input = new ProjectInput
                        {
                            BasePath = path,
                            FullPath = file,
                        };

                    SourceInputs.Add(input);
                }
            }

            foreach (var path in AssemblyDirs)
            {
                if (!Directory.Exists(path)) continue;

                var files = Directory.EnumerateFiles(path, "*.dll");

                foreach (var file in files)
                {
                    var matches = false;
                    foreach (var assembly in Assemblies)
                        matches |= Regex.IsMatch(Path.GetFileName(file), assembly);

                    if (!matches) continue;

                    var input = new ProjectInput
                        {
                            BasePath = path,
                            FullPath = file,
                        };

                    AssemblyInputs.Add(input);
                }
            }

            foreach (var path in DocumentDirs)
            {
                if (!Directory.Exists(path)) continue;

                var files = Directory.EnumerateFiles(path, "*.md");

                foreach (var file in files)
                {
                    var matches = false;
                    foreach (var doc in Documents)
                        matches |= Regex.IsMatch(Path.GetFileName(file), doc);

                    if (!matches) continue;

                    var input = new ProjectInput
                        {
                            BasePath = path,
                            FullPath = file,
                        };

                    DocumentInputs.Add(input);
                }
            }
        }
    }
}