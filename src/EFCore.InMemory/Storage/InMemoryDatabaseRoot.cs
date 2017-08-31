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
        ///     Entity Framework code will set this instance as needed. It should be considered opaque to
        ///     application code. The type of object may change at any time.
        /// </summary>
        public object Instance;
    }
}
