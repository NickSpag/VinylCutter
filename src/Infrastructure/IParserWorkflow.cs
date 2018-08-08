using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace VinylCutter.Infrastructure
{
    public interface IParserWorkflow
    {
        string Prelude { get; }
        FileInfo Parse(SemanticModel model, CSharpCompilation compilation);
    }
}
