// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    public class XGQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        private readonly QuerySqlGeneratorDependencies _dependencies;
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly IXGOptions _options;

        public XGQuerySqlGeneratorFactory(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            IRelationalTypeMappingSource typeMappingSource,
            IXGOptions options)
        {
            _dependencies = dependencies;
            _typeMappingSource = typeMappingSource;
            _options = options;
        }

        public virtual QuerySqlGenerator Create()
            => new XGQuerySqlGenerator(_dependencies, _typeMappingSource, _options);
    }
}
