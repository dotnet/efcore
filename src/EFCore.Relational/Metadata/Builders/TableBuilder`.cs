// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
        public TableBuilder([NotNull] string name, [CanBeNull] string schema, [NotNull] IMutableEntityType entityType)
            : base(name, schema, entityType)
        {
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
