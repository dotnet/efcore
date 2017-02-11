// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.DbFunctions.Internal.Initializers;

namespace Microsoft.EntityFrameworkCore.Query.DbFunctions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerDbFunctionInitalizer : IDbFunctionInitalizer
    {
        private static readonly IDbFunctionInitalizer[] _dbFunctionInitializers =
        {
            new SqlServerStringInitializer(),
            new SqlServerDateTimeInitializer(),
            new SqlServerMathInitializer(),
            new SqlServerGuidInitializer(),
            new SqlServerConvertInitializer(),
            new SqlServerEFFunctionsExtensionsInitializer()
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(ModelBuilder modelBuilder)
        {
            foreach (var initializer in _dbFunctionInitializers)
                initializer.Initialize(modelBuilder);
        }
    }
}
