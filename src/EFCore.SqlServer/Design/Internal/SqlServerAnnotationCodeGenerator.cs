// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerAnnotationCodeGenerator : AnnotationCodeGenerator
{
    #region MethodInfos

    private static readonly MethodInfo ModelUseIdentityColumnsMethodInfo
        = typeof(SqlServerModelBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerModelBuilderExtensions.UseIdentityColumns), [typeof(ModelBuilder), typeof(long), typeof(int)])!;

    private static readonly MethodInfo ModelUseHiLoMethodInfo
        = typeof(SqlServerModelBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerModelBuilderExtensions.UseHiLo), [typeof(ModelBuilder), typeof(string), typeof(string)])!;

    private static readonly MethodInfo ModelUseKeySequencesMethodInfo
        = typeof(SqlServerModelBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerModelBuilderExtensions.UseKeySequences), [typeof(ModelBuilder), typeof(string), typeof(string)])!;

    private static readonly MethodInfo ModelHasDatabaseMaxSizeMethodInfo
        = typeof(SqlServerModelBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerModelBuilderExtensions.HasDatabaseMaxSize), [typeof(ModelBuilder), typeof(string)])!;

    private static readonly MethodInfo ModelHasServiceTierSqlMethodInfo
        = typeof(SqlServerModelBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerModelBuilderExtensions.HasServiceTierSql), [typeof(ModelBuilder), typeof(string)])!;

    private static readonly MethodInfo ModelHasPerformanceLevelSqlMethodInfo
        = typeof(SqlServerModelBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerModelBuilderExtensions.HasPerformanceLevelSql), [typeof(ModelBuilder), typeof(string)])!;

    private static readonly MethodInfo ModelHasAnnotationMethodInfo
        = typeof(ModelBuilder).GetRuntimeMethod(
            nameof(ModelBuilder.HasAnnotation), [typeof(string), typeof(object)])!;

    private static readonly MethodInfo EntityTypeToTableMethodInfo
        = typeof(RelationalEntityTypeBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalEntityTypeBuilderExtensions.ToTable), [typeof(EntityTypeBuilder), typeof(string)])!;

    private static readonly MethodInfo EntityTypeIsMemoryOptimizedMethodInfo
        = typeof(SqlServerEntityTypeBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerEntityTypeBuilderExtensions.IsMemoryOptimized), [typeof(EntityTypeBuilder), typeof(bool)])!;

    private static readonly MethodInfo PropertyIsSparseMethodInfo
        = typeof(SqlServerPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerPropertyBuilderExtensions.IsSparse), [typeof(PropertyBuilder), typeof(bool)])!;

    private static readonly MethodInfo PropertyUseIdentityColumnsMethodInfo
        = typeof(SqlServerPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerPropertyBuilderExtensions.UseIdentityColumn), [typeof(PropertyBuilder), typeof(long), typeof(int)])!;

    private static readonly MethodInfo PropertyUseHiLoMethodInfo
        = typeof(SqlServerPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerPropertyBuilderExtensions.UseHiLo), [typeof(PropertyBuilder), typeof(string), typeof(string)])!;

    private static readonly MethodInfo PropertyUseSequenceMethodInfo
        = typeof(SqlServerPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerPropertyBuilderExtensions.UseSequence), [typeof(PropertyBuilder), typeof(string), typeof(string)])!;

    private static readonly MethodInfo IndexIsClusteredMethodInfo
        = typeof(SqlServerIndexBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerIndexBuilderExtensions.IsClustered), [typeof(IndexBuilder), typeof(bool)])!;

    private static readonly MethodInfo IndexIncludePropertiesMethodInfo
        = typeof(SqlServerIndexBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerIndexBuilderExtensions.IncludeProperties), [typeof(IndexBuilder), typeof(string[])])!;

    private static readonly MethodInfo IndexHasFillFactorMethodInfo
        = typeof(SqlServerIndexBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerIndexBuilderExtensions.HasFillFactor), [typeof(IndexBuilder), typeof(int)])!;

    private static readonly MethodInfo IndexSortInTempDbMethodInfo
        = typeof(SqlServerIndexBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerIndexBuilderExtensions.SortInTempDb), [typeof(IndexBuilder), typeof(bool)])!;

    private static readonly MethodInfo IndexUseDataCompressionMethodInfo
        = typeof(SqlServerIndexBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerIndexBuilderExtensions.UseDataCompression), [typeof(IndexBuilder), typeof(DataCompressionType)])!;

    private static readonly MethodInfo KeyIsClusteredMethodInfo
        = typeof(SqlServerKeyBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerKeyBuilderExtensions.IsClustered), [typeof(KeyBuilder), typeof(bool)])!;

    private static readonly MethodInfo KeyHasFillFactorMethodInfo
        = typeof(SqlServerKeyBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerKeyBuilderExtensions.HasFillFactor), [typeof(KeyBuilder), typeof(int)])!;

    private static readonly MethodInfo TableIsTemporalMethodInfo
        = typeof(SqlServerTableBuilderExtensions).GetRuntimeMethod(
            nameof(SqlServerTableBuilderExtensions.IsTemporal), [typeof(TableBuilder), typeof(bool)])!;

    private static readonly MethodInfo TemporalTableUseHistoryTableMethodInfo1
        = typeof(TemporalTableBuilder).GetRuntimeMethod(
            nameof(TemporalTableBuilder.UseHistoryTable), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo TemporalTableUseHistoryTableMethodInfo2
        = typeof(TemporalTableBuilder).GetRuntimeMethod(
            nameof(TemporalTableBuilder.UseHistoryTable), [typeof(string)])!;

    private static readonly MethodInfo TemporalTableHasPeriodStartMethodInfo
        = typeof(TemporalTableBuilder).GetRuntimeMethod(
            nameof(TemporalTableBuilder.HasPeriodStart), [typeof(string)])!;

    private static readonly MethodInfo TemporalTableHasPeriodEndMethodInfo
        = typeof(TemporalTableBuilder).GetRuntimeMethod(
            nameof(TemporalTableBuilder.HasPeriodEnd), [typeof(string)])!;

    private static readonly MethodInfo TemporalPropertyHasColumnNameMethodInfo
        = typeof(TemporalPeriodPropertyBuilder).GetRuntimeMethod(
            nameof(TemporalPeriodPropertyBuilder.HasColumnName), [typeof(string)])!;

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
            SqlServerAnnotationNames.MaxDatabaseSize, ModelHasDatabaseMaxSizeMethodInfo,
            fragments);

        GenerateSimpleFluentApiCall(
            annotations,
            SqlServerAnnotationNames.ServiceTierSql, ModelHasServiceTierSqlMethodInfo,
            fragments);

        GenerateSimpleFluentApiCall(
            annotations,
            SqlServerAnnotationNames.PerformanceLevelSql, ModelHasPerformanceLevelSqlMethodInfo,
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

        if (GenerateValueGenerationStrategy(annotations, property.DeclaringType.Model, onModel: false) is MethodCallCodeFragment
            valueGenerationStrategy)
        {
            fragments.Add(valueGenerationStrategy);
        }

        if (GetAndRemove<bool?>(annotations, SqlServerAnnotationNames.Sparse) is bool isSparse)
        {
            fragments.Add(
                isSparse
                    ? new MethodCallCodeFragment(PropertyIsSparseMethodInfo)
                    : new MethodCallCodeFragment(PropertyIsSparseMethodInfo, false));
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
                    ? new MethodCallCodeFragment(EntityTypeIsMemoryOptimizedMethodInfo)
                    : new MethodCallCodeFragment(EntityTypeIsMemoryOptimizedMethodInfo, false));
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
                        ? new MethodCallCodeFragment(TemporalTableUseHistoryTableMethodInfo1, historyTableName, historyTableSchema)
                        : new MethodCallCodeFragment(TemporalTableUseHistoryTableMethodInfo2, historyTableName));
            }

            // ttb => ttb.HasPeriodStart("Start").HasColumnName("ColumnStart")
            temporalTableBuilderCalls.Add(
                periodStartColumnName != null
                    ? new MethodCallCodeFragment(TemporalTableHasPeriodStartMethodInfo, periodStartPropertyName)
                        .Chain(new MethodCallCodeFragment(TemporalPropertyHasColumnNameMethodInfo, periodStartColumnName))
                    : new MethodCallCodeFragment(TemporalTableHasPeriodStartMethodInfo, periodStartPropertyName));

            // ttb => ttb.HasPeriodEnd("End").HasColumnName("ColumnEnd")
            temporalTableBuilderCalls.Add(
                periodEndColumnName != null
                    ? new MethodCallCodeFragment(TemporalTableHasPeriodEndMethodInfo, periodEndPropertyName)
                        .Chain(new MethodCallCodeFragment(TemporalPropertyHasColumnNameMethodInfo, periodEndColumnName))
                    : new MethodCallCodeFragment(TemporalTableHasPeriodEndMethodInfo, periodEndPropertyName));

            // ToTable(tb => tb.IsTemporal(ttb => { ... }))
            var toTemporalTableCall = new MethodCallCodeFragment(
                EntityTypeToTableMethodInfo,
                new NestedClosureCodeFragment(
                    "tb",
                    new MethodCallCodeFragment(
                        TableIsTemporalMethodInfo,
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
    protected override bool IsHandledByConvention(IProperty property, IAnnotation annotation)
    {
        if (annotation.Name == SqlServerAnnotationNames.ValueGenerationStrategy)
        {
            return (SqlServerValueGenerationStrategy)annotation.Value! == property.DeclaringType.Model.GetValueGenerationStrategy();
        }

        return base.IsHandledByConvention(property, annotation);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MethodCallCodeFragment? GenerateFluentApi(IKey key, IAnnotation annotation)
        => annotation.Name switch
        {
            SqlServerAnnotationNames.Clustered => (bool)annotation.Value! == false
                ? new MethodCallCodeFragment(KeyIsClusteredMethodInfo, false)
                : new MethodCallCodeFragment(KeyIsClusteredMethodInfo),

            SqlServerAnnotationNames.FillFactor => new MethodCallCodeFragment(KeyHasFillFactorMethodInfo, annotation.Value),

            _ => null
        };

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
                ? new MethodCallCodeFragment(IndexIsClusteredMethodInfo, false)
                : new MethodCallCodeFragment(IndexIsClusteredMethodInfo),

            SqlServerAnnotationNames.Include => new MethodCallCodeFragment(IndexIncludePropertiesMethodInfo, annotation.Value),
            SqlServerAnnotationNames.FillFactor => new MethodCallCodeFragment(IndexHasFillFactorMethodInfo, annotation.Value),
            SqlServerAnnotationNames.SortInTempDb => new MethodCallCodeFragment(IndexSortInTempDbMethodInfo, annotation.Value),
            SqlServerAnnotationNames.DataCompression => new MethodCallCodeFragment(IndexUseDataCompressionMethodInfo, annotation.Value),

            _ => null
        };

    private static MethodCallCodeFragment? GenerateValueGenerationStrategy(
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
                    ? 1L
                    : seedAnnotation.Value is int intValue
                        ? intValue
                        : (long?)seedAnnotation.Value ?? 1L;

                var increment = GetAndRemove<int?>(annotations, SqlServerAnnotationNames.IdentityIncrement)
                    ?? model.FindAnnotation(SqlServerAnnotationNames.IdentityIncrement)?.Value as int?
                    ?? 1;
                return new MethodCallCodeFragment(
                    onModel ? ModelUseIdentityColumnsMethodInfo : PropertyUseIdentityColumnsMethodInfo,
                    (seed, increment) switch
                    {
                        (1L, 1) => [],
                        (_, 1) => [seed],
                        _ => [seed, increment]
                    });

            case SqlServerValueGenerationStrategy.SequenceHiLo:
            {
                var name = GetAndRemove<string>(annotations, SqlServerAnnotationNames.HiLoSequenceName);
                var schema = GetAndRemove<string>(annotations, SqlServerAnnotationNames.HiLoSequenceSchema);
                return new MethodCallCodeFragment(
                    onModel ? ModelUseHiLoMethodInfo : PropertyUseHiLoMethodInfo,
                    (name, schema) switch
                    {
                        (null, null) => [],
                        (_, null) => [name],
                        _ => [name!, schema]
                    });
            }

            case SqlServerValueGenerationStrategy.Sequence:
            {
                var nameOrSuffix = GetAndRemove<string>(
                    annotations,
                    onModel ? SqlServerAnnotationNames.SequenceNameSuffix : SqlServerAnnotationNames.SequenceName);

                var schema = GetAndRemove<string>(annotations, SqlServerAnnotationNames.SequenceSchema);
                return new MethodCallCodeFragment(
                    onModel ? ModelUseKeySequencesMethodInfo : PropertyUseSequenceMethodInfo,
                    (name: nameOrSuffix, schema) switch
                    {
                        (null, null) => [],
                        (_, null) => [nameOrSuffix],
                        _ => [nameOrSuffix!, schema]
                    });
            }

            case SqlServerValueGenerationStrategy.None:
                return new MethodCallCodeFragment(
                    ModelHasAnnotationMethodInfo,
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
