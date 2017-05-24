// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         This interface must be implemented by any service that needs to be reset between
    ///         different uses of the same <see cref="DbContext" /> in different pools.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IResettableService
    {
        /// <summary>
        ///     Resets the service so that it can be used from the pool.
        /// </summary>
        void ResetState();
    }
}
