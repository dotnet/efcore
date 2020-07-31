// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a relationship where a foreign key property(s) in a dependent entity type
    ///         reference a corresponding primary or alternate key in a principal entity type.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IForeignKey" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IConventionForeignKey : IForeignKey, IConventionAnnotatable
    {
        /// <summary>
        ///     Gets the builder that can be used to configure this foreign key.
        /// </summary>
        new IConventionForeignKeyBuilder Builder { get; }

        /// <summary>
        ///     Gets the foreign key properties in the dependent entity.
        /// </summary>
        new IReadOnlyList<IConventionProperty> Properties { get; }

        /// <summary>
        ///     Gets the primary or alternate key that the relationship targets.
        /// </summary>
        new IConventionKey PrincipalKey { get; }

        /// <summary>
        ///     Gets the dependent entity type. This may be different from the type that <see cref="Properties" />
        ///     are defined on when the relationship is defined a derived type in an inheritance hierarchy (since the properties
        ///     may be defined on a base type).
        /// </summary>
        new IConventionEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the principal entity type that this relationship targets. This may be different from the type that
        ///     <see cref="PrincipalKey" /> is defined on when the relationship targets a derived type in an inheritance
        ///     hierarchy (since the key is defined on the base type of the hierarchy).
        /// </summary>
        new IConventionEntityType PrincipalEntityType { get; }

        /// <summary>
        ///     Gets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        new IConventionNavigation DependentToPrincipal { get; }

        /// <summary>
        ///     Gets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        new IConventionNavigation PrincipalToDependent { get; }

        /// <summary>
        ///     Returns the configuration source for this property.
        /// </summary>
        /// <returns> The configuration source. </returns>
        ConfigurationSource GetConfigurationSource();

        /// <summary>
        ///     Sets the foreign key properties and that target principal key.
        /// </summary>
        /// <param name="properties"> Foreign key properties in the dependent entity. </param>
        /// <param name="principalKey"> The primary or alternate key to target. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured foreign key properties. </returns>
        IReadOnlyList<IConventionProperty> SetProperties(
            [NotNull] IReadOnlyList<IConventionProperty> properties,
            [NotNull] IConventionKey principalKey,
            bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IForeignKey.Properties" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IForeignKey.Properties" />. </returns>
        ConfigurationSource? GetPropertiesConfigurationSource();

        /// <summary>
        ///     Returns the configuration source for <see cref="IForeignKey.PrincipalKey" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IForeignKey.PrincipalKey" />. </returns>
        ConfigurationSource? GetPrincipalKeyConfigurationSource();

        /// <summary>
        ///     Returns the configuration source for <see cref="IForeignKey.PrincipalEntityType" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IForeignKey.PrincipalEntityType" />. </returns>
        ConfigurationSource? GetPrincipalEndConfigurationSource();

        /// <summary>
        ///     Sets a value indicating whether the values assigned to the foreign key properties are unique.
        /// </summary>
        /// <param name="unique"> A value indicating whether the values assigned to the foreign key properties are unique. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured uniqueness. </returns>
        bool? SetIsUnique(bool? unique, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IForeignKey.IsUnique" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IForeignKey.IsUnique" />. </returns>
        ConfigurationSource? GetIsUniqueConfigurationSource();

        /// <summary>
        ///     Sets a value indicating whether the principal entity is required.
        ///     If <see langword="true" />, the dependent entity must always be assigned to a valid principal entity.
        /// </summary>
        /// <param name="required"> A value indicating whether the principal entity is required. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured requiredness. </returns>
        bool? SetIsRequired(bool? required, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IForeignKey.IsRequired" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IForeignKey.IsRequired" />. </returns>
        ConfigurationSource? GetIsRequiredConfigurationSource();

        /// <summary>
        ///     Sets a value indicating whether the dependent entity is required.
        ///     If <see langword="true" />, the principal entity must always have a valid dependent entity assigned.
        /// </summary>
        /// <param name="required"> A value indicating whether the dependent entity is required. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured requiredness. </returns>
        bool? SetIsRequiredDependent(bool? required, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IForeignKey.IsRequiredDependent" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IForeignKey.IsRequiredDependent" />. </returns>
        ConfigurationSource? GetIsRequiredDependentConfigurationSource();

        /// <summary>
        ///     Sets a value indicating whether this relationship defines an ownership.
        ///     If <see langword="true" />, the dependent entity must always be accessed via the navigation from the principal entity.
        /// </summary>
        /// <param name="ownership"> A value indicating whether this relationship defines an ownership. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured ownership. </returns>
        bool? SetIsOwnership(bool? ownership, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IForeignKey.IsOwnership" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IForeignKey.IsOwnership" />. </returns>
        ConfigurationSource? GetIsOwnershipConfigurationSource();

        /// <summary>
        ///     Sets a value indicating how a delete operation is applied to dependent entities in the relationship when the
        ///     principal is deleted or the relationship is severed.
        /// </summary>
        /// <param name="deleteBehavior">
        ///     A value indicating how a delete operation is applied to dependent entities in the relationship when the
        ///     principal is deleted or the relationship is severed.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured behavior. </returns>
        DeleteBehavior? SetDeleteBehavior(DeleteBehavior? deleteBehavior, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns the configuration source for <see cref="IForeignKey.DeleteBehavior" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IForeignKey.DeleteBehavior" />. </returns>
        ConfigurationSource? GetDeleteBehaviorConfigurationSource();

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="name">
        ///     The name of the navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        IConventionNavigation SetDependentToPrincipal([CanBeNull] string name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="property">
        ///     The navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        IConventionNavigation SetDependentToPrincipal([CanBeNull] MemberInfo property, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="name">
        ///     The name of the navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetDependentToPrincipal")]
        IConventionNavigation HasDependentToPrincipal([CanBeNull] string name, bool fromDataAnnotation = false)
            => SetDependentToPrincipal(name, fromDataAnnotation);

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="property">
        ///     The navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetDependentToPrincipal")]
        IConventionNavigation HasDependentToPrincipal([CanBeNull] MemberInfo property, bool fromDataAnnotation = false)
            => SetDependentToPrincipal(property, fromDataAnnotation);

        /// <summary>
        ///     Returns the configuration source for <see cref="IForeignKey.DependentToPrincipal" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IForeignKey.DependentToPrincipal" />. </returns>
        ConfigurationSource? GetDependentToPrincipalConfigurationSource();

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="name">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        IConventionNavigation SetPrincipalToDependent([CanBeNull] string name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="property">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        IConventionNavigation SetPrincipalToDependent([CanBeNull] MemberInfo property, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="name">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetPrincipalToDependent")]
        IConventionNavigation HasPrincipalToDependent([CanBeNull] string name, bool fromDataAnnotation = false)
            => SetPrincipalToDependent(name, fromDataAnnotation);

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="property">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetPrincipalToDependent")]
        IConventionNavigation HasPrincipalToDependent([CanBeNull] MemberInfo property, bool fromDataAnnotation = false)
            => SetPrincipalToDependent(property, fromDataAnnotation);

        /// <summary>
        ///     Returns the configuration source for <see cref="IForeignKey.PrincipalToDependent" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IForeignKey.PrincipalToDependent" />. </returns>
        ConfigurationSource? GetPrincipalToDependentConfigurationSource();

        /// <summary>
        ///     Gets all skip navigations using this foreign key.
        /// </summary>
        /// <returns> The skip navigations using this foreign key. </returns>
        new IEnumerable<IConventionSkipNavigation> GetReferencingSkipNavigations()
            => ((IForeignKey)this).GetReferencingSkipNavigations().Cast<IConventionSkipNavigation>();
    }
}
