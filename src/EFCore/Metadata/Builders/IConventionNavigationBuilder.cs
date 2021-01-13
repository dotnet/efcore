// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring an <see cref="IConventionNavigation" /> from conventions.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IConventionNavigationBuilder : IConventionPropertyBaseBuilder
    {
        /// <summary>
        ///     Gets the navigation being configured.
        /// </summary>
        new IConventionNavigation Metadata { get; }

        /// <summary>
        ///     Sets the backing field to use for this navigation.
        /// </summary>
        /// <param name="fieldName"> The field name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        new IConventionNavigationBuilder HasField([CanBeNull] string fieldName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the backing field to use for this navigation.
        /// </summary>
        /// <param name="fieldInfo"> The field. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        new IConventionNavigationBuilder HasField([CanBeNull] FieldInfo fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for this navigation.
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for this navigation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        new IConventionNavigationBuilder UsePropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether this navigation can be configured to be automatically included in a query
        ///     from the current configuration source.
        /// </summary>
        /// <param name="autoInclude"> A value indicating whether the navigation should be automatically included. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if automatically included can be set for this navigation. </returns>
        bool CanSetAutoInclude(bool? autoInclude, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures this navigation to be automatically included in a query.
        /// </summary>
        /// <param name="autoInclude"> A value indicating whether the navigation should be automatically included. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionNavigationBuilder AutoInclude(bool? autoInclude, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether this navigation requiredness can be configured
        ///     from the current configuration source.
        /// </summary>
        /// <param name="required"> A value indicating whether the navigation should be required. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if requiredness can be set for this navigation. </returns>
        bool CanSetIsRequired(bool? required, bool fromDataAnnotation = false);

        /// <summary>
        ///     Configures whether this navigation is required.
        /// </summary>
        /// <param name="required">
        ///     A value indicating whether this is a required navigation.
        ///     <see langword="null" /> to reset to default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the requiredness was configured, <see langword="null" /> otherwise.
        /// </returns>
        IConventionNavigationBuilder IsRequired(bool? required, bool fromDataAnnotation = false);
    }
}
