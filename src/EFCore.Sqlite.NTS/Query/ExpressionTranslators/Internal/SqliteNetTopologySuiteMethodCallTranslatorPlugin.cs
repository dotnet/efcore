// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteNetTopologySuiteMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IMethodCallTranslator> Translators { get; } = new IMethodCallTranslator[]
        {
            new SqliteGeometryMethodTranslator(),
            new SqliteGeometryCollectionMethodTranslator(),
            new SqliteLineStringMethodTranslator(),
            new SqlitePolygonMethodTranslator()
        };
    }
}
