// Copyright(c) Microsoft Open Technologies, Inc.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Extensions
{
    public class SqlServerDbContextOptions : RelationalDbContextOptions
    {
        public SqlServerDbContextOptions([NotNull] DbContextOptions options)
            : base(options)
        { }

        public virtual SqlServerDbContextOptions MaxBatchSize(int maxBatchSize)
        {
            ((IDbContextOptions)Options)
                .AddOrUpdateExtension<SqlServerOptionsExtension>(x => x.MaxBatchSize = maxBatchSize);

            return this;
        }

        public virtual SqlServerDbContextOptions CommandTimeout(int? commandTimeout)
        {
            ((IDbContextOptions)Options)
                .AddOrUpdateExtension<SqlServerOptionsExtension>(x => x.CommandTimeout = commandTimeout);

            return this;
        }

        public virtual SqlServerDbContextOptions MigrationsAssembly([NotNull] string assemblyName)
        {
            Check.NotEmpty(assemblyName, nameof(assemblyName));

            ((IDbContextOptions)Options)
                .AddOrUpdateExtension<SqlServerOptionsExtension>(x => x.MigrationsAssembly = assemblyName);

            return this;
        }
    }
}
