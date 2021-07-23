// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;
using Newtonsoft.Json.Linq;

#nullable disable

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public partial class CosmosShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
        private readonly Type _contextType;
        private readonly bool _threadSafetyChecksEnabled;
        private readonly string _partitionKeyFromExtension;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosShapedQueryCompilingExpressionVisitor(
            ShapedQueryCompilingExpressionVisitorDependencies dependencies,
            CosmosQueryCompilationContext cosmosQueryCompilationContext,
            ISqlExpressionFactory sqlExpressionFactory,
            IQuerySqlGeneratorFactory querySqlGeneratorFactory)
            : base(dependencies, cosmosQueryCompilationContext)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _contextType = cosmosQueryCompilationContext.ContextType;
            _threadSafetyChecksEnabled = dependencies.CoreSingletonOptions.AreThreadSafetyChecksEnabled;
            _partitionKeyFromExtension = cosmosQueryCompilationContext.PartitionKeyFromExtension;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
        {
            Check.NotNull(shapedQueryExpression, nameof(shapedQueryExpression));

            var jObjectParameter = Expression.Parameter(typeof(JObject), "jObject");

            var shaperBody = shapedQueryExpression.ShaperExpression;
            shaperBody = new JObjectInjectingExpressionVisitor().Visit(shaperBody);
            shaperBody = InjectEntityMaterializers(shaperBody);

            switch (shapedQueryExpression.QueryExpression)
            {
                case SelectExpression selectExpression:

                    shaperBody = new CosmosProjectionBindingRemovingExpressionVisitor(
                            selectExpression, jObjectParameter,
                            QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
                        .Visit(shaperBody);

                    var shaperLambda = Expression.Lambda(
                        shaperBody,
                        QueryCompilationContext.QueryContextParameter,
                        jObjectParameter);

                    return Expression.New(
                        typeof(QueryingEnumerable<>).MakeGenericType(shaperLambda.ReturnType).GetConstructors()[0],
                        Expression.Convert(
                            QueryCompilationContext.QueryContextParameter,
                            typeof(CosmosQueryContext)),
                        Expression.Constant(_sqlExpressionFactory),
                        Expression.Constant(_querySqlGeneratorFactory),
                        Expression.Constant(selectExpression),
                        Expression.Constant(shaperLambda.Compile()),
                        Expression.Constant(_contextType),
                        Expression.Constant(_partitionKeyFromExtension, typeof(string)),
                        Expression.Constant(
                            QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                        Expression.Constant(_threadSafetyChecksEnabled));

                case ReadItemExpression readItemExpression:

                    shaperBody = new CosmosProjectionBindingRemovingReadItemExpressionVisitor(
                            readItemExpression, jObjectParameter,
                            QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll)
                        .Visit(shaperBody);

                    var shaperReadItemLambda = Expression.Lambda(
                        shaperBody,
                        QueryCompilationContext.QueryContextParameter,
                        jObjectParameter);

                    return Expression.New(
                        typeof(ReadItemQueryingEnumerable<>).MakeGenericType(shaperReadItemLambda.ReturnType).GetConstructors()[0],
                        Expression.Convert(
                            QueryCompilationContext.QueryContextParameter,
                            typeof(CosmosQueryContext)),
                        Expression.Constant(readItemExpression),
                        Expression.Constant(shaperReadItemLambda.Compile()),
                        Expression.Constant(_contextType),
                        Expression.Constant(
                            QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                        Expression.Constant(_threadSafetyChecksEnabled));

                default:
                    throw new NotSupportedException(CoreStrings.UnhandledExpressionNode(shapedQueryExpression.QueryExpression));
            }
        }
    }
}
