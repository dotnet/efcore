// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         A <see cref="IReadOnlyPropertyBase" /> in the Entity Framework model that represents an
    ///         injected service from the <see cref="DbContext" />.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IServiceProperty" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">EF Core model building conventions</see> for more information.
    /// </remarks>
    public interface IConventionServiceProperty : IReadOnlyServiceProperty, IConventionPropertyBase
    {
        /// <summary>
        ///     Gets the builder that can be used to configure this service property.
        /// </summary>
        /// <exception cref="InvalidOperationException"> If the service property has been removed from the model. </exception>
        new IConventionServicePropertyBuilder Builder { get; }

        /// <summary>
        ///     Gets the type that this property belongs to.
        /// </summary>
        new IConventionEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Sets the <see cref="ServiceParameterBinding" /> for this property.
        /// </summary>
        /// <param name="parameterBinding"> The parameter binding. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured binding. </returns>
        ServiceParameterBinding? SetParameterBinding(ServiceParameterBinding? parameterBinding, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IReadOnlyServiceProperty.ParameterBinding" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IReadOnlyServiceProperty.ParameterBinding" />. </returns>
        ConfigurationSource? GetParameterBindingConfigurationSource();
    }
}
