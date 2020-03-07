// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

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
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        ///     <para>
        ///         Entity Framework code will set this instance as needed. It should be considered opaque to
        ///         application code; the type of object may change at any time.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public object Instance;
    }
}
