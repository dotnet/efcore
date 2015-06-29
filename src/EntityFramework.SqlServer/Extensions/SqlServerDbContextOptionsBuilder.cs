// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.SqlServer.Extensions
{
    public class SqlServerDbContextOptionsBuilder : RelationalDbContextOptionsBuilder
    {
        public SqlServerDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
        }

        public virtual SqlServerDbContextOptionsBuilder MaxBatchSize(int maxBatchSize)
        {
            var extension = new SqlServerOptionsExtension(OptionsBuilder.Options.GetExtension<SqlServerOptionsExtension>())
            {
                MaxBatchSize = maxBatchSize
            };

            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }

        public virtual SqlServerDbContextOptionsBuilder CommandTimeout(int? commandTimeout)
        {
            var extension = new SqlServerOptionsExtension(OptionsBuilder.Options.GetExtension<SqlServerOptionsExtension>())
            {
                CommandTimeout = commandTimeout
            };

            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }

        public virtual SqlServerDbContextOptionsBuilder MigrationsAssembly([NotNull] string assemblyName)
        {
            var extension = new SqlServerOptionsExtension(OptionsBuilder.Options.GetExtension<SqlServerOptionsExtension>())
            {
                MigrationsAssembly = assemblyName
            };

            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }

        public virtual SqlServerDbContextOptionsBuilder SuppressAmbientTransactionWarning()
        {
            var extension = new SqlServerOptionsExtension(OptionsBuilder.Options.GetExtension<SqlServerOptionsExtension>())
            {
                ThrowOnAmbientTransaction = false
            };

            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }
    }
}
