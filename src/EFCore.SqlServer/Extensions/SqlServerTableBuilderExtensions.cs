// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="TableBuilder" />.
    /// </summary>
    public static class SqlServerTableBuilderExtensions
    {
        /// <summary>
        ///     Configures the table as temporal.
        /// </summary>
        /// <param name="tableBuilder"> The builder for the table being configured. </param>
        /// <param name="temporal"> A value indicating whether the table is temporal. </param>
        /// <returns> An object that can be used to configure the temporal table. </returns>
        public static TemporalTableBuilder IsTemporal(
            this TableBuilder tableBuilder,
            bool temporal = true)
        {
            tableBuilder.EntityTypeBuilder.Metadata.SetIsTemporal(temporal);

            return new TemporalTableBuilder(tableBuilder.EntityTypeBuilder);
        }

        /// <summary>
        ///     Configures the table as temporal.
        /// </summary>
        /// <param name="tableBuilder"> The builder for the table being configured. </param>
        /// <param name="buildAction"> An action that performs configuration of the temporal table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static TableBuilder IsTemporal(
            this TableBuilder tableBuilder,
            Action<TemporalTableBuilder> buildAction)
        {
            tableBuilder.EntityTypeBuilder.Metadata.SetIsTemporal(true);
            buildAction(new TemporalTableBuilder(tableBuilder.EntityTypeBuilder));

            return tableBuilder;
        }

        /// <summary>
        ///     Configures the table as temporal.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="tableBuilder"> The builder for the table being configured. </param>
        /// <param name="temporal"> A value indicating whether the table is temporal. </param>
        /// <returns> An object that can be used to configure the temporal table. </returns>
        public static TemporalTableBuilder<TEntity> IsTemporal<TEntity>(
            this TableBuilder<TEntity> tableBuilder,
            bool temporal = true)
            where TEntity : class
        {
            tableBuilder.EntityTypeBuilder.Metadata.SetIsTemporal(temporal);

            return new TemporalTableBuilder<TEntity>(tableBuilder.EntityTypeBuilder);
        }

        /// <summary>
        ///     Configures the table as temporal.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="tableBuilder"> The builder for the table being configured. </param>
        /// <param name="buildAction"> An action that performs configuration of the temporal table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static TableBuilder<TEntity> IsTemporal<TEntity>(
            this TableBuilder<TEntity> tableBuilder,
            Action<TemporalTableBuilder<TEntity>> buildAction)
            where TEntity: class
        {
            tableBuilder.EntityTypeBuilder.Metadata.SetIsTemporal(true);
            buildAction(new TemporalTableBuilder<TEntity>(tableBuilder.EntityTypeBuilder));

            return tableBuilder;
        }
    }
}
