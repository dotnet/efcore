// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using NotNullWhenAttribute = System.Diagnostics.CodeAnalysis.NotNullWhenAttribute;

#pragma warning disable EF1001 // Accessing annotation names (internal)

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     <para>
    ///         Base class to be used by database providers when implementing an <see cref="IAnnotationCodeGenerator" />
    ///     </para>
    ///     <para>
    ///         This implementation returns <see langword="false" /> for all 'IsHandledByConvention' methods and
    ///         <see langword="null" /> for all 'GenerateFluentApi' methods. Providers should override for the
    ///         annotations that they understand.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information.
    /// </remarks>
    public class AnnotationCodeGenerator : IAnnotationCodeGenerator
    {
        private static readonly ISet<string> _ignoredRelationalAnnotations = new HashSet<string>
        {
            RelationalAnnotationNames.CheckConstraints,
            RelationalAnnotationNames.Sequences,
            RelationalAnnotationNames.DbFunctions,
            RelationalAnnotationNames.RelationalOverrides
        };

        #region MethodInfos

        private static readonly MethodInfo _modelHasDefaultSchemaMethodInfo
            = typeof(RelationalModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalModelBuilderExtensions.HasDefaultSchema), typeof(ModelBuilder), typeof(string));

        private static readonly MethodInfo _modelUseCollationMethodInfo
            = typeof(RelationalModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalModelBuilderExtensions.UseCollation), typeof(ModelBuilder), typeof(string));

        private static readonly MethodInfo _entityTypeHasCommentMethodInfo
            = typeof(RelationalEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalEntityTypeBuilderExtensions.HasComment), typeof(EntityTypeBuilder), typeof(string));

        private static readonly MethodInfo _propertyHasColumnNameMethodInfo
            = typeof(RelationalPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalPropertyBuilderExtensions.HasColumnName), typeof(PropertyBuilder), typeof(string));

        private static readonly MethodInfo _propertyHasDefaultValueSqlMethodInfo1
            = typeof(RelationalPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql), typeof(PropertyBuilder));

        private static readonly MethodInfo _propertyHasDefaultValueSqlMethodInfo2
            = typeof(RelationalPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql), typeof(PropertyBuilder), typeof(string));

        private static readonly MethodInfo _propertyHasComputedColumnSqlMethodInfo1
            = typeof(RelationalPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql), typeof(PropertyBuilder));

        private static readonly MethodInfo _propertyHasComputedColumnSqlMethodInfo2
            = typeof(RelationalPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql), typeof(PropertyBuilder), typeof(string));

        private static readonly MethodInfo _hasComputedColumnSqlMethodInfo3
            = typeof(RelationalPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql), typeof(PropertyBuilder), typeof(string), typeof(bool));

        private static readonly MethodInfo _propertyIsFixedLengthMethodInfo
            = typeof(RelationalPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalPropertyBuilderExtensions.IsFixedLength), typeof(PropertyBuilder), typeof(bool));

        private static readonly MethodInfo _propertyHasCommentMethodInfo
            = typeof(RelationalPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalPropertyBuilderExtensions.HasComment), typeof(PropertyBuilder), typeof(string));

        private static readonly MethodInfo _propertyUseCollationMethodInfo
            = typeof(RelationalPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalPropertyBuilderExtensions.UseCollation), typeof(PropertyBuilder), typeof(string));

        private static readonly MethodInfo _keyHasNameMethodInfo
            = typeof(RelationalKeyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalKeyBuilderExtensions.HasName), typeof(KeyBuilder), typeof(string));

        private static readonly MethodInfo _referenceReferenceHasConstraintNameMethodInfo
            = typeof(RelationalForeignKeyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalForeignKeyBuilderExtensions.HasConstraintName), typeof(ReferenceReferenceBuilder), typeof(string));

        private static readonly MethodInfo _referenceCollectionHasConstraintNameMethodInfo
            = typeof(RelationalForeignKeyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalForeignKeyBuilderExtensions.HasConstraintName), typeof(ReferenceCollectionBuilder), typeof(string));

        private static readonly MethodInfo _indexHasDatabaseNameMethodInfo
            = typeof(RelationalIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalIndexBuilderExtensions.HasDatabaseName), typeof(IndexBuilder), typeof(string));

        private static readonly MethodInfo _indexHasFilterNameMethodInfo
            = typeof(RelationalIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalIndexBuilderExtensions.HasFilter), typeof(IndexBuilder), typeof(string));

        #endregion MethodInfos

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public AnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

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
                    || _ignoredRelationalAnnotations.Contains(a.Name)));

        /// <inheritdoc />
        public virtual void RemoveAnnotationsHandledByConventions(IModel model, IDictionary<string, IAnnotation> annotations)
            => RemoveConventionalAnnotationsHelper(model, annotations, IsHandledByConvention);

        /// <inheritdoc />
        public virtual void RemoveAnnotationsHandledByConventions(
            IEntityType entityType,
            IDictionary<string, IAnnotation> annotations)
        {
            annotations.Remove(RelationalAnnotationNames.IsTableExcludedFromMigrations);

            RemoveConventionalAnnotationsHelper(entityType, annotations, IsHandledByConvention);
        }

        /// <inheritdoc />
        public virtual void RemoveAnnotationsHandledByConventions(
            IProperty property,
            IDictionary<string, IAnnotation> annotations)
        {
            var columnName = property.GetColumnBaseName();
            if (columnName == property.Name)
            {
                annotations.Remove(RelationalAnnotationNames.ColumnName);
            }

            RemoveConventionalAnnotationsHelper(property, annotations, IsHandledByConvention);
        }

        /// <inheritdoc />
        public virtual void RemoveAnnotationsHandledByConventions(IKey key, IDictionary<string, IAnnotation> annotations)
            => RemoveConventionalAnnotationsHelper(key, annotations, IsHandledByConvention);

        /// <inheritdoc />
        public virtual void RemoveAnnotationsHandledByConventions(
            IForeignKey foreignKey,
            IDictionary<string, IAnnotation> annotations)
            => RemoveConventionalAnnotationsHelper(foreignKey, annotations, IsHandledByConvention);

        /// <inheritdoc />
        public virtual void RemoveAnnotationsHandledByConventions(IIndex index, IDictionary<string, IAnnotation> annotations)
            => RemoveConventionalAnnotationsHelper(index, annotations, IsHandledByConvention);

        /// <inheritdoc />
        public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IModel model,
            IDictionary<string, IAnnotation> annotations)
        {
            var methodCallCodeFragments = new List<MethodCallCodeFragment>();

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.DefaultSchema, _modelHasDefaultSchemaMethodInfo,
                methodCallCodeFragments);

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.Collation, _modelUseCollationMethodInfo,
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
                RelationalAnnotationNames.Comment, _entityTypeHasCommentMethodInfo, methodCallCodeFragments);

            methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(entityType, annotations, GenerateFluentApi));

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
                RelationalAnnotationNames.ColumnName, _propertyHasColumnNameMethodInfo, methodCallCodeFragments);

            if (TryGetAndRemove(annotations, RelationalAnnotationNames.DefaultValueSql, out string? defaultValueSql))
            {
                methodCallCodeFragments.Add(
                    defaultValueSql.Length == 0
                        ? new MethodCallCodeFragment(_propertyHasDefaultValueSqlMethodInfo1)
                        : new MethodCallCodeFragment(_propertyHasDefaultValueSqlMethodInfo2, defaultValueSql));
            }

            if (TryGetAndRemove(annotations, RelationalAnnotationNames.ComputedColumnSql, out string? computedColumnSql))
            {
                methodCallCodeFragments.Add(
                    computedColumnSql.Length == 0
                        ? new MethodCallCodeFragment(_propertyHasComputedColumnSqlMethodInfo1)
                        : TryGetAndRemove(annotations, RelationalAnnotationNames.IsStored, out bool isStored)
                            ? new MethodCallCodeFragment(_hasComputedColumnSqlMethodInfo3, computedColumnSql, isStored)
                            : new MethodCallCodeFragment(_propertyHasComputedColumnSqlMethodInfo2, computedColumnSql));
            }

            if (TryGetAndRemove(annotations, RelationalAnnotationNames.IsFixedLength, out bool isFixedLength))
            {
                methodCallCodeFragments.Add(
                        isFixedLength
                        ? new MethodCallCodeFragment(_propertyIsFixedLengthMethodInfo)
                        : new MethodCallCodeFragment(_propertyIsFixedLengthMethodInfo, isFixedLength));
            }

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.Comment, _propertyHasCommentMethodInfo, methodCallCodeFragments);

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.Collation, _propertyUseCollationMethodInfo, methodCallCodeFragments);

            methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(property, annotations, GenerateFluentApi));

            return methodCallCodeFragments;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IKey key,
            IDictionary<string, IAnnotation> annotations)
        {
            var methodCallCodeFragments = new List<MethodCallCodeFragment>();

            GenerateSimpleFluentApiCall(annotations, RelationalAnnotationNames.Name, _keyHasNameMethodInfo, methodCallCodeFragments);

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
                foreignKey.IsUnique ? _referenceReferenceHasConstraintNameMethodInfo : _referenceCollectionHasConstraintNameMethodInfo,
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
                annotations, RelationalAnnotationNames.Name, _indexHasDatabaseNameMethodInfo, methodCallCodeFragments);
            GenerateSimpleFluentApiCall(
                annotations, RelationalAnnotationNames.Filter, _indexHasFilterNameMethodInfo, methodCallCodeFragments);

            methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(index, annotations, GenerateFluentApi));

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
        ///     <para>
        ///         Checks if the given <paramref name="annotation" /> is handled by convention when
        ///         applied to the given <paramref name="model" />.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="false" />.
        ///     </para>
        /// </summary>
        /// <param name="model"> The <see cref="IModel" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns>
        ///     <see langword="true" /> if the annotation is handled by convention;
        ///     <see langword="false" /> if code must be generated.
        /// </returns>
        protected virtual bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     <para>
        ///         Checks if the given <paramref name="annotation" /> is handled by convention when
        ///         applied to the given <paramref name="entityType" />.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="false" />.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The <see cref="IEntityType" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="false" />. </returns>
        protected virtual bool IsHandledByConvention(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     <para>
        ///         Checks if the given <paramref name="annotation" /> is handled by convention when
        ///         applied to the given <paramref name="key" />.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="false" />.
        ///     </para>
        /// </summary>
        /// <param name="key"> The <see cref="IKey" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="false" />. </returns>
        protected virtual bool IsHandledByConvention(IKey key, IAnnotation annotation)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     <para>
        ///         Checks if the given <paramref name="annotation" /> is handled by convention when
        ///         applied to the given <paramref name="property" />.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="false" />.
        ///     </para>
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="false" />. </returns>
        protected virtual bool IsHandledByConvention(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     <para>
        ///         Checks if the given <paramref name="annotation" /> is handled by convention when
        ///         applied to the given <paramref name="foreignKey" />.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="false" />.
        ///     </para>
        /// </summary>
        /// <param name="foreignKey"> The <see cref="IForeignKey" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="false" />. </returns>
        protected virtual bool IsHandledByConvention(IForeignKey foreignKey, IAnnotation annotation)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     <para>
        ///         Checks if the given <paramref name="annotation" /> is handled by convention when
        ///         applied to the given <paramref name="index" />.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="false" />.
        ///     </para>
        /// </summary>
        /// <param name="index"> The <see cref="IIndex" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="false" />. </returns>
        protected virtual bool IsHandledByConvention(IIndex index, IAnnotation annotation)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));

            return false;
        }

        /// <summary>
        ///     <para>
        ///         Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
        ///         if no fluent API call exists for it.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="null" />.
        ///     </para>
        /// </summary>
        /// <param name="model"> The <see cref="IModel" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="null" />. </returns>
        protected virtual MethodCallCodeFragment? GenerateFluentApi(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     <para>
        ///         Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
        ///         if no fluent API call exists for it.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="null" />.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The <see cref="IEntityType" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="null" />. </returns>
        protected virtual MethodCallCodeFragment? GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     <para>
        ///         Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
        ///         if no fluent API call exists for it.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="null" />.
        ///     </para>
        /// </summary>
        /// <param name="key"> The <see cref="IKey" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="null" />. </returns>
        protected virtual MethodCallCodeFragment? GenerateFluentApi(IKey key, IAnnotation annotation)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     <para>
        ///         Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
        ///         if no fluent API call exists for it.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="null" />.
        ///     </para>
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="null" />. </returns>
        protected virtual MethodCallCodeFragment? GenerateFluentApi(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     <para>
        ///         Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
        ///         if no fluent API call exists for it.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="null" />.
        ///     </para>
        /// </summary>
        /// <param name="foreignKey"> The <see cref="IForeignKey" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="null" />. </returns>
        protected virtual MethodCallCodeFragment? GenerateFluentApi(IForeignKey foreignKey, IAnnotation annotation)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     <para>
        ///         Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
        ///         if no fluent API call exists for it.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="null" />.
        ///     </para>
        /// </summary>
        /// <param name="navigation"> The <see cref="INavigation" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="null" />. </returns>
        protected virtual MethodCallCodeFragment? GenerateFluentApi(INavigation navigation, IAnnotation annotation)
        {
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     <para>
        ///         Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
        ///         if no fluent API call exists for it.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="null" />.
        ///     </para>
        /// </summary>
        /// <param name="navigation"> The <see cref="ISkipNavigation" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="null" />. </returns>
        protected virtual MethodCallCodeFragment? GenerateFluentApi(ISkipNavigation navigation, IAnnotation annotation)
        {
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     <para>
        ///         Returns a fluent API call for the given <paramref name="annotation" />, or <see langword="null" />
        ///         if no fluent API call exists for it.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="null" />.
        ///     </para>
        /// </summary>
        /// <param name="index"> The <see cref="IIndex" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="null" />. </returns>
        protected virtual MethodCallCodeFragment? GenerateFluentApi(IIndex index, IAnnotation annotation)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     <para>
        ///         Returns a data annotation attribute code fragment for the given <paramref name="annotation" />,
        ///         or <see langword="null" /> if no data annotation exists for it.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="null" />.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The <see cref="IEntityType" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="null" />. </returns>
        protected virtual AttributeCodeFragment? GenerateDataAnnotation(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        /// <summary>
        ///     <para>
        ///         Returns a data annotation attribute code fragment for the given <paramref name="annotation" />,
        ///         or <see langword="null" /> if no data annotation exists for it.
        ///     </para>
        ///     <para>
        ///         The default implementation always returns <see langword="null" />.
        ///     </para>
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="null" />. </returns>
        protected virtual AttributeCodeFragment? GenerateDataAnnotation(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        private IEnumerable<TCodeFragment> GenerateFluentApiCallsHelper<TAnnotatable, TCodeFragment>(
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

        private void RemoveConventionalAnnotationsHelper<TAnnotatable>(
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
            IDictionary<string, IAnnotation> annotations, string annotationName, [NotNullWhen(true)] out T? annotationValue)
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
}
