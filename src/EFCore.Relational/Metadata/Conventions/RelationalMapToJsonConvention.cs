// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures default settings for an entity mapped to a JSON column.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class RelationalMapToJsonConvention : IEntityTypeAnnotationChangedConvention, IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalMapToJsonConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
    public RelationalMapToJsonConvention(
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

    /// <inheritdoc />
    [Obsolete("Container column mappings are now obtained from IColumnBase.StoreTypeMapping")]
    public virtual void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var jsonEntityType in modelBuilder.Metadata.GetEntityTypes().Where(e => e.IsMappedToJson()))
        {
            foreach (var enumProperty in jsonEntityType
                         .GetDeclaredProperties()
                         .Where(p => p.ClrType.UnwrapNullableType().IsEnum))
            {
                // If the enum is mapped with no conversion, then use the reader/writer that handles legacy string values and warns.
                if (enumProperty.GetValueConverter() == null
                    && enumProperty.GetProviderClrType() == null)
                {
                    enumProperty.SetJsonValueReaderWriterType(
                        typeof(JsonWarningEnumReaderWriter<>).MakeGenericType(enumProperty.ClrType.UnwrapNullableType()));
                }
            }
        }
    }
}
