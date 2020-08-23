// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Factory for finding and creating <see cref="ParameterBinding" /> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
    ///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
    ///         instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IParameterBindingFactory
    {
        /// <summary>
        ///     Checks whether or not this factory can bind a parameter with the given type and name.
        /// </summary>
        /// <param name="parameterType"> The parameter type. </param>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> <see langword="true" /> if this parameter can be bound; <see langword="false" /> otherwise. </returns>
        bool CanBind(
            [NotNull] Type parameterType,
            [NotNull] string parameterName);

        /// <summary>
        ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="parameterType"> The parameter type. </param>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> The binding. </returns>
        ParameterBinding Bind(
            [NotNull] IMutableEntityType entityType,
            [NotNull] Type parameterType,
            [NotNull] string parameterName);

        /// <summary>
        ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="parameterType"> The parameter type. </param>
        /// <param name="parameterName"> The parameter name. </param>
        /// <returns> The binding. </returns>
        ParameterBinding Bind(
            [NotNull] IConventionEntityType entityType,
            [NotNull] Type parameterType,
            [NotNull] string parameterName);
    }
}
