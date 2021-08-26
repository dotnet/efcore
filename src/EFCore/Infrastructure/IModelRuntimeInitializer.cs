// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Initializes a <see cref="IModel" /> with the runtime dependencies.
    ///         This is typically implemented by database providers to ensure that any
    ///         runtime dependencies specific to their database are used.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see> for more information.
    /// </remarks>
    public interface IModelRuntimeInitializer
    {
        /// <summary>
        ///     Validates and initializes the given model with runtime dependencies.
        /// </summary>
        /// <param name="model"> The model to initialize. </param>
        /// <param name="designTime"> Whether the model should contain design-time configuration.</param>
        /// <param name="validationLogger">
        ///     The validation logger. If <see langword="null"/> is provided validation will not be performed.
        ///     </param>
        /// <returns> The initialized model. </returns>
        IModel Initialize(
            IModel model,
            bool designTime = true,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation>? validationLogger = null);
    }
}
