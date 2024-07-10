// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal;

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
        IDiscriminatorPropertySetConvention,
        IModelFinalizingConvention
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
    /// <param name="definitionFactory">The factory to create a <see cref="IJsonIdDefinition"/> for each entity type.</param>
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
    ///     The factory to create a <see cref="IJsonIdDefinition"/> for each entity type.
    /// </summary>
    protected virtual IJsonIdDefinitionFactory DefinitionFactory { get; }

    private void ProcessEntityType(IConventionEntityType entityType, IConventionContext context)
    {
        var primaryKey = entityType.FindPrimaryKey();
        if (entityType.BaseType == null // Reactions required for: IEntityTypeBaseTypeChangedConvention
            && entityType.IsDocumentRoot() // Reactions required for: IEntityTypeAnnotationChangedConvention (ContainerName),
            && !entityType.IsOwned() // Reactions required for: IForeignKeyOwnershipChangedConvention
            && primaryKey != null) // Reactions required for: IKeyAddedConvention, IKeyRemovedConvention
        {
            var entityTypeBuilder = entityType.Builder;

            // Reactions required for:
            // IPropertyAddedConvention, IPropertyRemovedConvention
            // IPropertyAddedConvention, IPropertyRemovedConvention, IPropertyAnnotationChangedConvention (PropertyName)

            // Explicit configuration:
            // - If the __id shadow property is already mapped, then do nothing by convention here.
            // - If a property is already mapped to the JSON id field, then do nothing by convention.
            var computedIdProperty = entityType.FindDeclaredProperty(DefaultIdPropertyName);
            var jsonIdProperty = entityType.GetDeclaredProperties().FirstOrDefault(p => p.GetJsonPropertyName() == IdPropertyJsonName);
            if ((jsonIdProperty != null
                    && jsonIdProperty.GetConfigurationSource().OverridesStrictly(ConfigurationSource.Convention))
                || (computedIdProperty != null
                    && computedIdProperty.GetConfigurationSource().OverridesStrictly(ConfigurationSource.Convention)))
            {
                if (jsonIdProperty != null
                    && !jsonIdProperty.GetConfigurationSource().OverridesStrictly(ConfigurationSource.Convention))
                {
                    jsonIdProperty.Builder.ToJsonProperty(null);
                }

                if (computedIdProperty != null
                    && !computedIdProperty.GetConfigurationSource().OverridesStrictly(ConfigurationSource.Convention))
                {
                    entityTypeBuilder.Metadata.RemoveProperty(computedIdProperty);
                }

                return;
            }

            // Reactions required for:
            // IEntityTypeAnnotationChangedConvention (AlwaysCreateShadowIdProperty)
            // IModelAnnotationChangedConvention (AlwaysCreateShadowIdProperty)
            var alwaysCreateId = entityType.GetAlwaysCreateShadowIdProperty() ?? entityType.Model.GetAlwaysCreateShadowIdProperty();
            if (alwaysCreateId != true)
            {
                // If there is one string primary key property after removing partition keys, then map it to the JSON id field directly,
                // unless it is explicitly mapped to some other property, in which case we compute the field value as below.

                // IKeyAddedConvention, IKeyRemovedConvention, IPropertyAddedConvention, IPropertyRemovedConvention,
                // IEntityTypeAnnotationChangedConvention (PartitionKeyNames) (DiscriminatorInKey)
                // IDiscriminatorPropertySetConvention
                var idDefinition = DefinitionFactory.Create((IEntityType)entityType)!;
                var keyProperty = (IConventionProperty?)idDefinition.Properties.FirstOrDefault();
                if (idDefinition.DiscriminatorEntityType == null
                    && idDefinition.Properties.Count == 1)
                {
                    var clrType = keyProperty!.GetValueConverter()?.ProviderClrType ?? keyProperty.ClrType;
                    if (clrType == typeof(string))
                    {
                        if (computedIdProperty != null)
                        {
                            entityTypeBuilder.Metadata.RemoveProperty(computedIdProperty);
                        }
                        keyProperty.SetJsonPropertyName(IdPropertyJsonName);
                        return;
                    }
                }
            }

            if (jsonIdProperty != null
                && jsonIdProperty != computedIdProperty)
            {
                jsonIdProperty.Builder.ToJsonProperty(null);
            }

            computedIdProperty = entityTypeBuilder
                .Property(typeof(string), DefaultIdPropertyName, setTypeConfigurationSource: false)!
                .ToJsonProperty(IdPropertyJsonName)!
                .IsRequired(true)!
                .HasValueGeneratorFactory(typeof(IdValueGeneratorFactory))!
                .Metadata;

            computedIdProperty.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => ProcessEntityType(entityTypeBuilder.Metadata, context);

    /// <inheritdoc />
    public void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
        => ProcessEntityType(entityTypeBuilder.Metadata, context);

    /// <inheritdoc />
    public void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        switch (name)
        {
            case CosmosAnnotationNames.ContainerName:
            case CosmosAnnotationNames.AlwaysCreateShadowIdProperty:
            case CosmosAnnotationNames.PartitionKeyNames:
            case CosmosAnnotationNames.DiscriminatorInKey:
                ProcessEntityType(entityTypeBuilder.Metadata, context);
                break;
        }
    }

    /// <inheritdoc />
    public void ProcessForeignKeyOwnershipChanged(IConventionForeignKeyBuilder relationshipBuilder, IConventionContext<bool?> context)
        => ProcessEntityType(relationshipBuilder.Metadata.DeclaringEntityType, context);

    /// <inheritdoc />
    public void ProcessKeyAdded(IConventionKeyBuilder keyBuilder, IConventionContext<IConventionKeyBuilder> context)
        => ProcessEntityType(keyBuilder.Metadata.DeclaringEntityType, context);

    /// <inheritdoc />
    public void ProcessKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey key,
        IConventionContext<IConventionKey> context)
        => ProcessEntityType(entityTypeBuilder.Metadata, context);

    /// <inheritdoc />
    public void ProcessPropertyAdded(IConventionPropertyBuilder propertyBuilder, IConventionContext<IConventionPropertyBuilder> context)
        => ProcessEntityType(propertyBuilder.Metadata.DeclaringType.ContainingEntityType, context);

    /// <inheritdoc />
    public void ProcessPropertyRemoved(
        IConventionTypeBaseBuilder typeBaseBuilder,
        IConventionProperty property,
        IConventionContext<IConventionProperty> context)
        => ProcessEntityType(typeBaseBuilder.Metadata.ContainingEntityType, context);

    /// <inheritdoc />
    public void ProcessPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        switch (name)
        {
            case CosmosAnnotationNames.PropertyName:
                ProcessEntityType(propertyBuilder.Metadata.DeclaringType.ContainingEntityType, context);
                break;
        }
    }

    /// <inheritdoc />
    public void ProcessModelAnnotationChanged(
        IConventionModelBuilder modelBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        switch (name)
        {
            case CosmosAnnotationNames.AlwaysCreateShadowIdProperty:
            case CosmosAnnotationNames.DiscriminatorInKey:
                foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
                {
                    ProcessEntityType(entityType, context);
                }

                break;
        }
    }

    /// <inheritdoc />
    public void ProcessDiscriminatorPropertySet(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        IConventionContext<string?> context)
        => ProcessEntityType(entityTypeBuilder.Metadata, context);

    /// <inheritdoc />
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            ProcessEntityType(entityType, context);
        }
    }
}
