// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a navigation property which can be used to navigate a relationship.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="INavigation" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IConventionNavigation : INavigation, IConventionNavigationBase
    {
        /// <summary>
        ///     Gets the builder that can be used to configure this navigation.
        /// </summary>
        new IConventionNavigationBuilder Builder { get; }

        /// <summary>
        ///     Gets the type that this navigation property belongs to.
        /// </summary>
        new IConventionEntityType DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => (IConventionEntityType)((INavigationBase)this).DeclaringEntityType;
        }

        /// <summary>
        ///     Gets the entity type that this navigation property will hold an instance(s) of.
        /// </summary>
        new IConventionEntityType TargetEntityType
        {
            [DebuggerStepThrough]
            get => (IConventionEntityType)((INavigationBase)this).TargetEntityType;
        }

        /// <summary>
        ///     Returns the configuration source for this navigation property.
        /// </summary>
        /// <returns> The configuration source. </returns>
        ConfigurationSource IConventionPropertyBase.GetConfigurationSource()
            => (ConfigurationSource)(IsOnDependent
                ? ForeignKey.GetDependentToPrincipalConfigurationSource()
                : ForeignKey.GetPrincipalToDependentConfigurationSource());

        /// <summary>
        ///     Gets the foreign key that defines the relationship this navigation property will navigate.
        /// </summary>
        new IConventionForeignKey ForeignKey
        {
            [DebuggerStepThrough]
            get => (IConventionForeignKey)((INavigation)this).ForeignKey;
        }

        /// <summary>
        ///     Gets the inverse navigation.
        /// </summary>
        new IConventionNavigation Inverse
        {
            [DebuggerStepThrough]
            get => (IConventionNavigation)((INavigation)this).Inverse;
        }

        /// <summary>
        ///     Sets the inverse navigation.
        /// </summary>
        /// <param name="inverseName">
        ///     The name of the inverse navigation property. Passing <see langword="null" /> will result in there being
        ///     no inverse navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The new inverse navigation. </returns>
        IConventionNavigation SetInverse([CanBeNull] string inverseName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the inverse navigation.
        /// </summary>
        /// <param name="inverse">
        ///     The name of the inverse navigation property. Passing <see langword="null" /> will result in there being
        ///     no inverse navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The new inverse navigation. </returns>
        IConventionNavigation SetInverse([CanBeNull] MemberInfo inverse, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="Inverse" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="Inverse" />. </returns>
        ConfigurationSource? GetInverseConfigurationSource();
    }
}
