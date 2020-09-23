// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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
        private static readonly ISet<string> _ignoredRelationalAnnotations = new HashSet<string>
        {
            RelationalAnnotationNames.RelationalModel,
            RelationalAnnotationNames.CheckConstraints,
            RelationalAnnotationNames.Sequences,
            RelationalAnnotationNames.DbFunctions,
            RelationalAnnotationNames.DefaultMappings,
            RelationalAnnotationNames.DefaultColumnMappings,
            RelationalAnnotationNames.TableMappings,
            RelationalAnnotationNames.TableColumnMappings,
            RelationalAnnotationNames.ViewMappings,
            RelationalAnnotationNames.ViewColumnMappings,
            RelationalAnnotationNames.FunctionMappings,
            RelationalAnnotationNames.FunctionColumnMappings,
            RelationalAnnotationNames.SqlQueryMappings,
            RelationalAnnotationNames.SqlQueryColumnMappings,
            RelationalAnnotationNames.ForeignKeyMappings,
            RelationalAnnotationNames.TableIndexMappings,
            RelationalAnnotationNames.UniqueConstraintMappings,
            RelationalAnnotationNames.RelationalOverrides
        };

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
        public virtual IEnumerable<IAnnotation> FilterIgnoredAnnotations(IEnumerable<IAnnotation> annotations)
            => annotations.Where(
                a => !(
                    a.Value is null
                    || CoreAnnotationNames.AllNames.Contains(a.Name)
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
                RelationalAnnotationNames.DefaultSchema, nameof(RelationalModelBuilderExtensions.HasDefaultSchema),
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
                RelationalAnnotationNames.Comment, nameof(RelationalEntityTypeBuilderExtensions.HasComment), methodCallCodeFragments);

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
                RelationalAnnotationNames.ColumnName, nameof(RelationalPropertyBuilderExtensions.HasColumnName), methodCallCodeFragments);

            if (TryGetAndRemove(annotations, RelationalAnnotationNames.DefaultValueSql, out string defaultValueSql))
            {
                methodCallCodeFragments.Add(
                    defaultValueSql?.Length == 0
                        ? new MethodCallCodeFragment(
                            nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql))
                        : new MethodCallCodeFragment(
                            nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql),
                            defaultValueSql));
            }

            if (TryGetAndRemove(annotations, RelationalAnnotationNames.ComputedColumnSql, out string computedColumnSql))
            {
                methodCallCodeFragments.Add(
                    computedColumnSql?.Length == 0
                        ? new MethodCallCodeFragment(
                            nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql))
                        : TryGetAndRemove(annotations, RelationalAnnotationNames.IsStored, out bool isStored)
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

            methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(property, annotations, GenerateFluentApi));

            return methodCallCodeFragments;
        }

        /// <inheritdoc />
        public virtual IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IKey key,
            IDictionary<string, IAnnotation> annotations)
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
            IForeignKey navigation,
            IDictionary<string, IAnnotation> annotations)
        {
            var methodCallCodeFragments = new List<MethodCallCodeFragment>();

            GenerateSimpleFluentApiCall(
                annotations,
                RelationalAnnotationNames.Name, nameof(RelationalForeignKeyBuilderExtensions.HasConstraintName), methodCallCodeFragments);

            methodCallCodeFragments.AddRange(GenerateFluentApiCallsHelper(navigation, annotations, GenerateFluentApi));

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
        protected virtual bool IsHandledByConvention([NotNull] IModel model, [NotNull] IAnnotation annotation)
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
        protected virtual bool IsHandledByConvention([NotNull] IEntityType entityType, [NotNull] IAnnotation annotation)
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
        protected virtual bool IsHandledByConvention([NotNull] IKey key, [NotNull] IAnnotation annotation)
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
        protected virtual bool IsHandledByConvention([NotNull] IProperty property, [NotNull] IAnnotation annotation)
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
        protected virtual bool IsHandledByConvention([NotNull] IForeignKey foreignKey, [NotNull] IAnnotation annotation)
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
        protected virtual bool IsHandledByConvention([NotNull] IIndex index, [NotNull] IAnnotation annotation)
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
        protected virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IModel model, [NotNull] IAnnotation annotation)
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
        protected virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IEntityType entityType, [NotNull] IAnnotation annotation)
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
        protected virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IKey key, [NotNull] IAnnotation annotation)
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
        protected virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IProperty property, [NotNull] IAnnotation annotation)
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
        protected virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IForeignKey foreignKey, [NotNull] IAnnotation annotation)
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
        protected virtual MethodCallCodeFragment GenerateFluentApi([NotNull] INavigation navigation, [NotNull] IAnnotation annotation)
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
        protected virtual MethodCallCodeFragment GenerateFluentApi([NotNull] ISkipNavigation navigation, [NotNull] IAnnotation annotation)
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
        protected virtual MethodCallCodeFragment GenerateFluentApi([NotNull] IIndex index, [NotNull] IAnnotation annotation)
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
        protected virtual AttributeCodeFragment GenerateDataAnnotation([NotNull] IEntityType entityType, [NotNull] IAnnotation annotation)
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
        protected virtual AttributeCodeFragment GenerateDataAnnotation([NotNull] IProperty property, [NotNull] IAnnotation annotation)
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
                && annotation.Value is object annotationValue)
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
