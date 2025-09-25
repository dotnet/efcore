// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;
using IOPath = System.IO.Path;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class BuildReference
{
    private BuildReference(IEnumerable<MetadataReference> references, bool copyLocal = false, string? path = null)
    {
        References = references;
        CopyLocal = copyLocal;
        Path = path;
    }

    public IEnumerable<MetadataReference> References { get; }

    public bool CopyLocal { get; }
    public string? Path { get; }

    public static BuildReference ByName(string name, bool copyLocal = false)
    {
        var references = (from l in DependencyContext.Default!.CompileLibraries
                          where l.Assemblies.Any(a => IOPath.GetFileNameWithoutExtension(a) == name)
                          from r in l.ResolveReferencePaths()
                          where IOPath.GetFileNameWithoutExtension(r) == name
                          select MetadataReference.CreateFromFile(r)).ToList();
        if (references.Count == 0)
        {
            throw new InvalidOperationException(
                $"Assembly '{name}' not found. "
                + "You may be missing '<PreserveCompilationContext>true</PreserveCompilationContext>' in your test project's csproj.");
        }

        return new BuildReference(
            references,
            copyLocal);
    }

    public static BuildReference ByPath(string path)
        => new(new[] { MetadataReference.CreateFromFile(path) }, path: path);
}
