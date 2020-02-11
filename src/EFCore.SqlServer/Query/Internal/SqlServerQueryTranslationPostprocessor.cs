// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
    {
        public SqlServerQueryTranslationPostprocessor(
            [NotNull] QueryTranslationPostprocessorDependencies dependencies,
            [NotNull] RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(dependencies, relationalDependencies, queryCompilationContext)
        {
        }

        public override Expression Process(Expression query)
        {
            query = new StringEqualityConvertingExpressionVisitor(RelationalDependencies.SqlExpressionFactory).Visit(query);

            query = base.Process(query);

            return query;
        }
    }
}
