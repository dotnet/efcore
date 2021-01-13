// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
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
    public interface IModelSource
    {
        /// <summary>
        ///     Gets the model to be used.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <returns> The model to be used. </returns>
        [Obsolete("Use the overload with ModelDependencies")]
        IModel GetModel(
            [NotNull] DbContext context,
            [NotNull] IConventionSetBuilder conventionSetBuilder);

        /// <summary>
        ///     Gets the model to be used.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <param name="modelDependencies"> The dependencies object for the model. </param>
        /// <returns> The model to be used. </returns>
        IModel GetModel(
            [NotNull] DbContext context,
            [NotNull] IConventionSetBuilder conventionSetBuilder,
            [NotNull] ModelDependencies modelDependencies);
    }
}
