// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public interface IConventionNavigationBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     Gets the navigation being configured.
        /// </summary>
        new IConventionNavigation Metadata { get; }

        /// <summary>
        ///     Returns a value indicating whether the <see cref="PropertyAccessMode" /> can be set for this navigation
        ///     from the current configuration source.
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for this navigation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the <see cref="PropertyAccessMode" /> can be set for this navigation. </returns>
        bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for this navigation.
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for this navigation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionNavigationBuilder UsePropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);
    }
}
