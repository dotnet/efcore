// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;

namespace Microsoft.Data.Entity.SqlServer.Extensions
{
    public class SqlServerEntityOptionsBuilder : RelationalEntityOptionsBuilder
    {
        public SqlServerEntityOptionsBuilder([NotNull] EntityOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
        }

        public virtual SqlServerEntityOptionsBuilder MaxBatchSize(int maxBatchSize)
        {
            var extension = new SqlServerOptionsExtension(OptionsBuilder.Options.GetExtension<SqlServerOptionsExtension>())
                {
                    MaxBatchSize = maxBatchSize
                };

            ((IOptionsBuilderExtender)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }

        public virtual SqlServerEntityOptionsBuilder CommandTimeout(int? commandTimeout)
        {
            var extension = new SqlServerOptionsExtension(OptionsBuilder.Options.GetExtension<SqlServerOptionsExtension>())
                {
                    CommandTimeout = commandTimeout
                };

            ((IOptionsBuilderExtender)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }

        public virtual SqlServerEntityOptionsBuilder MigrationsAssembly([NotNull] string assemblyName)
        {
            var extension = new SqlServerOptionsExtension(OptionsBuilder.Options.GetExtension<SqlServerOptionsExtension>())
                {
                    MigrationsAssembly = assemblyName
                };

            ((IOptionsBuilderExtender)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }

        public virtual SqlServerEntityOptionsBuilder SuppressAmbientTransactionWarning()
        {
            var extension = new SqlServerOptionsExtension(OptionsBuilder.Options.GetExtension<SqlServerOptionsExtension>())
                {
                    ThrowOnAmbientTransaction = false
                };

            ((IOptionsBuilderExtender)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }
    }
}
