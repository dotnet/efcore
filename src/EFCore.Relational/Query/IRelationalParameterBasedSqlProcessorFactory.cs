﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A factory for creating <see cref="RelationalParameterBasedSqlProcessor" /> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IRelationalParameterBasedSqlProcessorFactory
    {
        /// <summary>
        ///     Creates a new <see cref="RelationalParameterBasedSqlProcessor" />.
        /// </summary>
        /// <param name="useRelationalNulls"> A bool value indicating if relational nulls should be used. </param>
        /// <returns> A relational parameter based sql processor. </returns>
        RelationalParameterBasedSqlProcessor Create(bool useRelationalNulls);
    }
}
