using System;
using Microsoft.CodeAnalysis.CSharp;
namespace VinylCutter.Infrastructure
{
    public abstract class BaseCodeGenerator
    {
        protected FileInfo File;

        public BaseCodeGenerator(FileInfo file)
        {
            File = file;
        }

        public abstract string Generate();
        
        protected void GenerateTopLevelInjects(CodeWriter writer)
        {
            if (!string.IsNullOrEmpty(File.InjectCode))
            {
                writer.WriteLineIgnoringIndent(File.InjectCode);
                writer.WriteLine();
            }
        }

        protected void GenerateNamespaceHeader(CodeWriter writer)
        {
            if (!string.IsNullOrEmpty(File.GlobalNamespace))
            {
                writer.WriteLine($"namespace {File.GlobalNamespace}");
                writer.WriteLine("{");
                writer.Indent();
            }
        }

        protected void GenerateNamespaceFooter(CodeWriter writer)
        {
            if (!string.IsNullOrEmpty(File.GlobalNamespace))
            {
                writer.Dedent();
                writer.WriteLine("}");
            }
        }

        protected void GenerateUsings(CodeWriter writer, bool withCollections)
        {
            writer.WriteLine("using System;");

            if(withCollections)
            {
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using System.Collections.Immutable;");
            }
        }
    }
}
