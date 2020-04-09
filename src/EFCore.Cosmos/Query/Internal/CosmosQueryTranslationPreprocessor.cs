// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public class CosmosQueryTranslationPreprocessor : QueryTranslationPreprocessor
    {

        private readonly CosmosQueryCompilationContext _queryCompilationContext;

        public CosmosQueryTranslationPreprocessor(
            [NotNull] QueryTranslationPreprocessorDependencies dependencies,
            [NotNull] CosmosQueryCompilationContext cosmosQueryCompilationContext)
            : base(dependencies, cosmosQueryCompilationContext)
        {
            _queryCompilationContext = cosmosQueryCompilationContext;
        }

        public override Expression NormalizeQueryableMethodCall(Expression query)
        {
            query = base.NormalizeQueryableMethodCall(query);

            query = new CosmosQueryMetadataExtractingExpressionVisitor(_queryCompilationContext).Visit(query);

            return query;
        }
    }
}
