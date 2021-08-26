// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Produces an <see cref="IModel" /> based on a context. This is typically implemented by database providers to ensure that any
    ///         conventions and validation specific to their database are used.
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
    public interface IModelSource
    {
        /// <summary>
        ///     Gets the model to be used.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <returns> The model to be used. </returns>
        [Obsolete("Use the overload with ModelCreationDependencies")]
        IModel GetModel(
            DbContext context,
            IConventionSetBuilder conventionSetBuilder);

        /// <summary>
        ///     Gets the model to be used.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <param name="modelDependencies"> The dependencies object for the model. </param>
        /// <returns> The model to be used. </returns>
        [Obsolete("Use the overload with ModelCreationDependencies")]
        IModel GetModel(
            DbContext context,
            IConventionSetBuilder conventionSetBuilder,
            ModelDependencies modelDependencies);

        /// <summary>
        ///     Gets the model to be used.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="modelCreationDependencies"> The dependencies object used during the creation of the model. </param>
        /// <param name="designTime"> Whether the model should contain design-time configuration.</param>
        /// <returns> The model to be used. </returns>
        IModel GetModel(
            DbContext context,
            ModelCreationDependencies modelCreationDependencies,
            bool designTime);
    }
}
