// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Implemented by database providers to control which annotations need to
///     have code generated (as opposed to being handled by convention) and then to generate
///     the code if needed.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IAnnotationCodeGenerator
{
    /// <summary>
    ///     Filters out annotations in <paramref name="annotations" /> for which code should never be generated.
    /// </summary>
    /// <param name="annotations">The annotations from which to filter the ignored ones.</param>
    /// <returns>The filtered annotations.</returns>
    IEnumerable<IAnnotation> FilterIgnoredAnnotations(IEnumerable<IAnnotation> annotations);

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="model">The model to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(IModel model, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="entityType">The entity type to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(IEntityType entityType, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="complexType">The complex type to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(IComplexType complexType, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="fragment">The entity mapping fragment to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(IEntityTypeMappingFragment fragment, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="property">The property to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(IProperty property, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="complexProperty">The complex property to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(IComplexProperty complexProperty, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="key">The key to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(IKey key, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="foreignKey">The foreign key to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(IForeignKey foreignKey, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="navigation">The navigation to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(INavigation navigation, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="navigation">The navigation to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(ISkipNavigation navigation, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="index">The index to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(IIndex index, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="checkConstraint">The check constraint to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(ICheckConstraint checkConstraint, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="trigger">The trigger to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(ITrigger trigger, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="overrides">The property overrides to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(IRelationalPropertyOverrides overrides, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="sequence">The sequence to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to remove the conventional ones.</param>
    void RemoveAnnotationsHandledByConventions(ISequence sequence, IDictionary<string, IAnnotation> annotations)
    {
    }

    /// <summary>
    ///     Removes annotation whose configuration is already applied by convention, and do not need to be
    ///     specified explicitly.
    /// </summary>
    /// <param name="annotatable">The annotatable to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    void RemoveAnnotationsHandledByConventions(IAnnotatable annotatable, IDictionary<string, IAnnotation> annotations)
        => RemoveAnnotationsHandledByConventionsInternal(annotatable, annotations);

    // Issue #28537.
    internal sealed void RemoveAnnotationsHandledByConventionsInternal(
        IAnnotatable annotatable,
        IDictionary<string, IAnnotation> annotations)
    {
        switch (annotatable)
        {
            case IModel model:
                RemoveAnnotationsHandledByConventions(model, annotations);
                return;

            case IEntityType entityType:
                RemoveAnnotationsHandledByConventions(entityType, annotations);
                return;

            case IComplexType complexType:
                RemoveAnnotationsHandledByConventions(complexType, annotations);
                return;

            case IEntityTypeMappingFragment fragment:
                RemoveAnnotationsHandledByConventions(fragment, annotations);
                return;

            case IProperty property:
                RemoveAnnotationsHandledByConventions(property, annotations);
                return;

            case IComplexProperty complexProperty:
                RemoveAnnotationsHandledByConventions(complexProperty, annotations);
                return;

            case IKey key:
                RemoveAnnotationsHandledByConventions(key, annotations);
                return;

            case IForeignKey foreignKey:
                RemoveAnnotationsHandledByConventions(foreignKey, annotations);
                return;

            case INavigation navigation:
                RemoveAnnotationsHandledByConventions(navigation, annotations);
                return;

            case ISkipNavigation skipNavigation:
                RemoveAnnotationsHandledByConventions(skipNavigation, annotations);
                return;

            case ICheckConstraint checkConstraint:
                RemoveAnnotationsHandledByConventions(checkConstraint, annotations);
                return;

            case IIndex index:
                RemoveAnnotationsHandledByConventions(index, annotations);
                return;

            case ITrigger trigger:
                RemoveAnnotationsHandledByConventions(trigger, annotations);
                return;

            case IRelationalPropertyOverrides overrides:
                RemoveAnnotationsHandledByConventions(overrides, annotations);
                return;

            case ISequence sequence:
                RemoveAnnotationsHandledByConventions(sequence, annotations);
                return;

            default:
                throw new ArgumentException(RelationalStrings.UnhandledAnnotatableType(annotatable.GetType()));
        }
    }

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="model">The model to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IModel model,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="entityType">The entity type to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IEntityType entityType,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="complexType">The entity type to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IComplexType complexType,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="fragment">The entity mapping fragment to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IEntityTypeMappingFragment fragment,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="property">The property to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="complexProperty">The complex property to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IComplexProperty complexProperty,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="key">The key to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IKey key,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="foreignKey">The foreign key to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IForeignKey foreignKey,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="navigation">The navigation to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        INavigation navigation,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="navigation">The skip navigation to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        ISkipNavigation navigation,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="index">The index to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IIndex index,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="checkConstraint">The check constraint to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        ICheckConstraint checkConstraint,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="trigger">The trigger to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        ITrigger trigger,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="overrides">The property overrides to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IRelationalPropertyOverrides overrides,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="sequence">The sequence to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        ISequence sequence,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding fluent API calls, returns those fluent API calls
    ///     and removes the annotations.
    /// </summary>
    /// <param name="annotatable">The annotatable to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(IAnnotatable annotatable, IDictionary<string, IAnnotation> annotations)
        => GenerateFluentApiCallsInternal(annotatable, annotations);

    // Issue #28537.
    internal sealed IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCallsInternal(
        IAnnotatable annotatable,
        IDictionary<string, IAnnotation> annotations)
        => annotatable switch
        {
            IModel model => GenerateFluentApiCalls(model, annotations),
            IEntityType entityType => GenerateFluentApiCalls(entityType, annotations),
            IComplexType complexType => GenerateFluentApiCalls(complexType, annotations),
            IEntityTypeMappingFragment fragment => GenerateFluentApiCalls(fragment, annotations),
            IProperty property => GenerateFluentApiCalls(property, annotations),
            IComplexProperty complexProperty => GenerateFluentApiCalls(complexProperty, annotations),
            IRelationalPropertyOverrides overrides => GenerateFluentApiCalls(overrides, annotations),
            IKey key => GenerateFluentApiCalls(key, annotations),
            IForeignKey foreignKey => GenerateFluentApiCalls(foreignKey, annotations),
            INavigation navigation => GenerateFluentApiCalls(navigation, annotations),
            ISkipNavigation skipNavigation => GenerateFluentApiCalls(skipNavigation, annotations),
            IIndex index => GenerateFluentApiCalls(index, annotations),
            ICheckConstraint checkConstraint => GenerateFluentApiCalls(checkConstraint, annotations),
            ITrigger trigger => GenerateFluentApiCalls(trigger, annotations),
            ISequence sequence => GenerateFluentApiCalls(sequence, annotations),

            _ => throw new ArgumentException(RelationalStrings.UnhandledAnnotatableType(annotatable.GetType()))
        };

    /// <summary>
    ///     For the given annotations which have corresponding data annotation attributes, returns those attribute code fragments
    ///     and removes the annotations.
    /// </summary>
    /// <param name="entityType">The entity type to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<AttributeCodeFragment> GenerateDataAnnotationAttributes(
        IEntityType entityType,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding data annotation attributes, returns those attribute code fragments
    ///     and removes the annotations.
    /// </summary>
    /// <param name="property">The property to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<AttributeCodeFragment> GenerateDataAnnotationAttributes(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
        => [];

    /// <summary>
    ///     For the given annotations which have corresponding data annotation attributes, returns those attribute code fragments
    ///     and removes the annotations.
    /// </summary>
    /// <param name="annotatable">The annotatable to which the annotations are applied.</param>
    /// <param name="annotations">The set of annotations from which to generate fluent API calls.</param>
    IReadOnlyList<AttributeCodeFragment> GenerateDataAnnotationAttributes(
        IAnnotatable annotatable,
        IDictionary<string, IAnnotation> annotations)
        => GenerateDataAnnotationAttributesInternal(annotatable, annotations);

    // Issue #28537.
    internal sealed IReadOnlyList<AttributeCodeFragment> GenerateDataAnnotationAttributesInternal(
        IAnnotatable annotatable,
        IDictionary<string, IAnnotation> annotations)
        => annotatable switch
        {
            IEntityType entityType => GenerateDataAnnotationAttributes(entityType, annotations),
            IProperty property => GenerateDataAnnotationAttributes(property, annotations),
            _ => throw new ArgumentException(RelationalStrings.UnhandledAnnotatableType(annotatable.GetType()))
        };
}
