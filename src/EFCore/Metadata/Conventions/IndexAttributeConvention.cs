// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures database indexes based on the <see cref="IndexAttribute" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class IndexAttributeConvention :
    IEntityTypeAddedConvention,
    IEntityTypeBaseTypeChangedConvention,
    IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="IndexAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public IndexAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => CheckIndexAttributesAndEnsureIndex(entityTypeBuilder.Metadata, shouldThrow: false);

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

        CheckIndexAttributesAndEnsureIndex(entityTypeBuilder.Metadata, shouldThrow: false);
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            CheckIndexAttributesAndEnsureIndex(entityType, shouldThrow: true);
        }
    }

    private static void CheckIndexAttributesAndEnsureIndex(
        IConventionEntityType entityType,
        bool shouldThrow)
    {
        foreach (var indexAttribute in
                 entityType.ClrType.GetCustomAttributes<IndexAttribute>(inherit: true))
        {
            IConventionIndexBuilder? indexBuilder;
            if (!shouldThrow
                && !entityType.Builder.CanHaveIndex(indexAttribute.PropertyNames, fromDataAnnotation: true))
            {
                continue;
            }

            try
            {
                indexBuilder = indexAttribute.Name == null
                    ? entityType.Builder.HasIndex(
                        indexAttribute.PropertyNames, fromDataAnnotation: true)
                    : entityType.Builder.HasIndex(
                        indexAttribute.PropertyNames, indexAttribute.Name, fromDataAnnotation: true);
            }
            catch (InvalidOperationException exception)
            {
                CheckMissingProperties(entityType, indexAttribute, exception);

                throw;
            }

            if (indexBuilder == null)
            {
                if (shouldThrow)
                {
                    CheckIgnoredProperties(entityType, indexAttribute);
                }
            }
            else
            {
                if (indexAttribute.IsUniqueHasValue)
                {
                    indexBuilder = indexBuilder.IsUnique(indexAttribute.IsUnique, fromDataAnnotation: true);
                }

                if (indexBuilder is not null)
                {
                    if (indexAttribute.AllDescending)
                    {
                        indexBuilder.IsDescending([], fromDataAnnotation: true);
                    }
                    else if (indexAttribute.IsDescending is not null)
                    {
                        indexBuilder.IsDescending(indexAttribute.IsDescending, fromDataAnnotation: true);
                    }
                }
            }
        }
    }

    private static void CheckIgnoredProperties(IConventionEntityType entityType, IndexAttribute indexAttribute)
    {
        foreach (var propertyName in indexAttribute.PropertyNames)
        {
            if (entityType.Builder.IsIgnored(propertyName, fromDataAnnotation: true))
            {
                if (indexAttribute.Name == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.UnnamedIndexDefinedOnIgnoredProperty(
                            entityType.DisplayName(),
                            indexAttribute.PropertyNames.Format(),
                            propertyName));
                }

                throw new InvalidOperationException(
                    CoreStrings.NamedIndexDefinedOnIgnoredProperty(
                        indexAttribute.Name,
                        entityType.DisplayName(),
                        indexAttribute.PropertyNames.Format(),
                        propertyName));
            }
        }
    }

    private static void CheckMissingProperties(
        IConventionEntityType entityType,
        IndexAttribute indexAttribute,
        InvalidOperationException exception)
    {
        foreach (var propertyName in indexAttribute.PropertyNames)
        {
            var property = entityType.FindProperty(propertyName);
            if (property == null)
            {
                if (indexAttribute.Name == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.UnnamedIndexDefinedOnNonExistentProperty(
                            entityType.DisplayName(),
                            indexAttribute.PropertyNames.Format(),
                            propertyName),
                        exception);
                }

                throw new InvalidOperationException(
                    CoreStrings.NamedIndexDefinedOnNonExistentProperty(
                        indexAttribute.Name,
                        entityType.DisplayName(),
                        indexAttribute.PropertyNames.Format(),
                        propertyName),
                    exception);
            }
        }
    }
}
