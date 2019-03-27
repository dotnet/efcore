// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Acts as a root for all in-memory databases such that they will be available
    ///     across context instances and service providers as long as the same instance
    ///     of this type is passed to
    ///     <see
    ///         cref="InMemoryDbContextOptionsExtensions.UseInMemoryDatabase{TContext}(DbContextOptionsBuilder{TContext},string,System.Action{Infrastructure.InMemoryDbContextOptionsBuilder})" />
    /// </summary>
    public sealed class InMemoryDatabaseRoot
    {
        /// <summary>
        ///     <para>
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
        ///     </para>
        ///     <para>
        ///         Entity Framework code will set this instance as needed. It should be considered opaque to
        ///         application code; the type of object may change at any time.
        ///     </para>
        /// </summary>
        public object Instance;
    }
}
