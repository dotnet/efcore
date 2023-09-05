// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Extensions.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that ensures that properties aren't configured to have a default value, as computed column
///     or using a <see cref="SqlServerValueGenerationStrategy" /> at the same time.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SqlServerStoreGenerationConvention : StoreGenerationConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerStoreGenerationConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public SqlServerStoreGenerationConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <summary>
    ///     Called after an annotation is changed on a property.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public override void ProcessPropertyAnnotationChanged(
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
                if (propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) == null
                    && propertyBuilder.HasDefaultValue(null, fromDataAnnotation) != null)
                {
                    context.StopProcessing();
                    return;
                }

                break;
            case RelationalAnnotationNames.DefaultValueSql:
                if (propertyBuilder.Metadata.GetValueGenerationStrategy() != SqlServerValueGenerationStrategy.Sequence
                    && propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) == null
                    && propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) != null)
                {
                    context.StopProcessing();
                    return;
                }

                break;
            case RelationalAnnotationNames.ComputedColumnSql:
                if (propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) == null
                    && propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) != null)
                {
                    context.StopProcessing();
                    return;
                }

                break;
            case SqlServerAnnotationNames.ValueGenerationStrategy:
                if (((propertyBuilder.Metadata.GetValueGenerationStrategy() != SqlServerValueGenerationStrategy.Sequence
                            && (propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                                || propertyBuilder.HasDefaultValueSql(null, fromDataAnnotation) == null
                                || propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null))
                        || (propertyBuilder.HasDefaultValue(null, fromDataAnnotation) == null
                            || propertyBuilder.HasComputedColumnSql(null, fromDataAnnotation) == null))
                    && propertyBuilder.HasValueGenerationStrategy(null, fromDataAnnotation) != null)
                {
                    context.StopProcessing();
                    return;
                }

                break;
        }

        base.ProcessPropertyAnnotationChanged(propertyBuilder, name, annotation, oldAnnotation, context);
    }

    /// <inheritdoc />
    protected override void Validate(IConventionProperty property, in StoreObjectIdentifier storeObject)
    {
        if (property.GetValueGenerationStrategyConfigurationSource() != null)
        {
            var generationStrategy = property.GetValueGenerationStrategy(storeObject, Dependencies.TypeMappingSource);
            if (generationStrategy == SqlServerValueGenerationStrategy.None)
            {
                base.Validate(property, storeObject);
                return;
            }

            if (property.TryGetDefaultValue(storeObject, out _))
            {
                Dependencies.ValidationLogger.ConflictingValueGenerationStrategiesWarning(
                    generationStrategy, "DefaultValue", property);
            }

            if (property.GetDefaultValueSql(storeObject) != null)
            {
                Dependencies.ValidationLogger.ConflictingValueGenerationStrategiesWarning(
                    generationStrategy, "DefaultValueSql", property);
            }

            if (property.GetComputedColumnSql(storeObject) != null)
            {
                Dependencies.ValidationLogger.ConflictingValueGenerationStrategiesWarning(
                    generationStrategy, "ComputedColumnSql", property);
            }
        }

        base.Validate(property, storeObject);
    }
}
