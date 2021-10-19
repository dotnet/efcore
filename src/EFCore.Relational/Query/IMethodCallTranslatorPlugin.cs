// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Represents plugin for <see cref="IMethodCallTranslator" />.
    /// </summary>
    /// <remarks>
    ///     The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
    ///     are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
    ///     instances. The implementation must be thread-safe.
    ///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    /// </remarks>
    public interface IMethodCallTranslatorPlugin
    {
        /// <summary>
        ///     Gets the method call translators.
        /// </summary>
        IEnumerable<IMethodCallTranslator> Translators { get; }
    }
}
