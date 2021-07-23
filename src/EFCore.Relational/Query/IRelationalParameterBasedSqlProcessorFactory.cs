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
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
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
