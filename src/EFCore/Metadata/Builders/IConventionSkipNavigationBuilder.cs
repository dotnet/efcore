// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring an <see cref="IConventionSkipNavigation" /> from conventions.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IConventionSkipNavigationBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     The navigation property being configured.
        /// </summary>
        new IConventionSkipNavigation Metadata { get; }

        /// <summary>
        ///     Sets the backing field to use for this navigation.
        /// </summary>
        /// <param name="fieldName"> The field name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionSkipNavigationBuilder HasField([CanBeNull] string fieldName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the backing field to use for this navigation.
        /// </summary>
        /// <param name="fieldInfo"> The field. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionSkipNavigationBuilder HasField([CanBeNull] FieldInfo fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the backing field can be set for this navigation
        ///     from the given configuration source.
        /// </summary>
        /// <param name="fieldName"> The field name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the backing field can be set for this property. </returns>
        bool CanSetField([CanBeNull] string fieldName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the backing field can be set for this navigation
        ///     from the given configuration source.
        /// </summary>
        /// <param name="fieldInfo"> The field. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the backing field can be set for this property. </returns>
        bool CanSetField([CanBeNull] FieldInfo fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for this navigation.
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for this navigation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionSkipNavigationBuilder UsePropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the <see cref="PropertyAccessMode" /> can be set for this navigation
        ///     from the given configuration source.
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for this navigation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the <see cref="PropertyAccessMode" /> can be set for this property. </returns>
        bool CanSetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the foreign key.
        /// </summary>
        /// <param name="foreignKey">
        ///     The foreign key. Passing <c>null</c> will result in there being no foreign key associated.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionSkipNavigationBuilder HasForeignKey([CanBeNull] IConventionForeignKey foreignKey, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the <see cref="IConventionSkipNavigation.ForeignKey" /> can be set for this navigation
        ///     from the given configuration source.
        /// </summary>
        /// <param name="foreignKey">
        ///     The foreign key. Passing <c>null</c> will result in there being no foreign key associated.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the <see cref="IConventionSkipNavigation.ForeignKey" /> can be set for this property. </returns>
        bool CanSetForeignKey([CanBeNull] IConventionForeignKey foreignKey, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the inverse skip navigation.
        /// </summary>
        /// <param name="inverse">
        ///     The inverse skip navigation. Passing <c>null</c> will result in there being no inverse navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionSkipNavigationBuilder HasInverse([CanBeNull] IConventionSkipNavigation inverse, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the <see cref="IConventionSkipNavigation.Inverse" /> can be set for this navigation
        ///     from the given configuration source.
        /// </summary>
        /// <param name="inverse">
        ///     The inverse skip navigation. Passing <c>null</c> will result in there being no inverse navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the <see cref="IConventionSkipNavigation.Inverse" /> can be set for this property. </returns>
        bool CanSetInverse([CanBeNull] IConventionSkipNavigation inverse, bool fromDataAnnotation = false);
    }
}
