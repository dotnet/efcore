// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class SelectExpressionFactory : ISelectExpressionFactory
    {
        private readonly ISqlQueryGeneratorFactory _sqlQueryGeneratorFactory;

        public SelectExpressionFactory([NotNull] ISqlQueryGeneratorFactory sqlQueryGeneratorFactory)
        {
            Check.NotNull(sqlQueryGeneratorFactory, nameof(sqlQueryGeneratorFactory));

            _sqlQueryGeneratorFactory = sqlQueryGeneratorFactory;
        }

        public virtual SelectExpression Create()
            => new SelectExpression(_sqlQueryGeneratorFactory);

        public virtual  SelectExpression Create([NotNull] string alias)
            => new SelectExpression(
                _sqlQueryGeneratorFactory,
                Check.NotEmpty(alias, nameof(alias)));
    }
}
