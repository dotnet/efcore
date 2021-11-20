// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that finds primary key property for the entity type based on the names, ignoring case:
    ///     * Id
    ///     * [entity name]Id
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If the entity type is owned through a reference navigation property then the corresponding foreign key
    ///         properties are used.
    ///     </para>
    ///     <para>
    ///         If the entity type is owned through a collection navigation property then a composite primary key
    ///         is configured using the foreign key properties with an extra property that matches the naming convention above.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public class KeyDiscoveryConvention :
        IEntityTypeAddedConvention,
        IPropertyAddedConvention,
        IKeyRemovedConvention,
        IEntityTypeBaseTypeChangedConvention,
        IEntityTypeMemberIgnoredConvention,
        IForeignKeyAddedConvention,
        IForeignKeyRemovedConvention,
        IForeignKeyPropertiesChangedConvention,
        IForeignKeyUniquenessChangedConvention,
        IForeignKeyOwnershipChangedConvention,
        ISkipNavigationForeignKeyChangedConvention
    {
        private const string KeySuffix = "Id";

        /// <summary>
        ///     Creates a new instance of <see cref="KeyDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
        public KeyDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Discovers primary key candidates and configures the primary key if found.
        /// </summary>
        /// <param name="entityTypeBuilder">The entity type builder.</param>
        protected virtual void TryConfigurePrimaryKey(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (entityType.BaseType != null
                || (entityType.IsKeyless && entityType.GetIsKeylessConfigurationSource() != ConfigurationSource.Convention)
                || !entityTypeBuilder.CanSetPrimaryKey(null))
            {
                return;
            }

            List<IConventionProperty>? keyProperties = null;
            var ownership = entityType.FindOwnership();
            if (ownership != null
                && ownership.DeclaringEntityType != entityType)
            {
                ownership = null;
            }

            if (ownership?.IsUnique == true)
            {
                keyProperties = ownership.Properties.ToList();
            }

            if (keyProperties == null)
            {
                var candidateProperties = entityType.GetProperties().Where(
                    p => !p.IsImplicitlyCreated()
                        || !ConfigurationSource.Convention.Overrides(p.GetConfigurationSource()));
                keyProperties = DiscoverKeyProperties(entityType, candidateProperties).ToList();
                if (keyProperties.Count > 1)
                {
                    Dependencies.Logger.MultiplePrimaryKeyCandidates(keyProperties[0], keyProperties[1]);
                    return;
                }
            }

            if (ownership?.IsUnique == false)
            {
                if (keyProperties.Count == 0
                    || ownership.Properties.Contains(keyProperties.First()))
                {
                    var primaryKey = entityType.FindPrimaryKey();
                    var shadowProperty = primaryKey?.Properties.Last();
                    if (shadowProperty == null
                        || primaryKey!.Properties.Count == 1
                        || ownership.Properties.Contains(shadowProperty))
                    {
                        shadowProperty = entityTypeBuilder.CreateUniqueProperty(typeof(int), "Id", required: true)!.Metadata;
                    }

                    keyProperties.Clear();
                    keyProperties.Add(shadowProperty);
                }

                var extraProperty = keyProperties[0];
                keyProperties.RemoveAt(0);
                keyProperties.AddRange(ownership.Properties);
                keyProperties.Add(extraProperty);
            }

            if (keyProperties.Count == 0)
            {
                var manyToManyForeignKeys = entityType.GetForeignKeys()
                    .Where(fk => fk.GetReferencingSkipNavigations().Any(n => n.IsCollection)).ToList();
                if (manyToManyForeignKeys.Count == 2
                    && !manyToManyForeignKeys.Any(fk => fk.PrincipalEntityType == entityType))
                {
                    keyProperties.AddRange(manyToManyForeignKeys.SelectMany(fk => fk.Properties));
                }
            }

            for (var i = keyProperties.Count - 1; i >= 0; i--)
            {
                var property = keyProperties[i];
                for (var j = i - 1; j >= 0; j--)
                {
                    if (property == keyProperties[j])
                    {
                        keyProperties.RemoveAt(j);
                        i--;
                    }
                }
            }

            ProcessKeyProperties(keyProperties, entityType);

            if (keyProperties.Count > 0)
            {
                entityTypeBuilder.PrimaryKey(keyProperties);
            }
        }

        /// <summary>
        ///     Adds or removes properties to be used for the primary key.
        /// </summary>
        /// <param name="keyProperties">The properties that will be used to configure the key.</param>
        /// <param name="entityType">The entity type being configured.</param>
        protected virtual void ProcessKeyProperties(
            IList<IConventionProperty> keyProperties,
            IConventionEntityType entityType)
        {
        }

        /// <summary>
        ///     Returns the properties that should be used for the primary key.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="candidateProperties">The properties to consider.</param>
        /// <returns>The properties that should be used for the primary key.</returns>
        public static IEnumerable<IConventionProperty> DiscoverKeyProperties(
            IConventionEntityType entityType,
            IEnumerable<IConventionProperty> candidateProperties)
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
        ///     Called after an entity type member is ignored.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type.</param>
        /// <param name="name">The name of the ignored member.</param>
        /// <param name="context">Additional information associated with convention execution.</param>
        public virtual void ProcessEntityTypeMemberIgnored(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionContext<string> context)
        {
            var entityTypeName = entityTypeBuilder.Metadata.ShortName();
            if (string.Equals(name, KeySuffix, StringComparison.OrdinalIgnoreCase)
                || (name.Length == entityTypeName.Length + KeySuffix.Length
                    && name.StartsWith(entityTypeName, StringComparison.OrdinalIgnoreCase)
                    && name.EndsWith(KeySuffix, StringComparison.OrdinalIgnoreCase)))
            {
                TryConfigurePrimaryKey(entityTypeBuilder);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
            => TryConfigurePrimaryKey(entityTypeBuilder);

        /// <inheritdoc />
        public virtual void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType? newBaseType,
            IConventionEntityType? oldBaseType,
            IConventionContext<IConventionEntityType> context)
        {
            if (entityTypeBuilder.Metadata.BaseType != newBaseType)
            {
                return;
            }

            TryConfigurePrimaryKey(entityTypeBuilder);
        }

        /// <inheritdoc />
        public virtual void ProcessPropertyAdded(
            IConventionPropertyBuilder propertyBuilder,
            IConventionContext<IConventionPropertyBuilder> context)
            => TryConfigurePrimaryKey(propertyBuilder.Metadata.DeclaringEntityType.Builder);

        /// <inheritdoc />
        public virtual void ProcessKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionKey key,
            IConventionContext<IConventionKey> context)
        {
            if (entityTypeBuilder.Metadata.FindPrimaryKey() == null)
            {
                TryConfigurePrimaryKey(entityTypeBuilder);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyAdded(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<IConventionForeignKeyBuilder> context)
        {
            if (relationshipBuilder.Metadata.IsOwnership)
            {
                TryConfigurePrimaryKey(relationshipBuilder.Metadata.DeclaringEntityType.Builder);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyPropertiesChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IReadOnlyList<IConventionProperty> oldDependentProperties,
            IConventionKey oldPrincipalKey,
            IConventionContext<IReadOnlyList<IConventionProperty>> context)
        {
            var foreignKey = relationshipBuilder.Metadata;
            if (foreignKey.IsOwnership
                && !foreignKey.Properties.SequenceEqual(oldDependentProperties)
                && relationshipBuilder.Metadata.IsInModel)
            {
                TryConfigurePrimaryKey(foreignKey.DeclaringEntityType.Builder);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessForeignKeyOwnershipChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<bool?> context)
            => TryConfigurePrimaryKey(relationshipBuilder.Metadata.DeclaringEntityType.Builder);

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual void ProcessForeignKeyUniquenessChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionContext<bool?> context)
        {
            if (relationshipBuilder.Metadata.IsOwnership)
            {
                TryConfigurePrimaryKey(relationshipBuilder.Metadata.DeclaringEntityType.Builder);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessSkipNavigationForeignKeyChanged(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionForeignKey? foreignKey,
            IConventionForeignKey? oldForeignKey,
            IConventionContext<IConventionForeignKey> context)
        {
            var joinEntityTypeBuilder = skipNavigationBuilder.Metadata.ForeignKey?.DeclaringEntityType.Builder;
            if (joinEntityTypeBuilder != null
                && skipNavigationBuilder.Metadata.IsCollection)
            {
                TryConfigurePrimaryKey(joinEntityTypeBuilder);
            }
        }
    }
}
