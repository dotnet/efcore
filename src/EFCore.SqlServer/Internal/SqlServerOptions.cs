// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext"/>
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class SqlServerOptions : ISqlServerOptions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(IDbContextOptions options)
        {
            var sqlServerOptions = options.FindExtension<SqlServerOptionsExtension>() ?? new SqlServerOptionsExtension();

            RowNumberPagingEnabled = sqlServerOptions.RowNumberPaging ?? false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Validate(IDbContextOptions options)
        {
            var sqlServerOptions = options.FindExtension<SqlServerOptionsExtension>() ?? new SqlServerOptionsExtension();

            if (RowNumberPagingEnabled != (sqlServerOptions.RowNumberPaging ?? false))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(SqlServerDbContextOptionsBuilder.UseRowNumberForPaging),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool RowNumberPagingEnabled { get; private set; }
    }
}
