// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        #region MethodInfos

        private static readonly MethodInfo _modelUseIdentityColumnsMethodInfo
            = typeof(SqlServerModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerModelBuilderExtensions.UseIdentityColumns), typeof(ModelBuilder), typeof(long), typeof(int));

        private static readonly MethodInfo _modelUseHiLoMethodInfo
            = typeof(SqlServerModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerModelBuilderExtensions.UseHiLo), typeof(ModelBuilder), typeof(string), typeof(string));

        private static readonly MethodInfo _modelHasDatabaseMaxSizeMethodInfo
            = typeof(SqlServerModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerModelBuilderExtensions.HasDatabaseMaxSize), typeof(ModelBuilder), typeof(string));

        private static readonly MethodInfo _modelHasServiceTierSqlMethodInfo
            = typeof(SqlServerModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerModelBuilderExtensions.HasServiceTierSql), typeof(ModelBuilder), typeof(string));

        private static readonly MethodInfo _modelHasPerformanceLevelSqlMethodInfo
            = typeof(SqlServerModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerModelBuilderExtensions.HasPerformanceLevelSql), typeof(ModelBuilder), typeof(string));

        private static readonly MethodInfo _modelHasAnnotationMethodInfo
            = typeof(ModelBuilder).GetRequiredRuntimeMethod(
                nameof(ModelBuilder.HasAnnotation), typeof(string), typeof(object));

        private static readonly MethodInfo _entityTypeToTableMethodInfo
            = typeof(RelationalEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalEntityTypeBuilderExtensions.ToTable), typeof(EntityTypeBuilder), typeof(string));

        private static readonly MethodInfo _entityTypeIsMemoryOptimizedMethodInfo
            = typeof(SqlServerEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerEntityTypeBuilderExtensions.IsMemoryOptimized), typeof(EntityTypeBuilder), typeof(bool));

        private static readonly MethodInfo _propertyIsSparseMethodInfo
            = typeof(SqlServerPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerPropertyBuilderExtensions.IsSparse), typeof(PropertyBuilder), typeof(bool));

        private static readonly MethodInfo _propertyUseIdentityColumnsMethodInfo
            = typeof(SqlServerPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerPropertyBuilderExtensions.UseIdentityColumn), typeof(PropertyBuilder), typeof(long), typeof(int));

        private static readonly MethodInfo _propertyUseHiLoMethodInfo
            = typeof(SqlServerPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerPropertyBuilderExtensions.UseHiLo), typeof(PropertyBuilder), typeof(string), typeof(string));

        private static readonly MethodInfo _indexIsClusteredMethodInfo
            = typeof(SqlServerIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerIndexBuilderExtensions.IsClustered), typeof(IndexBuilder), typeof(bool));

        private static readonly MethodInfo _indexIncludePropertiesMethodInfo
            = typeof(SqlServerIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerIndexBuilderExtensions.IncludeProperties), typeof(IndexBuilder), typeof(string[]));

        private static readonly MethodInfo _indexHasFillFactorMethodInfo
            = typeof(SqlServerIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerIndexBuilderExtensions.HasFillFactor), typeof(IndexBuilder), typeof(int));

        private static readonly MethodInfo _keyIsClusteredMethodInfo
            = typeof(SqlServerKeyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerKeyBuilderExtensions.IsClustered), typeof(KeyBuilder), typeof(bool));

        private static readonly MethodInfo _tableIsTemporalMethodInfo
            = typeof(SqlServerTableBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerTableBuilderExtensions.IsTemporal), typeof(TableBuilder), typeof(bool));

        private static readonly MethodInfo _temporalTableUseHistoryTableMethodInfo1
            = typeof(TemporalTableBuilder).GetRequiredRuntimeMethod(
                nameof(TemporalTableBuilder.UseHistoryTable), typeof(string), typeof(string));

        private static readonly MethodInfo _temporalTableUseHistoryTableMethodInfo2
            = typeof(TemporalTableBuilder).GetRequiredRuntimeMethod(
                nameof(TemporalTableBuilder.UseHistoryTable), typeof(string));

        private static readonly MethodInfo _temporalTableHasPeriodStartMethodInfo
            = typeof(TemporalTableBuilder).GetRequiredRuntimeMethod(
                nameof(TemporalTableBuilder.HasPeriodStart), typeof(string));

        private static readonly MethodInfo _temporalTableHasPeriodEndMethodInfo
            = typeof(TemporalTableBuilder).GetRequiredRuntimeMethod(
                nameof(TemporalTableBuilder.HasPeriodEnd), typeof(string));

        private static readonly MethodInfo _temporalPropertyHasColumnNameMethodInfo
            = typeof(TemporalPeriodPropertyBuilder).GetRequiredRuntimeMethod(
                nameof(TemporalPeriodPropertyBuilder.HasColumnName), typeof(string));

        #endregion MethodInfos

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IModel model,
            IDictionary<string, IAnnotation> annotations)
        {
            var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(model, annotations));

            if (GenerateValueGenerationStrategy(annotations, model, onModel: true) is MethodCallCodeFragment valueGenerationStrategy)
            {
                fragments.Add(valueGenerationStrategy);
            }

            GenerateSimpleFluentApiCall(
                annotations,
                SqlServerAnnotationNames.MaxDatabaseSize, _modelHasDatabaseMaxSizeMethodInfo,
                fragments);

            GenerateSimpleFluentApiCall(
                annotations,
                SqlServerAnnotationNames.ServiceTierSql, _modelHasServiceTierSqlMethodInfo,
                fragments);

            GenerateSimpleFluentApiCall(
                annotations,
                SqlServerAnnotationNames.PerformanceLevelSql, _modelHasPerformanceLevelSqlMethodInfo,
                fragments);

            return fragments;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IProperty property,
            IDictionary<string, IAnnotation> annotations)
        {
            var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(property, annotations));

            if (GenerateValueGenerationStrategy(annotations, property.DeclaringEntityType.Model, onModel: false) is MethodCallCodeFragment
                valueGenerationStrategy)
            {
                fragments.Add(valueGenerationStrategy);
            }

            if (GetAndRemove<bool?>(annotations, SqlServerAnnotationNames.Sparse) is bool isSparse)
            {
                fragments.Add(isSparse ? new(_propertyIsSparseMethodInfo) : new(_propertyIsSparseMethodInfo, false));
            }

            return fragments;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IEntityType entityType,
            IDictionary<string, IAnnotation> annotations)
        {
            var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(entityType, annotations));

            if (GetAndRemove<bool?>(annotations, SqlServerAnnotationNames.MemoryOptimized) is bool isMemoryOptimized)
            {
                fragments.Add(
                    isMemoryOptimized
                        ? new(_entityTypeIsMemoryOptimizedMethodInfo)
                        : new(_entityTypeIsMemoryOptimizedMethodInfo, false));
            }

            if (annotations.TryGetValue(SqlServerAnnotationNames.IsTemporal, out var isTemporalAnnotation)
                && isTemporalAnnotation.Value as bool? == true)
            {
                var historyTableName = annotations.ContainsKey(SqlServerAnnotationNames.TemporalHistoryTableName)
                    ? annotations[SqlServerAnnotationNames.TemporalHistoryTableName].Value as string
                    : null;

                var historyTableSchema = annotations.ContainsKey(SqlServerAnnotationNames.TemporalHistoryTableSchema)
                    ? annotations[SqlServerAnnotationNames.TemporalHistoryTableSchema].Value as string
                    : null;

                // for the RevEng path, we avoid adding period properties to the entity
                // because we don't want code for them to be generated - they need to be in shadow state
                // so if we don't find property on the entity, we know it's this scenario
                // and in that case period column name is actually the same as the period property name annotation
                // since in RevEng scenario there can't be custom column mapping
                // see #26007
                var periodStartPropertyName = entityType.GetPeriodStartPropertyName();
                var periodStartProperty = entityType.FindProperty(periodStartPropertyName!);
                var periodStartColumnName = periodStartProperty != null
                    ? periodStartProperty[RelationalAnnotationNames.ColumnName] as string
                    : periodStartPropertyName;

                var periodEndPropertyName = entityType.GetPeriodEndPropertyName();
                var periodEndProperty = entityType.FindProperty(periodEndPropertyName!);
                var periodEndColumnName = periodEndProperty != null
                    ? periodEndProperty[RelationalAnnotationNames.ColumnName] as string
                    : periodEndPropertyName;

                // ttb => ttb.UseHistoryTable("HistoryTable", "schema")
                var temporalTableBuilderCalls = new List<MethodCallCodeFragment>();
                if (historyTableName != null)
                {
                    temporalTableBuilderCalls.Add(
                        historyTableSchema != null
                            ? new MethodCallCodeFragment(_temporalTableUseHistoryTableMethodInfo1, historyTableName, historyTableSchema)
                            : new MethodCallCodeFragment(_temporalTableUseHistoryTableMethodInfo2, historyTableName));
                }

                // ttb => ttb.HasPeriodStart("Start").HasColumnName("ColumnStart")
                temporalTableBuilderCalls.Add(
                    periodStartColumnName != null
                        ? new MethodCallCodeFragment(_temporalTableHasPeriodStartMethodInfo, periodStartPropertyName)
                            .Chain(new MethodCallCodeFragment(_temporalPropertyHasColumnNameMethodInfo, periodStartColumnName))
                        : new MethodCallCodeFragment(_temporalTableHasPeriodStartMethodInfo, periodStartPropertyName));

                // ttb => ttb.HasPeriodEnd("End").HasColumnName("ColumnEnd")
                temporalTableBuilderCalls.Add(
                    periodEndColumnName != null
                        ? new MethodCallCodeFragment(_temporalTableHasPeriodEndMethodInfo, periodEndPropertyName)
                            .Chain(new MethodCallCodeFragment(_temporalPropertyHasColumnNameMethodInfo, periodEndColumnName))
                        : new MethodCallCodeFragment(_temporalTableHasPeriodEndMethodInfo, periodEndPropertyName));

                // ToTable(tb => tb.IsTemporal(ttb => { ... }))
                var toTemporalTableCall = new MethodCallCodeFragment(
                    _entityTypeToTableMethodInfo,
                    new NestedClosureCodeFragment(
                        "tb",
                        new MethodCallCodeFragment(
                            _tableIsTemporalMethodInfo,
                            new NestedClosureCodeFragment(
                                "ttb",
                                temporalTableBuilderCalls))));

                fragments.Add(toTemporalTableCall);

                annotations.Remove(SqlServerAnnotationNames.IsTemporal);
                annotations.Remove(SqlServerAnnotationNames.TemporalHistoryTableName);
                annotations.Remove(SqlServerAnnotationNames.TemporalHistoryTableSchema);
                annotations.Remove(SqlServerAnnotationNames.TemporalPeriodStartPropertyName);
                annotations.Remove(SqlServerAnnotationNames.TemporalPeriodEndPropertyName);
            }

            return fragments;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            if (annotation.Name == RelationalAnnotationNames.DefaultSchema)
            {
                return (string?)annotation.Value == "dbo";
            }

            return annotation.Name == SqlServerAnnotationNames.ValueGenerationStrategy
                && (SqlServerValueGenerationStrategy)annotation.Value! == SqlServerValueGenerationStrategy.IdentityColumn;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override MethodCallCodeFragment? GenerateFluentApi(IKey key, IAnnotation annotation)
            => annotation.Name == SqlServerAnnotationNames.Clustered
                ? (bool)annotation.Value! == false
                    ? new MethodCallCodeFragment(_keyIsClusteredMethodInfo, false)
                    : new MethodCallCodeFragment(_keyIsClusteredMethodInfo)
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override MethodCallCodeFragment? GenerateFluentApi(IIndex index, IAnnotation annotation)
            => annotation.Name switch
            {
                SqlServerAnnotationNames.Clustered => (bool)annotation.Value! == false
                    ? new MethodCallCodeFragment(_indexIsClusteredMethodInfo, false)
                    : new MethodCallCodeFragment(_indexIsClusteredMethodInfo),

                SqlServerAnnotationNames.Include => new MethodCallCodeFragment(_indexIncludePropertiesMethodInfo, annotation.Value),
                SqlServerAnnotationNames.FillFactor => new MethodCallCodeFragment(_indexHasFillFactorMethodInfo, annotation.Value),

                _ => null
            };

        private MethodCallCodeFragment? GenerateValueGenerationStrategy(
            IDictionary<string, IAnnotation> annotations,
            IModel model,
            bool onModel)
        {
            SqlServerValueGenerationStrategy strategy;
            if (annotations.TryGetValue(SqlServerAnnotationNames.ValueGenerationStrategy, out var strategyAnnotation)
                && strategyAnnotation.Value != null)
            {
                annotations.Remove(SqlServerAnnotationNames.ValueGenerationStrategy);
                strategy = (SqlServerValueGenerationStrategy)strategyAnnotation.Value;
            }
            else
            {
                return null;
            }

            switch (strategy)
            {
                case SqlServerValueGenerationStrategy.IdentityColumn:
                    // Support pre-6.0 IdentitySeed annotations, which contained an int rather than a long
                    if (annotations.TryGetValue(SqlServerAnnotationNames.IdentitySeed, out var seedAnnotation)
                        && seedAnnotation.Value != null)
                    {
                        annotations.Remove(SqlServerAnnotationNames.IdentitySeed);
                    }
                    else
                    {
                        seedAnnotation = model.FindAnnotation(SqlServerAnnotationNames.IdentitySeed);
                    }

                    var seed = seedAnnotation is null
                        ? 1
                        : seedAnnotation.Value is int intValue
                            ? intValue
                            : (long?)seedAnnotation.Value ?? 1;

                    var increment = GetAndRemove<int?>(annotations, SqlServerAnnotationNames.IdentityIncrement)
                        ?? model.FindAnnotation(SqlServerAnnotationNames.IdentityIncrement)?.Value as int?
                        ?? 1;
                    return new(
                        onModel ? _modelUseIdentityColumnsMethodInfo : _propertyUseIdentityColumnsMethodInfo,
                        seed,
                        increment);

                case SqlServerValueGenerationStrategy.SequenceHiLo:
                    var name = GetAndRemove<string>(annotations, SqlServerAnnotationNames.HiLoSequenceName);
                    var schema = GetAndRemove<string>(annotations, SqlServerAnnotationNames.HiLoSequenceSchema);
                    return new(
                        onModel ? _modelUseHiLoMethodInfo : _propertyUseHiLoMethodInfo,
                        (name, schema) switch
                        {
                            (null, null) => Array.Empty<object>(),
                            (_, null) => new object[] { name! },
                            _ => new object[] { name!, schema! }
                        });

                case SqlServerValueGenerationStrategy.None:
                    return new(
                        _modelHasAnnotationMethodInfo,
                        SqlServerAnnotationNames.ValueGenerationStrategy,
                        SqlServerValueGenerationStrategy.None);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static T? GetAndRemove<T>(IDictionary<string, IAnnotation> annotations, string annotationName)
        {
            if (annotations.TryGetValue(annotationName, out var annotation)
                && annotation.Value != null)
            {
                annotations.Remove(annotationName);
                return (T)annotation.Value;
            }

            return default;
        }

        private static void GenerateSimpleFluentApiCall(
            IDictionary<string, IAnnotation> annotations,
            string annotationName,
            MethodInfo methodInfo,
            List<MethodCallCodeFragment> methodCallCodeFragments)
        {
            if (annotations.TryGetValue(annotationName, out var annotation))
            {
                annotations.Remove(annotationName);
                if (annotation.Value is object annotationValue)
                {
                    methodCallCodeFragments.Add(
                        new MethodCallCodeFragment(methodInfo, annotationValue));
                }
            }
        }
    }
}
