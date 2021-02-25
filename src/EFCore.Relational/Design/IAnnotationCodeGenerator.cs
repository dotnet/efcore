// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Implemented by database providers to control which <see cref="IAnnotation" />s need to
    ///     have code generated (as opposed to being handled by convention) and then to generate
    ///     the code if needed.
    /// </summary>
    public interface IAnnotationCodeGenerator
    {
        /// <summary>
        ///     Filters out annotations in <paramref name="annotations" /> for which code should never be generated.
        /// </summary>
        /// <param name="annotations"> The annotations from which to filter the ignored ones. </param>
        /// <returns> The filtered annotations. </returns>
        IEnumerable<IAnnotation> FilterIgnoredAnnotations([NotNull] IEnumerable<IAnnotation> annotations);

        /// <summary>
        ///     Removes annotation whose configuration is already applied by convention, and do not need to be
        ///     specified explicitly.
        /// </summary>
        /// <param name="model"> The model to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to remove the conventional ones. </param>
        void RemoveAnnotationsHandledByConventions([NotNull] IModel model, [NotNull] IDictionary<string, IAnnotation> annotations) { }

        /// <summary>
        ///     Removes annotation whose configuration is already applied by convention, and do not need to be
        ///     specified explicitly.
        /// </summary>
        /// <param name="entity"> The entity to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to remove the conventional ones. </param>
        void RemoveAnnotationsHandledByConventions([NotNull] IEntityType entity, [NotNull] IDictionary<string, IAnnotation> annotations) { }

        /// <summary>
        ///     Removes annotation whose configuration is already applied by convention, and do not need to be
        ///     specified explicitly.
        /// </summary>
        /// <param name="property"> The property to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to remove the conventional ones. </param>
        void RemoveAnnotationsHandledByConventions([NotNull] IProperty property, [NotNull] IDictionary<string, IAnnotation> annotations) { }

        /// <summary>
        ///     Removes annotation whose configuration is already applied by convention, and do not need to be
        ///     specified explicitly.
        /// </summary>
        /// <param name="key"> The key to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to remove the conventional ones. </param>
        void RemoveAnnotationsHandledByConventions([NotNull] IKey key, [NotNull] IDictionary<string, IAnnotation> annotations) { }

        /// <summary>
        ///     Removes annotation whose configuration is already applied by convention, and do not need to be
        ///     specified explicitly.
        /// </summary>
        /// <param name="foreignKey"> The foreign key to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to remove the conventional ones. </param>
        void RemoveAnnotationsHandledByConventions([NotNull] IForeignKey foreignKey, [NotNull] IDictionary<string, IAnnotation> annotations)
        {
        }

        /// <summary>
        ///     Removes annotation whose configuration is already applied by convention, and do not need to be
        ///     specified explicitly.
        /// </summary>
        /// <param name="index"> The index to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to remove the conventional ones. </param>
        void RemoveAnnotationsHandledByConventions([NotNull] IIndex index, [NotNull] IDictionary<string, IAnnotation> annotations) { }

        /// <summary>
        ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
        ///     and removes the annotations.
        /// </summary>
        /// <param name="model"> The model to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to generate fluent API calls. </param>
        IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            [NotNull] IModel model,
            [NotNull] IDictionary<string, IAnnotation> annotations)
            => Array.Empty<MethodCallCodeFragment>();

        /// <summary>
        ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
        ///     and removes the annotations.
        /// </summary>
        /// <param name="entityType"> The entity type to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to generate fluent API calls. </param>
        IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            [NotNull] IEntityType entityType,
            [NotNull] IDictionary<string, IAnnotation> annotations)
            => Array.Empty<MethodCallCodeFragment>();

        /// <summary>
        ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
        ///     and removes the annotations.
        /// </summary>
        /// <param name="property"> The property to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to generate fluent API calls. </param>
        IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            [NotNull] IProperty property,
            [NotNull] IDictionary<string, IAnnotation> annotations)
            => Array.Empty<MethodCallCodeFragment>();

        /// <summary>
        ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
        ///     and removes the annotations.
        /// </summary>
        /// <param name="key"> The key to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to generate fluent API calls. </param>
        IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            [NotNull] IKey key,
            [NotNull] IDictionary<string, IAnnotation> annotations)
            => Array.Empty<MethodCallCodeFragment>();

        /// <summary>
        ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
        ///     and removes the annotations.
        /// </summary>
        /// <param name="foreignKey"> The foreign key to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to generate fluent API calls. </param>
        IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            [NotNull] IForeignKey foreignKey,
            [NotNull] IDictionary<string, IAnnotation> annotations)
            => Array.Empty<MethodCallCodeFragment>();

        /// <summary>
        ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
        ///     and removes the annotations.
        /// </summary>
        /// <param name="navigation"> The navigation to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to generate fluent API calls. </param>
        IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            [NotNull] INavigation navigation,
            [NotNull] IDictionary<string, IAnnotation> annotations)
            => Array.Empty<MethodCallCodeFragment>();

        /// <summary>
        ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
        ///     and removes the annotations.
        /// </summary>
        /// <param name="navigation"> The skip navigation to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to generate fluent API calls. </param>
        IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            [NotNull] ISkipNavigation navigation,
            [NotNull] IDictionary<string, IAnnotation> annotations)
            => Array.Empty<MethodCallCodeFragment>();

        /// <summary>
        ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
        ///     and removes the annotations.
        /// </summary>
        /// <param name="index"> The index to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to generate fluent API calls. </param>
        IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            [NotNull] IIndex index,
            [NotNull] IDictionary<string, IAnnotation> annotations)
            => Array.Empty<MethodCallCodeFragment>();

        /// <summary>
        ///     For the given annotations which have corresponding data annotation attributes, returns those attribute code fragments
        ///     and removes the annotations.
        /// </summary>
        /// <param name="entityType"> The entity type to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to generate fluent API calls. </param>
        IReadOnlyList<AttributeCodeFragment> GenerateDataAnnotationAttributes(
            [NotNull] IEntityType entityType,
            [NotNull] IDictionary<string, IAnnotation> annotations)
            => Array.Empty<AttributeCodeFragment>();

        /// <summary>
        ///     For the given annotations which have corresponding data annotation attributes, returns those attribute code fragments
        ///     and removes the annotations.
        /// </summary>
        /// <param name="property"> The property to which the annotations are applied. </param>
        /// <param name="annotations"> The set of annotations from which to generate fluent API calls. </param>
        IReadOnlyList<AttributeCodeFragment> GenerateDataAnnotationAttributes(
            [NotNull] IProperty property,
            [NotNull] IDictionary<string, IAnnotation> annotations)
            => Array.Empty<AttributeCodeFragment>();
    }
}
