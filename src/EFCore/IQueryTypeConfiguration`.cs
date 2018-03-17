// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Allows configuration for a query type to be factored into a separate class,
    ///     rather than in-line in <see cref="DbContext.OnModelCreating(ModelBuilder)" />.
    ///     Implement this interface, applying configuration for the query in the
    ///     <see cref="Configure(QueryTypeBuilder{TQuery})" /> method,
    ///     and then apply the configuration to the model using
    ///     <see cref="ModelBuilder.ApplyConfiguration{TQuery}(IQueryTypeConfiguration{TQuery})" />
    ///     in <see cref="DbContext.OnModelCreating(ModelBuilder)" />.
    /// </summary>
    /// <typeparam name="TQuery"> The query type to be configured. </typeparam>
    public interface IQueryTypeConfiguration<TQuery>
        where TQuery : class
    {
        /// <summary>
        ///     Configures the query of type <typeparamref name="TQuery" />.
        /// </summary>
        /// <param name="builder"> The builder to be used to configure the query type. </param>
        void Configure(QueryTypeBuilder<TQuery> builder);
    }
}
