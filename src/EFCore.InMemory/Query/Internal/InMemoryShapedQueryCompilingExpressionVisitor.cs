// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal;

using static Expression;

public partial class InMemoryShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
{
    private readonly Type _contextType;
    private readonly bool _threadSafetyChecksEnabled;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryShapedQueryCompilingExpressionVisitor(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
        this._contextType = queryCompilationContext.ContextType;
        this._threadSafetyChecksEnabled = dependencies.CoreSingletonOptions.AreThreadSafetyChecksEnabled;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case InMemoryTableExpression inMemoryTableExpression:
                return Call(
                    TableMethodInfo,
                    QueryCompilationContext.QueryContextParameter,
                    Constant(inMemoryTableExpression.EntityType));
        }

        return base.VisitExtension(extensionExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        var inMemoryQueryExpression = (InMemoryQueryExpression)shapedQueryExpression.QueryExpression;
        inMemoryQueryExpression.ApplyProjection();

        var shaperExpression = new ShaperExpressionProcessingExpressionVisitor(
                this,
                inMemoryQueryExpression,
                this.QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            .ProcessShaper(shapedQueryExpression.ShaperExpression);
        var innerEnumerable = this.Visit(inMemoryQueryExpression.ServerQueryExpression);

        return New(
            typeof(QueryingEnumerable<>).MakeGenericType(shaperExpression.ReturnType).GetConstructors()[0],
            QueryCompilationContext.QueryContextParameter,
            innerEnumerable,
            Constant(shaperExpression.Compile()),
            Constant(this._contextType),
            Constant(this.QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
            Constant(this._threadSafetyChecksEnabled));
    }

    private static readonly MethodInfo TableMethodInfo
        = typeof(InMemoryShapedQueryCompilingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(Table))!;

    private static IEnumerable<ValueBuffer> Table(
        QueryContext queryContext,
        IEntityType entityType)
        => ((InMemoryQueryContext)queryContext).GetValueBuffers(entityType);
}
