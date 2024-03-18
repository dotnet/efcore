// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyModel;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class LinqQuerySourceGeneratorTests
{
    [Fact]
    public Task Query_on_multiple_operators_and_DbSet_property()
        => Test("""
var blogs = context.Blogs.Where(b => b.Id > 3).OrderBy(b => b.Id).ToList();
Assert.Collection(blogs,
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [Fact]
    public Task Query_directly_on_DbSet_property()
        => Test(
            """
var blogs = context.Blogs.ToList();
Assert.Collection(blogs,
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [Fact]
    public Task Query_directly_on_Set_method()
        => Test(
            """
var blogs = context.Set<Blog>().ToList();
Assert.Collection(blogs,
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [Fact]
    public Task Query_over_enumerable_is_not_processed()
        => Test(
            "_ = new[] { 1, 2, 3 }.Where(i => i > 1).ToList();",
            generatedCodeAsserter: code => Assert.Null(code));

    [Fact]
    public async Task Broken_up_query_does_not_work()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Test(
            """
var query = context.Blogs.Where(b => b.Id > 3);
_ = query.OrderBy(b => b.Id).ToList();
""",
            diagnosticsAsserter: d => Assert.Equal(LinqQuerySourceGenerator.Id, Assert.Single(d).Id)));

        Assert.Equal(CoreStrings.RuntimeQueryCompilationDisabled, exception.Message);
    }

    [Fact]
    public Task Same_terminating_operators_get_one_interceptor()
        => Test("""
var blogs = context.Blogs.Where(b => b.Id > 3).OrderBy(b => b.Id).ToList();
Assert.Collection(blogs,
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));

var ids = context.Blogs.Select(b => b.Id).OrderBy(id => id).ToList();
Assert.Equivalent(new[] { 8, 9 }, ids);
""",
            generatedCodeAsserter: code =>
            {
                Assert.NotNull(code);
                Assert.Equal(2, CountOccurrences(code, "[InterceptsLocation("));
                Assert.Equal(1, CountOccurrences(code, "ToList_Safe"));
            });

    [Fact]
    public async Task Source_generator_is_disabled_without_config_option()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Test(
            "var blogs = context.Blogs.ToList();",
            enableSourceGenerator: false,
            generatedCodeAsserter: Assert.Null));

        Assert.Equal(CoreStrings.RuntimeQueryCompilationDisabled, exception.Message);
    }

    #region Terminating operators

    [Fact]
    public Task ToList()
        => Test("""
var blogs = context.Blogs.ToList();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [Fact]
    public Task ToListAsync()
        => Test("""
var blogs = await context.Blogs.ToListAsync();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [Fact]
    public Task ToArray()
        => Test("""
var blogs = context.Blogs.ToArray();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [Fact]
    public Task ToArrayAsync()
        => Test("""
var blogs = await context.Blogs.ToArrayAsync();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [Fact]
    public Task ToDictionary()
        => Test("""
var blogs = context.Blogs.ToDictionary(kv => kv.Id, kv => kv.Name);
Assert.Equal(2, blogs.Count);
Assert.Equal("Blog1", blogs[8]);
Assert.Equal("Blog2", blogs[9]);
""");

    [Fact]
    public Task ToDictionaryAsync()
        => Test("""
var blogs = await context.Blogs.ToDictionaryAsync(kv => kv.Id, kv => kv.Name);
Assert.Equal(2, blogs.Count);
Assert.Equal("Blog1", blogs[8]);
Assert.Equal("Blog2", blogs[9]);
""");

    [Fact]
    public Task ToHashSet()
        => Test("""
var blogs = context.Blogs.ToHashSet();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [Fact]
    public Task ToHashSetAsync()
        => Test("""
var blogs = await context.Blogs.ToHashSetAsync();
Assert.Collection(
    blogs.OrderBy(b => b.Id),
    b => Assert.Equal(8, b.Id),
    b => Assert.Equal(9, b.Id));
""");

    [Fact]
    public Task AsEnumerable()
        => Test("""
foreach (var blog in context.Blogs.Where(b => b.Id == 8).AsEnumerable())
{
    Assert.Equal("Blog1", blog.Name);
}
""");

    [Fact]
    public Task AsAsyncEnumerable()
        => Test("""
await foreach (var blog in context.Blogs.Where(b => b.Id == 8).AsAsyncEnumerable())
{
    Assert.Equal("Blog1", blog.Name);
}
""");

    #endregion Terminating operators

    private static async Task Test(
        string code,
        bool enableSourceGenerator = true,
        Action<ImmutableArray<Diagnostic>>? diagnosticsAsserter = null,
        Action<string?>? generatedCodeAsserter = null)
    {
        var fullCode = $$"""
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

public static class TestContainer
{
    public static async Task Test()
    {
        await using var context = new BlogContext();

        context.Blogs.AddRange(new Blog[]
        {
            new() { Id = 8, Name = "Blog1" },
            new() { Id = 9, Name = "Blog2" }
        });
        context.SaveChanges();

{{code}}
    }
}

public class BlogContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
}

public class Blog
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
""";

        var metadataReferences
            = DependencyContext.Load(typeof(LinqQuerySourceGeneratorTests).Assembly)!
                .CompileLibraries
                .SelectMany(c => c.ResolveReferencePaths())
                .Select(path => MetadataReference.CreateFromFile(path))
                .Cast<MetadataReference>()
                .ToList();

        var interceptorsFeature =
            new[]
            {
                new KeyValuePair<string, string>("InterceptorsPreviewNamespaces", "Microsoft.EntityFrameworkCore.GeneratedInterceptors")
            };

        var parseOptions = new CSharpParseOptions().WithFeatures(interceptorsFeature);
        var syntaxTree = CSharpSyntaxTree.ParseText(fullCode, path: "Test.cs", options: parseOptions);

        var compilation = CSharpCompilation.Create(
            "SourceGeneratorTests",
            syntaxTrees: [syntaxTree],
            metadataReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errorDiagnostics = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (errorDiagnostics.Count > 0)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Compilation failed:").AppendLine();

            foreach (var errorDiagnostic in errorDiagnostics)
            {
                stringBuilder.AppendLine(errorDiagnostic.ToString());

                var textLines = errorDiagnostic.Location.SourceTree!.GetText().Lines;
                var startLine = errorDiagnostic.Location.GetLineSpan().StartLinePosition.Line;
                var endLine = errorDiagnostic.Location.GetLineSpan().EndLinePosition.Line;

                if (startLine == endLine)
                {
                    stringBuilder.Append("Line: ").AppendLine(textLines[startLine].ToString().TrimStart());
                }
                else
                {
                    stringBuilder.AppendLine("Lines:");
                    for (var i = startLine; i <= endLine; i++)
                    {
                        stringBuilder.AppendLine(textLines[i].ToString());
                    }
                }
            }

            throw new InvalidOperationException("Compilation failed:" + stringBuilder);
        }

        var generator = new LinqQuerySourceGenerator();

        var optionsProvider = new FakeAnalyzerConfigOptionsProvider(
            enableSourceGenerator
                ? [(LinqQuerySourceGenerator.DisableRuntimeCompilationMsbuildProperty, "true")]
                : []);

        CSharpGeneratorDriver
            .Create(generator)
            .WithUpdatedParseOptions(parseOptions)
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics);

        diagnosticsAsserter ??= d => Assert.Empty(d);
        diagnosticsAsserter(diagnostics);

        var (assemblyLoadContext, assembly) = EmitAndLoadAssembly(outputCompilation, "");

        try
        {
            var testContainer = assembly.ExportedTypes.Single(t => t.Name == "TestContainer");
            var testMethod = testContainer.GetMethod("Test")!;
            await (Task)testMethod.Invoke(obj: null, parameters: [])!;
        }
        finally
        {
            assemblyLoadContext.Unload();
        }

        if (generatedCodeAsserter is not null)
        {
            generatedCodeAsserter(outputCompilation.SyntaxTrees.Skip(1).SingleOrDefault()?.ToString());
        }

        static (AssemblyLoadContext, Assembly) EmitAndLoadAssembly(Compilation compilation, string assemblyLoadContextName)
        {
            var errorDiagnostics = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errorDiagnostics.Count > 0)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Compilation failed:").AppendLine();

                foreach (var errorDiagnostic in errorDiagnostics)
                {
                    stringBuilder.AppendLine(errorDiagnostic.ToString());

                    var textLines = errorDiagnostic.Location.SourceTree!.GetText().Lines;
                    var startLine = errorDiagnostic.Location.GetLineSpan().StartLinePosition.Line;
                    var endLine = errorDiagnostic.Location.GetLineSpan().EndLinePosition.Line;

                    if (startLine == endLine)
                    {
                        stringBuilder.Append("Line: ").AppendLine(textLines[startLine].ToString().TrimStart());
                    }
                    else
                    {
                        stringBuilder.AppendLine("Lines:");
                        for (var i = startLine; i <= endLine; i++)
                        {
                            stringBuilder.AppendLine(textLines[i].ToString());
                        }
                    }
                }

                throw new InvalidOperationException("Compilation failed:" + stringBuilder);
            }

            using var memoryStream = new MemoryStream();
            var emitResult = compilation.Emit(memoryStream);
            memoryStream.Position = 0;

            errorDiagnostics = emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errorDiagnostics.Count > 0)
            {
                throw new InvalidOperationException(
                    "Compilation emit failed:" + Environment.NewLine + string.Join(Environment.NewLine, errorDiagnostics));
            }

            var assemblyLoadContext = new AssemblyLoadContext(assemblyLoadContextName, isCollectible: true);
            var assembly = assemblyLoadContext.LoadFromStream(memoryStream);
            return (assemblyLoadContext, assembly);
        }
    }

    private static int CountOccurrences(string s, string substring)
        => s.Split(substring).Length - 1;
}
