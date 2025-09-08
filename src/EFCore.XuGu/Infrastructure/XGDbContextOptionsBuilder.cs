// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class XGDbContextOptionsBuilder : RelationalDbContextOptionsBuilder<XGDbContextOptionsBuilder, XGOptionsExtension>
    {
        public XGDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
        }

        [Obsolete("Call the Fluent API extension method 'HasCharSet()' on the builder object of your model/entities/properties instead. To get the exact behavior as with `CharSet()`, call 'modelBuilder.HasCharSet(charSet, DelegationModes.ApplyToColumns)'.", true)]
        public virtual XGDbContextOptionsBuilder CharSet(CharSet charSet) // TODO: Remove for EF Core 6.
            => throw new NotImplementedException("Call the Fluent API extension method 'HasCharSet()' on the builder object of your model/entities/properties instead. To get the exact behavior as with `CharSet()`, call 'modelBuilder.HasCharSet(charSet, DelegationModes.ApplyToColumns)'.");

        /// <summary>
        ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
        /// </summary>
        public virtual XGDbContextOptionsBuilder EnableRetryOnFailure()
            => ExecutionStrategy(c => new XGRetryingExecutionStrategy(c));

        /// <summary>
        ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
        /// </summary>
        public virtual XGDbContextOptionsBuilder EnableRetryOnFailure(int maxRetryCount)
            => ExecutionStrategy(c => new XGRetryingExecutionStrategy(c, maxRetryCount));

        /// <summary>
        ///     Configures the context to use the default retrying <see cref="IExecutionStrategy" />.
        /// </summary>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="maxRetryDelay"> The maximum delay between retries. </param>
        /// <param name="errorNumbersToAdd"> Additional error codes that should be considered transient. </param>
        public virtual XGDbContextOptionsBuilder EnableRetryOnFailure(
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [CanBeNull] ICollection<int> errorNumbersToAdd)
            => ExecutionStrategy(c => new XGRetryingExecutionStrategy(c, maxRetryCount, maxRetryDelay, errorNumbersToAdd));

        /// <summary>
        ///     Configures string escaping in SQL query generation to ignore backslashes, and assumes
        ///     that `sql_mode` has been set to `NO_BACKSLASH_ESCAPES`.
        ///     This applies to both constant and parameter values (i. e. user input, potentially).
        /// </summary>
        /// <param name="setSqlModeOnOpen">When `true`, enables the <see cref="SetSqlModeOnOpen" /> option,
        /// which sets `sql_mode` to `NO_BACKSLASH_ESCAPES` automatically, when a connection has been
        /// opened. This is the default.
        /// When `false`, does not change the <see cref="SetSqlModeOnOpen" /> option, when calling this method.</param>
        public virtual XGDbContextOptionsBuilder DisableBackslashEscaping(bool setSqlModeOnOpen = true)
        {
            var builder = WithOption(e => e.WithDisabledBackslashEscaping());

            if (setSqlModeOnOpen)
            {
                builder = builder.WithOption(e => e.WithSettingSqlModeOnOpen());
            }

            return builder;
        }

        /// <summary>
        ///     When `true`, implicitly executes a `SET SESSION sql_mode` statement after opening
        ///     a connection to the database server, adding the modes enabled by other options.
        ///     When `false`, the `sql_mode` is not being set by the provider and has to be manually
        ///     handled by the caller, to synchronize it with other options that have been set.
        /// </summary>
        public virtual XGDbContextOptionsBuilder SetSqlModeOnOpen()
            => WithOption(e => e.WithSettingSqlModeOnOpen());

        /// <summary>
        ///     Skip replacing `\r` and `\n` with `CHAR()` calls in strings inside queries.
        /// </summary>
        public virtual XGDbContextOptionsBuilder DisableLineBreakToCharSubstition()
            => WithOption(e => e.WithDisabledLineBreakToCharSubstition());

        /// <summary>
        ///     Configures default mappings between specific CLR and MySQL types.
        /// </summary>
        public virtual XGDbContextOptionsBuilder DefaultDataTypeMappings(Func<XGDefaultDataTypeMappings, XGDefaultDataTypeMappings> defaultDataTypeMappings)
            => WithOption(e => e.WithDefaultDataTypeMappings(defaultDataTypeMappings(new XGDefaultDataTypeMappings())));

        /// <summary>
        ///     Configures the behavior for cases when a schema has been set for an entity. Because
        ///     MySQL does not support the EF Core concept of schemas, the default is to throw an
        ///     exception.
        /// </summary>
        public virtual XGDbContextOptionsBuilder SchemaBehavior(XGSchemaBehavior behavior, XGSchemaNameTranslator translator = null)
            => WithOption(e => e.WithSchemaBehavior(behavior, translator));

        /// <summary>
        ///     Configures the context to optimize `System.Boolean` mapped columns for index usage,
        ///     by translating `e.BoolColumn` to `BoolColumn = TRUE` and `!e.BoolColumn` to `BoolColumn = FALSE`.
        /// </summary>
        public virtual XGDbContextOptionsBuilder EnableIndexOptimizedBooleanColumns(bool enable = true)
            => WithOption(e => e.WithIndexOptimizedBooleanColumns(enable));

        /// <summary>
        ///     Configures the context to automatically limit the length of `System.String` mapped columns, that have not explicitly mapped
        ///     to a store type (e.g. `varchar(1024)`), to ensure that at least two indexed columns will be allowed on a given table (this
        ///     is the default if you don't configure this option).
        ///     If you intend to use `HasPrefixLength()` for those kind of columns, set this option to `false`.
        /// </summary>
        public virtual XGDbContextOptionsBuilder LimitKeyedOrIndexedStringColumnLength(bool enable = true)
            => WithOption(e => e.WithKeyedOrIndexedStringColumnLengthLimit(enable));

        /// <summary>
        ///     Configures the context to translate string related methods, containing a parameter of type <see cref="StringComparison"/>,
        ///     to their SQL equivalent, even though MySQL might not be able to use indexes when executing the query, resulting in decreased
        ///     performance. Whether MySQL is able to use indexes for the query, depends on the <see cref="StringComparison"/> option, the
        ///     underlying collation and the scenario.
        ///     It is also possible to just use `EF.Functions.Collate()`, possibly in addition to `string.ToUpper()` if needed, to achieve
        ///     the same result but with full control over the SQL generation.
        /// </summary>
        public virtual XGDbContextOptionsBuilder EnableStringComparisonTranslations(bool enable = true)
            => WithOption(e => e.WithStringComparisonTranslations(enable));

        /// <summary>
        ///     Configures the context to translate using primitive collections. At the time of the Pomelo 8.0.0 release, MySQL Server can
        ///     crash when using primitive collections with JSON and MariaDB support is incomplete. Support and translations in regards to
        ///     this option can change at any time in the future. This optin is disabled by default. Enabled at your own risk.
        /// </summary>
        public virtual XGDbContextOptionsBuilder EnablePrimitiveCollectionsSupport(bool enable = true)
            => WithOption(e => e.WithPrimitiveCollectionsSupport(enable));
    }
}
