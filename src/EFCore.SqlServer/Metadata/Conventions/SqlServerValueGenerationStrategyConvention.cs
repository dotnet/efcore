// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

#nullable enable

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the default model <see cref="SqlServerValueGenerationStrategy" /> as
    ///     <see cref="SqlServerValueGenerationStrategy.IdentityColumn" />.
    /// </summary>
    public class SqlServerValueGenerationStrategyConvention : IModelInitializedConvention, IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerValueGenerationStrategyConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public SqlServerValueGenerationStrategyConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Dependencies = dependencies;
        }

        private ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after a model is initialized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelInitialized(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            modelBuilder.HasValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);
        }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    SqlServerValueGenerationStrategy? strategy = null;
                    var table = entityType.GetTableName();
                    if (table != null)
                    {
                        var storeObject = StoreObjectIdentifier.Table(table, entityType.GetSchema());
                        strategy = property.GetValueGenerationStrategy(storeObject, Dependencies.TypeMappingSource);
                        if (strategy == SqlServerValueGenerationStrategy.None
                            && !IsStrategyNoneNeeded(property, storeObject))
                        {
                            strategy = null;
                        }
                    }
                    else
                    {
                        var view = entityType.GetViewName();
                        if (view != null)
                        {
                            var storeObject = StoreObjectIdentifier.View(view, entityType.GetViewSchema());
                            strategy = property.GetValueGenerationStrategy(storeObject, Dependencies.TypeMappingSource);
                            if (strategy == SqlServerValueGenerationStrategy.None
                                && !IsStrategyNoneNeeded(property, storeObject))
                            {
                                strategy = null;
                            }
                        }
                    }

                    // Needed for the annotation to show up in the model snapshot
                    if (strategy != null)
                    {
                        property.Builder.HasValueGenerationStrategy(strategy);
                    }
                }
            }

            bool IsStrategyNoneNeeded(IReadOnlyProperty property, StoreObjectIdentifier storeObject)
            {
                if (property.ValueGenerated == ValueGenerated.OnAdd
                    && property.GetDefaultValue(storeObject) == null
                    && property.GetDefaultValueSql(storeObject) == null
                    && property.GetComputedColumnSql(storeObject) == null
                    && property.DeclaringEntityType.Model.GetValueGenerationStrategy() == SqlServerValueGenerationStrategy.IdentityColumn)
                {
                    var providerClrType = (property.GetValueConverter()
                        ?? (property.FindRelationalTypeMapping(storeObject)
                            ?? Dependencies.TypeMappingSource.FindMapping((IProperty)property))?.Converter)
                        ?.ProviderClrType.UnwrapNullableType();

                    return providerClrType != null
                        && (providerClrType.IsInteger() || providerClrType == typeof(decimal));
                }

                return false;
            }
        }
    }
}
