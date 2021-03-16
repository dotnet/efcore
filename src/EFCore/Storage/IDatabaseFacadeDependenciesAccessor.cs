// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Provides access to <see cref="IDatabaseFacadeDependencies" /> for providers and extensions.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IDatabaseFacadeDependenciesAccessor
    {
        /// <summary>
        ///     The dependencies.
        /// </summary>
        IDatabaseFacadeDependencies Dependencies { get; }

        /// <summary>
        ///     The DbContext instance associated with the database facade.
        /// </summary>
        DbContext Context { get; }
    }
}
