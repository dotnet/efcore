// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Finds a <see cref="ParameterBinding" /> specifically for some form of property
    ///         (that is, some <see cref="IPropertyBase" />) of the model.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IPropertyParameterBindingFactory
    {
        /// <summary>
        ///     Finds a <see cref="ParameterBinding" /> specifically for an <see cref="IPropertyBase" /> in the model.
        /// </summary>
        /// <param name="entityType"> The entity type on which the <see cref="IPropertyBase" /> is defined. </param>
        /// <param name="parameterType"> The parameter name. </param>
        /// <param name="parameterName"> The parameter type. </param>
        /// <returns> The parameter binding, or <see langword="null" /> if none was found. </returns>
        ParameterBinding? FindParameter(
            IEntityType entityType,
            Type parameterType,
            string parameterName);
    }
}
