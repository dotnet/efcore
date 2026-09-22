// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures mapping the PK and/or discriminator properties to the JSON 'id' property.
/// </summary>
/// <remarks>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///         <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public class CosmosJsonIdConvention
    : IEntityTypeAddedConvention,
        IEntityTypeBaseTypeChangedConvention,
        IEntityTypeAnnotationChangedConvention,
        IForeignKeyOwnershipChangedConvention,
        IKeyAddedConvention,
        IKeyRemovedConvention,
        IPropertyAddedConvention,
        IPropertyRemovedConvention,
        IPropertyAnnotationChangedConvention,
        IModelAnnotationChangedConvention,
        IDiscriminatorPropertySetConvention
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static readonly string IdPropertyJsonName = "id";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static readonly string DefaultIdPropertyName = "__id";

    /// <summary>
    ///     Creates a new instance of <see cref="CosmosJsonIdConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="definitionFactory">The factory to create a <see cref="IJsonIdDefinition" /> for each entity type.</param>
    public CosmosJsonIdConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        IJsonIdDefinitionFactory definitionFactory)
    {
        Dependencies = dependencies;
        DefinitionFactory = definitionFactory;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     The factory to create a <see cref="IJsonIdDefinition" /> for each entity type.
    /// </summary>
    protected virtual IJsonIdDefinitionFactory DefinitionFactory { get; }

    private void ProcessEntityType(IConventionEntityType entityType, IConventionContext context)
    {
        var jsonIdProperty = entityType.GetDeclaredProperties().FirstOrDefault(p => p.GetJsonPropertyName() == IdPropertyJsonName);
        var computedIdProperty = entityType.FindDeclaredProperty(DefaultIdPropertyName);

        var primaryKey = entityType.FindPrimaryKey();
        if (entityType.BaseType != null
            || !entityType.IsDocumentRoot()
            || entityType.GetForeignKeys().Any(fk => fk.IsOwnership)
            || primaryKey == null)
        {
            // If the entity type is not a keyed, root document in the container, then it doesn't have an `id` mapping, so
            // undo anything that was done by previous execution of this convention.
            if (jsonIdProperty is not null)
            {
                jsonIdProperty.Builder.ToJsonProperty(null);
                entityType.Builder.RemoveUnusedImplicitProperties([jsonIdProperty]);
            }

            if (computedIdProperty is not null
                && computedIdProperty != jsonIdProperty)
            {
                entityType.Builder.RemoveUnusedImplicitProperties([computedIdProperty]);
            }

            return;
        }

        // Next, see if we can map the PK property directly to ths JSON `id` property. This requires that the
        // key is represented by a single string property, and the discriminator is not being included in the JSON `id`.
        // If these conditions are not met, or if the user has opted-in, then we will create a computed property that transforms
        // the appropriate values into a single string for the JSON `id` property.
        var alwaysCreateId = entityType.GetHasShadowId();
        if (alwaysCreateId != true)
        {
            var idDefinition = DefinitionFactory.Create((IEntityType)entityType)!;
            if (idDefinition is { IncludesDiscriminator: false, Properties.Count: 1 })
            {
                // If the property maps to a string in the JSON document, then we can use it directly, even if a value converter
                // is applied. On the other hand, if it maps to a numeric or bool, then we need to duplicate this to preserve the
                // non-string value for queries.
                var keyProperty = (IConventionProperty)idDefinition.Properties.First();
                var mapping = Dependencies.TypeMappingSource.FindMapping((IProperty)keyProperty);
                var clrType = mapping?.Converter?.ProviderClrType
                    ?? mapping?.ClrType
                    ?? keyProperty!.ClrType;

                if (clrType == typeof(string)
                    && keyProperty.Builder.CanSetJsonProperty(IdPropertyJsonName))
                {
                    // We are at the point where we are going to map the `id` directly to the PK.
                    // However, if a previous run of this convention create the computed property, then we need to remove that
                    // mapping since it is now not needed.
                    if (computedIdProperty != null
                        && entityType.Builder.HasNoProperty(computedIdProperty) == null)
                    {
                        computedIdProperty.Builder.ToJsonProperty(null);
                    }

                    // If there was previously a different property mapped to `id`, but not one of our computed properties,
                    // then remove the mapping to `id`. For example, when the key property has been changed.
                    if (jsonIdProperty != null
                        && keyProperty != jsonIdProperty
                        && jsonIdProperty != computedIdProperty)
                    {
                        jsonIdProperty.Builder.ToJsonProperty(null);
                    }

                    // Finally, actually map the primary key directly to the JSON `id`.
                    keyProperty.Builder.ToJsonProperty(IdPropertyJsonName);

                    return;
                }
            }
        }

        // We are now close to the point where we need to create the computed property.
        // But first, we need to check if the original property found pointing to JSON `id` is not our computed property.
        // If so, then stop mapping it to JSON `id`.
        if (jsonIdProperty != null
            && jsonIdProperty != computedIdProperty
            && jsonIdProperty.Builder.ToJsonProperty(null) == null)
        {
            // But if this fails (ToJsonProperty returns null) because the mapping to `id` is explicit, then we can't actually
            // create a computed property at all, so if we did, remove it.
            if (computedIdProperty != null)
            {
                entityType.Builder.HasNoProperty(computedIdProperty);
            }

            return;
        }

        // Everything fits for making a computed property, so do it.
        var computedIdPropertyBuilder = entityType.Builder
            .Property(typeof(string), DefaultIdPropertyName, setTypeConfigurationSource: false);

        if (computedIdPropertyBuilder == null)
        {
            // The user explicitly ignored DefaultIdPropertyName.
            return;
        }

        // Don't chain, because each of these could return null if the property has been explicitly configured with some other value.
        computedIdPropertyBuilder.ToJsonProperty(IdPropertyJsonName);
        computedIdPropertyBuilder.HasValueGeneratorFactory(typeof(IdValueGeneratorFactory));
        computedIdPropertyBuilder.AfterSave(PropertySaveBehavior.Throw);
        computedIdPropertyBuilder.IsRequired(true);
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => ProcessEntityType(entityTypeBuilder.Metadata, context);

    /// <inheritdoc />
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
        => ProcessEntityType(entityTypeBuilder.Metadata, context);

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        switch (name)
        {
            case CosmosAnnotationNames.ContainerName:
            case CosmosAnnotationNames.PartitionKeyNames:
                ProcessEntityType(entityTypeBuilder.Metadata, context);
                break;

            case CosmosAnnotationNames.DiscriminatorInKey:
                if (oldAnnotation?.Value != null
                    || !Equals(annotation?.Value, entityTypeBuilder.ModelBuilder.Metadata.GetDiscriminatorInKey()))
                {
                    ProcessEntityType(entityTypeBuilder.Metadata, context);
                }

                break;

            case CosmosAnnotationNames.HasShadowId:
                if (oldAnnotation?.Value != null
                    || !Equals(annotation?.Value, entityTypeBuilder.ModelBuilder.Metadata.GetHasShadowIds()))
                {
                    ProcessEntityType(entityTypeBuilder.Metadata, context);
                }

                break;
        }
    }

    /// <inheritdoc />
    public virtual void ProcessForeignKeyOwnershipChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<bool?> context)
        => ProcessEntityType(relationshipBuilder.Metadata.DeclaringEntityType, context);

    /// <inheritdoc />
    public virtual void ProcessKeyAdded(IConventionKeyBuilder keyBuilder, IConventionContext<IConventionKeyBuilder> context)
        => ProcessEntityType(keyBuilder.Metadata.DeclaringEntityType, context);

    /// <inheritdoc />
    public virtual void ProcessKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey key,
        IConventionContext<IConventionKey> context)
    {
        if (!entityTypeBuilder.Metadata.IsInModel)
        {
            return;
        }

        ProcessEntityType(entityTypeBuilder.Metadata, context);
    }

    /// <inheritdoc />
    public virtual void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context)
        => ProcessEntityType(propertyBuilder.Metadata.DeclaringType.ContainingEntityType, context);

    /// <inheritdoc />
    public virtual void ProcessPropertyRemoved(
        IConventionTypeBaseBuilder typeBaseBuilder,
        IConventionProperty property,
        IConventionContext<IConventionProperty> context)
    {
        if (!typeBaseBuilder.Metadata.IsInModel)
        {
            return;
        }

        ProcessEntityType(typeBaseBuilder.Metadata.ContainingEntityType, context);
    }

    /// <inheritdoc />
    public virtual void ProcessPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        switch (name)
        {
            case CosmosAnnotationNames.PropertyName:
                if (Equals(oldAnnotation?.Value, IdPropertyJsonName)
                    || Equals(annotation?.Value, IdPropertyJsonName))
                {
                    ProcessEntityType(propertyBuilder.Metadata.DeclaringType.ContainingEntityType, context);
                }

                break;
        }
    }

    /// <inheritdoc />
    public virtual void ProcessModelAnnotationChanged(
        IConventionModelBuilder modelBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        switch (name)
        {
            case CosmosAnnotationNames.HasShadowId:
            case CosmosAnnotationNames.DiscriminatorInKey:
                foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
                {
                    // Only process entity types that do not have an annotation that overrides the model annotation.
                    if (entityType.FindAnnotation(name) == null)
                    {
                        ProcessEntityType(entityType, context);
                    }
                }

                break;
        }
    }

    /// <inheritdoc />
    public virtual void ProcessDiscriminatorPropertySet(
        IConventionTypeBaseBuilder structuralTypeBuilder,
        string? name,
        IConventionContext<string?> context)
    {
        if (structuralTypeBuilder is IConventionEntityTypeBuilder entityTypeBuilder)
        {
            ProcessEntityType(entityTypeBuilder.Metadata, context);
        }
    }
}
