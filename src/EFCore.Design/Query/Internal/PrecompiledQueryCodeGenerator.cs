// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

#pragma warning disable CS8602 // TODO

namespace Microsoft.EntityFrameworkCore.Query.Internal;

// TODO: Should extend ILanguageBasedService, go through IQueryCodeGeneratorSelector

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class PrecompiledQueryCodeGenerator : IPrecompiledQueryCodeGenerator
{
    private readonly IQueryLocator _queryLocator;
    private readonly ICSharpToLinqTranslator _csharpToLinqTranslator;
    private readonly ISqlTreeQuoter _sqlTreeQuoter;

    private const string OutputFileName = "EFPrecompiledQueryBootstrapper.cs";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public PrecompiledQueryCodeGenerator(
        IQueryLocator queryLocator,
        ICSharpToLinqTranslator csharpToLinqTranslator,
        ISqlTreeQuoter sqlTreeQuoter)
    {
        _queryLocator = queryLocator;
        _csharpToLinqTranslator = csharpToLinqTranslator;
        _sqlTreeQuoter = sqlTreeQuoter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public async Task GeneratePrecompiledQueries(string projectDir, DbContext context, string outputDir, CancellationToken cancellationToken = default)
    {
        // https://gist.github.com/DustinCampbell/32cd69d04ea1c08a16ae5c4cd21dd3a3
        MSBuildLocator.RegisterDefaults();

        Console.Error.WriteLine("Loading project...");
        using var workspace = MSBuildWorkspace.Create();

        var project = await workspace.OpenProjectAsync(projectDir, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        Console.WriteLine("Compiling project...");
        var compilation = await project.GetCompilationAsync(cancellationToken)
            .ConfigureAwait(false);

        var errorDiagnostics = compilation.GetDiagnostics(cancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errorDiagnostics.Any())
        {
            Console.Error.WriteLine("Compilation failed with errors:");
            Console.Error.WriteLine();
            foreach (var diagnostic in errorDiagnostics)
            {
                Console.WriteLine(diagnostic);
            }
            Environment.Exit(1);
        }

        Console.WriteLine($"Compiled assembly {compilation.Assembly.Name}");

        // TODO: check reference to EF, bail early if not found?
        _queryLocator.LoadCompilation(compilation);

        Console.WriteLine("Locating EF LINQ queries...");

        // TODO: Ignore our auto-generated code! Also compiled model... Recognize [CompilerGenerated]?
        foreach (var syntaxTree in compilation.SyntaxTrees
                     .Where(t => t.FilePath.Split(Path.DirectorySeparatorChar)[^1] != OutputFileName))
        {
            var newSyntaxTree = _queryLocator.LocateQueries(syntaxTree);
            if (!ReferenceEquals(newSyntaxTree, syntaxTree))
            {
                compilation = compilation.ReplaceSyntaxTree(syntaxTree, newSyntaxTree);
            }
        }

        Console.WriteLine("Generating precompiled queries...");
        _csharpToLinqTranslator.Load(compilation, context);

        var g = SyntaxGenerator.GetGenerator(project);
        var linqToCSharpTranslator = new LinqToCSharpTranslator(g);
        var liftableConstantProcessor = new LiftableConstantProcessor(null!);

        var queryCompiler = context.GetService<IQueryCompiler>();
        var queryCacheKeyGenerator = context.GetService<ICompiledQueryCacheKeyGenerator>();
        var materializerLiftableConstantContext =
            Expression.Parameter(typeof(RelationalMaterializerLiftableConstantContext), "materializerLiftableConstantContext");

        var sqlExpressionPrinter = new ExpressionPrinter();
        var stringBuilder = new StringBuilder();

        var namespaces = new HashSet<string>();

        var bootstrapBlock = Block(
            AddLocalVariable("model", "context.Model"),
            AddLocalVariable("dbSetToQueryRootReplacer", "new DbSetToQueryRootReplacer(model)"));

        var variableNames = new HashSet<string> { "model", "dbSetToQueryRootReplacer" };

        var queriesPrecompiled = 0;

        foreach (var syntaxTree in _queryLocator.SyntaxTreesWithQueryCandidates)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            foreach (var querySyntax in (await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false))
                         .GetAnnotatedNodes(IQueryLocator.EfQueryCandidateAnnotationKind))
            {
                Console.WriteLine("*** " + querySyntax);

                var async =
                    querySyntax.GetAnnotations(IQueryLocator.EfQueryCandidateAnnotationKind).Single().Data switch
                    {
                        "Async" => true,
                        "Sync" => false,
                        _ => throw new InvalidOperationException(
                            $"Invalid data for syntax annotation {IQueryLocator.EfQueryCandidateAnnotationKind}")
                    };

                // Convert the query's Roslyn syntax tree into a LINQ expression tree, and compile the query via EF's query pipeline.
                // This returns:
                // 1. The query after parameter extraction (this will become the cache key)
                // 2. The query's executor function, which can produce an enumerable that invokes the query.
                var (queryAfterParameterExtraction, queryExecutorExpression) = CompileQuery(querySyntax, semanticModel, async);

                var queryBlock = Block();

                queryBlock = queryBlock.AddStatements(
                    GenerateQueryCacheKey(queryAfterParameterExtraction, namespaces, async));

                var executorFactoryCode = GenerateExecutorFactory(queryExecutorExpression, namespaces, variableNames);

                // factories[cacheKey] = context => { ... }
                queryBlock = queryBlock.AddStatements(
                    (StatementSyntax)g.ExpressionStatement(
                        g.AssignmentStatement(
                            g.ElementAccessExpression(g.IdentifierName("factories"), g.IdentifierName("cacheKey")),
                            executorFactoryCode)));

                queryBlock = queryBlock.WithLeadingTrivia(
                    Comment(
                        $"// Query from {syntaxTree.FilePath}:{syntaxTree.GetLineSpan(querySyntax.Span, cancellationToken).StartLinePosition}"));

                bootstrapBlock = bootstrapBlock.AddStatements(queryBlock);
                queriesPrecompiled++;
            }
        }

        if (queriesPrecompiled == 0)
        {
            Console.WriteLine("Query precompilation complete, no queries processed.");
            return;
        }

        bootstrapBlock = bootstrapBlock.AddStatements(
            ParseStatement(@"Console.WriteLine(""Bootstrapped EF precompiled queries"");"));

        var usingDirectives = List(namespaces
            // In addition to the namespaces auto-detected by LinqToCSharpTranslator, we manually add these namespaces which are required
            // by manually generated code above.
            .Append("System")
            .Append("System.Collections.Concurrent")
            .Append("System.Linq.Expressions")
            .Append("System.Runtime.CompilerServices")
            .Append("System.Reflection")
            .Append("System.Collections.Generic")
            .Append("Microsoft.EntityFrameworkCore")
            .Append("Microsoft.EntityFrameworkCore.ChangeTracking.Internal")
            .Append("Microsoft.EntityFrameworkCore.Diagnostics")
            .Append("Microsoft.EntityFrameworkCore.Infrastructure")
            .Append("Microsoft.EntityFrameworkCore.Infrastructure.Internal")
            .Append("Microsoft.EntityFrameworkCore.Metadata")
            .Append("Microsoft.Extensions.Caching.Memory")
            .OrderBy(
                ns => ns switch
                {
                    _ when ns.StartsWith("System.", StringComparison.Ordinal) => 10,
                    _ when ns.StartsWith("Microsoft.", StringComparison.Ordinal) => 9,
                    _ => 0
                })
            .ThenBy(ns => ns)
            .Select(g.NamespaceImportDeclaration));

        var dbSetMethodField =
            ParseMemberDeclaration(
                @"private static readonly MethodInfo DbSetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Array.Empty<Type>())!;")!;

        var bootstrapMethod =
            ((MethodDeclarationSyntax)ParseMemberDeclaration(
                "public static void Bootstrap(DbContext context, ConcurrentDictionary<object, Func<DbContext, Delegate>> factories) {}")!)
            .WithBody(bootstrapBlock);

        var registerMethod =
            ((MethodDeclarationSyntax)ParseMemberDeclaration(
$"""
[ModuleInitializer]
public static void Register()
    => PrecompiledQueryFactoryRegistry.RegisterBootstrapper(typeof({context.GetType().Name}), Bootstrap);
""")!);

        var dbSetToQueryRootReplacerClass =
            ParseMemberDeclaration(
"""
class DbSetToQueryRootReplacer : ExpressionVisitor
{
    private readonly IModel _model;

    public DbSetToQueryRootReplacer(IModel model)
        => _model = model;

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // TODO: SQL query
        // TODO: STET
        var method = node.Method;
        if (method.IsGenericMethod && method.GetGenericMethodDefinition() == DbSetMethod)
        {
            var entityType = _model.FindEntityType(method.GetGenericArguments()[0])!;
            return new EntityQueryRootExpression(entityType);
        }

        return base.VisitMethodCall(node);
    }
}
""")!;

        var bootstrapperSyntaxRoot = CompilationUnit()
            .WithUsings(usingDirectives)
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    ClassDeclaration("EFPrecompiledQueryBootstrapper")
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                        .WithMembers(
                            List(
                                new[]
                                {
                                    dbSetMethodField,
                                    bootstrapMethod,
                                    registerMethod,
                                    dbSetToQueryRootReplacerClass
                                }))
                        // TODO: Enable nullable reference types by inspecting Roslyn symbols of corresponding LINQ expression methods etc.
                        .WithLeadingTrivia(Trivia(NullableDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)))));

        // var document = project.AddDocument(OutputFileName, bootstrapperSyntaxRoot);

        var bootstrapperText = bootstrapperSyntaxRoot.NormalizeWhitespace().ToFullString();
        // var outputFilePath = Path.Combine(outputDir, OutputFileName);
        // File.WriteAllText(outputFilePath, bootstrapperText);

        var document = project.AddDocument(OutputFileName, bootstrapperText);

        // document = await ImportAdder.AddImportsAsync(document, options: null, cancellationToken).ConfigureAwait(false);
        // document = await ImportAdder.AddImportsFromSymbolAnnotationAsync(
        //     document, Simplifier.AddImportsAnnotation, cancellationToken: cancellationToken).ConfigureAwait(false);

        // document = await ImportAdder.AddImportsAsync(document, options: null, cancellationToken).ConfigureAwait(false);

        // Run the simplifier to e.g. get rid of unneeded parentheses
        var syntaxRootFoo = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
        var annotatedDocument = document.WithSyntaxRoot(syntaxRootFoo.WithAdditionalAnnotations(Simplifier.Annotation));
        document = await Simplifier.ReduceAsync(annotatedDocument, optionSet: null, cancellationToken).ConfigureAwait(false);

        // format any node with explicit formatter annotation
        // document = await Formatter.FormatAsync(document, Formatter.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);

        // format any elastic whitespace
        // document = await Formatter.FormatAsync(document, SyntaxAnnotation.ElasticAnnotation, cancellationToken: cancellationToken).ConfigureAwait(false);

        document = await Formatter.FormatAsync(document, options: null, cancellationToken).ConfigureAwait(false);

        // document = await CaseCorrector.CaseCorrectAsync(document, CaseCorrector.Annotation, cancellationToken).ConfigureAwait(false);


        var outputFilePath = Path.Combine(outputDir, OutputFileName);
        var finalSyntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
        var finalText = await finalSyntaxTree.GetTextAsync(cancellationToken).ConfigureAwait(false);
        File.WriteAllText(outputFilePath, finalText.ToString());

        // TODO: This is nicer - it adds the file to the project, but also adds a <Compile> node in the csproj for some reason.
        // var applied = workspace.TryApplyChanges(document.Project.Solution);
        // if (!applied)
        // {
        //     Console.WriteLine("Failed to apply changes to project");
        // }

        Console.WriteLine($"Query precompilation complete, processed {queriesPrecompiled} queries.");

        (Expression QueryAfterParameterExtraction, Expression QueryExecutorExpression) CompileQuery(
            SyntaxNode querySyntax,
            SemanticModel semanticModel,
            bool async)
        {
            // We have a query lambda, as a Roslyn syntax tree. Translate to LINQ expression tree.
            // TODO: Add verification that this is an EF query over our user's context. If translation returns null the moment
            // there's another query root (another context or another LINQ provider), that's fine.
            var linqExpression = _csharpToLinqTranslator.Translate(querySyntax, semanticModel);

            // We have the query as a LINQ expression tree.

            // We now need to figure out the return type of the query's executor.
            // Non-scalar query expressions return an IQueryable; the query executor will return an enumerable (sync or async).
            // Scalar query expressions just return the scalar type, wrap that in a Task for async.
            var returnType = linqExpression.Type;
            if (returnType.IsGenericType
                && returnType.GetGenericTypeDefinition().IsAssignableTo(typeof(IQueryable)))
            {
                returnType = (async ? typeof(IAsyncEnumerable<>) : typeof(IEnumerable<>))
                    .MakeGenericType(returnType.GetGenericArguments()[0]);
            }
            else if (async)
            {
                returnType = typeof(Task<>).MakeGenericType(returnType);
            }

            // Compile the query, invoking CompileQueryToExpression on the IQueryCompiler from the user's context instance.
            var resultTuple = (ITuple)queryCompiler.GetType()
                .GetMethod(nameof(IQueryCompiler.CompileQueryToExpression))
                .MakeGenericMethod(returnType)
                .Invoke(queryCompiler, new object[] { linqExpression, async })!;

            return ((Expression)resultTuple[0]!, (Expression)resultTuple[1]!);
        }

        StatementSyntax[] GenerateQueryCacheKey(Expression queryAfterParameterExtraction, HashSet<string> namespaces, bool async)
        {
            // We need to translate the query expression itself (after parameter extraction) to a Roslyn syntax tree as
            // well - we'll be outputting that as the key for the compiled query cache.
            // Note that this is different from the original query tree, since ParameterExtractingExpressionVisitor transforms it in various
            // ways (e.g. captured variables are converted to parameters).
            var querySyntaxTree = linqToCSharpTranslator.TranslateExpression(queryAfterParameterExtraction, namespaces);

            // Wrap the query in a lambda accepting all the detected captured variables as lambda parameters. This ensures that the tree
            // that will come out at runtime is identical to the one we have here - any difference will cause a cache miss.
            var capturedVariables = linqToCSharpTranslator.CapturedVariables;
            var lambdaParameters = new ParameterSyntax[capturedVariables.Count + 1];
            lambdaParameters[0] = Parameter(Identifier("context")).WithType(IdentifierName("DbContext"));
            var i = 1;
            foreach (var capturedVariable in capturedVariables)
            {
                if (capturedVariable.Name is null)
                {
                    throw new NotSupportedException("Unnamed parameter in query syntax tree");
                }

                lambdaParameters[i++] = Parameter(Identifier(capturedVariable.Name))
                    .WithType(capturedVariable.Type.GetTypeSyntax());
            }

            var lambdaWrappedQuerySyntaxTree =
                ParenthesizedLambdaExpression(
                        parameterList: ParameterList(SeparatedList(lambdaParameters)),
                        body: (CSharpSyntaxNode)querySyntaxTree)
                    .WithModifiers(TokenList(Token(SyntaxKind.StaticKeyword)));

            var queryCacheKeyExpression =
                queryCacheKeyGenerator.GenerateCacheKeyExpression(
                    Expression.Parameter(typeof(Expression), "query"),
                    Expression.Parameter(typeof(IModel), "model"),
                    async);

            // And translate the query cache key to a Roslyn syntax tree, replacing linqExpression with our already-translated
            // query syntax tree
            var queryCacheKeySyntaxTree =
                (ExpressionSyntax)linqToCSharpTranslator.TranslateExpression(queryCacheKeyExpression, namespaces);

            // We have a lambda wrapping the query, but its body is what's needed for the query cache key.
            // Also, the expression tree contains context.Set<TEntity>(), but we need an EntityRootQueryExpression; replace.

            // Expression query = (DbContext context, int __p_0) => context.Blogs.Where(...);
            // query = dbSetToQueryRootReplacer.Visit(((LambdaExpression)query).Body);
            // var cacheKey = new RelationalCompiledQueryCacheKey(...);
            return new[]
            {
                (StatementSyntax)g.LocalDeclarationStatement(type: IdentifierName("Expression"), "query", lambdaWrappedQuerySyntaxTree),
                ParseStatement("query = dbSetToQueryRootReplacer.Visit(((LambdaExpression)query).Body);"),
                (StatementSyntax)g.LocalDeclarationStatement("cacheKey", queryCacheKeySyntaxTree)
            };
        }

        SyntaxNode GenerateExecutorFactory(Expression queryExecutorExpression, HashSet<string> namespaces, HashSet<string> variableNames)
        {
            var statements = new List<StatementSyntax>();

            statements.AddRange(new[] {
                AddLocalVariable("relationalModel", "model.GetRelationalModel()"),
                AddLocalVariable("relationalTypeMappingSource", "context.GetService<IRelationalTypeMappingSource>()"),
                AddLocalVariable("materializerLiftableConstantContext",
"""
new RelationalMaterializerLiftableConstantContext(
    context.GetService<ShapedQueryCompilingExpressionVisitorDependencies>(),
    context.GetService<RelationalShapedQueryCompilingExpressionVisitorDependencies>())
""") });
            variableNames.UnionWith(new[] { "relationalModel", "relationalTypeMappingSource", "materializerLiftableConstantContext" });

            // The materializer expression tree contains LiftedConstantExpression nodes, which contain instructions on how to resolve
            // constant values which need to be lifted.
            var queryExecutorAfterLiftingExpression =
                liftableConstantProcessor.LiftConstants(queryExecutorExpression, materializerLiftableConstantContext, variableNames);

            var sqlTreeCounter = 0;

            foreach (var liftedConstant in liftableConstantProcessor.LiftedConstants)
            {
                var (parameter, variableValue) = liftedConstant;

                // TODO: Somewhat hacky, special handling for the SQL tree argument of RelationalCommandCache (since it requires
                // very special rendering logic
                if (parameter.Type == typeof(RelationalCommandCache))
                {
                    var sqlTreeVariable = "sqlTree" + (++sqlTreeCounter);

                    if (variableValue is NewExpression newRelationalCommandCacheExpression
                        && newRelationalCommandCacheExpression.Arguments.FirstOrDefault(a => a.Type == typeof(Expression)) is
                            ConstantExpression { Value: Expression queryExpression })
                    {
                        // Render out the SQL tree, preceded by an ExpressionPrinter dump of it in a comment for easier debugging.
                        // Note that since the SQL tree is a graph (columns reference their SelectExpression's tables), rendering happens
                        // in multiple statements.
                        var sqlTreeBlock = _sqlTreeQuoter.Quote(queryExpression, sqlTreeVariable, variableNames);
                        var sqlTreeSyntaxStatements =
                            ((BlockSyntax)linqToCSharpTranslator.TranslateStatement(sqlTreeBlock, namespaces)).Statements
                            .ToArray();
                        sqlTreeSyntaxStatements[0] = sqlTreeSyntaxStatements[0].WithLeadingTrivia(
                            Comment(
                                stringBuilder
                                    .Clear()
                                    .AppendLine("/*")
                                    .AppendLine(sqlExpressionPrinter.PrintExpression(queryExpression))
                                    .AppendLine("*/")
                                    .ToString()));

                        statements.AddRange(sqlTreeSyntaxStatements);

                        // We've rendered the SQL tree, assigning it to variable "sqlTree". Update the RelationalCommandCache to point
                        // to it
                        variableValue = newRelationalCommandCacheExpression.Update(newRelationalCommandCacheExpression.Arguments
                            .Select(a => a.Type == typeof(Expression)
                                ? Expression.Parameter(typeof(Expression), sqlTreeVariable)
                                : a));
                    }
                    else
                    {
                        throw new InvalidOperationException($"Could not find SQL query in lifted {nameof(RelationalCommandCache)}");
                    }
                }

                statements.Add(
                    (StatementSyntax)g.LocalDeclarationStatement(
                        parameter.Name!, linqToCSharpTranslator.TranslateExpression(variableValue, namespaces)));
            }

            // We compiled the query and now have an expression tree for invoking it. Translate that to a Roslyn syntax tree
            // for outputting as C# code.
            var queryExecutorSyntaxTree =
                (AnonymousFunctionExpressionSyntax)linqToCSharpTranslator.TranslateExpression(queryExecutorAfterLiftingExpression,
                    namespaces);

            // var executor = (QueryContext queryContext) => SingleQueryingEnumerable.Create(...)
            statements.Add(
                (StatementSyntax)g.ReturnStatement(
                    ParenthesizedLambdaExpression(
                        ParameterList(
                            SingletonSeparatedList(
                                Parameter(Identifier("queryContext")).WithType(IdentifierName("QueryContext")))),
                        // The original executor expression was a lambda accepting the QueryContext, which is fine.
                        // But when rendering to C#, we need a typed lambda parameter ((QueryContext queryContext) =>).
                        body: queryExecutorSyntaxTree.Body)));

            return g.ValueReturningLambdaExpression("context", statements);
        }

        StatementSyntax AddLocalVariable(string name, string initializerExpression)
            => (StatementSyntax)g.LocalDeclarationStatement(name, ParseExpression(initializerExpression));
    }
}
