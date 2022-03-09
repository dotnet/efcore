// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the entity type key based on the <see cref="KeyAttribute" /> specified on a property or
///     <see cref="PrimaryKeyAttribute"/> specified on a CLR type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class KeyAttributeConvention
    : PropertyAttributeConventionBase<KeyAttribute>,
        IModelFinalizingConvention,
        IEntityTypeAddedConvention,
        IEntityTypeBaseTypeChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="KeyAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public KeyAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => CheckAttributesAndEnsurePrimaryKey((EntityType)entityTypeBuilder.Metadata, null, shouldThrow: false);

    /// <inheritdoc />
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if (oldBaseType == null)
        {
            return;
        }

        CheckAttributesAndEnsurePrimaryKey((EntityType)entityTypeBuilder.Metadata, null, shouldThrow: false);
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        KeyAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        var entityType = propertyBuilder.Metadata.DeclaringEntityType;
        if (entityType.IsKeyless)
        {
            switch (entityType.GetIsKeylessConfigurationSource())
            {
                case ConfigurationSource.DataAnnotation:
                    Dependencies.Logger.ConflictingKeylessAndKeyAttributesWarning(propertyBuilder.Metadata);
                    return;

                case ConfigurationSource.Explicit:
                    // fluent API overrides the attribute - no warning
                    return;
            }
        }

        CheckAttributesAndEnsurePrimaryKey(
            (EntityType)propertyBuilder.Metadata.DeclaringEntityType,
            propertyBuilder,
            shouldThrow: false);
    }

    private bool CheckAttributesAndEnsurePrimaryKey(
        EntityType entityType,
        IConventionPropertyBuilder? propertyBuilder,
        bool shouldThrow)
    {
        if (entityType.BaseType != null)
        {
            return false;
        }

        var primaryKeyAttributeExists = CheckPrimaryKeyAttributeAndEnsurePrimaryKey(entityType, shouldThrow);

        if (!primaryKeyAttributeExists
            && propertyBuilder != null)
        {
            var properties = new List<string> { propertyBuilder.Metadata.Name };

            var currentKey = entityType.FindPrimaryKey();
            if (currentKey != null
                && entityType.GetPrimaryKeyConfigurationSource() == ConfigurationSource.DataAnnotation)
            {
                properties.AddRange(
                    currentKey.Properties
                        .Where(p => !p.Name.Equals(propertyBuilder.Metadata.Name, StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.Name));

                if (properties.Count > 1)
                {
                    properties.Sort(StringComparer.OrdinalIgnoreCase);
                    entityType.Builder.HasNoKey(currentKey, ConfigurationSource.DataAnnotation);
                }
            }

            entityType.Builder.PrimaryKey(properties, ConfigurationSource.DataAnnotation);
        }

        return primaryKeyAttributeExists;
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        var entityTypes = modelBuilder.Metadata.GetEntityTypes();
        foreach (var entityType in entityTypes)
        {
            var primaryKeyAttributeExits = CheckAttributesAndEnsurePrimaryKey((EntityType)entityType, null, shouldThrow: true);

            if (entityType.BaseType == null)
            {
                if (!primaryKeyAttributeExits)
                {
                    var currentPrimaryKey = entityType.FindPrimaryKey();
                    if (currentPrimaryKey?.Properties.Count > 1
                        && entityType.GetPrimaryKeyConfigurationSource() == ConfigurationSource.DataAnnotation)
                    {
                        throw new InvalidOperationException(CoreStrings.CompositePKWithDataAnnotation(entityType.DisplayName()));
                    }
                }
            }
            else
            {
                if (Attribute.IsDefined(entityType.ClrType, typeof(PrimaryKeyAttribute), inherit: false))
                {
                    throw new InvalidOperationException(
                        CoreStrings.PrimaryKeyAttributeOnDerivedEntity(
                            entityType.DisplayName(), entityType.GetRootType().DisplayName()));
                }

                if (!Attribute.IsDefined(entityType.ClrType, typeof(PrimaryKeyAttribute), inherit: true))
                {
                    foreach (var declaredProperty in entityType.GetDeclaredProperties())
                    {
                        var memberInfo = declaredProperty.GetIdentifyingMemberInfo();

                        if (memberInfo != null
                            && Attribute.IsDefined(memberInfo, typeof(KeyAttribute), inherit: true))
                        {
                            throw new InvalidOperationException(
                                CoreStrings.KeyAttributeOnDerivedEntity(
                                    entityType.DisplayName(), declaredProperty.Name, entityType.GetRootType().DisplayName()));
                        }
                    }
                }
            }
        }
    }

    private static bool CheckPrimaryKeyAttributeAndEnsurePrimaryKey(
        IConventionEntityType entityType,
        bool shouldThrow)
    {
        var primaryKeyAttribute = entityType.ClrType.GetCustomAttributes<PrimaryKeyAttribute>(inherit: true).FirstOrDefault();
        if (primaryKeyAttribute == null)
        {
            return false;
        }

        if (entityType.ClrType.GetCustomAttributes<KeylessAttribute>(inherit: true).Any())
        {
            throw new InvalidOperationException(
                CoreStrings.ConflictingKeylessAndPrimaryKeyAttributes(entityType.DisplayName()));
        }

        IConventionKeyBuilder? keyBuilder;
        if (!shouldThrow)
        {
            var keyProperties = new List<IConventionProperty>();
            foreach (var propertyName in primaryKeyAttribute.PropertyNames)
            {
                var property = entityType.FindProperty(propertyName);
                if (property == null)
                {
                    return true;
                }

                keyProperties.Add(property);
            }

            keyBuilder = entityType.Builder.PrimaryKey(keyProperties, fromDataAnnotation: true);
        }
        else
        {
            try
            {
                // Using the PrimaryKey(propertyNames) overload gives us a chance to create a missing property
                // e.g. if the CLR property existed but was non-public.
                keyBuilder = entityType.Builder.PrimaryKey(primaryKeyAttribute.PropertyNames, fromDataAnnotation: true);
            }
            catch (InvalidOperationException exception)
            {
                foreach (var propertyName in primaryKeyAttribute.PropertyNames)
                {
                    var property = entityType.FindProperty(propertyName);
                    if (property == null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.PrimaryKeyDefinedOnNonExistentProperty(
                                entityType.DisplayName(),
                                primaryKeyAttribute.PropertyNames.Format(),
                                propertyName),
                            exception);
                    }
                }

                throw;
            }
        }

        if (keyBuilder == null
            && shouldThrow)
        {
            foreach (var propertyName in primaryKeyAttribute.PropertyNames)
            {
                if (entityType.Builder.IsIgnored(propertyName, fromDataAnnotation: true))
                {
                    throw new InvalidOperationException(
                        CoreStrings.PrimaryKeyDefinedOnIgnoredProperty(
                            entityType.DisplayName(),
                            propertyName));
                }
            }
        }

        return true;
    }
}
