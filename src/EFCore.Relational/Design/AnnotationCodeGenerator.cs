// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

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
    public class AnnotationCodeGenerator : IAnnotationCodeGenerator
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public AnnotationCodeGenerator([NotNull] AnnotationCodeGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual AnnotationCodeGeneratorDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void RemoveIgnoredAnnotations(IDictionary<string, IAnnotation> annotations)
        {
            annotations.Remove(CoreAnnotationNames.NavigationCandidates);
            annotations.Remove(CoreAnnotationNames.AmbiguousNavigations);
            annotations.Remove(CoreAnnotationNames.InverseNavigations);
            annotations.Remove(ChangeDetector.SkipDetectChangesAnnotation);
            annotations.Remove(CoreAnnotationNames.OwnedTypes);
            annotations.Remove(CoreAnnotationNames.ChangeTrackingStrategy);
            annotations.Remove(CoreAnnotationNames.BeforeSaveBehavior);
            annotations.Remove(CoreAnnotationNames.AfterSaveBehavior);
            annotations.Remove(CoreAnnotationNames.TypeMapping);
            annotations.Remove(CoreAnnotationNames.ValueComparer);
#pragma warning disable 618
            annotations.Remove(CoreAnnotationNames.KeyValueComparer);
            annotations.Remove(CoreAnnotationNames.StructuralValueComparer);
#pragma warning restore 618
            annotations.Remove(CoreAnnotationNames.ConstructorBinding);
            annotations.Remove(CoreAnnotationNames.NavigationAccessMode);
            annotations.Remove(CoreAnnotationNames.PropertyAccessMode);
            annotations.Remove(CoreAnnotationNames.ProviderClrType);
            annotations.Remove(CoreAnnotationNames.ValueConverter);
            annotations.Remove(CoreAnnotationNames.ValueGeneratorFactory);
            annotations.Remove(CoreAnnotationNames.DefiningQuery);
            annotations.Remove(CoreAnnotationNames.QueryFilter);
            annotations.Remove(RelationalAnnotationNames.RelationalModel);
            annotations.Remove(RelationalAnnotationNames.CheckConstraints);
            annotations.Remove(RelationalAnnotationNames.Sequences);
            annotations.Remove(RelationalAnnotationNames.DbFunctions);
            annotations.Remove(RelationalAnnotationNames.TableMappings);
            annotations.Remove(RelationalAnnotationNames.TableColumnMappings);
            annotations.Remove(RelationalAnnotationNames.ViewMappings);
            annotations.Remove(RelationalAnnotationNames.ViewColumnMappings);
            annotations.Remove(RelationalAnnotationNames.ForeignKeyMappings);
            annotations.Remove(RelationalAnnotationNames.TableIndexMappings);
            annotations.Remove(RelationalAnnotationNames.UniqueConstraintMappings);
            annotations.Remove(RelationalAnnotationNames.RelationalOverrides);

            foreach (var (name, annotation) in EnumerateForRemoval(annotations))
            {
                if (annotation.Value is null)
                {
                    annotations.Remove(name);
                }
            }
        }

        /// <inheritdoc />
        public virtual void RemoveConventionalAnnotations(IModel model, IDictionary<string, IAnnotation> annotations)
            => RemoveConventionalAnnotationsHelper(model, annotations, IsHandledByConvention);

        /// <inheritdoc />
        public virtual void RemoveConventionalAnnotations(
            IEntityType entityType, IDictionary<string, IAnnotation> annotations)
            => RemoveConventionalAnnotationsHelper(entityType, annotations, IsHandledByConvention);

        /// <inheritdoc />
        public virtual void RemoveConventionalAnnotations(
            IProperty property, IDictionary<string, IAnnotation> annotations)
        {
            var columnName = property.GetColumnName();

            if (columnName == property.Name)
            {
                annotations.Remove(RelationalAnnotationNames.ColumnName);
            }

            if (annotations.TryGetValue(RelationalAnnotationNames.ViewColumnName, out var viewColumnNameAnnotation)
                && viewColumnNameAnnotation.Value is string viewColumnName
                && viewColumnName != columnName)
            {
                annotations.Remove(RelationalAnnotationNames.ViewColumnName);
            }

            RemoveConventionalAnnotationsHelper(property, annotations, IsHandledByConvention);
        }

        /// <inheritdoc />
        public virtual void RemoveConventionalAnnotations(IKey key, IDictionary<string, IAnnotation> annotations)
            => RemoveConventionalAnnotationsHelper(key, annotations, IsHandledByConvention);

        /// <inheritdoc />
        public virtual void RemoveConventionalAnnotations(
            IForeignKey foreignKey, IDictionary<string, IAnnotation> annotations)
            => RemoveConventionalAnnotationsHelper(foreignKey, annotations, IsHandledByConvention);

        /// <inheritdoc />
        public virtual void RemoveConventionalAnnotations(IIndex index, IDictionary<string, IAnnotation> annotations)
            => RemoveConventionalAnnotationsHelper(index, annotations, IsHandledByConvention);

        /// <inheritdoc />
        public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IModel model, IDictionary<string, IAnnotation> annotations)
        {
            var methodCallCodeFragments = new List<MethodCallCodeFragment>();

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.DefaultSchema, nameof(RelationalModelBuilderExtensions.HasDefaultSchema),
                methodCallCodeFragments);

            methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(model, annotations, GenerateFluentApi));
            return methodCallCodeFragments;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IEntityType entityType, IDictionary<string, IAnnotation> annotations)
        {
            var methodCallCodeFragments = new List<MethodCallCodeFragment>();

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.Comment, nameof(RelationalEntityTypeBuilderExtensions.HasComment), methodCallCodeFragments);

            methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(entityType, annotations, GenerateFluentApi));

            return methodCallCodeFragments;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IProperty property, IDictionary<string, IAnnotation> annotations)
        {
            var methodCallCodeFragments = new List<MethodCallCodeFragment>();

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.ColumnName, nameof(RelationalPropertyBuilderExtensions.HasColumnName), methodCallCodeFragments);

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.ViewColumnName, nameof(RelationalPropertyBuilderExtensions.HasViewColumnName),
                methodCallCodeFragments);

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.DefaultValueSql, nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql),
                methodCallCodeFragments);

            if (TryGetAndRemove(annotations, RelationalAnnotationNames.DefaultValue, out object defaultValue))
            {
                var valueConverter = property.GetValueConverter()
                    ?? (property.FindTypeMapping()
                        ?? Dependencies.RelationalTypeMappingSource.FindMapping(property))?.Converter;

                methodCallCodeFragments.Add(
                    new MethodCallCodeFragment(
                        nameof(RelationalPropertyBuilderExtensions.HasDefaultValue),
                        valueConverter == null ? defaultValue : valueConverter.ConvertToProvider(defaultValue)));
            }

            if (TryGetAndRemove(annotations, RelationalAnnotationNames.ComputedColumnSql, out object computedColumnSql))
            {
                methodCallCodeFragments.Add(
                    TryGetAndRemove(annotations, RelationalAnnotationNames.ComputedColumnIsStored, out bool isStored)
                        ? new MethodCallCodeFragment(
                            nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql),
                            computedColumnSql,
                            isStored)
                        : new MethodCallCodeFragment(
                            nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql),
                            computedColumnSql));
            }

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.IsFixedLength, nameof(RelationalPropertyBuilderExtensions.IsFixedLength),
                methodCallCodeFragments);

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.Comment, nameof(RelationalPropertyBuilderExtensions.HasComment), methodCallCodeFragments);

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.Collation, nameof(RelationalPropertyBuilderExtensions.UseCollation), methodCallCodeFragments);

            GenerateSimpleFluentApiCall(
                annotations,
                CoreAnnotationNames.MaxLength, nameof(PropertyBuilder.HasMaxLength), methodCallCodeFragments);

            var hasScale = TryGetAndRemove(annotations, CoreAnnotationNames.Scale, out int scale) && scale != 0;

            if (TryGetAndRemove(annotations, CoreAnnotationNames.Precision, out int precision))
            {
                methodCallCodeFragments.Add(
                    hasScale
                        ? new MethodCallCodeFragment(nameof(PropertyBuilder.HasPrecision), precision, scale)
                        : new MethodCallCodeFragment(nameof(PropertyBuilder.HasPrecision), precision));
            }

            GenerateSimpleFluentApiCall(
                annotations,
                CoreAnnotationNames.Unicode, nameof(PropertyBuilder.IsUnicode), methodCallCodeFragments);

            methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(property, annotations, GenerateFluentApi));

            return methodCallCodeFragments;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IKey key, IDictionary<string, IAnnotation> annotations)
        {
            var methodCallCodeFragments = new List<MethodCallCodeFragment>();

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.Name, nameof(RelationalKeyBuilderExtensions.HasName), methodCallCodeFragments);

            methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(key, annotations, GenerateFluentApi));

            return methodCallCodeFragments;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IForeignKey foreignKey, IDictionary<string, IAnnotation> annotations)
        {
            var methodCallCodeFragments = new List<MethodCallCodeFragment>();

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.Name, nameof(RelationalForeignKeyBuilderExtensions.HasConstraintName), methodCallCodeFragments);

            methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(foreignKey, annotations, GenerateFluentApi));

            return methodCallCodeFragments;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IIndex index, IDictionary<string, IAnnotation> annotations)
        {
            var methodCallCodeFragments = new List<MethodCallCodeFragment>();

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.Name, nameof(RelationalIndexBuilderExtensions.HasDatabaseName), methodCallCodeFragments);

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.Filter, nameof(RelationalIndexBuilderExtensions.HasFilter), methodCallCodeFragments);

            methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(index, annotations, GenerateFluentApi));

            return methodCallCodeFragments;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<AttributeCodeFragment> GenerateDataAnnotationAttributes(
            IEntityType entityType, IDictionary<string, IAnnotation> annotations)
        {
            var attributeCodeFragments = new List<AttributeCodeFragment>();

            attributeCodeFragments.AddRange(GenerateFluentApiCallsHelper(entityType, annotations, GenerateDataAnnotation));

            return attributeCodeFragments;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<AttributeCodeFragment> GenerateDataAnnotationAttributes(
            IProperty property, IDictionary<string, IAnnotation> annotations)
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
        ///     <see langword="true"/> if the annotation is handled by convention;
        ///     <see langword="false"/> if code must be generated.
        /// </returns>
        public virtual bool IsHandledByConvention([NotNull] IModel model, [NotNull] IAnnotation annotation)
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
        public virtual bool IsHandledByConvention([NotNull] IEntityType entityType, [NotNull] IAnnotation annotation)
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
        public virtual bool IsHandledByConvention([NotNull] IKey key, [NotNull] IAnnotation annotation)
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
        public virtual bool IsHandledByConvention([NotNull] IProperty property, [NotNull] IAnnotation annotation)
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
        public virtual bool IsHandledByConvention([NotNull] IForeignKey foreignKey, [NotNull] IAnnotation annotation)
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
        public virtual bool IsHandledByConvention([NotNull] IIndex index, [NotNull] IAnnotation annotation)
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
        public virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IModel model, [NotNull] IAnnotation annotation)
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
        public virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IEntityType entityType, [NotNull] IAnnotation annotation)
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
        public virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IKey key, [NotNull] IAnnotation annotation)
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
        public virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IProperty property, [NotNull] IAnnotation annotation)
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
        public virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IForeignKey foreignKey, [NotNull] IAnnotation annotation)
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
        /// <param name="index"> The <see cref="IIndex" />. </param>
        /// <param name="annotation"> The <see cref="IAnnotation" />. </param>
        /// <returns> <see langword="null" />. </returns>
        public virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IIndex index, [NotNull] IAnnotation annotation)
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
        public virtual AttributeCodeFragment GenerateDataAnnotation([NotNull] IEntityType entityType, [NotNull] IAnnotation annotation)
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
        public virtual AttributeCodeFragment GenerateDataAnnotation([NotNull] IProperty property, [NotNull] IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            return null;
        }

        private IEnumerable<TCodeFragment> GenerateFluentApiCallsHelper<TAnnotatable, TCodeFragment>(
            TAnnotatable annotatable,
            IDictionary<string, IAnnotation> annotations,
            Func<TAnnotatable, IAnnotation, TCodeFragment> generateCodeFragment)
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

        private static bool TryGetAndRemove<T>(IDictionary<string, IAnnotation> annotations, string annotationName, out T annotationValue)
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
            string methodName,
            List<MethodCallCodeFragment> methodCallCodeFragments)
        {
            if (annotations.TryGetValue(annotationName, out var annotation)
                && annotation.Value is { } annotationValue)
            {
                annotations.Remove(annotationName);
                methodCallCodeFragments.Add(
                    new MethodCallCodeFragment(methodName, annotationValue));
            }
        }

        // Dictionary is safe for removal during enumeration
        private static IEnumerable<KeyValuePair<string, IAnnotation>> EnumerateForRemoval(IDictionary<string, IAnnotation> annotations)
            => annotations is Dictionary<string, IAnnotation>
                ? (IEnumerable<KeyValuePair<string, IAnnotation>>)annotations
                : annotations.ToList();
    }
}
