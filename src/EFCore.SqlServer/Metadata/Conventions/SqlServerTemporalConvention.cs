﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that manipulates temporal settings for an entity mapped to a temporal table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and SQL Azure databases with EF Core</see>
    ///     for more information.
    /// </remarks>
    public class SqlServerTemporalConvention : IEntityTypeAnnotationChangedConvention, ISkipNavigationForeignKeyChangedConvention
    {
        private const string PeriodStartDefaultName = "PeriodStart";
        private const string PeriodEndDefaultName = "PeriodEnd";

        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerTemporalConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention.</param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
        public SqlServerTemporalConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Dependencies = dependencies;
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
        public virtual void ProcessEntityTypeAnnotationChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            if (name == SqlServerAnnotationNames.IsTemporal)
            {
                if (annotation?.Value as bool? == true)
                {
                    if (entityTypeBuilder.Metadata.GetPeriodStartPropertyName() == null)
                    {
                        entityTypeBuilder.HasPeriodStart(PeriodStartDefaultName);
                    }

                    if (entityTypeBuilder.Metadata.GetPeriodEndPropertyName() == null)
                    {
                        entityTypeBuilder.HasPeriodEnd(PeriodEndDefaultName);
                    }

                    foreach (var skipLevelNavigation in entityTypeBuilder.Metadata.GetSkipNavigations())
                    {
                        if (skipLevelNavigation.DeclaringEntityType.IsTemporal()
                            && skipLevelNavigation.Inverse is IConventionSkipNavigation inverse
                            && inverse.DeclaringEntityType.IsTemporal()
                            && skipLevelNavigation.JoinEntityType is IConventionEntityType joinEntityType
                            && joinEntityType.HasSharedClrType
                            && !joinEntityType.IsTemporal()
                            && joinEntityType.GetConfigurationSource() == ConfigurationSource.Convention)
                        {
                            joinEntityType.SetIsTemporal(true);
                        }
                    }
                }
                else
                {
                    entityTypeBuilder.HasPeriodStart(null);
                    entityTypeBuilder.HasPeriodEnd(null);
                }
            }

            if (name == SqlServerAnnotationNames.TemporalPeriodStartPropertyName
                || name == SqlServerAnnotationNames.TemporalPeriodEndPropertyName)
            {
                if (oldAnnotation?.Value is string oldPeriodPropertyName)
                {
                    var oldPeriodProperty = entityTypeBuilder.Metadata.GetProperty(oldPeriodPropertyName);
                    entityTypeBuilder.RemoveUnusedImplicitProperties(new[] { oldPeriodProperty });

                    if (oldPeriodProperty.GetTypeConfigurationSource() == ConfigurationSource.Explicit)
                    {
                        if ((name == SqlServerAnnotationNames.TemporalPeriodStartPropertyName
                                && oldPeriodProperty.GetDefaultValue() is DateTime start
                                && start == DateTime.MinValue)
                            || (name == SqlServerAnnotationNames.TemporalPeriodEndPropertyName
                                && oldPeriodProperty.GetDefaultValue() is DateTime end
                                && end == DateTime.MaxValue))
                        {
                            oldPeriodProperty.Builder.HasDefaultValue(null);
                        }
                    }
                }

                if (annotation?.Value is string periodPropertyName)
                {
                    var periodPropertyBuilder = entityTypeBuilder.Property(
                        typeof(DateTime),
                        periodPropertyName);

                    if (periodPropertyBuilder != null)
                    {
                        // set column name explicitly so that we don't try to uniquefy it to some other column
                        // in case another property is defined that maps to the same column
                        periodPropertyBuilder.HasColumnName(periodPropertyName);
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void ProcessSkipNavigationForeignKeyChanged(
            IConventionSkipNavigationBuilder skipNavigationBuilder,
            IConventionForeignKey? foreignKey,
            IConventionForeignKey? oldForeignKey,
            IConventionContext<IConventionForeignKey> context)
        {
            if (skipNavigationBuilder.Metadata.JoinEntityType is IConventionEntityType joinEntityType
                && joinEntityType.HasSharedClrType
                && !joinEntityType.IsTemporal()
                && joinEntityType.GetConfigurationSource() == ConfigurationSource.Convention
                && skipNavigationBuilder.Metadata.DeclaringEntityType.IsTemporal()
                && skipNavigationBuilder.Metadata.Inverse is IConventionSkipNavigation inverse
                && inverse.DeclaringEntityType.IsTemporal())
            {
                joinEntityType.SetIsTemporal(true);
            }
        }
    }
}
