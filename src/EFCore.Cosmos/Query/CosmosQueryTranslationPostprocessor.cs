// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query
{
    /// <inheritdoc />
    public class CosmosQueryTranslationPostprocessor : QueryTranslationPostprocessor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     Creates a new instance of the <see cref="CosmosQueryTranslationPostprocessor" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="sqlExpressionFactory"> The SqlExpressionFactory object to use. </param>
        /// <param name="queryCompilationContext"> The query compilation context object to use. </param>
        public CosmosQueryTranslationPostprocessor(
            [NotNull] QueryTranslationPostprocessorDependencies dependencies,
            [NotNull] ISqlExpressionFactory sqlExpressionFactory,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies, queryCompilationContext)
        {
            Check.NotNull(sqlExpressionFactory, nameof(sqlExpressionFactory));

            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <inheritdoc />
        public override Expression Process(Expression query)
        {
            query = base.Process(query);
            query = new CosmosValueConverterCompensatingExpressionVisitor(_sqlExpressionFactory).Visit(query);

            return query;
        }
    }
}
