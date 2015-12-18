// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public abstract class RelationalDbContextOptionsBuilder<TBuilder, TExtension>
        where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
        where TExtension : RelationalOptionsExtension
    {
        private static readonly int _maxQueryClientEvaluationBehavior
            = Enum.GetValues(typeof(QueryClientEvaluationBehavior)).Cast<int>().Max();

        protected RelationalDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            OptionsBuilder = optionsBuilder;
        }

        protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

        protected abstract TExtension CloneExtension();

        public virtual TBuilder MaxBatchSize(int maxBatchSize)
            => SetOption(e => e.MaxBatchSize = maxBatchSize);

        public virtual TBuilder CommandTimeout(int? commandTimeout)
            => SetOption(e => e.CommandTimeout = commandTimeout);

        public virtual TBuilder MigrationsAssembly([NotNull] string assemblyName)
            => SetOption(e => e.MigrationsAssembly = Check.NullButNotEmpty(assemblyName, nameof(assemblyName)));

        public virtual TBuilder MigrationsHistoryTable([NotNull] string tableName, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(tableName, nameof(tableName));
            Check.NullButNotEmpty(schema, nameof(schema));

            return SetOption(
                e =>
                    {
                        e.MigrationsHistoryTableName = tableName;
                        e.MigrationsHistoryTableSchema = schema;
                    });
        }

        public virtual TBuilder SuppressAmbientTransactionWarning()
            => SetOption(e => e.ThrowOnAmbientTransaction = false);

        public virtual TBuilder UseRelationalNulls()
            => SetOption(e => e.UseRelationalNulls = true);

        public virtual TBuilder QueryClientEvaluationBehavior(QueryClientEvaluationBehavior queryClientEvaluationBehavior)
        {
            if (queryClientEvaluationBehavior < 0
                || (int)queryClientEvaluationBehavior > _maxQueryClientEvaluationBehavior)
            {
                throw new ArgumentException(CoreStrings.InvalidEnumValue(
                    nameof(queryClientEvaluationBehavior), typeof(QueryClientEvaluationBehavior)));
            }

            return SetOption(e => e.QueryClientEvaluationBehavior = queryClientEvaluationBehavior);
        }

        protected virtual TBuilder SetOption([NotNull] Action<TExtension> setAction)
        {
            Check.NotNull(setAction, nameof(setAction));

            var extension = CloneExtension();

            setAction(extension);

            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

            return (TBuilder)this;
        }
    }
}
