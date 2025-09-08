// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XuGu.Metadata.Conventions;

/// <summary>
///     A convention that configures the default model <see cref="XGValueGenerationStrategy" /> as
///     <see cref="XGValueGenerationStrategy.IdentityColumn" />.
/// </summary>
public class XGValueGenerationStrategyConvention : IModelInitializedConvention, IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="XGValueGenerationStrategyConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public XGValueGenerationStrategyConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Parameter object containing service dependencies.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelInitialized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        => modelBuilder.HasValueGenerationStrategy(XGValueGenerationStrategy.IdentityColumn);

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                XGValueGenerationStrategy? strategy = null;
                var declaringTable = property.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
                if (declaringTable.Name != null!)
                {
                    strategy = property.GetValueGenerationStrategy(declaringTable, Dependencies.TypeMappingSource);
                    if (strategy == XGValueGenerationStrategy.None &&
                        !IsStrategyNoneNeeded(property, declaringTable))
                    {
                        strategy = null;
                    }
                }
                else
                {
                    var declaringView = property.GetMappedStoreObjects(StoreObjectType.View).FirstOrDefault();
                    if (declaringView.Name != null!)
                    {
                        strategy = property.GetValueGenerationStrategy(declaringView, Dependencies.TypeMappingSource);
                        if (strategy == XGValueGenerationStrategy.None &&
                            !IsStrategyNoneNeeded(property, declaringView))
                        {
                            strategy = null;
                        }
                    }
                }

                // Needed for the annotation to show up in the model snapshot.
                if (strategy != null &&
                    declaringTable.Name != null)
                {
                    property.Builder.HasValueGenerationStrategy(strategy);
                }
            }
        }

        bool IsStrategyNoneNeeded(IReadOnlyProperty property, StoreObjectIdentifier storeObject)
        {
            if (property.ValueGenerated == ValueGenerated.OnAdd &&
                !property.TryGetDefaultValue(storeObject, out _) &&
                property.GetDefaultValueSql(storeObject) is null &&
                property.GetComputedColumnSql(storeObject) is null &&
                property.DeclaringType.Model.GetValueGenerationStrategy() != XGValueGenerationStrategy.None)
            {
                var providerClrType = (property.GetValueConverter() ??
                                       (property.FindRelationalTypeMapping(storeObject) ??
                                        Dependencies.TypeMappingSource.FindMapping((IProperty)property))?.Converter)
                    ?.ProviderClrType.UnwrapNullableType();

                return providerClrType is not null && (providerClrType.IsInteger());
            }

            return false;
        }
    }
}
