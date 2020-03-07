// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Allows relational database specific configuration to be performed on <see cref="DbContextOptions" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are typically returned from methods that configure the context to use a
    ///         particular relational database provider.
    ///     </para>
    /// </summary>
    public abstract class RelationalDbContextOptionsBuilder<TBuilder, TExtension> : IRelationalDbContextOptionsBuilderInfrastructure
        where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
        where TExtension : RelationalOptionsExtension, new()
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalDbContextOptionsBuilder{TBuilder, TExtension}" /> class.
        /// </summary>
        /// <param name="optionsBuilder"> The core options builder. </param>
        protected RelationalDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            OptionsBuilder = optionsBuilder;
        }

        /// <summary>
        ///     Gets the core options builder.
        /// </summary>
        protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

        /// <inheritdoc />
        DbContextOptionsBuilder IRelationalDbContextOptionsBuilderInfrastructure.OptionsBuilder => OptionsBuilder;

        /// <summary>
        ///     Configures the maximum number of statements that will be included in commands sent to the database
        ///     during <see cref="DbContext.SaveChanges()" />.
        /// </summary>
        /// <param name="maxBatchSize"> The maximum number of statements. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual TBuilder MaxBatchSize(int maxBatchSize)
            => WithOption(e => (TExtension)e.WithMaxBatchSize(maxBatchSize));

        /// <summary>
        ///     Configures the minimum number of statements that are needed for a multi-statement command sent to the database
        ///     during <see cref="DbContext.SaveChanges()" />.
        /// </summary>
        /// <param name="minBatchSize"> The minimum number of statements. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual TBuilder MinBatchSize(int minBatchSize)
            => WithOption(e => (TExtension)e.WithMinBatchSize(minBatchSize));

        /// <summary>
        ///     Configures the wait time (in seconds) before terminating the attempt to execute a command and generating an error.
        /// </summary>
        /// <param name="commandTimeout"> The time in seconds to wait for the command to execute. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual TBuilder CommandTimeout(int? commandTimeout)
            => WithOption(e => (TExtension)e.WithCommandTimeout(commandTimeout));

        /// <summary>
        ///     Configures the assembly where migrations are maintained for this context.
        /// </summary>
        /// <param name="assemblyName"> The name of the assembly. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual TBuilder MigrationsAssembly([CanBeNull] string assemblyName)
            => WithOption(e => (TExtension)e.WithMigrationsAssembly(Check.NullButNotEmpty(assemblyName, nameof(assemblyName))));

        /// <summary>
        ///     Configures the name of the table used to record which migrations have been applied to the database.
        /// </summary>
        /// <param name="tableName"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual TBuilder MigrationsHistoryTable([NotNull] string tableName, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(tableName, nameof(tableName));
            Check.NullButNotEmpty(schema, nameof(schema));

            return WithOption(e => (TExtension)e.WithMigrationsHistoryTableName(tableName).WithMigrationsHistoryTableSchema(schema));
        }

        /// <summary>
        ///     Configures the context to use relational database semantics when comparing null values. By default,
        ///     Entity Framework will use C# semantics for null values, and generate SQL to compensate for differences
        ///     in how the database handles nulls.
        /// </summary>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual TBuilder UseRelationalNulls(bool useRelationalNulls = true)
            => WithOption(e => (TExtension)e.WithUseRelationalNulls(useRelationalNulls));

        /// <summary>
        ///     Configures the context to use the provided <see cref="IExecutionStrategy" />.
        /// </summary>
        /// <param name="getExecutionStrategy"> A function that returns a new instance of an execution strategy. </param>
        public virtual TBuilder ExecutionStrategy(
            [NotNull] Func<ExecutionStrategyDependencies, IExecutionStrategy> getExecutionStrategy)
            => WithOption(
                e => (TExtension)e.WithExecutionStrategyFactory(Check.NotNull(getExecutionStrategy, nameof(getExecutionStrategy))));

        /// <summary>
        ///     Sets an option by cloning the extension used to store the settings. This ensures the builder
        ///     does not modify options that are already in use elsewhere.
        /// </summary>
        /// <param name="setAction"> An action to set the option. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        protected virtual TBuilder WithOption([NotNull] Func<TExtension, TExtension> setAction)
        {
            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(
                setAction(OptionsBuilder.Options.FindExtension<TExtension>() ?? new TExtension()));

            return (TBuilder)this;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
