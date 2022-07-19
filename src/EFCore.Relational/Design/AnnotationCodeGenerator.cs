// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

#pragma warning disable EF1001 // Accessing annotation names (internal)

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Base class to be used by database providers when implementing an <see cref="IAnnotationCodeGenerator" />
/// </summary>
/// <remarks>
///     <para>
///         This implementation returns <see langword="false" /> for all 'IsHandledByConvention' methods and
///         <see langword="null" /> for all 'GenerateFluentApi' methods. Providers should override for the
///         annotations that they understand.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class AnnotationCodeGenerator : IAnnotationCodeGenerator
{
    private static readonly ISet<string> IgnoredRelationalAnnotations = new HashSet<string>
    {
        RelationalAnnotationNames.CheckConstraints,
        RelationalAnnotationNames.Triggers,
        RelationalAnnotationNames.Sequences,
        RelationalAnnotationNames.DbFunctions,
        RelationalAnnotationNames.DeleteStoredProcedure,
        RelationalAnnotationNames.InsertStoredProcedure,
        RelationalAnnotationNames.UpdateStoredProcedure,
        RelationalAnnotationNames.MappingFragments,
        RelationalAnnotationNames.RelationalOverrides,
        RelationalAnnotationNames.ParameterDirection
    };

    #region MethodInfos

    private static readonly MethodInfo ModelHasDefaultSchemaMethodInfo
        = typeof(RelationalModelBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalModelBuilderExtensions.HasDefaultSchema), new[] { typeof(ModelBuilder), typeof(string) })!;

    private static readonly MethodInfo ModelUseCollationMethodInfo
        = typeof(RelationalModelBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalModelBuilderExtensions.UseCollation), new[] { typeof(ModelBuilder), typeof(string) })!;

    private static readonly MethodInfo EntityTypeHasCommentMethodInfo
        = typeof(RelationalEntityTypeBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalEntityTypeBuilderExtensions.HasComment), new[] { typeof(EntityTypeBuilder), typeof(string) })!;

    private static readonly MethodInfo EntityTypeUseTpcMappingStrategyMethodInfo
        = typeof(RelationalEntityTypeBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalEntityTypeBuilderExtensions.UseTpcMappingStrategy), new[] { typeof(EntityTypeBuilder) })!;

    private static readonly MethodInfo EntityTypeUseTphMappingStrategyMethodInfo
        = typeof(RelationalEntityTypeBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalEntityTypeBuilderExtensions.UseTphMappingStrategy), new[] { typeof(EntityTypeBuilder) })!;

    private static readonly MethodInfo EntityTypeUseTptMappingStrategyMethodInfo
        = typeof(RelationalEntityTypeBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalEntityTypeBuilderExtensions.UseTptMappingStrategy), new[] { typeof(EntityTypeBuilder) })!;

    private static readonly MethodInfo PropertyHasColumnNameMethodInfo
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.HasColumnName), new[] { typeof(PropertyBuilder), typeof(string) })!;

    private static readonly MethodInfo PropertyHasColumnOrderMethodInfo
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.HasColumnOrder), new[] { typeof(PropertyBuilder), typeof(int?) })!;

    private static readonly MethodInfo PropertyHasDefaultValueMethodInfo1
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.HasDefaultValue), new[] { typeof(PropertyBuilder) })!;

    private static readonly MethodInfo PropertyHasDefaultValueMethodInfo2
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.HasDefaultValue), new[] { typeof(PropertyBuilder), typeof(object) })!;

    private static readonly MethodInfo PropertyHasDefaultValueSqlMethodInfo1
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql), new[] { typeof(PropertyBuilder) })!;

    private static readonly MethodInfo PropertyHasDefaultValueSqlMethodInfo2
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql), new[] { typeof(PropertyBuilder), typeof(string) })!;

    private static readonly MethodInfo PropertyHasComputedColumnSqlMethodInfo1
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql), new[] { typeof(PropertyBuilder) })!;

    private static readonly MethodInfo PropertyHasComputedColumnSqlMethodInfo2
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql), new[] { typeof(PropertyBuilder), typeof(string) })!;

    private static readonly MethodInfo HasComputedColumnSqlMethodInfo3
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql), new[] { typeof(PropertyBuilder), typeof(string), typeof(bool) })!;

    private static readonly MethodInfo PropertyIsFixedLengthMethodInfo
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.IsFixedLength), new[] { typeof(PropertyBuilder), typeof(bool) })!;

    private static readonly MethodInfo PropertyHasCommentMethodInfo
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.HasComment), new[] { typeof(PropertyBuilder), typeof(string) })!;

    private static readonly MethodInfo PropertyUseCollationMethodInfo
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.UseCollation), new[] { typeof(PropertyBuilder), typeof(string) })!;

    private static readonly MethodInfo PropertyHasColumnTypeMethodInfo
        = typeof(RelationalPropertyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalPropertyBuilderExtensions.HasColumnType), new[] { typeof(PropertyBuilder), typeof(string) })!;

    private static readonly MethodInfo KeyHasNameMethodInfo
        = typeof(RelationalKeyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalKeyBuilderExtensions.HasName), new[] { typeof(KeyBuilder), typeof(string) })!;

    private static readonly MethodInfo ReferenceReferenceHasConstraintNameMethodInfo
        = typeof(RelationalForeignKeyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalForeignKeyBuilderExtensions.HasConstraintName), new[] { typeof(ReferenceReferenceBuilder), typeof(string) })!;

    private static readonly MethodInfo ReferenceCollectionHasConstraintNameMethodInfo
        = typeof(RelationalForeignKeyBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalForeignKeyBuilderExtensions.HasConstraintName), new[] { typeof(ReferenceCollectionBuilder), typeof(string) })!;

    private static readonly MethodInfo IndexHasDatabaseNameMethodInfo
        = typeof(RelationalIndexBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalIndexBuilderExtensions.HasDatabaseName), new[] { typeof(IndexBuilder), typeof(string) })!;

    private static readonly MethodInfo IndexHasFilterNameMethodInfo
        = typeof(RelationalIndexBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalIndexBuilderExtensions.HasFilter), new[] { typeof(IndexBuilder), typeof(string) })!;

    #endregion MethodInfos

    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public AnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual AnnotationCodeGeneratorDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual IEnumerable<IAnnotation> FilterIgnoredAnnotations(IEnumerable<IAnnotation> annotations)
        => annotations.Where(
            a => !(CoreAnnotationNames.AllNames.Contains(a.Name)
                || IgnoredRelationalAnnotations.Contains(a.Name)));

    /// <inheritdoc />
    public virtual void RemoveAnnotationsHandledByConventions(IModel model, IDictionary<string, IAnnotation> annotations)
        => RemoveConventionalAnnotationsHelper(model, annotations, IsHandledByConvention);

    /// <inheritdoc />
    public virtual void RemoveAnnotationsHandledByConventions(
        IEntityType entityType,
        IDictionary<string, IAnnotation> annotations)
    {
        annotations.Remove(RelationalAnnotationNames.IsTableExcludedFromMigrations);

        var schema = entityType.GetSchema();
        var defaultSchema = entityType.Model.GetDefaultSchema();
        if (schema != null && schema != defaultSchema)
        {
            annotations.Remove(RelationalAnnotationNames.Schema);
        }

        var viewSchema = entityType.GetViewSchema();
        if (viewSchema != null && viewSchema != defaultSchema)
        {
            annotations.Remove(RelationalAnnotationNames.ViewSchema);
        }

        RemoveConventionalAnnotationsHelper(entityType, annotations, IsHandledByConvention);
    }

    /// <inheritdoc />
    public virtual void RemoveAnnotationsHandledByConventions(
        IEntityTypeMappingFragment fragment,
        IDictionary<string, IAnnotation> annotations)
        => RemoveConventionalAnnotationsHelper(fragment, annotations, IsHandledByConvention);

    /// <inheritdoc />
    public virtual void RemoveAnnotationsHandledByConventions(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
    {
        var columnName = property.GetColumnName();
        if (columnName == property.Name)
        {
            annotations.Remove(RelationalAnnotationNames.ColumnName);
        }

        RemoveConventionalAnnotationsHelper(property, annotations, IsHandledByConvention);
    }

    /// <inheritdoc />
    public virtual void RemoveAnnotationsHandledByConventions(IKey key, IDictionary<string, IAnnotation> annotations)
    {
        if (key.GetName() == key.GetDefaultName())
        {
            annotations.Remove(RelationalAnnotationNames.Name);
        }

        RemoveConventionalAnnotationsHelper(key, annotations, IsHandledByConvention);
    }

    /// <inheritdoc />
    public virtual void RemoveAnnotationsHandledByConventions(
        IForeignKey foreignKey,
        IDictionary<string, IAnnotation> annotations)
        => RemoveConventionalAnnotationsHelper(foreignKey, annotations, IsHandledByConvention);

    /// <inheritdoc />
    public virtual void RemoveAnnotationsHandledByConventions(IIndex index, IDictionary<string, IAnnotation> annotations)
        => RemoveConventionalAnnotationsHelper(index, annotations, IsHandledByConvention);

    /// <inheritdoc />
    public virtual void RemoveAnnotationsHandledByConventions(
        ICheckConstraint checkConstraint, IDictionary<string, IAnnotation> annotations)
        => RemoveConventionalAnnotationsHelper(checkConstraint, annotations, IsHandledByConvention);

    /// <inheritdoc />
    public virtual void RemoveAnnotationsHandledByConventions(ITrigger trigger, IDictionary<string, IAnnotation> annotations)
        => RemoveConventionalAnnotationsHelper(trigger, annotations, IsHandledByConvention);

    /// <inheritdoc />
    public virtual void RemoveAnnotationsHandledByConventions(
        IRelationalPropertyOverrides overrides, IDictionary<string, IAnnotation> annotations)
        => RemoveConventionalAnnotationsHelper(overrides, annotations, IsHandledByConvention);

    /// <inheritdoc />
    public virtual void RemoveAnnotationsHandledByConventions(
        ISequence sequence, IDictionary<string, IAnnotation> annotations)
        => RemoveConventionalAnnotationsHelper(sequence, annotations, IsHandledByConvention);

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IModel model,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        GenerateSimpleFluentApiCall(
            annotations,
            RelationalAnnotationNames.DefaultSchema, ModelHasDefaultSchemaMethodInfo,
            methodCallCodeFragments);

        GenerateSimpleFluentApiCall(
            annotations,
            RelationalAnnotationNames.Collation, ModelUseCollationMethodInfo,
            methodCallCodeFragments);

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(model, annotations, GenerateFluentApi));
        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IEntityType entityType,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        GenerateSimpleFluentApiCall(
            annotations,
            RelationalAnnotationNames.Comment, EntityTypeHasCommentMethodInfo, methodCallCodeFragments);

        if (annotations.TryGetValue(RelationalAnnotationNames.MappingStrategy, out var mappingStrategyAnnotation)
            && mappingStrategyAnnotation.Value is string mappingStrategy)
        {
            var strategyCall = mappingStrategy switch
            {
                RelationalAnnotationNames.TpcMappingStrategy => EntityTypeUseTpcMappingStrategyMethodInfo,
                RelationalAnnotationNames.TptMappingStrategy => EntityTypeUseTptMappingStrategyMethodInfo,
                RelationalAnnotationNames.TphMappingStrategy => EntityTypeUseTphMappingStrategyMethodInfo,
                _ => null
            };

            if (strategyCall != null)
            {
                if (entityType.BaseType == null)
                {
                    methodCallCodeFragments.Add(new MethodCallCodeFragment(strategyCall));
                }

                annotations.Remove(mappingStrategyAnnotation.Name);
            }
        }

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(entityType, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IEntityTypeMappingFragment fragment,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(fragment, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        GenerateSimpleFluentApiCall(
            annotations,
            RelationalAnnotationNames.ColumnType, PropertyHasColumnTypeMethodInfo, methodCallCodeFragments);

        if (TryGetAndRemove(annotations, RelationalAnnotationNames.DefaultValue, out object? defaultValue))
        {
            methodCallCodeFragments.Add(
                defaultValue == DBNull.Value
                    ? new MethodCallCodeFragment(PropertyHasDefaultValueMethodInfo1)
                    : new MethodCallCodeFragment(PropertyHasDefaultValueMethodInfo2, defaultValue));
        }

        GenerateSimpleFluentApiCall(
            annotations,
            RelationalAnnotationNames.ColumnName, PropertyHasColumnNameMethodInfo, methodCallCodeFragments);

        GenerateSimpleFluentApiCall(
            annotations,
            RelationalAnnotationNames.ColumnOrder, PropertyHasColumnOrderMethodInfo, methodCallCodeFragments);

        if (TryGetAndRemove(annotations, RelationalAnnotationNames.DefaultValueSql, out string? defaultValueSql))
        {
            methodCallCodeFragments.Add(
                defaultValueSql.Length == 0
                    ? new MethodCallCodeFragment(PropertyHasDefaultValueSqlMethodInfo1)
                    : new MethodCallCodeFragment(PropertyHasDefaultValueSqlMethodInfo2, defaultValueSql));
        }

        if (TryGetAndRemove(annotations, RelationalAnnotationNames.ComputedColumnSql, out string? computedColumnSql))
        {
            methodCallCodeFragments.Add(
                computedColumnSql.Length == 0
                    ? new MethodCallCodeFragment(PropertyHasComputedColumnSqlMethodInfo1)
                    : TryGetAndRemove(annotations, RelationalAnnotationNames.IsStored, out bool isStored)
                        ? new MethodCallCodeFragment(HasComputedColumnSqlMethodInfo3, computedColumnSql, isStored)
                        : new MethodCallCodeFragment(PropertyHasComputedColumnSqlMethodInfo2, computedColumnSql));
        }

        if (TryGetAndRemove(annotations, RelationalAnnotationNames.IsFixedLength, out bool isFixedLength))
        {
            methodCallCodeFragments.Add(
                isFixedLength
                    ? new MethodCallCodeFragment(PropertyIsFixedLengthMethodInfo)
                    : new MethodCallCodeFragment(PropertyIsFixedLengthMethodInfo, false));
        }

        GenerateSimpleFluentApiCall(
            annotations,
            RelationalAnnotationNames.Comment, PropertyHasCommentMethodInfo, methodCallCodeFragments);

        GenerateSimpleFluentApiCall(
            annotations,
            RelationalAnnotationNames.Collation, PropertyUseCollationMethodInfo, methodCallCodeFragments);

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(property, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IKey key,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        GenerateSimpleFluentApiCall(annotations, RelationalAnnotationNames.Name, KeyHasNameMethodInfo, methodCallCodeFragments);

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(key, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IForeignKey foreignKey,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        GenerateSimpleFluentApiCall(
            annotations,
            RelationalAnnotationNames.Name,
            foreignKey.IsUnique ? ReferenceReferenceHasConstraintNameMethodInfo : ReferenceCollectionHasConstraintNameMethodInfo,
            methodCallCodeFragments);

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(foreignKey, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        INavigation navigation,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(navigation, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        ISkipNavigation navigation,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(navigation, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IIndex index,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        GenerateSimpleFluentApiCall(
            annotations, RelationalAnnotationNames.Name, IndexHasDatabaseNameMethodInfo, methodCallCodeFragments);
        GenerateSimpleFluentApiCall(
            annotations, RelationalAnnotationNames.Filter, IndexHasFilterNameMethodInfo, methodCallCodeFragments);

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(index, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        ICheckConstraint checkConstraint,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(checkConstraint, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        ITrigger trigger,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(trigger, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IRelationalPropertyOverrides overrides,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(overrides, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        ISequence sequence,
        IDictionary<string, IAnnotation> annotations)
    {
        var methodCallCodeFragments = new List<MethodCallCodeFragment>();

        methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(sequence, annotations, GenerateFluentApi));

        return methodCallCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<AttributeCodeFragment> GenerateDataAnnotationAttributes(
        IEntityType entityType,
        IDictionary<string, IAnnotation> annotations)
    {
        var attributeCodeFragments = new List<AttributeCodeFragment>();

        attributeCodeFragments.AddRange(GenerateFluentApiCallsHelper(entityType, annotations, GenerateDataAnnotation));

        return attributeCodeFragments;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<AttributeCodeFragment> GenerateDataAnnotationAttributes(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
    {
        var attributeCodeFragments = new List<AttributeCodeFragment>();

        if (TryGetAndRemove(annotations, CoreAnnotationNames.MaxLength, out int maxLength))
        {
            attributeCodeFragments.Add(
                new AttributeCodeFragment(
                    property.ClrType == typeof(string)
                        ? typeof(StringLengthAttribute)
                        : typeof(MaxLengthAttribute),
                    maxLength));
        }

        attributeCodeFragments.AddRange(GenerateFluentApiCallsHelper(property, annotations, GenerateDataAnnotation));

        return attributeCodeFragments;
    }

    /// <summary>
    ///     Checks if the given <paramref name="annotation" /> is handled by convention when
    ///     applied to the given <paramref name="model" />.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="false" />.
    /// </remarks>
    /// <param name="model">The <see cref="IModel" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns>
    ///     <see langword="true" /> if the annotation is handled by convention;
    ///     <see langword="false" /> if code must be generated.
    /// </returns>
    protected virtual bool IsHandledByConvention(IModel model, IAnnotation annotation)
        => false;

    /// <summary>
    ///     Checks if the given <paramref name="annotation" /> is handled by convention when
    ///     applied to the given <paramref name="entityType" />.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="false" />.
    /// </remarks>
    /// <param name="entityType">The <see cref="IEntityType" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="false" />.</returns>
    protected virtual bool IsHandledByConvention(IEntityType entityType, IAnnotation annotation)
        => false;

    /// <summary>
    ///     Checks if the given <paramref name="annotation" /> is handled by convention when
    ///     applied to the given <paramref name="fragment" />.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="false" />.
    /// </remarks>
    /// <param name="fragment">The <see cref="IEntityTypeMappingFragment" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="false" />.</returns>
    protected virtual bool IsHandledByConvention(IEntityTypeMappingFragment fragment, IAnnotation annotation)
        => false;

    /// <summary>
    ///     Checks if the given <paramref name="annotation" /> is handled by convention when
    ///     applied to the given <paramref name="key" />.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="false" />.
    /// </remarks>
    /// <param name="key">The <see cref="IKey" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="false" />.</returns>
    protected virtual bool IsHandledByConvention(IKey key, IAnnotation annotation)
        => false;

    /// <summary>
    ///     Checks if the given <paramref name="annotation" /> is handled by convention when
    ///     applied to the given <paramref name="property" />.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="false" />.
    /// </remarks>
    /// <param name="property">The <see cref="IProperty" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="false" />.</returns>
    protected virtual bool IsHandledByConvention(IProperty property, IAnnotation annotation)
        => false;

    /// <summary>
    ///     Checks if the given <paramref name="annotation" /> is handled by convention when
    ///     applied to the given <paramref name="foreignKey" />.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="false" />.
    /// </remarks>
    /// <param name="foreignKey">The <see cref="IForeignKey" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="false" />.</returns>
    protected virtual bool IsHandledByConvention(IForeignKey foreignKey, IAnnotation annotation)
        => false;

    /// <summary>
    ///     Checks if the given <paramref name="annotation" /> is handled by convention when
    ///     applied to the given <paramref name="index" />.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="false" />.
    /// </remarks>
    /// <param name="index">The <see cref="IIndex" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="false" />.</returns>
    protected virtual bool IsHandledByConvention(IIndex index, IAnnotation annotation)
        => false;

    /// <summary>
    ///     Checks if the given <paramref name="annotation" /> is handled by convention when
    ///     applied to the given <paramref name="checkConstraint" />.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="false" />.
    /// </remarks>
    /// <param name="checkConstraint">The <see cref="ICheckConstraint" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="false" />.</returns>
    protected virtual bool IsHandledByConvention(ICheckConstraint checkConstraint, IAnnotation annotation)
        => false;

    /// <summary>
    ///     Checks if the given <paramref name="annotation" /> is handled by convention when
    ///     applied to the given <paramref name="trigger" />.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="false" />.
    /// </remarks>
    /// <param name="trigger">The <see cref="ITrigger" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="false" />.</returns>
    protected virtual bool IsHandledByConvention(ITrigger trigger, IAnnotation annotation)
        => false;

    /// <summary>
    ///     Checks if the given <paramref name="annotation" /> is handled by convention when
    ///     applied to the given <paramref name="overrides" />.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="false" />.
    /// </remarks>
    /// <param name="overrides">The <see cref="IRelationalPropertyOverrides" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="false" />.</returns>
    protected virtual bool IsHandledByConvention(IRelationalPropertyOverrides overrides, IAnnotation annotation)
        => false;

    /// <summary>
    ///     Checks if the given <paramref name="annotation" /> is handled by convention when
    ///     applied to the given <paramref name="sequence" />.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="false" />.
    /// </remarks>
    /// <param name="sequence">The <see cref="ISequence" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="false" />.</returns>
    protected virtual bool IsHandledByConvention(ISequence sequence, IAnnotation annotation)
        => false;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="model">The <see cref="IModel" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(IModel model, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="entityType">The <see cref="IEntityType" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="fragment">The <see cref="IEntityTypeMappingFragment" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(IEntityTypeMappingFragment fragment, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="key">The <see cref="IKey" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(IKey key, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="property">The <see cref="IProperty" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(IProperty property, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="foreignKey">The <see cref="IForeignKey" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(IForeignKey foreignKey, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="navigation">The <see cref="INavigation" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(INavigation navigation, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="navigation">The <see cref="ISkipNavigation" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(ISkipNavigation navigation, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="index">The <see cref="IIndex" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(IIndex index, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="checkConstraint">The <see cref="ICheckConstraint" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(ICheckConstraint checkConstraint, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="trigger">The <see cref="ITrigger" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(ITrigger trigger, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="overrides">The <see cref="IRelationalPropertyOverrides" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(IRelationalPropertyOverrides overrides, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
    ///     if no fluent API call exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="sequence">The <see cref="ISequence" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual MethodCallCodeFragment? GenerateFluentApi(ISequence sequence, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a data annotation attribute code fragment for the given <paramref name="annotation" />,
    ///     or <see langword="null" /> if no data annotation exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="entityType">The <see cref="IEntityType" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual AttributeCodeFragment? GenerateDataAnnotation(IEntityType entityType, IAnnotation annotation)
        => null;

    /// <summary>
    ///     Returns a data annotation attribute code fragment for the given <paramref name="annotation" />,
    ///     or <see langword="null" /> if no data annotation exists for it.
    /// </summary>
    /// <remarks>
    ///     The default implementation always returns <see langword="null" />.
    /// </remarks>
    /// <param name="property">The <see cref="IProperty" />.</param>
    /// <param name="annotation">The <see cref="IAnnotation" />.</param>
    /// <returns><see langword="null" />.</returns>
    protected virtual AttributeCodeFragment? GenerateDataAnnotation(IProperty property, IAnnotation annotation)
        => null;

    private static IEnumerable<TCodeFragment> GenerateFluentApiCallsHelper<TAnnotatable, TCodeFragment>(
        TAnnotatable annotatable,
        IDictionary<string, IAnnotation> annotations,
        Func<TAnnotatable, IAnnotation, TCodeFragment?> generateCodeFragment)
        where TCodeFragment : notnull
    {
        foreach (var (name, annotation) in EnumerateForRemoval(annotations))
        {
            var codeFragment = generateCodeFragment(annotatable, annotation);
            if (codeFragment != null)
            {
                yield return codeFragment;
                annotations.Remove(name);
            }
        }
    }

    private static void RemoveConventionalAnnotationsHelper<TAnnotatable>(
        TAnnotatable annotatable,
        IDictionary<string, IAnnotation> annotations,
        Func<TAnnotatable, IAnnotation, bool> isHandledByConvention)
    {
        foreach (var (name, annotation) in EnumerateForRemoval(annotations))
        {
            if (isHandledByConvention(annotatable, annotation))
            {
                annotations.Remove(name);
            }
        }
    }

    private static bool TryGetAndRemove<T>(
        IDictionary<string, IAnnotation> annotations,
        string annotationName,
        [NotNullWhen(true)] out T? annotationValue)
    {
        if (annotations.TryGetValue(annotationName, out var annotation)
            && annotation.Value != null)
        {
            annotations.Remove(annotationName);
            annotationValue = (T)annotation.Value;
            return true;
        }

        annotationValue = default;
        return false;
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

    // Dictionary is safe for removal during enumeration
    private static IEnumerable<KeyValuePair<string, IAnnotation>> EnumerateForRemoval(IDictionary<string, IAnnotation> annotations)
        => annotations is Dictionary<string, IAnnotation> ? annotations : annotations.ToList();
}
