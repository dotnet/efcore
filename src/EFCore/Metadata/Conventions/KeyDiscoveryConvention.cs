// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     <para>
    ///         A convention that finds primary key property for the entity type based on the names, ignoring case:
    ///             * Id
    ///             * [entity name]Id
    ///     </para>
    ///         If the entity type is owned through a reference navigation property then the corresponding foreign key
    ///         properties are used.
    ///     <para>
    ///     </para>
    ///     <para>
    ///         If the entity type is owned through a collection navigation property then a composite primary key
    ///         is configured using the foreign key properties with an extra property that matches the naming convention above.
    ///     </para>
    /// </summary>
    public class KeyDiscoveryConvention :
        IEntityTypeAddedConvention,
        IPropertyAddedConvention,
        IKeyRemovedConvention,
        IEntityTypeBaseTypeChangedConvention,
        IPropertyFieldChangedConvention,
        IForeignKeyAddedConvention,
        IForeignKeyRemovedConvention,
        IForeignKeyPropertiesChangedConvention,
        IForeignKeyUniquenessChangedConvention,
        IForeignKeyOwnershipChangedConvention
    {
        private const string KeySuffix = "Id";

        /// <summary>
        ///     Creates a new instance of <see cref="KeyDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public KeyDiscoveryConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        private void TryConfigurePrimaryKey(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (entityType.BaseType != null
                || entityType.IsKeyless
                || !entityTypeBuilder.CanSetPrimaryKey(null))
            {
                return;
            }

            List<IConventionProperty> keyProperties = null;
            var definingFk = entityType.FindDefiningNavigation()?.ForeignKey
                             ?? entityType.FindOwnership();
            if (definingFk != null
                && definingFk.DeclaringEntityType != entityType)
            {
                definingFk = null;
            }

            if (definingFk?.IsUnique == true)
            {
                keyProperties = definingFk.Properties.ToList();
            }

            if (keyProperties == null)
            {
                var candidateProperties = entityType.GetProperties().Where(
                    p => !p.IsShadowProperty()
                         || !ConfigurationSource.Convention.Overrides(p.GetConfigurationSource()));
                keyProperties = DiscoverKeyProperties(entityType, candidateProperties).ToList();
                if (keyProperties.Count > 1)
                {
                    Dependencies.Logger.MultiplePrimaryKeyCandidates(keyProperties[0], keyProperties[1]);
                    return;
                }
            }

            if (definingFk?.IsUnique == false)
            {
                if (keyProperties.Count == 0
                    || definingFk.Properties.Contains(keyProperties.First()))
                {
                    var shadowProperty = entityType.FindPrimaryKey()?.Properties.Last();
                    if (shadowProperty == null
                        || entityType.FindPrimaryKey().Properties.Count == 1
                        || definingFk.Properties.Contains(shadowProperty))
                    {
                        shadowProperty = ((InternalEntityTypeBuilder)entityTypeBuilder)
                            .CreateUniqueProperty("Id", typeof(int), isRequired: true);
                    }

                    keyProperties.Clear();
                    keyProperties.Add(shadowProperty);
                }

                var extraProperty = keyProperties[0];
                keyProperties.RemoveAt(0);
                keyProperties.AddRange(definingFk.Properties);
                keyProperties.Add(extraProperty);
            }

            if (keyProperties.Count > 0)
            {
                entityTypeBuilder.PrimaryKey(keyProperties);
            }
        }

        /// <summary>
        ///     Returns the properties that should be used for the primary key.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="candidateProperties"> The properties to consider. </param>
        /// <returns> The properties that should be used for the primary key. </returns>
        public static IEnumerable<IConventionProperty> DiscoverKeyProperties(
            [NotNull] IConventionEntityType entityType,
            [NotNull] IEnumerable<IConventionProperty> candidateProperties)
        {
            Check.NotNull(entityType, nameof(entityType));

            // ReSharper disable PossibleMultipleEnumeration
            var keyProperties = candidateProperties.Where(p => string.Equals(p.Name, KeySuffix, StringComparison.OrdinalIgnoreCase));
            if (!keyProperties.Any())
            {
                var entityTypeName = entityType.ShortName();
                keyProperties = candidateProperties.Where(
                    p => p.Name.Length == entityTypeName.Length + KeySuffix.Length
                         && p.Name.StartsWith(entityTypeName, StringComparison.OrdinalIgnoreCase)
                         && p.Name.EndsWith(KeySuffix, StringComparison.OrdinalIgnoreCase));
            }

            return keyProperties;
            // ReSharper restore PossibleMultipleEnumeration
        }

        /// <summary>
        ///     Called after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
            => TryConfigurePrimaryKey(entityTypeBuilder);

        /// <summary>
        ///     Called after the base type of an entity type changes.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="newBaseType"> The new base entity type. </param>
        /// <param name="oldBaseType"> The old base entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType newBaseType,
            IConventionEntityType oldBaseType,
            IConventionContext<IConventionEntityType> context)
        {
            if (entityTypeBuilder.Metadata.BaseType != newBaseType)
            {
                return;
            }

            TryConfigurePrimaryKey(entityTypeBuilder);
        }

        /// <summary>
        ///     Called after a property is added to the entity type.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder, IConventionContext<IConventionPropertyBuilder> context)
        {
            TryConfigurePrimaryKey(propertyBuilder.Metadata.DeclaringEntityType.Builder);
        }

        /// <summary>
        ///     Called after the backing field for a property is changed.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="newFieldInfo"> The new field. </param>
        /// <param name="oldFieldInfo"> The old field. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessPropertyFieldChanged(
            IConventionPropertyBuilder propertyBuilder,
            FieldInfo newFieldInfo,
            FieldInfo oldFieldInfo,
            IConventionContext<FieldInfo> context)
            => TryConfigurePrimaryKey(propertyBuilder.Metadata.DeclaringEntityType.Builder);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ProcessKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder, IConventionKey key, IConventionContext<IConventionKey> context)
        {
            if (entityTypeBuilder.Metadata.FindPrimaryKey() == null)
            {
                TryConfigurePrimaryKey(entityTypeBuilder);
            }
        }

        /// <summary>
        ///     Called after a foreign key is added to the entity type.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessForeignKeyAdded(
            IConventionRelationshipBuilder relationshipBuilder,
            IConventionContext<IConventionRelationshipBuilder> context)
        {
            if (relationshipBuilder.Metadata.IsOwnership)
            {
                TryConfigurePrimaryKey(relationshipBuilder.Metadata.DeclaringEntityType.Builder);
            }
        }

        /// <summary>
        ///     Called after the foreign key properties or principal key are changed.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="oldDependentProperties"> The old foreign key properties. </param>
        /// <param name="oldPrincipalKey"> The old principal key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessForeignKeyPropertiesChanged(
            IConventionRelationshipBuilder relationshipBuilder,
            IReadOnlyList<IConventionProperty> oldDependentProperties,
            IConventionKey oldPrincipalKey,
            IConventionContext<IConventionRelationshipBuilder> context)
        {
            var foreignKey = relationshipBuilder.Metadata;
            if (foreignKey.IsOwnership
                && !foreignKey.Properties.SequenceEqual(oldDependentProperties)
                && relationshipBuilder.Metadata.Builder != null)
            {
                TryConfigurePrimaryKey(foreignKey.DeclaringEntityType.Builder);
            }
        }

        /// <summary>
        ///     Called after the ownership value for a foreign key is changed.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessForeignKeyOwnershipChanged(
            IConventionRelationshipBuilder relationshipBuilder,
            IConventionContext<IConventionRelationshipBuilder> context)
        {
            TryConfigurePrimaryKey(relationshipBuilder.Metadata.DeclaringEntityType.Builder);
        }

        /// <summary>
        ///     Called after a foreign key is removed.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="foreignKey"> The removed foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessForeignKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionForeignKey foreignKey,
            IConventionContext<IConventionForeignKey> context)
        {
            if (foreignKey.IsOwnership)
            {
                TryConfigurePrimaryKey(entityTypeBuilder);
            }
        }

        /// <summary>
        ///     Called after the uniqueness for a foreign key is changed.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessForeignKeyUniquenessChanged(
            IConventionRelationshipBuilder relationshipBuilder,
            IConventionContext<IConventionRelationshipBuilder> context)
        {
            if (relationshipBuilder.Metadata.IsOwnership)
            {
                TryConfigurePrimaryKey(relationshipBuilder.Metadata.DeclaringEntityType.Builder);
            }
        }
    }
}
