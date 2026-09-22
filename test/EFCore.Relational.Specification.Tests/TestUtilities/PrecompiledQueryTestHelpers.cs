// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public abstract class PrecompiledQueryTestHelpers
{
    private readonly MetadataReference[] _metadataReferences;

    protected PrecompiledQueryTestHelpers()
        => _metadataReferences = BuildMetadataReferences().ToArray();

    public Task Test(
        string sourceCode,
        DbContextOptions dbContextOptions,
        Type dbContextType,
        Action<string>? interceptorCodeAsserter,
        Action<List<PrecompiledQueryCodeGenerator.QueryPrecompilationError>>? errorAsserter,
        ITestOutputHelper testOutputHelper,
        bool alwaysPrintGeneratedSources,
        string callerName)
    {
        var source = $$"""
public static class TestContainer
{
    public static async Task Test(DbContextOptions dbContextOptions)
    {
{{sourceCode}}
    }
}
""";
        return FullSourceTest(
            source, dbContextOptions, dbContextType, interceptorCodeAsserter, errorAsserter, testOutputHelper, alwaysPrintGeneratedSources,
            callerName);
    }

    public async Task FullSourceTest(
        string sourceCode,
        DbContextOptions dbContextOptions,
        Type dbContextType,
        Action<string>? interceptorCodeAsserter,
        Action<List<PrecompiledQueryCodeGenerator.QueryPrecompilationError>>? errorAsserter,
        ITestOutputHelper testOutputHelper,
        bool alwaysPrintGeneratedSources,
        string callerName)
    {
        // The overall end-to-end testing for precompiled queries is as follows:
        // 1. Compile the user code, produce an assembly from it and load it. We need to do this since precompiled query generation requires
        //    an actual DbContext instance, from which we get the model, services, ec.
        // 2. Do precompiled query generation. This outputs additional source files (syntax trees) containing interceptors for the located
        //    EF LINQ queries.
        // 3. Integrate the additional syntax trees into the compilation, and again, produce an assembly from it and load it.
        // 4. Use reflection to find the EntryPoint (Main method) on this assembly, and invoke it.
        var source = $"""
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using static Microsoft.EntityFrameworkCore.Query.PrecompiledQueryRelationalTestBase;

{sourceCode}
""";

        // This turns on the interceptors feature for the designated namespace(s).
        var parseOptions = new CSharpParseOptions().WithFeatures(
        [
            new KeyValuePair<string, string>("InterceptorsNamespaces", "Microsoft.EntityFrameworkCore.GeneratedInterceptors")
        ]);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions, path: "Test.cs");

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            syntaxTrees: [syntaxTree],
            _metadataReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        IReadOnlyList<ScaffoldedFile>? generatedFiles = null;

        try
        {
            // The test code compiled - emit and assembly and load it.
            var (assemblyLoadContext, assembly) = EmitAndLoadAssembly(compilation, callerName + "_Original");
            try
            {
                var workspace = new AdhocWorkspace();
                var syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.CSharp);

                // TODO: Look up as regular dependencies
                var precompiledQueryCodeGenerator = new PrecompiledQueryCodeGenerator();

                await using var dbContext = (DbContext)Activator.CreateInstance(dbContextType, args: [dbContextOptions])!;

                // Perform precompilation
                var precompilationErrors = new List<PrecompiledQueryCodeGenerator.QueryPrecompilationError>();
                generatedFiles = precompiledQueryCodeGenerator.GeneratePrecompiledQueries(
                    compilation, syntaxGenerator, dbContext, memberAccessReplacements: new Dictionary<MemberInfo, QualifiedName>(),
                    precompilationErrors, new HashSet<string>(), additionalAssembly: assembly);

                if (errorAsserter is null)
                {
                    if (precompilationErrors.Count > 0)
                    {
                        Assert.Fail("Precompilation error: " + precompilationErrors[0].Exception);
                    }
                }
                else
                {
                    errorAsserter(precompilationErrors);
                    return;
                }

                interceptorCodeAsserter?.Invoke(generatedFiles.Single().Code);
            }
            finally
            {
                assemblyLoadContext.Unload();
            }

            // We now have the code-generated interceptors; add them to the compilation and re-emit.
            compilation = compilation.AddSyntaxTrees(
                generatedFiles.Select(f => CSharpSyntaxTree.ParseText(f.Code, parseOptions, f.Path)));

            // We have the final compilation, including the interceptors. Emit and load it, and then invoke its entry point, which contains
            // the original test code with the EF LINQ query, etc.
            (assemblyLoadContext, assembly) = EmitAndLoadAssembly(compilation, callerName + "_WithInterceptors");
            try
            {
                await using var dbContext = (DbContext)Activator.CreateInstance(dbContextType, dbContextOptions)!;

                var testContainer = assembly.ExportedTypes.Single(t => t.Name == "TestContainer");
                var testMethod = testContainer.GetMethod("Test")!;
                await (Task)testMethod.Invoke(obj: null, parameters: [dbContextOptions])!;
            }
            finally
            {
                assemblyLoadContext.Unload();
            }
        }
        catch
        {
            PrintGeneratedSources();

            throw;
        }

        if (alwaysPrintGeneratedSources)
        {
            PrintGeneratedSources();
        }

        void PrintGeneratedSources()
        {
            if (generatedFiles is not null)
            {
                foreach (var generatedFile in generatedFiles)
                {
                    testOutputHelper.WriteLine($"Generated file {generatedFile.Path}: ");
                    testOutputHelper.WriteLine("");
                    testOutputHelper.WriteLine(generatedFile.Code);
                }
            }
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

    protected virtual IEnumerable<MetadataReference> BuildMetadataReferences()
    {
        var netAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        return new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Queryable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IQueryable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Regex).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JsonSerializer).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JavaScriptEncoder).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DatabaseGeneratedAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DbContext).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(RelationalOptionsExtension).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DbConnection).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IListSource).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IMemoryCache).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Assert).Assembly.Location),
                // This is to allow referencing types from this file, e.g. NonCompilingQueryCompiler
                MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location),
                MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "System.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "System.Collections.dll"))
            }
            .Concat(BuildProviderMetadataReferences());
    }

    protected abstract IEnumerable<MetadataReference> BuildProviderMetadataReferences();

    // Used from inside the tested code to ensure that we never end up compiling queries at runtime.
    // TODO: Probably remove this later, once we have a regular mechanism for failing non-intercepted queries at runtime.
    // ReSharper disable once UnusedMember.Global
    public class NonCompilingQueryCompiler(
        IQueryContextFactory queryContextFactory,
        ICompiledQueryCache compiledQueryCache,
        ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator,
        IDatabase database,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger,
        ICurrentDbContext currentContext,
        IEvaluatableExpressionFilter evaluatableExpressionFilter,
        IModel model)
        : QueryCompiler(
            queryContextFactory, compiledQueryCache, compiledQueryCacheKeyGenerator, database, logger,
            currentContext, evaluatableExpressionFilter, model)
    {
        public const string ErrorMessage =
            "A query reached the query compilation pipeline, indicating that it was not intercepted as a precompiled query.";

        public override TResult Execute<TResult>(Expression query)
        {
            Assert.Fail(ErrorMessage);
            throw new UnreachableException();
        }

        public override TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken = default)
        {
            Assert.Fail(ErrorMessage);
            throw new UnreachableException();
        }
    }
}
