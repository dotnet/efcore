// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.ExceptionServices;
using System.Text;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Simplification;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class PrecompiledQueryCodeGenerator : IPrecompiledQueryCodeGenerator
{
    private readonly QueryLocator _queryLocator;
    private readonly CSharpToLinqTranslator _csharpToLinqTranslator;

    private SyntaxGenerator _g = null!;
    private IQueryCompiler _queryCompiler = null!;
    private ExpressionTreeFuncletizer _funcletizer = null!;
    private LinqToCSharpSyntaxTranslator _linqToCSharpTranslator = null!;
    private LiftableConstantProcessor _liftableConstantProcessor = null!;

    private Symbols _symbols;

    private readonly HashSet<string> _namespaces = new();
    private readonly HashSet<MethodDeclarationSyntax> _unsafeAccessors = new();
    private readonly ExpressionPrinter _sqlExpressionPrinter = new();
    private readonly IndentedStringBuilder _code = new();
    private static readonly ShaperPublicMethodVerifier ShaperPublicMethodVerifier = new();

    private const string InterceptorsNamespace = "Microsoft.EntityFrameworkCore.GeneratedInterceptors";
    private const string OutputFileName = "EFPrecompiledQueryBootstrapper.cs";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public PrecompiledQueryCodeGenerator()
    {
        _queryLocator = new QueryLocator();
        _csharpToLinqTranslator = new CSharpToLinqTranslator();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public async Task GeneratePrecompiledQueries(
        string projectFilePath,
        DbContext dbContext,
        string outputDir,
        List<QueryPrecompilationError> precompilationErrors,
        CancellationToken cancellationToken = default)
    {
        // https://gist.github.com/DustinCampbell/32cd69d04ea1c08a16ae5c4cd21dd3a3
        MSBuildLocator.RegisterDefaults();

        Console.Error.WriteLine("Loading project...");
        using var workspace = MSBuildWorkspace.Create();

        var project = await workspace.OpenProjectAsync(projectFilePath, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!project.SupportsCompilation)
        {
            throw new NotSupportedException("The project does not support compilation");
        }

        Console.WriteLine("Compiling project...");
        var compilation = (await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false))!;

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

        var syntaxGenerator = SyntaxGenerator.GetGenerator(project);

        var generatedFiles = GeneratePrecompiledQueries(
            compilation, syntaxGenerator, dbContext, precompilationErrors, additionalAssembly: null, cancellationToken);

        foreach (var generatedFile in generatedFiles)
        {
            // var document = project.AddDocument(OutputFileName, bootstrapperSyntaxRoot);

            // var generatedSource = (await generatedSyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false))
                // .ToFullString();
            // var outputFilePath = Path.Combine(outputDir, OutputFileName);
            // File.WriteAllText(outputFilePath, bootstrapperText);

            var document = project.AddDocument(OutputFileName, generatedFile.Code);

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
            var finalSyntaxTree = (await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false))!;
            var finalText = await finalSyntaxTree.GetTextAsync(cancellationToken).ConfigureAwait(false);
            File.WriteAllText(outputFilePath, finalText.ToString());

            // TODO: This is nicer - it adds the file to the project, but also adds a <Compile> node in the csproj for some reason.
            // var applied = workspace.TryApplyChanges(document.Project.Solution);
            // if (!applied)
            // {
            //     Console.WriteLine("Failed to apply changes to project");
            // }
        }

        // Console.WriteLine($"Query precompilation complete, processed {queriesPrecompiled} queries.");
        Console.WriteLine("Query precompilation complete.");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<GeneratedInterceptorFile> GeneratePrecompiledQueries(
        Compilation compilation,
        SyntaxGenerator syntaxGenerator,
        DbContext dbContext,
        List<QueryPrecompilationError> precompilationErrors,
        Assembly? additionalAssembly = null,
        CancellationToken cancellationToken = default)
    {
        _queryLocator.LoadCompilation(compilation);
        _symbols = Symbols.Load(compilation);
        _g = syntaxGenerator;
        _linqToCSharpTranslator = new LinqToCSharpSyntaxTranslator(_g);
        _liftableConstantProcessor = new LiftableConstantProcessor(null!);
        _queryCompiler = dbContext.GetService<IQueryCompiler>();
        _unsafeAccessors.Clear();
        _funcletizer = new ExpressionTreeFuncletizer(
            dbContext.Model,
            dbContext.GetService<IEvaluatableExpressionFilter>(),
            dbContext.GetType(),
            generateContextAccessors: false,
            dbContext.GetService<IDiagnosticsLogger<DbLoggerCategory.Query>>());

        // This must be done after we complete generating the final compilation above
        _csharpToLinqTranslator.Load(compilation, dbContext, additionalAssembly);

        // TODO: Ignore our auto-generated code! Also compiled model, generated code (comment, filename...?).
        var generatedSyntaxTrees = new List<GeneratedInterceptorFile>();
        foreach (var syntaxTree in compilation.SyntaxTrees
                     .Where(t => t.FilePath.Split(Path.DirectorySeparatorChar)[^1] != OutputFileName))
        {
            if (_queryLocator.LocateQueries(syntaxTree) is not { Count: > 0 } locatedQueries)
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var generatedSyntaxTree = ProcessSyntaxTreeAsync(
                syntaxTree, semanticModel, locatedQueries, precompilationErrors, cancellationToken);
            if (generatedSyntaxTree is not null)
            {
                generatedSyntaxTrees.Add(generatedSyntaxTree);
            }
        }

        return generatedSyntaxTrees;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual GeneratedInterceptorFile? ProcessSyntaxTreeAsync(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        IReadOnlyList<InvocationExpressionSyntax> locatedQueries,
        List<QueryPrecompilationError> precompilationErrors,
        CancellationToken cancellationToken)
    {
        var queriesPrecompiledInFile = 0;
        _namespaces.Clear();
        _code.Clear();
        _code
            .AppendLine()
            .Append("namespace ").AppendLine(InterceptorsNamespace)
            .AppendLine("{")
            .IncrementIndent()
            .AppendLine("file static class EntityFrameworkCoreInterceptors")
            .AppendLine("{")
            .IncrementIndent();

        for (var queryNum = 0; queryNum < locatedQueries.Count; queryNum++)
        {
            var querySyntax = locatedQueries[queryNum];

            try
            {
                // We have a query lambda, as a Roslyn syntax tree. Translate to LINQ expression tree.
                // TODO: Add verification that this is an EF query over our user's context. If translation returns null the moment
                // there's another query root (another context or another LINQ provider), that's fine.
                if (_csharpToLinqTranslator.Translate(querySyntax, semanticModel) is not MethodCallExpression queryTree)
                {
                    throw new UnreachableException("Non-method call encountered as the root of a LINQ query");
                }

                // Convert the query's Roslyn syntax tree into a LINQ expression tree, and compile the query via EF's query pipeline.
                // This returns the query's executor function, which can produce an enumerable that invokes the query.
                var queryExecutor = CompileQuery(queryTree);

                // The query has been compiled successfully by the EF query pipeline.
                // Now go over each LINQ operator, generating an interceptor for it.
                _code.AppendLine($"#region Query{queryNum + 1}").AppendLine();

                try
                {
                    ProcessQueryOperator(
                        _code, semanticModel, querySyntax, queryTree, queryNum + 1, operatorNum: out _, isTerminatingOperator: true,
                        cancellationToken, queryExecutor);
                }
                finally
                {
                    _code
                        .AppendLine()
                        .AppendLine($"#endregion Query{queryNum + 1}");
                }
            }
            catch (Exception e)
            {
                precompilationErrors.Add(new(syntaxTree, querySyntax, e));
                continue;
            }

            // We're done generating the interceptors for the query's LINQ operators.

            queriesPrecompiledInFile++;
        }

        if (queriesPrecompiledInFile == 0)
        {
            return null;
        }

        // Output all the unsafe accessors that were generated for all intercepted shapers, e.g.:
        // [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Name>k__BackingField")]
        // static extern ref int GetSet_Foo_Name(Foo f);
        if (_unsafeAccessors.Count > 0)
        {
            _code.AppendLine("#region Unsafe accessors");
            foreach (var unsafeAccessor in _unsafeAccessors)
            {
                _code.AppendLine(unsafeAccessor.NormalizeWhitespace().ToFullString());
            }
            _code.AppendLine("#endregion Unsafe accessors");
        }

        _code
            .DecrementIndent().AppendLine("}")
            .DecrementIndent().AppendLine("}");

        // TODO: Recycle
        var generatedFile = new StringBuilder();
        generatedFile.AppendLine("// <auto-generated />").AppendLine();

        foreach (var ns in _namespaces
                // In addition to the namespaces auto-detected by LinqToCSharpTranslator, we manually add these namespaces which are required
                // by manually generated code above.
                .Append("System")
                .Append("System.Collections.Concurrent")
                .Append("System.Collections.Generic")
                .Append("System.Linq")
                .Append("System.Linq.Expressions")
                .Append("System.Runtime.CompilerServices")
                .Append("System.Reflection")
                .Append("System.Threading.Tasks")
                .Append("Microsoft.EntityFrameworkCore")
                .Append("Microsoft.EntityFrameworkCore.ChangeTracking.Internal")
                .Append("Microsoft.EntityFrameworkCore.Diagnostics")
                .Append("Microsoft.EntityFrameworkCore.Infrastructure")
                .Append("Microsoft.EntityFrameworkCore.Infrastructure.Internal")
                .Append("Microsoft.EntityFrameworkCore.Internal")
                .Append("Microsoft.EntityFrameworkCore.Metadata")
                .Append("Microsoft.EntityFrameworkCore.Query")
                .Append("Microsoft.EntityFrameworkCore.Query.Internal")
                .Append("Microsoft.EntityFrameworkCore.Storage")
                .OrderBy(
                    ns => ns switch
                    {
                        _ when ns.StartsWith("System.", StringComparison.Ordinal) => 10,
                        _ when ns.StartsWith("Microsoft.", StringComparison.Ordinal) => 9,
                        _ => 0
                    })
                .ThenBy(ns => ns))
        {
            generatedFile.Append("using ").Append(ns).AppendLine(";");
        }

        generatedFile.AppendLine(_code.ToString());

        generatedFile.AppendLine(
            """
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    sealed class InterceptsLocationAttribute : Attribute
    {
        public InterceptsLocationAttribute(string filePath, int line, int column) { }
    }
}
""");

        return new(
            $"{Path.GetFileNameWithoutExtension(syntaxTree.FilePath)}.EFInterceptors.g{Path.GetExtension(syntaxTree.FilePath)}"
            ,generatedFile.ToString());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression CompileQuery(MethodCallExpression terminatingOperator)
    {
        // First, check whether this is an async query.
        var async = terminatingOperator.Type.IsGenericType
            && terminatingOperator.Type.GetGenericTypeDefinition() is var genericDefinition
            && (genericDefinition == typeof(Task<>) || genericDefinition == typeof(ValueTask<>));

        // We have a LINQ representation of the query tree as it appears in the user's source code, but this isn't the same as the
        // LINQ tree the EF query pipeline needs to get; the latter is the result of evaluating the queryable operators in the user's
        // source code. For example, in the user's code the root is a DbSet as the root, but the expression tree we require needs to
        // contain an EntityQueryRootExpression. To get the LINQ tree for EF, we need to evaluate the operator chain, building an
        // expression tree as usual.

        // However, we cannot evaluate the last operator, since that would execute the query instead of returning an expression tree.
        // So we need to chop off the last operator before evaluation, and then (optionally) recompose it back afterwards.
        // For ToList(), we don't actually recompose it (since ToList() isn't a node in the expression tree), and for async operators,
        // we need to rewrite them to their sync counterparts (since that's what gets injected into the query tree).
        var penultimateOperator = terminatingOperator switch
        {
            { Object: Expression @object } => @object, // This is needed e.g. for DbSet.AsAsyncEnumerable (non-static operator)
            { Arguments: [var sourceArgument, ..] } => sourceArgument,
            _ => throw new UnreachableException()
        };

        var evaluatedPenultimateOperator = Expression.Lambda<Func<IQueryable>>(penultimateOperator).Compile(preferInterpretation: true)();

        var rewrittenQueryTree = ProcessQueryTerminatingOperator(terminatingOperator, evaluatedPenultimateOperator.Expression);

        // We now need to figure out the return type of the query's executor.
        // Non-scalar query expressions (e.g. ToList()) return an IQueryable; the query executor will return an enumerable (sync or async).
        // Scalar query expressions just return the scalar type.
        var returnType = rewrittenQueryTree.Type.IsGenericType
            && rewrittenQueryTree.Type.GetGenericTypeDefinition().IsAssignableTo(typeof(IQueryable))
                ? (async
                    ? typeof(IAsyncEnumerable<>)
                    : typeof(IEnumerable<>)).MakeGenericType(rewrittenQueryTree.Type.GetGenericArguments()[0])
                : terminatingOperator.Type;

        // We now have the query as a finalized LINQ expression tree, ready for compilation.
        // Compile the query, invoking CompileQueryToExpression on the IQueryCompiler from the user's context instance.
        try
        {
            var queryExecutor = (Expression)_queryCompiler.GetType()
                .GetMethod(nameof(IQueryCompiler.PrecompileQuery))!
                .MakeGenericMethod(returnType)
                .Invoke(_queryCompiler, [rewrittenQueryTree, async])!;

            // TODO: Move this into CompileQueryToExpression
            ShaperPublicMethodVerifier.Visit(queryExecutor);

            return queryExecutor;
        }
        catch (TargetInvocationException e) when (e.InnerException is not null)
        {
            // Unwrap the TargetInvocationException wrapper we get from Invoke()
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private void ProcessQueryOperator(
        IndentedStringBuilder code,
        SemanticModel semanticModel,
        InvocationExpressionSyntax operatorSyntax,
        MethodCallExpression operatorLinq,
        int queryNum,
        out int operatorNum,
        bool isTerminatingOperator,
        CancellationToken cancellationToken,
        Expression? queryExecutor = null)
    {
        var memberAccess = (MemberAccessExpressionSyntax)operatorSyntax.Expression;

        // TODO: Also need to detect DbContext.Set<T>() invocation
        InvocationExpressionSyntax? nestedOperatorSyntax = null;
        if (memberAccess.Expression is InvocationExpressionSyntax tempNestedOperatorSyntax)
        {
            if (operatorLinq.Arguments[0] is not MethodCallExpression nestedOperatorLinq)
            {
                throw new UnreachableException(
                    $"Encountered non-MethodCallExpression node '{operatorLinq.Arguments[0].GetType().Name}' although the corresponding syntax node was an invocation");
            }

            nestedOperatorSyntax = tempNestedOperatorSyntax;

            // This isn't the first query operator in the chain.
            // First recurse into the nested operator, to generate its interceptor first.
            ProcessQueryOperator(
                code, semanticModel, nestedOperatorSyntax, nestedOperatorLinq, queryNum, out operatorNum,
                isTerminatingOperator: false, cancellationToken);
            operatorNum++;
        }
        else
        {
            operatorNum = 1;
        }

        // Create the parameter list for our interceptor method from the LINQ operator method's parameter list
        if (semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol is not IMethodSymbol operatorSymbol)
        {
            throw new InvalidOperationException("Couldn't find method symbol for: " + memberAccess);
        }

        // Throughout the code generation below, we will only be dealing with the original generic definition of the operator (and
        // generating a generic interceptor); we'll never be dealing with the concrete types for this invocation, since these may
        // be unspeakable anonymous types which we can't embed in generated code.
        operatorSymbol = operatorSymbol.OriginalDefinition;

        // For extension methods, this provides the form which has the "this" as its first parameter.
        // TODO: Validate the below, throw informative (e.g. top-level TVF fails here because non-generic)
        var reducedOperatorSymbol = operatorSymbol.GetConstructedReducedFrom() ?? operatorSymbol;

        var (sourceIdentifier, sourceType) = reducedOperatorSymbol.IsStatic
            ? (_g.IdentifierName(reducedOperatorSymbol.Parameters[0].Name), reducedOperatorSymbol.Parameters[0].Type)
            : (_g.ThisExpression(), reducedOperatorSymbol.ReceiverType!);

        // var sourceParameter = reducedOperatorSymbol.IsStatic ? reducedOperatorSymbol.Parameters[0] : reducedOperatorSymbol.ReceiverType;
        // var sourceParameterIdentifier = _g.IdentifierName(sourceParameter.Name);
        if (sourceType is not INamedTypeSymbol { TypeArguments: [var sourceElementTypeSymbol]})
        {
            throw new UnreachableException($"Non-IQueryable first parameter in LINQ operator '{operatorLinq.Method.Name}'");
        }

        var returnTypeSymbol = reducedOperatorSymbol.ReturnType;

        // Unwrap Task<T> to get the element type (e.g. Task<List<int>>)
        var returnTypeWithoutTask = returnTypeSymbol is INamedTypeSymbol namedReturnType
            && returnTypeSymbol.OriginalDefinition.Equals(_symbols.GenericTask, SymbolEqualityComparer.Default)
                ? namedReturnType.TypeArguments[0]
                : returnTypeSymbol;

        var returnElementTypeSymbol = returnTypeWithoutTask switch
        {
            IArrayTypeSymbol arrayTypeSymbol => arrayTypeSymbol.ElementType,
            INamedTypeSymbol namedReturnType2
                when namedReturnType2.AllInterfaces.Prepend(namedReturnType2)
                    .Any(
                        i => i.OriginalDefinition.Equals(_symbols.GenericEnumerable, SymbolEqualityComparer.Default)
                            || i.OriginalDefinition.Equals(_symbols.GenericAsyncEnumerable, SymbolEqualityComparer.Default))
                => namedReturnType2.TypeArguments[0],
            _ => null
        };

        // Output the interceptor method method signature (and [InterceptsLocation] attribute)
        var startPosition = operatorSyntax.SyntaxTree.GetLineSpan(memberAccess.Name.Span, cancellationToken).StartLinePosition;
        var interceptorName = $"Query{queryNum}_{memberAccess.Name}{operatorNum}";

        code.AppendLine($"""[InterceptsLocation("{operatorSyntax.SyntaxTree.FilePath}", {startPosition.Line + 1}, {startPosition.Character + 1})]""");

        // To create the interceptor method declaration, we copy the method definition of the original intercepted method, replacing the
        // name and nulling out the body (we'll be appending the body statements as strings, not providing them as syntax nodes).
        code.AppendLine(
                ((MethodDeclarationSyntax)_g.WithName(
                    _g.MethodDeclaration(reducedOperatorSymbol, statements: null),
                    interceptorName))
                .WithBody(null)
                .NormalizeWhitespace().ToFullString())
            .AppendLine("{")
            .IncrementIndent();

        // If this is the first query operator (no nested operator), cast the input source to IDbContextContainer and extract the EF
        // service provider, create a new QueryContext, and wrap it all in a PrecompiledQueryContext that will flow through to the
        // terminating operator, where the query will actually get executed.
        // Otherwise, if this is a non-first operator, receive the PrecompiledQueryContext from the nested operator and flow it forward.
        // TODO: These should be strings, not symbols and syntax nodes
        code.AppendLine(
            "var precompiledQueryContext = "
            + (nestedOperatorSyntax is null
                ? $"new PrecompiledQueryContext<{sourceElementTypeSymbol.Name}>(((IDbContextContainer){sourceIdentifier}).DbContext);"
                : $"(PrecompiledQueryContext<{sourceElementTypeSymbol.Name}>){sourceIdentifier};"));

        // Go over the operator's arguments (skipping the first, which is the source).
        // For those which have captured variables, run them through our funcletizer, which will return code for extracting any captured
        // variables from them.
        var declaredQueryContextVariable = false;
        var variableCounter = 0;

        var parameters = operatorLinq.Method.GetParameters();

        for (var i = 1; i < operatorLinq.Arguments.Count; i++)
        {
            if (operatorLinq.Arguments[i].Type == typeof(CancellationToken))
            {
                continue;
            }

            if (_funcletizer.CalculatePathsToEvaluatableRoots(operatorLinq, i) is not ExpressionTreeFuncletizer.PathNode
                evaluatableRootPaths)
            {
                // There are no captured variables in this lambda argument - skip the argument
                continue;
            }

            // We have a lambda argument with captured variables. Use the information returned by the funcletizer to generate code
            // which extracts them and sets them on our query context.
            if (!declaredQueryContextVariable)
            {
                code.AppendLine("var queryContext = precompiledQueryContext.QueryContext;");
                declaredQueryContextVariable = true;
            }

            var parameter = parameters[i];

            if (!parameter.ParameterType.IsSubclassOf(typeof(Expression)))
            {
                // Special case: this is a non-lambda argument (Skip/Take/FromSql).
                // Simply add the argument directly as a parameter
                code.AppendLine($"""queryContext.AddParameter("{evaluatableRootPaths.ParameterName}", {parameter.Name});""");
                continue;
            }

            // Lambda argument. Recurse through evaluatable path trees.
            foreach (var child in evaluatableRootPaths.Children!)
            {
                GenerateCapturedVariableExtractors(parameter.Name!, parameter.ParameterType, child);

                void GenerateCapturedVariableExtractors(string currentIdentifier, Type currentType, ExpressionTreeFuncletizer.PathNode capturedVariablesPathTree)
                {
                    var linqPathSegment = capturedVariablesPathTree.PathFromParent!(Expression.Parameter(currentType, currentIdentifier));
                    var collectedNamespaces = new HashSet<string>();
                    var unsafeAccessors = new HashSet<MethodDeclarationSyntax>();
                    var roslynPathSegment = _linqToCSharpTranslator.TranslateExpression(
                        linqPathSegment, constantReplacements: null, collectedNamespaces, unsafeAccessors);

                    var variableName = capturedVariablesPathTree.ExpressionType.Name;
                    variableName = char.ToLower(variableName[0]) + variableName[1..^"Expression".Length] + ++variableCounter;
                    code.AppendLine($"var {variableName} = ({capturedVariablesPathTree.ExpressionType.Name}){roslynPathSegment};");

                    if (capturedVariablesPathTree.Children?.Count > 0)
                    {
                        // This is an intermediate node which has captured variables in the children. Continue recursing down.
                        foreach (var child in capturedVariablesPathTree.Children)
                        {
                            GenerateCapturedVariableExtractors(variableName, capturedVariablesPathTree.ExpressionType, child);
                        }

                        return;
                    }

                    // We've reached a leaf, meaning that it's an evaluatable node that contains captured variables.
                    // Generate code to evaluate this node and assign the result to the parameters dictionary:

                    // TODO: For the common case of a simple parameter (member access over closure type), generate reflection code directly
                    // TODO: instead of going through the interpreter, as we do in the funcletizer itself (for perf)
                    // TODO: Remove the convert to object. We can flow out the actual type of the evaluatable root, and just stick it
                    //       in Func<> instead of object.
                    // TODO: For specific cases, don't go through the interpreter, but just integrate code that extracts the value directly.
                    //       (see ExpressionTreeFuncletizer.Evaluate()).
                    // TODO: Basically this means that the evaluator should come from ExpressionTreeFuncletizer itself, as part of its outputs
                    // TODO: Integrate try/catch around the evaluation?
                    code.AppendLine("queryContext.AddParameter(");
                    using (code.Indent())
                    {
                        code
                            .Append('"').Append(capturedVariablesPathTree.ParameterName!).AppendLine("\",")
                            .AppendLine($"Expression.Lambda<Func<object?>>(Expression.Convert({variableName}, typeof(object)))")
                            .AppendLine(".Compile(preferInterpretation: true)")
                            .AppendLine(".Invoke());");
                    }
                }
            }
        }

        if (isTerminatingOperator)
        {
            // We're intercepting the query's terminating operator - this is where the query actually gets executed.
            if (!declaredQueryContextVariable)
            {
                code.AppendLine("var queryContext = precompiledQueryContext.QueryContext;");
            }

            var executorFieldIdentifier = $"Query{queryNum}_Executor";
            code.AppendLine(
                $"{executorFieldIdentifier} ??= Query{queryNum}_GenerateExecutor(precompiledQueryContext.DbContext, precompiledQueryContext.QueryContext);");

            if (returnElementTypeSymbol is null)
            {
                // The query returns a scalar, not an enumerable (e.g. the terminating operator is Max()).
                // The executor directly returns the needed result (e.g. int), so just return that.
                var returnType = _g.TypeExpression(returnTypeSymbol).NormalizeWhitespace().ToFullString();
                code.AppendLine($"return ((Func<QueryContext, {returnType}>)({executorFieldIdentifier}))(queryContext);");
            }
            else
            {
                // The query returns an IEnumerable/IAsyncEnumerable/IQueryable, which is a bit trickier: the executor doesn't return a
                // simple value as in the scalar case, but rather e.g. SingleQueryingEnumerable; we need to compose the terminating
                // operator (e.g. ToList()) on top of that. Cast the executor delegate to Func<QueryContext, IEnumerable<T>>
                // (contravariance).
                var isAsync =
                    operatorLinq.Type.IsGenericType
                    && operatorLinq.Type.GetGenericTypeDefinition() is var genericDefinition
                    && (
                        genericDefinition == typeof(Task<>)
                        || genericDefinition == typeof(ValueTask<>)
                        || genericDefinition == typeof(IAsyncEnumerable<>));

                var isQueryable = !isAsync
                        && operatorLinq.Type.IsGenericType
                        && operatorLinq.Type.GetGenericTypeDefinition() == typeof(IQueryable<>);

                var returnValue = isAsync
                    ? $"IAsyncEnumerable<{sourceElementTypeSymbol.Name}>"
                    : $"IEnumerable<{sourceElementTypeSymbol.Name}>";

                code.AppendLine(
                    $"var queryingEnumerable = ((Func<QueryContext, {returnValue}>)({executorFieldIdentifier}))(queryContext);");

                if (isQueryable)
                {
                    // If the terminating operator returns IQueryable<T>, that means the query is actually evaluated via foreach
                    // (i.e. there's no method such as AsEnumerable/ToList which evaluates). Note that this is necessarily sync only -
                    // IQueryable can't be directly inside await foreach (AsAsyncEnumerable() is required).
                    // For this case, we need to compose AsQueryable() on top, to make the querying enumerable compatible with the
                    // operator signature.
                    code.AppendLine("return queryingEnumerable.AsQueryable();");
                }
                else
                {
                    if (isAsync)
                    {
                        // For sync queries, we get an IEnumerable<TSource> above, and can just compose the original terminating operator
                        // directly on top of that (ToList(), ToDictionary()...).
                        // But for async queries, we get an IAsyncEnumerable<TSource> above, but cannot directly compose the original
                        // terminating operator (ToListAsync(), ToDictionaryAsync()...), since those require an IQueryable<T> in their
                        // signature (which they internally case to IAsyncEnumerable<T>).
                        // So we introduce an adapter in the middle, which implements both IQueryable<T> (to be able to compose
                        // ToListAsync() on top), and IAsyncEnumerable<T> (so that the actual implementation of ToListAsync() works).
                        // TODO: This is an additional runtime allocation; if we had System.Linq.Async we wouldn't need this. We could
                        // have additional versions of all async terminating operators over IAsyncEnumerable<T> (effectively duplicating
                        // System.Linq.Async) as an alternative.
                        code.AppendLine($"var asyncQueryingEnumerable = new PrecompiledQueryableAsyncEnumerableAdapter<{sourceElementTypeSymbol}>(queryingEnumerable);");
                        code.Append("return asyncQueryingEnumerable");
                    }
                    else
                    {
                        code.Append("return queryingEnumerable");
                    }

                    // Invoke the original terminating operator (e.g. ToList(), ToDictionary()...) on the querying enumerable, passing
                    // through the interceptor's arguments.
                    code.AppendLine(
                        $".{memberAccess.Name}({string.Join(", ", operatorSymbol.Parameters.Select(p => p.Name))});");
                }
            }
        }
        else
        {
            // Non-terminating operator - we need to flow precompiledQueryContext forward.

            // The operator returns a different IQueryable type as its source (e.g. Select), convert the precompiledQueryContext
            // before returning it.
            Check.DebugAssert(returnElementTypeSymbol is not null, "Non-terminating operator must return IEnumerable<T>");

            code.AppendLine(
                returnTypeSymbol switch
                {
                    // The operator return IQueryable<T> or IOrderedQueryable<T>.
                    // If T is the same as the source, simply return our context as is (note that PrecompiledQueryContext implements
                    // IOrderedQueryable). Otherwise, e.g. Select() is being applied - change the context's type.
                    _ when returnTypeSymbol.OriginalDefinition.Equals(_symbols.IQueryable, SymbolEqualityComparer.Default)
                        || returnTypeSymbol.OriginalDefinition.Equals(_symbols.IOrderedQueryable, SymbolEqualityComparer.Default)
                        => SymbolEqualityComparer.Default.Equals(sourceElementTypeSymbol, returnElementTypeSymbol)
                            ? "return precompiledQueryContext;"
                            : $"return precompiledQueryContext.ToType<{returnElementTypeSymbol}>();",

                    _ when returnTypeSymbol.OriginalDefinition.Equals(_symbols.IIncludableQueryable, SymbolEqualityComparer.Default)
                        && returnTypeSymbol is INamedTypeSymbol { OriginalDefinition.TypeArguments: [_, var includedPropertySymbol] }
                        => $"return precompiledQueryContext.ToIncludable<{includedPropertySymbol}>();",

                    _ => throw new UnreachableException()
                });
        }

        code.DecrementIndent().AppendLine("}").AppendLine();

        // After the terminating operator definition, generate the query's executor generator.
        if (isTerminatingOperator)
        {
            var variableNames = new HashSet<string>(); // TODO
            GenerateQueryExecutor(code, queryNum, queryExecutor!, _namespaces, _unsafeAccessors, variableNames);
        }
    }

    private void GenerateQueryExecutor(
        IndentedStringBuilder code,
        int queryNum,
        Expression queryExecutor,
        HashSet<string> namespaces,
        HashSet<MethodDeclarationSyntax> unsafeAccessors,
        HashSet<string> variableNames)
    {
        // We're going to generate the method which will create the query executor (Func<QueryContext, TResult>).
        // Note that the we store the executor itself (and return it) as object, not as a typed Func<QueryContext, TResult>.
        // We can't strong-type it since it may return an anonymous type, which is unspeakable; so instead we cast down from object to
        // the real strongly-typed signature inside the interceptor, where the return value is represented as a generic type parameter
        // (which can be an anonymous type).
        // TODO: We can use strong types instead of object (and avoid the downcast) for cases where there are no unspeakable types.
        code
            .AppendLine($"private static object Query{queryNum}_GenerateExecutor(DbContext dbContext, QueryContext queryContext)")
            .AppendLine("{")
            .IncrementIndent()
            .AppendLine("var relationalModel = dbContext.Model.GetRelationalModel();")
            .AppendLine("var relationalTypeMappingSource = dbContext.GetService<IRelationalTypeMappingSource>();")
            .AppendLine("var materializerLiftableConstantContext = new RelationalMaterializerLiftableConstantContext(dbContext.GetService<ShapedQueryCompilingExpressionVisitorDependencies>(), dbContext.GetService<RelationalShapedQueryCompilingExpressionVisitorDependencies>());");

        variableNames.UnionWith(new[] { "relationalModel", "relationalTypeMappingSource", "materializerLiftableConstantContext" });

        var materializerLiftableConstantContext =
            Expression.Parameter(typeof(RelationalMaterializerLiftableConstantContext), "materializerLiftableConstantContext");

        // The materializer expression tree contains LiftedConstantExpression nodes, which contain instructions on how to resolve
        // constant values which need to be lifted.
        var queryExecutorAfterLiftingExpression =
            _liftableConstantProcessor.LiftConstants(queryExecutor, materializerLiftableConstantContext, variableNames);

        var sqlTreeCounter = 0;

        foreach (var liftedConstant in _liftableConstantProcessor.LiftedConstants)
        {
            var (parameter, variableValue) = liftedConstant;

            // TODO: Somewhat hacky, special handling for the SQL tree argument of RelationalCommandCache (since it requires
            // very special rendering logic
            if (parameter.Type == typeof(RelationalCommandCache))
            {
                if (variableValue is NewExpression newRelationalCommandCacheExpression
                    && newRelationalCommandCacheExpression.Arguments.FirstOrDefault(a => a.Type == typeof(Expression)) is
                        ConstantExpression { Value: Expression queryExpression })
                {
                    if (queryExpression is not IRelationalQuotableExpression quotableExpression)
                    {
                        throw new InvalidOperationException("SQL tree expression isn't quotable: " + queryExpression.GetType().Name);
                    }

                    var quotedSqlTree = quotableExpression.Quote();

                    // Render out the SQL tree, preceded by an ExpressionPrinter dump of it in a comment for easier debugging.
                    // Note that since the SQL tree is a graph (columns reference their SelectExpression's tables), rendering happens
                    // in multiple statements.
                    var sqlTreeVariable = "sqlTree" + (++sqlTreeCounter);
                    variableNames.Add(sqlTreeVariable);

                    code
                        .AppendLine("/*")
                        .AppendLine(_sqlExpressionPrinter.PrintExpression(queryExpression))
                        .AppendLine("*/");

                    var quotedSqlTreeSyntax = _linqToCSharpTranslator.TranslateExpression(
                        quotedSqlTree, constantReplacements: null, namespaces, unsafeAccessors);

                    code.AppendLine($"var {sqlTreeVariable} = {quotedSqlTreeSyntax.NormalizeWhitespace().ToFullString()};");

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

            var variableValueSyntax = _linqToCSharpTranslator.TranslateExpression(
                variableValue, constantReplacements: null, namespaces, unsafeAccessors);
            code.AppendLine($"var {parameter.Name} = {variableValueSyntax.NormalizeWhitespace().ToFullString()};");
        }

        var queryExecutorSyntaxTree =
            (AnonymousFunctionExpressionSyntax)_linqToCSharpTranslator.TranslateExpression(
                queryExecutorAfterLiftingExpression,
                constantReplacements: null,
                namespaces,
                unsafeAccessors);

        code
            .AppendLine($"return {queryExecutorSyntaxTree.NormalizeWhitespace().ToFullString()};")
            .DecrementIndent()
            .AppendLine("}")
            .AppendLine()
            .AppendLine($"private static object Query{queryNum}_Executor;");
    }

    /// <summary>
    ///     Performs processing of a query's terminating operator before handing the query off for EF compilation.
    ///     This involves removing the operator when it shouldn't be in the tree (e.g. ToList()), and rewriting async terminating operators
    ///     to their sync counterparts (e.g. MaxAsync() -> Max()). This only needs to be modified/overridden if a new terminating operator
    ///     is introduced which needs to be rewritten.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    protected virtual Expression ProcessQueryTerminatingOperator(MethodCallExpression terminatingOperator, Expression penultimateOperator)
    {
        var method = terminatingOperator.Method;

        return method.Name switch
        {
            // These sync terminating operators are defined over IEnumerable, and don't inject a node into the query tree. Simply remove them.
            nameof(Enumerable.AsEnumerable)
                or nameof(Enumerable.ToArray)
                or nameof(Enumerable.ToDictionary)
                or nameof(Enumerable.ToHashSet)
                or nameof(Enumerable.ToLookup)
                or nameof(Enumerable.ToList)
                when method.DeclaringType == typeof(Enumerable)
                => penultimateOperator,

            // Async ToListAsync, ToArrayAsync and AsAsyncEnumerable don't inject a node into the query tree - remove these as well.
            nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable)
                or nameof(EntityFrameworkQueryableExtensions.ToArrayAsync)
                or nameof(EntityFrameworkQueryableExtensions.ToDictionaryAsync)
                or nameof(EntityFrameworkQueryableExtensions.ToHashSetAsync)
                // or nameof(EntityFrameworkQueryableExtensions.ToLookupAsync)
                or nameof(EntityFrameworkQueryableExtensions.ToListAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                => penultimateOperator,

            // There's also an instance method version of AsAsyncEnumerable on DbSet, remove that as well.
            nameof(EntityFrameworkQueryableExtensions.AsAsyncEnumerable)
                when method.DeclaringType?.IsConstructedGenericType == true
                && method.DeclaringType.GetGenericTypeDefinition() == typeof(DbSet<>)
                => penultimateOperator,

            // The EF async counterparts to all the standard scalar-returning terminating operators. These need to be rewritten, as they
            // inject the sync versions into the query tree.
            nameof(EntityFrameworkQueryableExtensions.AllAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                => RewriteToSync(QueryableMethods.All),
            nameof(EntityFrameworkQueryableExtensions.AnyAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.AnyWithoutPredicate),
            nameof(EntityFrameworkQueryableExtensions.AnyAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(QueryableMethods.AnyWithPredicate),
            nameof(EntityFrameworkQueryableExtensions.AverageAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(
                    QueryableMethods.GetAverageWithoutSelector(method.GetParameters()[0].ParameterType.GenericTypeArguments[0])),
            nameof(EntityFrameworkQueryableExtensions.AverageAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(
                    QueryableMethods.GetAverageWithSelector(
                        method.GetParameters()[1].ParameterType.GenericTypeArguments[0].GenericTypeArguments[1])),
            nameof(EntityFrameworkQueryableExtensions.ContainsAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                => RewriteToSync(QueryableMethods.Contains),
            nameof(EntityFrameworkQueryableExtensions.CountAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.CountWithoutPredicate),
            nameof(EntityFrameworkQueryableExtensions.CountAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(QueryableMethods.CountWithPredicate),
            // nameof(EntityFrameworkQueryableExtensions.DefaultIfEmptyAsync)
            nameof(EntityFrameworkQueryableExtensions.ElementAtAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                => RewriteToSync(QueryableMethods.ElementAt),
            nameof(EntityFrameworkQueryableExtensions.ElementAtOrDefaultAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                => RewriteToSync(QueryableMethods.ElementAtOrDefault),
            nameof(EntityFrameworkQueryableExtensions.FirstAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.FirstWithoutPredicate),
            nameof(EntityFrameworkQueryableExtensions.FirstAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(QueryableMethods.FirstWithPredicate),
            nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.FirstOrDefaultWithoutPredicate),
            nameof(EntityFrameworkQueryableExtensions.FirstOrDefaultAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(QueryableMethods.FirstOrDefaultWithPredicate),
            nameof(EntityFrameworkQueryableExtensions.LastAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.LastWithoutPredicate),
            nameof(EntityFrameworkQueryableExtensions.LastAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(QueryableMethods.LastWithPredicate),
            nameof(EntityFrameworkQueryableExtensions.LastOrDefaultAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.LastOrDefaultWithoutPredicate),
            nameof(EntityFrameworkQueryableExtensions.LastOrDefaultAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(QueryableMethods.LastOrDefaultWithPredicate),
            nameof(EntityFrameworkQueryableExtensions.LongCountAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.LongCountWithoutPredicate),
            nameof(EntityFrameworkQueryableExtensions.LongCountAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(QueryableMethods.LongCountWithPredicate),
            nameof(EntityFrameworkQueryableExtensions.MaxAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.MaxWithoutSelector),
            nameof(EntityFrameworkQueryableExtensions.MaxAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(QueryableMethods.MaxWithSelector),
            nameof(EntityFrameworkQueryableExtensions.MinAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.MinWithoutSelector),
            nameof(EntityFrameworkQueryableExtensions.MinAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(QueryableMethods.MinWithSelector),
            // nameof(EntityFrameworkQueryableExtensions.MaxByAsync)
            // nameof(EntityFrameworkQueryableExtensions.MinByAsync)
            nameof(EntityFrameworkQueryableExtensions.SingleAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.SingleWithoutPredicate),
            nameof(EntityFrameworkQueryableExtensions.SingleAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(QueryableMethods.SingleWithPredicate),
            nameof(EntityFrameworkQueryableExtensions.SingleOrDefaultAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.SingleOrDefaultWithoutPredicate),
            nameof(EntityFrameworkQueryableExtensions.SingleOrDefaultAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(QueryableMethods.SingleOrDefaultWithPredicate),
            nameof(EntityFrameworkQueryableExtensions.SumAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 2
                => RewriteToSync(QueryableMethods.GetSumWithoutSelector(method.GetParameters()[0].ParameterType.GenericTypeArguments[0])),
            nameof(EntityFrameworkQueryableExtensions.SumAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions) && method.GetParameters().Length == 3
                => RewriteToSync(
                    QueryableMethods.GetSumWithSelector(
                        method.GetParameters()[1].ParameterType.GenericTypeArguments[0].GenericTypeArguments[1])),

            // ExecuteDelete/Update behave just like other scalar-returning operators
            nameof(RelationalQueryableExtensions.ExecuteDeleteAsync) when method.DeclaringType == typeof(RelationalQueryableExtensions)
                => RewriteToSync(typeof(RelationalQueryableExtensions).GetMethod(nameof(RelationalQueryableExtensions.ExecuteDelete))),
            nameof(RelationalQueryableExtensions.ExecuteUpdateAsync) when method.DeclaringType == typeof(RelationalQueryableExtensions)
                => RewriteToSync(typeof(RelationalQueryableExtensions).GetMethod(nameof(RelationalQueryableExtensions.ExecuteUpdate))),

            // In the regular case, we don't perform any rewriting, just composing the terminating operator on the penultimate one.
            _ when terminatingOperator.Object is null && terminatingOperator.Arguments.Count > 0
                => terminatingOperator.Update(null, [penultimateOperator, .. terminatingOperator.Arguments.Skip(1)]),

            _ => throw new InvalidOperationException($"Terminating operator '{method.Name}' is not supported.")
        };

        MethodCallExpression RewriteToSync(MethodInfo? syncMethod)
        {
            if (syncMethod is null)
            {
                throw new UnreachableException($"Could find replacement method for {method.Name}");
            }

            if (syncMethod.IsGenericMethodDefinition)
            {
                syncMethod = syncMethod.MakeGenericMethod(method.GetGenericArguments());
            }

            // Replace the first argument with the (evaluated) penultimate argument, and chop off the CancellationToken argument
            Expression[] syncArguments =
                [penultimateOperator, .. terminatingOperator.Arguments.Skip(1).Take(terminatingOperator.Arguments.Count - 2)];

            return Expression.Call(terminatingOperator.Object, syncMethod, syncArguments);
        }
    }

    /// <summary>
    ///     Contains information on a failure to precompile a specific query in the user's source code.
    ///     Includes information about the query, its location, and the exception that occured.
    /// </summary>
    public record QueryPrecompilationError(SyntaxTree SyntaxTree, SyntaxNode SyntaxNode, Exception Exception);

    private readonly struct Symbols
    {
        private readonly Compilation _compilation;

        // ReSharper disable InconsistentNaming
        public readonly INamedTypeSymbol GenericEnumerable;
        public readonly INamedTypeSymbol GenericAsyncEnumerable;
        public readonly INamedTypeSymbol IQueryable;
        public readonly INamedTypeSymbol IOrderedQueryable;
        public readonly INamedTypeSymbol IIncludableQueryable;
        public readonly INamedTypeSymbol GenericTask;
        // ReSharper restore InconsistentNaming

        private Symbols(Compilation compilation)
        {
            _compilation = compilation;

            GenericEnumerable =
                GetTypeSymbolOrThrow("System.Collections.Generic.IEnumerable`1");
            GenericAsyncEnumerable =
                GetTypeSymbolOrThrow("System.Collections.Generic.IAsyncEnumerable`1");
            IQueryable =
                GetTypeSymbolOrThrow("System.Linq.IQueryable`1");
            IOrderedQueryable =
                GetTypeSymbolOrThrow("System.Linq.IOrderedQueryable`1");
            IIncludableQueryable =
                GetTypeSymbolOrThrow("Microsoft.EntityFrameworkCore.Query.IIncludableQueryable`2");
            GenericTask =
                GetTypeSymbolOrThrow("System.Threading.Tasks.Task`1");
        }

        public static Symbols Load(Compilation compilation)
            => new(compilation);

        private INamedTypeSymbol GetTypeSymbolOrThrow(string fullyQualifiedMetadataName)
            => _compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)
                ?? throw new InvalidOperationException("Could not find type symbol for: " + fullyQualifiedMetadataName);
    }

    /// <summary>
    ///     A generated file containing LINQ operator interceptors.
    /// </summary>
    /// <param name="Path">The path of the generated file.</param>
    /// <param name="Code">The code of the generated file.</param>
    public record GeneratedInterceptorFile(string Path, string Code);
}
