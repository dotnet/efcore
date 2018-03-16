// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemorySingletonOptions : IInMemorySingletonOptions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(IDbContextOptions options)
        {
            var inMemoryOptions = options.FindExtension<InMemoryOptionsExtension>();

            if (inMemoryOptions != null)
            {
                DatabaseRoot = inMemoryOptions.DatabaseRoot;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Validate(IDbContextOptions options)
        {
            var inMemoryOptions = options.FindExtension<InMemoryOptionsExtension>();

            if (inMemoryOptions != null
                && DatabaseRoot != inMemoryOptions.DatabaseRoot)
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(InMemoryDbContextOptionsExtensions.UseInMemoryDatabase),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InMemoryDatabaseRoot DatabaseRoot { get; private set; }
    }
}
