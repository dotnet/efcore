// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext"/>
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class SqliteNetTopologySuiteMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        public SqliteNetTopologySuiteMethodCallTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            Translators = new IMethodCallTranslator[]
            {
                new SqliteGeometryMethodTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                new SqliteGeometryCollectionMethodTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                new SqliteLineStringMethodTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor),
                new SqlitePolygonMethodTranslator(typeMappingSource, typeMappingApplyingExpressionVisitor)
            };
        }


        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IMethodCallTranslator> Translators { get; }
    }
}
