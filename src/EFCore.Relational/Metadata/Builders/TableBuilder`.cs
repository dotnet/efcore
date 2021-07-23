// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
    public class TableBuilder<TEntity> : TableBuilder
        where TEntity : class
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public TableBuilder(string? name, string? schema, EntityTypeBuilder<TEntity> entityTypeBuilder)
            : base(name, schema, entityTypeBuilder)
        {
        }

        /// <summary>
        ///     The entity type builder for the entity being configured.
        /// </summary>
        public new virtual EntityTypeBuilder<TEntity> EntityTypeBuilder
        {
            [DebuggerStepThrough] get => (EntityTypeBuilder<TEntity>)base.EntityTypeBuilder;
        }

        /// <summary>
        ///     Configures the table to be ignored by migrations.
        /// </summary>
        /// <param name="excluded"> A value indicating whether the table should be managed by migrations. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual TableBuilder<TEntity> ExcludeFromMigrations(bool excluded = true)
            => (TableBuilder<TEntity>)base.ExcludeFromMigrations(excluded);
    }
}
