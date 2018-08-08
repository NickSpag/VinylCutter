using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using VinylCutter.Records;

namespace VinylCutter.Infrastructure
{
    public class Parser
    {
        string FirstNamespace;

        string Text = "";

        SemanticModel Model;
        SourceText SourceText;
        
        Symbols Symbols;

        public Parser(string text)
        {
            Text = text;
        }

        public FileInfo Parse(IParserWorkflow workflow)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(workflow.Prelude + Text);
            CSharpCompilation compilation = Compile(tree);

            return workflow.Parse(compilation.GetSemanticModel(tree), compilation);
        }

        static CSharpCompilation Compile(SyntaxTree tree)
        {
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("Vinyl").AddReferences(mscorlib).AddSyntaxTrees(tree).WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var compilerDiagnostics = compilation.GetDiagnostics();
            var compilerErrors = compilerDiagnostics.Where(i => i.Severity == DiagnosticSeverity.Error);
            if (compilerErrors.Count() > 0)
                throw new ParseCompileError(string.Join("\n", compilerErrors.Select(x => x.ToString())));

            return compilation;
        }
    }
}
