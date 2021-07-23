﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    public class TemporalTableBuilder<TEntity> : TemporalTableBuilder
        where TEntity : class
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public TemporalTableBuilder(EntityTypeBuilder<TEntity> entityTypeBuilder)
            : base(entityTypeBuilder)
        {
        }

        /// <summary>
        ///     Configures a history table for the entity mapped to a temporal table.
        /// </summary>
        /// <param name="name"> The name of the history table. </param>
        /// <param name="schema"> The schema of the history table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public new virtual TemporalTableBuilder<TEntity> WithHistoryTable(string name, string? schema = null)
            => (TemporalTableBuilder<TEntity>)base.WithHistoryTable(name, schema);
    }
}
