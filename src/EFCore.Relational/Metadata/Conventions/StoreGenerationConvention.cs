// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that ensures that properties aren't configured to have a default value and as computed column at the same time.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///     <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
/// </remarks>
public class StoreGenerationConvention : IPropertyAnnotationChangedConvention, IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="StoreGenerationConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public StoreGenerationConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Called after an annotation is changed on a property.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (annotation == null
            || oldAnnotation?.Value != null)
        {
            return;
        }

        var configurationSource = annotation.GetConfigurationSource();
        var fromDataAnnotation = configurationSource != ConfigurationSource.Convention;
        switch (name)
        {
            case RelationalAnnotationNames.DefaultValue:
                if ((propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null
                        | propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null)
                    && propertyBuilder.HasDefaultValue(null, fromDataAnnotation) != null)
                {
                    context.StopProcessing();
                }

                break;
            case RelationalAnnotationNames.DefaultValueSql:
                if ((propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                        | propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null)
                    && propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) != null)
                {
                    context.StopProcessing();
                }

                break;
            case RelationalAnnotationNames.ComputedColumnSql:
                if ((propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                        | propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null)
                    && propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) != null)
                {
                    context.StopProcessing();
                }

                break;
        }
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName == null)
            {
                continue;
            }

            foreach (var declaredProperty in entityType.GetDeclaredProperties())
            {
                var declaringTable = declaredProperty.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
                if (declaringTable.Name == null!)
                {
                    continue;
                }

                Validate(declaredProperty, declaringTable);
            }
        }
    }

    /// <summary>
    ///     Throws if there is conflicting store generation configuration for this property.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    protected virtual void Validate(
        IConventionProperty property,
        in StoreObjectIdentifier storeObject)
    {
        if (property.TryGetDefaultValue(storeObject, out _))
        {
            if (property.GetDefaultValueSql(storeObject) != null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ConflictingColumnServerGeneration("DefaultValue", property.Name, "DefaultValueSql"));
            }

            if (property.GetComputedColumnSql(storeObject) != null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ConflictingColumnServerGeneration("DefaultValue", property.Name, "ComputedColumnSql"));
            }
        }
        else if (property.GetDefaultValueSql(storeObject) != null)
        {
            if (property.GetComputedColumnSql(storeObject) != null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ConflictingColumnServerGeneration("DefaultValueSql", property.Name, "ComputedColumnSql"));
            }
        }
    }
}
