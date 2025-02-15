// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Runtime.ExceptionServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

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
    private RuntimeModelLinqToCSharpSyntaxTranslator _linqToCSharpTranslator = null!;
    private LiftableConstantProcessor _liftableConstantProcessor = null!;

    private Symbols _symbols;

    private readonly HashSet<string> _namespaces = [];
    private IReadOnlyDictionary<MemberInfo, QualifiedName> _memberAccessReplacements = new Dictionary<MemberInfo, QualifiedName>();
    private readonly HashSet<MethodDeclarationSyntax> _unsafeAccessors = [];
    private readonly IndentedStringBuilder _code = new();

    private const string InterceptorsNamespace = "Microsoft.EntityFrameworkCore.GeneratedInterceptors";

    /// <inheritdoc />
    public string? Language
        => "C#";

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
    public virtual IReadOnlyList<ScaffoldedFile> GeneratePrecompiledQueries(
        Compilation compilation,
        SyntaxGenerator syntaxGenerator,
        DbContext dbContext,
        IReadOnlyDictionary<MemberInfo, QualifiedName> memberAccessReplacements,
        List<QueryPrecompilationError> precompilationErrors,
        ISet<string> generatedFileNames,
        Assembly? additionalAssembly = null,
        string? suffix = null,
        CancellationToken cancellationToken = default)
    {
        _queryLocator.Initialize(compilation);
        _symbols = Symbols.Load(compilation);
        _g = syntaxGenerator;
        _linqToCSharpTranslator = new RuntimeModelLinqToCSharpSyntaxTranslator(_g);
        _memberAccessReplacements = memberAccessReplacements;
        _liftableConstantProcessor = new LiftableConstantProcessor(null!);
        _queryCompiler = dbContext.GetService<IQueryCompiler>();
        _unsafeAccessors.Clear();
        var contextType = dbContext.GetType();
        _funcletizer = new ExpressionTreeFuncletizer(
            dbContext.Model,
            dbContext.GetService<IEvaluatableExpressionFilter>(),
            contextType,
            generateContextAccessors: false,
            dbContext.GetService<IDiagnosticsLogger<DbLoggerCategory.Query>>());

        // This must be done after we complete generating the final compilation above
        _csharpToLinqTranslator.Load(compilation, dbContext, additionalAssembly);

        // TODO: Ignore our auto-generated code! Also compiled model, generated code (comment, filename...?).
        var generatedFiles = new List<ScaffoldedFile>();
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            if (_queryLocator.LocateQueries(syntaxTree, precompilationErrors, cancellationToken) is not { Count: > 0 } locatedQueries)
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var generatedFile = ProcessSyntaxTree(
                syntaxTree,
                semanticModel,
                locatedQueries,
                precompilationErrors,
                "." + contextType.ShortDisplayName() + (suffix ?? ".g"),
                generatedFileNames,
                cancellationToken);
            if (generatedFile is not null)
            {
                generatedFiles.Add(generatedFile);
            }
        }

        return generatedFiles;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual ScaffoldedFile? ProcessSyntaxTree(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        IReadOnlyList<InvocationExpressionSyntax> locatedQueries,
        List<QueryPrecompilationError> precompilationErrors,
        string suffix,
        ISet<string> generatedFileNames,
        CancellationToken cancellationToken)
    {
        var queriesPrecompiledInFile = 0;
        _namespaces.Clear();
        _code.Clear();
        _code
            .AppendLine()
            .AppendLine("#pragma warning disable EF9100 // Precompiled query is experimental")
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
                if (_csharpToLinqTranslator.Translate(querySyntax, semanticModel) is not MethodCallExpression terminatingOperator)
                {
                    throw new UnreachableException("Non-method call encountered as the root of a LINQ query");
                }

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
                    // This is needed e.g. for GetEnumerator(), DbSet.AsAsyncEnumerable (non-static terminating operators)
                    { Object: Expression @object } => @object,
                    { Arguments: [var sourceArgument, ..] } => sourceArgument,
                    _ => throw new UnreachableException()
                };

                penultimateOperator = Expression.Lambda<Func<IQueryable>>(penultimateOperator)
                    .Compile(preferInterpretation: true)().Expression;

                // Pass the query through EF's query pipeline; this returns the query's executor function, which can produce an enumerable
                // that invokes the query.
                // Note that we cannot recompose the terminating operator on top of the evaluated penultimate, since method signatures
                // may not allow that (e.g. DbSet.AsAsyncEnumerable() requires a DbSet, but the evaluated value for a DbSet is
                // EntityQueryRootExpression. So we handle the penultimate and the terminating separately.
                var queryExecutor = CompileQuery(penultimateOperator, terminatingOperator);

                // The query has been compiled successfully by the EF query pipeline.
                // Now go over each LINQ operator, generating an interceptor for it.
                _code.AppendLine($"#region Query{queryNum + 1}").AppendLine();

                try
                {
                    _funcletizer.ResetPathCalculation();

                    if (querySyntax is not { Expression: MemberAccessExpressionSyntax { Expression: var penultimateOperatorSyntax } })
                    {
                        throw new UnreachableException();
                    }

                    // Generate interceptors for all LINQ operators in the query, starting from the root up until the penultimate.
                    // Then generate the interceptor for the terminating operator, and finally the query's executor.
                    GenerateOperatorInterceptorsRecursively(
                        _code, penultimateOperator, penultimateOperatorSyntax, semanticModel, queryNum + 1, out var operatorNum,
                        cancellationToken: cancellationToken);

                    GenerateOperatorInterceptor(
                        _code, terminatingOperator, querySyntax, semanticModel, queryNum + 1, operatorNum + 1, isTerminatingOperator: true,
                        cancellationToken);

                    GenerateQueryExecutor(_code, queryNum + 1, queryExecutor, _namespaces, _unsafeAccessors);
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
                precompilationErrors.Add(new QueryPrecompilationError(querySyntax, e));
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

        var mainCode = _code.ToString();

        _code.Clear();
        _code.AppendLine("// <auto-generated />").AppendLine();

        // In addition to the namespaces auto-detected by LinqToCSharpTranslator, we manually add these namespaces which are required
        // by manually generated code above.
        _namespaces.UnionWith(
        [
            "System",
            "System.Collections.Concurrent",
            "System.Collections.Generic",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Runtime.CompilerServices",
            "System.Reflection",
            "System.Threading.Tasks",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore.ChangeTracking.Internal",
            "Microsoft.EntityFrameworkCore.Diagnostics",
            "Microsoft.EntityFrameworkCore.Infrastructure",
            "Microsoft.EntityFrameworkCore.Infrastructure.Internal",
            "Microsoft.EntityFrameworkCore.Internal",
            "Microsoft.EntityFrameworkCore.Metadata",
            "Microsoft.EntityFrameworkCore.Query",
            "Microsoft.EntityFrameworkCore.Query.Internal",
            "Microsoft.EntityFrameworkCore.Storage"
        ]);

        foreach (var ns in _namespaces
                     .OrderBy(
                         ns => ns switch
                         {
                             _ when ns.StartsWith("System.", StringComparison.Ordinal) => 10,
                             _ when ns.StartsWith("Microsoft.", StringComparison.Ordinal) => 9,
                             _ => 0
                         })
                     .ThenBy(ns => ns))
        {
            _code.Append("using ").Append(ns).AppendLine(";");
        }

        _code.AppendLine(mainCode);

        _code.AppendLine(
            """
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute : Attribute
    {
        public InterceptsLocationAttribute(string filePath, int line, int column) { }
    }
}
"""
        );

        var name = Uniquifier.Uniquify(
            Path.GetFileNameWithoutExtension(syntaxTree.FilePath),
            generatedFileNames,
            ".EFInterceptors" + suffix + Path.GetExtension(syntaxTree.FilePath),
            CompiledModelScaffolder.MaxFileNameLength);
        return new ScaffoldedFile(name, _code.ToString());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual Expression CompileQuery(Expression penultimateOperator, MethodCallExpression terminatingOperator)
    {
        // First, check whether this is an async query.
        var async = terminatingOperator.Type.IsGenericType
            && terminatingOperator.Type.GetGenericTypeDefinition() is var genericDefinition
            && (genericDefinition == typeof(Task<>)
                || genericDefinition == typeof(ValueTask<>)
                || genericDefinition == typeof(IAsyncEnumerable<>));

        var preparedQuery = PrepareQueryForCompilation(penultimateOperator, terminatingOperator);

        // We now need to figure out the return type of the query's executor.
        // Non-scalar query expressions (e.g. ToList()) return an IQueryable; the query executor will return an enumerable (sync or async).
        // Scalar query expressions just return the scalar type.
        var returnType = preparedQuery.Type.IsGenericType
            && preparedQuery.Type.GetGenericTypeDefinition().IsAssignableTo(typeof(IQueryable))
                ? (async
                    ? typeof(IAsyncEnumerable<>)
                    : typeof(IEnumerable<>)).MakeGenericType(preparedQuery.Type.GetGenericArguments()[0])
                : terminatingOperator.Type;

        // We now have the query as a finalized LINQ expression tree, ready for compilation.
        // Compile the query, invoking CompileQueryToExpression on the IQueryCompiler from the user's context instance.
        try
        {
            return (Expression)_queryCompiler.GetType()
                .GetMethod(nameof(IQueryCompiler.PrecompileQuery))!
                .MakeGenericMethod(returnType)
                .Invoke(_queryCompiler, [preparedQuery, async])!;
        }
        catch (TargetInvocationException e) when (e.InnerException is not null)
        {
            // Unwrap the TargetInvocationException wrapper we get from Invoke()
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private void GenerateOperatorInterceptorsRecursively(
        IndentedStringBuilder code,
        Expression operatorExpression,
        ExpressionSyntax operatorSyntax,
        SemanticModel semanticModel,
        int queryNum,
        out int operatorNum,
        CancellationToken cancellationToken)
    {
        // For non-root operators, we get here with an InvocationExpressionSyntax and its corresponding LINQ MethodCallExpression.
        // For the query root, we usually don't get called here: a regular EntityQueryRootExpression corresponds to a DbSet (either
        // property access on DbContext or a Set<>() method invocation). We can't intercept property accesses, and in any case there's
        // nothing to intercept there.
        // However, for FromSql specifically, we get here with an InvocationExpressionSyntax (representing the FromSql() invocation), but
        // with a corresponding FromSqlQueryRootExpression - not a MethodCallExpression. We must pass this query root through the
        // funcletizer as usual to mimic the normal flow.
        switch (operatorExpression)
        {
            // Regular, non-root LINQ operator; the LINQ method call must correspond to a Roslyn syntax invocation.
            // We first recurse to handle the nested operator (i.e. generate the interceptor from the root outer).
            case MethodCallExpression operatorMethodCall:
                if (operatorSyntax is not InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax { Expression: var nestedOperatorSyntax }
                    })
                {
                    throw new UnreachableException();
                }

                // We're an operator (not the query root).
                // Continue recursing down - we want to handle from the root up.

                var nestedOperatorExpression = operatorMethodCall switch
                {
                    // This is needed e.g. for GetEnumerator(), DbSet.AsAsyncEnumerable (non-static terminating operators)
                    { Object: Expression @object } => @object,
                    { Arguments: [var sourceArgument, ..] } => sourceArgument,
                    _ => throw new UnreachableException()
                };

                GenerateOperatorInterceptorsRecursively(
                    code, nestedOperatorExpression, nestedOperatorSyntax, semanticModel, queryNum, out operatorNum,
                    cancellationToken: cancellationToken);

                operatorNum++;

                GenerateOperatorInterceptor(
                    code, operatorExpression, operatorSyntax, semanticModel, queryNum, operatorNum, isTerminatingOperator: false,
                    cancellationToken);
                return;

            // For FromSql() queries, an InvocationExpressionSyntax (representing the FromSql() invocation), but with a corresponding
            // FromSqlQueryRootExpression - not a MethodCallExpression.
            // We must generate an interceptor for FromSql() and pass the arguments array through the funcletizer as usual.
            case FromSqlQueryRootExpression:
                operatorNum = 1;
                GenerateOperatorInterceptor(
                    code, operatorExpression, operatorSyntax, semanticModel, queryNum, operatorNum, isTerminatingOperator: false,
                    cancellationToken);
                return;

            // For other query roots, we don't generate interceptors - there are no possible captured variables that need to be
            // pass through funcletization (as with FromSqlQueryRootExpression). Simply return to process the first non-root operator.
            case QueryRootExpression:
                operatorNum = 0;
                return;

            default:
                throw new UnreachableException();
        }
    }

    private void GenerateOperatorInterceptor(
        IndentedStringBuilder code,
        Expression operatorExpression,
        ExpressionSyntax operatorSyntax,
        SemanticModel semanticModel,
        int queryNum,
        int operatorNum,
        bool isTerminatingOperator,
        CancellationToken cancellationToken)
    {
        // At this point we know we're intercepting a method call invocation.
        // Extract the MemberAccessExpressionSyntax for the invocation, representing the method being called.
        var memberAccessSyntax = (operatorSyntax as InvocationExpressionSyntax)?.Expression as MemberAccessExpressionSyntax
            ?? throw new UnreachableException();

        // Create the parameter list for our interceptor method from the LINQ operator method's parameter list
        if (semanticModel.GetSymbolInfo(memberAccessSyntax, cancellationToken).Symbol is not IMethodSymbol operatorSymbol)
        {
            throw new InvalidOperationException("Couldn't find method symbol for: " + memberAccessSyntax);
        }

        // Throughout the code generation below, we will only be dealing with the original generic definition of the operator (and
        // generating a generic interceptor); we'll never be dealing with the concrete types for this invocation, since these may
        // be unspeakable anonymous types which we can't embed in generated code.
        operatorSymbol = operatorSymbol.OriginalDefinition;

        // For extension methods, this provides the form which has the "this" as its first parameter.
        // TODO: Validate the below, throw informative (e.g. top-level TVF fails here because non-generic)
        var reducedOperatorSymbol = operatorSymbol.GetConstructedReducedFrom() ?? operatorSymbol;

        var (sourceVariableName, sourceTypeSymbol) = reducedOperatorSymbol.IsStatic
            ? (reducedOperatorSymbol.Parameters[0].Name, reducedOperatorSymbol.Parameters[0].Type)
            : ("source", reducedOperatorSymbol.ReceiverType!);

        if (sourceTypeSymbol is not INamedTypeSymbol { TypeArguments: [var sourceElementTypeSymbol] })
        {
            throw new UnreachableException($"Non-IQueryable first parameter in LINQ operator '{operatorSymbol.Name}'");
        }

        var sourceElementTypeName = sourceElementTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

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
                            || i.OriginalDefinition.Equals(_symbols.GenericAsyncEnumerable, SymbolEqualityComparer.Default)
                            || i.OriginalDefinition.Equals(_symbols.GenericEnumerator, SymbolEqualityComparer.Default))
                => namedReturnType2.TypeArguments[0],
            _ => null
        };

        // Output the interceptor method signature preceded by the [InterceptsLocation] attribute.
        var startPosition = operatorSyntax.SyntaxTree.GetLineSpan(memberAccessSyntax.Name.Span, cancellationToken).StartLinePosition;
        var interceptorName = $"Query{queryNum}_{memberAccessSyntax.Name}{operatorNum}";
        code.AppendLine(
            $"""[InterceptsLocation(@"{operatorSyntax.SyntaxTree.FilePath.Replace("\"", "\"\"")}", {startPosition.Line + 1}, {startPosition.Character + 1})]""");
        GenerateInterceptorMethodSignature();
        code.AppendLine("{").IncrementIndent();

        // If this is the first query operator (no nested operator), cast the input source to IInfrastructure<DbContext> and extract the
        // DbContext, create a new QueryContext, and wrap it all in a PrecompiledQueryContext that will flow through to the
        // terminating operator, where the query will actually get executed.
        // Otherwise, if this is a non-first operator, receive the PrecompiledQueryContext from the nested operator and flow it forward.
        code.AppendLine(
            "var precompiledQueryContext = "
            + (operatorNum == 1
                ? $"new PrecompiledQueryContext<{sourceElementTypeName}>(((IInfrastructure<DbContext>){sourceVariableName}).Instance);"
                : $"(PrecompiledQueryContext<{sourceElementTypeName}>){sourceVariableName};"));

        var declaredQueryContextVariable = false;

        ProcessCapturedVariables();

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
                var returnType = returnTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                code.AppendLine($"return ((Func<QueryContext, {returnType}>)({executorFieldIdentifier}))(queryContext);");
            }
            else
            {
                // The query returns an IEnumerable/IAsyncEnumerable/IQueryable, which is a bit trickier: the executor doesn't return a
                // simple value as in the scalar case, but rather e.g. SingleQueryingEnumerable; we need to compose the terminating
                // operator (e.g. ToList()) on top of that. Cast the executor delegate to Func<QueryContext, IEnumerable<T>>
                // (contravariance).
                var isAsync =
                    operatorExpression.Type.IsGenericType
                    && operatorExpression.Type.GetGenericTypeDefinition() is var genericDefinition
                    && (
                        genericDefinition == typeof(Task<>)
                        || genericDefinition == typeof(ValueTask<>)
                        || genericDefinition == typeof(IAsyncEnumerable<>));

                var isQueryable = !isAsync
                    && operatorExpression.Type.IsGenericType
                    && operatorExpression.Type.GetGenericTypeDefinition() == typeof(IQueryable<>);

                var returnValue = isAsync
                    ? $"IAsyncEnumerable<{sourceElementTypeName}>"
                    : $"IEnumerable<{sourceElementTypeName}>";

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
                        code.AppendLine(
                            $"var asyncQueryingEnumerable = new PrecompiledQueryableAsyncEnumerableAdapter<{sourceElementTypeName}>(queryingEnumerable);");
                        code.Append("return asyncQueryingEnumerable");
                    }
                    else
                    {
                        code.Append("return queryingEnumerable");
                    }

                    // Invoke the original terminating operator (e.g. ToList(), ToDictionary()...) on the querying enumerable, passing
                    // through the interceptor's arguments.
                    code.AppendLine(
                        $".{memberAccessSyntax.Name}({string.Join(", ", operatorSymbol.Parameters.Select(p => p.Name))});");
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
                            : $"return precompiledQueryContext.ToType<{returnElementTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>();",

                    _ when returnTypeSymbol.OriginalDefinition.Equals(_symbols.IIncludableQueryable, SymbolEqualityComparer.Default)
                        && returnTypeSymbol is INamedTypeSymbol { OriginalDefinition.TypeArguments: [_, var includedPropertySymbol] }
                        => $"return precompiledQueryContext.ToIncludable<{includedPropertySymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>();",

                    _ => throw new UnreachableException()
                });
        }

        code.DecrementIndent().AppendLine("}").AppendLine();

        void GenerateInterceptorMethodSignature()
        {
            code
                .Append("public static ")
                .Append(_g.TypeExpression(reducedOperatorSymbol.ReturnType).ToFullString())
                .Append(' ')
                .Append(interceptorName);

            var (typeParameters, constraints) =
                (reducedOperatorSymbol.IsGenericMethod, reducedOperatorSymbol.ContainingType.IsGenericType) switch
                {
                    (true, false) => (reducedOperatorSymbol.TypeParameters,
                        ((MethodDeclarationSyntax)_g.MethodDeclaration(reducedOperatorSymbol)).ConstraintClauses),
                    (false, true) => (reducedOperatorSymbol.ContainingType.TypeParameters,
                        ((TypeDeclarationSyntax)_g.Declaration(reducedOperatorSymbol.ContainingType)).ConstraintClauses),
                    (false, false) => ([], []),
                    (true, true) => throw new NotImplementedException("Generic method on generic type not supported")
                };

            if (typeParameters.Length > 0)
            {
                code.Append('<');
                for (var i = 0; i < typeParameters.Length; i++)
                {
                    if (i > 0)
                    {
                        code.Append(", ");
                    }

                    code.Append(_g.TypeExpression(typeParameters[i]).ToFullString());
                }

                code.Append('>');
            }

            code.Append('(');

            // For instance methods (IEnumerable<T>.GetEnumerator(), DbSet.GetAsyncEnumerable()...), we generate an extension method
            // (with this) for the interceptor.
            if (reducedOperatorSymbol is { IsStatic: false, ReceiverType: not null })
            {
                code
                    .Append("this ")
                    .Append(_g.TypeExpression(reducedOperatorSymbol.ReceiverType).ToFullString())
                    .Append(' ')
                    .Append(sourceVariableName);
            }

            for (var i = 0; i < reducedOperatorSymbol.Parameters.Length; i++)
            {
                var parameter = reducedOperatorSymbol.Parameters[i];

                if (i == 0)
                {
                    switch (reducedOperatorSymbol)
                    {
                        case { IsExtensionMethod: true }:
                            code.Append("this ");
                            break;

                        // For instance methods we already added a this parameter above
                        case { IsStatic: false, ReceiverType: not null }:
                            code.Append(", ");
                            break;

                        default:
                            throw new NotImplementedException("Non-extension static method not supported");
                    }
                }
                else
                {
                    code.Append(", ");
                }

                code
                    .Append(_g.TypeExpression(parameter.Type).ToFullString())
                    .Append(' ')
                    .Append(parameter.Name);
            }

            code.AppendLine(")");

            foreach (var f in constraints)
            {
                code.AppendLine(f.NormalizeWhitespace().ToFullString());
            }
        }

        void ProcessCapturedVariables()
        {
            // Go over the operator's arguments (skipping the first, which is the source).
            // For those which have captured variables, run them through our funcletizer, which will return code for extracting any captured
            // variables from them.
            switch (operatorExpression)
            {
                // Regular case: this is an operator method
                case MethodCallExpression operatorMethodCall:
                {
                    var parameters = operatorMethodCall.Method.GetParameters();

                    for (var i = 1; i < parameters.Length; i++)
                    {
                        var (parameterName, parameterType) = (parameters[i].Name!, parameters[i].ParameterType);

                        if (parameterType == typeof(CancellationToken))
                        {
                            continue;
                        }

                        ExpressionTreeFuncletizer.PathNode? evaluatableRootPaths;

                        // ExecuteUpdate requires really special handling: the function accepts a Func<UpdateSettersBuilder...> argument, but
                        // we need to run funcletization on the setter lambdas added via that Func<>.
                        if (operatorMethodCall.Method is
                            {
                                Name: nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate)
                                or nameof(EntityFrameworkQueryableExtensions.ExecuteUpdateAsync),
                                IsGenericMethod: true
                            }
                            && operatorMethodCall.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions))
                        {
                            // First, statically convert the Action<UpdateSettersBuilder> to a NewArrayExpression which represents all the
                            // setters; since that's an expression, we can run the funcletizer on it.
                            var settersExpression = ProcessExecuteUpdate(operatorMethodCall);
                            evaluatableRootPaths = _funcletizer.CalculatePathsToEvaluatableRoots(settersExpression);

                            if (evaluatableRootPaths is null)
                            {
                                // There are no captured variables in this lambda argument - skip the argument
                                continue;
                            }

                            // If there were captured variables, generate code to evaluate and build the same NewArrayExpression at runtime,
                            // and then fall through to the normal logic, generating variable extractors against that NewArrayExpression
                            // (local var) instead of against the method argument.
                            code.AppendLine($"""
                                             var setterBuilder = new UpdateSettersBuilder<{sourceElementTypeName}>();
                                             {parameterName}(setterBuilder);
                                             var setters = setterBuilder.BuildSettersExpression();
                                             """);
                            parameterName = "setters";
                            parameterType = typeof(NewArrayExpression);
                        }
                        else
                        {
                            evaluatableRootPaths = _funcletizer.CalculatePathsToEvaluatableRoots(operatorMethodCall, i);
                            if (evaluatableRootPaths is null)
                            {
                                // There are no captured variables in this lambda argument - skip the argument
                                continue;
                            }
                        }

                        // We have a lambda argument with captured variables. Use the information returned by the funcletizer to generate code
                        // which extracts them and sets them on our query context.
                        if (!declaredQueryContextVariable)
                        {
                            code.AppendLine("var queryContext = precompiledQueryContext.QueryContext;");
                            declaredQueryContextVariable = true;
                        }

                        if (!parameterType.IsSubclassOf(typeof(Expression)))
                        {
                            // Special case: this is a non-lambda argument (Skip/Take/FromSql).
                            // Simply add the argument directly as a parameter
                            code.AppendLine($"""queryContext.AddParameter("{evaluatableRootPaths.ParameterName}", {parameterName});""");
                            continue;
                        }

                        var variableCounter = 0;

                        // Lambda argument. Recurse through evaluatable path trees.
                        foreach (var child in evaluatableRootPaths.Children!)
                        {
                            GenerateCapturedVariableExtractors(parameterName, parameterType, child);

                            void GenerateCapturedVariableExtractors(
                                string currentIdentifier,
                                Type currentType,
                                ExpressionTreeFuncletizer.PathNode capturedVariablesPathTree)
                            {
                                var linqPathSegment =
                                    capturedVariablesPathTree.PathFromParent!(Expression.Parameter(currentType, currentIdentifier));
                                var collectedNamespaces = new HashSet<string>();
                                var unsafeAccessors = new HashSet<MethodDeclarationSyntax>();
                                var roslynPathSegment = _linqToCSharpTranslator.TranslateExpression(
                                    linqPathSegment, constantReplacements: null, _memberAccessReplacements, collectedNamespaces,
                                    unsafeAccessors);

                                var variableName = capturedVariablesPathTree.ExpressionType.Name;
                                variableName = char.ToLower(variableName[0]) + variableName[1..^"Expression".Length] + ++variableCounter;

                                if (capturedVariablesPathTree.Children?.Count > 0)
                                {
                                    // This is an intermediate node which has captured variables in the children. Continue recursing down.
                                    code.AppendLine(
                                        $"var {variableName} = ({capturedVariablesPathTree.ExpressionType.Name}){roslynPathSegment};");

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
                                        .AppendLine($"Expression.Lambda<Func<object?>>(Expression.Convert({roslynPathSegment}, typeof(object)))")
                                        .AppendLine(".Compile(preferInterpretation: true)")
                                        .AppendLine(".Invoke());");
                                }
                            }
                        }
                    }

                    break;
                }

                // Special case: this is a FromSql query root; we're intercepting the invocation syntax for the FromSql() call, but on the LINQ
                // side we have a query root (i.e. not the MethodCallExpression for the FromSql(), but rather its evaluated result)
                case FromSqlQueryRootExpression fromSqlQueryRoot:
                {
                    if (_funcletizer.CalculatePathsToEvaluatableRoots(fromSqlQueryRoot.Argument) is not ExpressionTreeFuncletizer.PathNode
                        evaluatableRootPaths)
                    {
                        // There are no captured variables in this FromSqlQueryRootExpression, skip it.
                        break;
                    }

                    // We have a lambda argument with captured variables. Use the information returned by the funcletizer to generate code
                    // which extracts them and sets them on our query context.
                    if (!declaredQueryContextVariable)
                    {
                        code.AppendLine("var queryContext = precompiledQueryContext.QueryContext;");
                        declaredQueryContextVariable = true;
                    }

                    var argumentsParameter = reducedOperatorSymbol switch
                    {
                        { Name: "FromSqlRaw", Parameters: [_, _, { Name: "parameters" }] } => "parameters",
                        { Name: "FromSql", Parameters: [_, { Name: "sql" }] } => "sql.GetArguments()",
                        { Name: "FromSqlInterpolated", Parameters: [_, { Name: "sql" }] } => "sql.GetArguments()",
                        _ => throw new UnreachableException()
                    };

                    code.AppendLine(
                        $"""queryContext.AddParameter("{evaluatableRootPaths.ParameterName}", {argumentsParameter});""");

                    break;
                }

                default:
                    throw new UnreachableException();
            }
        }
    }

    private void GenerateQueryExecutor(
        IndentedStringBuilder code,
        int queryNum,
        Expression queryExecutor,
        HashSet<string> namespaces,
        HashSet<MethodDeclarationSyntax> unsafeAccessors)
    {
        // We're going to generate the method which will create the query executor (Func<QueryContext, TResult>).
        // Note that the we store the executor itself (and return it) as object, not as a typed Func<QueryContext, TResult>.
        // We can't strong-type it since it may return an anonymous type, which is unspeakable; so instead we cast down from object to
        // the real strongly-typed signature inside the interceptor, where the return value is represented as a generic type parameter
        // (which can be an anonymous type).
        code
            .AppendLine($"private static object Query{queryNum}_GenerateExecutor(DbContext dbContext, QueryContext queryContext)")
            .AppendLine("{")
            .IncrementIndent()
            .AppendLine("var relationalModel = dbContext.Model.GetRelationalModel();")
            .AppendLine("var relationalTypeMappingSource = dbContext.GetService<IRelationalTypeMappingSource>();")
            .AppendLine("var materializerLiftableConstantContext = new RelationalMaterializerLiftableConstantContext(")
            .AppendLine("    dbContext.GetService<ShapedQueryCompilingExpressionVisitorDependencies>(),")
            .AppendLine("    dbContext.GetService<RelationalShapedQueryCompilingExpressionVisitorDependencies>(),")
            .AppendLine("    dbContext.GetService<RelationalCommandBuilderDependencies>());");

        HashSet<string> variableNames = ["relationalModel", "relationalTypeMappingSource", "materializerLiftableConstantContext"];

        var materializerLiftableConstantContext =
            Expression.Parameter(typeof(RelationalMaterializerLiftableConstantContext), "materializerLiftableConstantContext");

        // The materializer expression tree contains LiftedConstantExpression nodes, which contain instructions on how to resolve
        // constant values which need to be lifted.
        var queryExecutorAfterLiftingExpression =
            _liftableConstantProcessor.LiftConstants(queryExecutor, materializerLiftableConstantContext, variableNames);

        foreach (var liftedConstant in _liftableConstantProcessor.LiftedConstants)
        {
            var variableValueSyntax = _linqToCSharpTranslator.TranslateExpression(
                liftedConstant.Expression, constantReplacements: null, _memberAccessReplacements, namespaces, unsafeAccessors);
            // code.AppendLine($"{liftedConstant.Parameter.Type.Name} {liftedConstant.Parameter.Name} = {variableValueSyntax.NormalizeWhitespace().ToFullString()};");
            code.AppendLine($"var {liftedConstant.Parameter.Name} = {variableValueSyntax.NormalizeWhitespace().ToFullString()};");
        }

        var queryExecutorSyntaxTree =
            (AnonymousFunctionExpressionSyntax)_linqToCSharpTranslator.TranslateExpression(
                queryExecutorAfterLiftingExpression,
                constantReplacements: null,
                _memberAccessReplacements,
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
    protected virtual Expression PrepareQueryForCompilation(Expression penultimateOperator, MethodCallExpression terminatingOperator)
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

            nameof(IEnumerable.GetEnumerator)
                when method.DeclaringType is { IsConstructedGenericType: true } declaringType
                && declaringType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
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

            // ExecuteDelete behaves just like other scalar-returning operators
            nameof(EntityFrameworkQueryableExtensions.ExecuteDeleteAsync) when method.DeclaringType
                == typeof(EntityFrameworkQueryableExtensions)
                => RewriteToSync(
                    typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.ExecuteDelete))),

            // ExecuteUpdate is special; it accepts a non-expression-tree argument (Action<UpdateSettersBuilder>),
            // evaluates it immediately, and injects a different MethodCall node into the expression tree with the resulting setter
            // expressions.
            // When statically analyzing ExecuteUpdate, we have to manually perform the same thing.
            nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate) or nameof(EntityFrameworkQueryableExtensions.ExecuteUpdateAsync)
                when method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                => Expression.Call(
                    EntityFrameworkQueryableExtensions.ExecuteUpdateMethodInfo.MakeGenericMethod(
                        terminatingOperator.Arguments[0].Type.GetSequenceType()),
                    penultimateOperator,
                    ProcessExecuteUpdate(terminatingOperator)),

            // In the regular case (sync terminating operator which needs to stay in the query tree), simply compose the terminating
            // operator over the penultimate and return that.
            _ => terminatingOperator switch
            {
                // This is needed e.g. for GetEnumerator(), DbSet.AsAsyncEnumerable (non-static terminating operators)
                { Object: Expression }
                    => terminatingOperator.Update(penultimateOperator, terminatingOperator.Arguments),
                { Arguments: [_, ..] }
                    => terminatingOperator.Update(@object: null, [penultimateOperator, .. terminatingOperator.Arguments.Skip(1)]),
                _ => throw new UnreachableException()
            }
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

            // Replace the first argument with the penultimate argument, and chop off the CancellationToken argument
            Expression[] syncArguments =
                [penultimateOperator, .. terminatingOperator.Arguments.Skip(1).Take(terminatingOperator.Arguments.Count - 2)];

            return Expression.Call(terminatingOperator.Object, syncMethod, syncArguments);
        }
    }

    // Accepts an expression tree representing a series of SetProperty() calls, parses them and passes them through the
    // UpdateSettersBuilder; returns the resulting NewArrayExpression representing all the setters.
    private static NewArrayExpression ProcessExecuteUpdate(MethodCallExpression executeUpdateCall)
    {
        var settersBuilder = new UpdateSettersBuilder();
        var settersLambda = (LambdaExpression)executeUpdateCall.Arguments[1];
        var settersParameter = settersLambda.Parameters.Single();
        var expression = settersLambda.Body;

        while (expression != settersParameter)
        {
            if (expression is MethodCallExpression
                {
                    Method:
                    {
                        IsGenericMethod: true,
                        Name: nameof(UpdateSettersBuilder<int>.SetProperty),
                        DeclaringType.IsGenericType: true,
                    },
                    Arguments:
                    [
                        UnaryExpression { NodeType: ExpressionType.Quote, Operand: LambdaExpression propertySelector },
                        Expression valueSelector
                    ]
                } methodCallExpression
                && methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(UpdateSettersBuilder<>))
            {
                if (valueSelector is UnaryExpression
                    {
                        NodeType: ExpressionType.Quote,
                        Operand: LambdaExpression unwrappedValueSelector
                    })
                {
                    settersBuilder.SetProperty(propertySelector, unwrappedValueSelector);
                }
                else
                {
                    settersBuilder.SetProperty(propertySelector, valueSelector);
                }

                expression = methodCallExpression.Object;
                continue;
            }

            throw new InvalidOperationException(RelationalStrings.InvalidArgumentToExecuteUpdate);
        }

        return settersBuilder.BuildSettersExpression();
    }

    /// <summary>
    ///     Contains information on a failure to precompile a specific query in the user's source code.
    ///     Includes information about the query, its location, and the exception that occured.
    /// </summary>
    public sealed record QueryPrecompilationError(SyntaxNode SyntaxNode, Exception Exception);

    private readonly struct Symbols
    {
        private readonly Compilation _compilation;

        // ReSharper disable InconsistentNaming
        public readonly INamedTypeSymbol GenericEnumerable;
        public readonly INamedTypeSymbol GenericAsyncEnumerable;
        public readonly INamedTypeSymbol GenericEnumerator;
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
            GenericEnumerator =
                GetTypeSymbolOrThrow("System.Collections.Generic.IEnumerator`1");
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
}
