// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         Base class for types that support reading and writing annotations.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class RuntimeAnnotatableBase : IAnnotatable
{
    private Dictionary<string, Annotation>? _annotations;
    private ConcurrentDictionary<string, Annotation>? _runtimeAnnotations;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeAnnotatableBase()
    {
    }

    /// <summary>
    ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The key of the annotation to be added.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The newly added annotation.</returns>
    public virtual Annotation AddAnnotation(string name, object? value)
    {
        Check.NotEmpty(name, nameof(name));

        var annotation = CreateAnnotation(name, value);

        if (FindAnnotation(name) != null)
        {
            throw new InvalidOperationException(CoreStrings.DuplicateAnnotation(name, ToString()));
        }

        SetAnnotation(name, annotation);

        return annotation;
    }

    /// <summary>
    ///     Adds annotations to this object overriding any existing annotations with the same name.
    /// </summary>
    /// <param name="annotations">The annotations to be added.</param>
    public virtual void AddAnnotations(IEnumerable<IAnnotation> annotations)
    {
        foreach (var annotation in annotations)
        {
            SetAnnotation(annotation.Name, (annotation as Annotation) ?? CreateAnnotation(annotation.Name, annotation.Value));
        }
    }

    /// <summary>
    ///     Adds annotations to this object overriding any existing annotations with the same name.
    /// </summary>
    /// <param name="annotations">The annotations to be added.</param>
    public virtual void AddAnnotations(IReadOnlyDictionary<string, object?> annotations)
    {
        foreach (var annotation in annotations)
        {
            SetAnnotation(annotation.Key, CreateAnnotation(annotation.Key, annotation.Value));
        }
    }

    /// <summary>
    ///     Sets the annotation stored under the given key. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The key of the annotation to be added.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    public virtual void SetAnnotation(string name, object? value)
    {
        var oldAnnotation = FindAnnotation(name);
        if (oldAnnotation != null
            && Equals(oldAnnotation.Value, value))
        {
            return;
        }

        SetAnnotation(name, CreateAnnotation(name, value));
    }

    private Annotation? SetAnnotation(
        string name,
        Annotation annotation)
    {
        _annotations ??= new(StringComparer.Ordinal);
        _annotations[name] = annotation;

        return annotation;
    }

    /// <summary>
    ///     Gets the annotation with the given name, returning <see langword="null" /> if it does not exist.
    /// </summary>
    /// <param name="name">The key of the annotation to find.</param>
    /// <returns>
    ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
    /// </returns>
    public virtual Annotation? FindAnnotation(string name)
    {
        Check.NotEmpty(name, nameof(name));

        return _annotations != null && _annotations.TryGetValue(name, out var annotation)
                ? annotation
                : null;
    }

    /// <summary>
    ///     Gets the annotation with the given name, throwing if it does not exist.
    /// </summary>
    /// <param name="annotationName">The key of the annotation to find.</param>
    /// <returns>The annotation with the specified name.</returns>
    public virtual Annotation GetAnnotation(string annotationName)
    {
        Check.NotEmpty(annotationName, nameof(annotationName));

        var annotation = FindAnnotation(annotationName);
        if (annotation == null)
        {
            throw new InvalidOperationException(CoreStrings.AnnotationNotFound(annotationName, ToString()));
        }

        return annotation;
    }

    /// <summary>
    ///     Removes the given annotation from this object.
    /// </summary>
    /// <param name="name">The annotation to remove.</param>
    /// <returns>The annotation that was removed.</returns>
    public virtual Annotation? RemoveAnnotation(string name)
    {
        Check.NotNull(name, nameof(name));

        var annotation = FindAnnotation(name);
        if (annotation == null)
        {
            return null;
        }

        _annotations!.Remove(name);

        return annotation;
    }

    /// <summary>
    ///     Gets the value annotation with the given name, returning <see langword="null" /> if it does not exist.
    /// </summary>
    /// <param name="name">The key of the annotation to find.</param>
    /// <returns>
    ///     The value of the existing annotation if an annotation with the specified name already exists.
    ///     Otherwise, <see langword="null" />.
    /// </returns>
    public virtual object? this[string name]
    {
        get => FindAnnotation(name)?.Value;

        set
        {
            Check.NotEmpty(name, nameof(name));

            if (value == null)
            {
                RemoveAnnotation(name);
            }
            else
            {
                SetAnnotation(name, value);
            }
        }
    }

    /// <summary>
    ///     Creates a new annotation.
    /// </summary>
    /// <param name="name">The key of the annotation.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The newly created annotation.</returns>
    protected virtual Annotation CreateAnnotation(string name, object? value)
        => new(name, value);

    /// <summary>
    ///     Gets all runtime annotations on the current object.
    /// </summary>
    public virtual IEnumerable<Annotation> GetRuntimeAnnotations()
        => _runtimeAnnotations?.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => p.Value)
            ?? Enumerable.Empty<Annotation>();

    /// <summary>
    ///     Adds a runtime annotation to this object. Throws if an annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The key of the annotation to be added.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The newly added annotation.</returns>
    public virtual Annotation AddRuntimeAnnotation(string name, object? value)
    {
        Check.NotEmpty(name, nameof(name));

        var annotation = CreateRuntimeAnnotation(name, value);

        return AddRuntimeAnnotation(name, annotation);
    }

    /// <summary>
    ///     Adds a runtime annotation to this object. Throws if an annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The key of the annotation to be added.</param>
    /// <param name="annotation">The annotation to be added.</param>
    /// <returns>The added annotation.</returns>
    protected virtual Annotation AddRuntimeAnnotation(string name, Annotation annotation)
        => GetOrCreateRuntimeAnnotations().TryAdd(name, annotation)
            ? annotation
            : throw new InvalidOperationException(CoreStrings.DuplicateAnnotation(name, ToString()));

    /// <summary>
    ///     Adds runtime annotations to this object.
    /// </summary>
    /// <param name="annotations">The annotations to be added.</param>
    public virtual void AddRuntimeAnnotations(IEnumerable<Annotation> annotations)
    {
        foreach (var annotation in annotations)
        {
            AddRuntimeAnnotation(annotation.Name, annotation);
        }
    }

    /// <summary>
    ///     Adds runtime annotations to this object.
    /// </summary>
    /// <param name="annotations">The annotations to be added.</param>
    public virtual void AddRuntimeAnnotations(IReadOnlyDictionary<string, object?> annotations)
    {
        foreach (var annotation in annotations)
        {
            AddRuntimeAnnotation(annotation.Key, CreateRuntimeAnnotation(annotation.Key, annotation.Value));
        }
    }

    /// <summary>
    ///     Sets the runtime annotation stored under the given key. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The key of the annotation to be added.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    public virtual Annotation SetRuntimeAnnotation(string name, object? value)
        => GetOrCreateRuntimeAnnotations().AddOrUpdate(
            name,
            static (n, a) => a.Annotatable.CreateRuntimeAnnotation(n, a.Value),
            static (n, oldAnnotation, a) =>
                !Equals(oldAnnotation.Value, a.Value) ? a.Annotatable.CreateRuntimeAnnotation(n, a.Value) : oldAnnotation,
            (Value: value, Annotatable: this));

    /// <summary>
    ///     Sets the runtime annotation stored under the given key. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The key of the annotation to be added.</param>
    /// <param name="annotation">The annotation to be set.</param>
    /// <param name="oldAnnotation">The annotation being replaced.</param>
    /// <returns>The annotation that was set.</returns>
    protected virtual Annotation SetRuntimeAnnotation(
        string name,
        Annotation annotation,
        Annotation? oldAnnotation)
    {
        GetOrCreateRuntimeAnnotations()[name] = annotation;

        return annotation;
    }

    /// <summary>
    ///     Gets the value of the runtime annotation with the given name, adding it if one does not exist.
    /// </summary>
    /// <param name="name">The name of the annotation.</param>
    /// <param name="valueFactory">The factory used to create the value if the annotation doesn't exist.</param>
    /// <param name="factoryArgument">An argument for the factory method.</param>
    /// <returns>
    ///     The value of the existing runtime annotation if an annotation with the specified name already exists.
    ///     Otherwise a newly created value.
    /// </returns>
    public virtual TValue GetOrAddRuntimeAnnotationValue<TValue, TArg>(
        string name,
        Func<TArg?, TValue> valueFactory,
        TArg? factoryArgument)
        => (TValue)GetOrCreateRuntimeAnnotations().GetOrAdd(
            name,
            static (n, t) => t.Annotatable.CreateRuntimeAnnotation(n, t.CreateValue(t.Argument)),
            (CreateValue: valueFactory, Argument: factoryArgument, Annotatable: this)).Value!;

    /// <summary>
    ///     Gets the runtime annotation with the given name, returning <see langword="null" /> if it does not exist.
    /// </summary>
    /// <param name="name">The key of the annotation to find.</param>
    /// <returns>
    ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
    /// </returns>
    public virtual Annotation? FindRuntimeAnnotation(string name)
    {
        Check.NotEmpty(name, nameof(name));

        return _runtimeAnnotations == null
            ? null
            : _runtimeAnnotations.TryGetValue(name, out var annotation)
                ? annotation
                : null;
    }

    /// <summary>
    ///     Removes the given runtime annotation from this object.
    /// </summary>
    /// <param name="name">The annotation to remove.</param>
    /// <returns>The annotation that was removed.</returns>
    public virtual Annotation? RemoveRuntimeAnnotation(string name)
    {
        Check.NotNull(name, nameof(name));

        if (_runtimeAnnotations == null)
        {
            return null;
        }

        _runtimeAnnotations.Remove(name, out var annotation);

        return annotation;
    }

    /// <summary>
    ///     Creates a new runtime annotation.
    /// </summary>
    /// <param name="name">The key of the annotation.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The newly created annotation.</returns>
    protected virtual Annotation CreateRuntimeAnnotation(string name, object? value)
        => new(name, value);

    private ConcurrentDictionary<string, Annotation> GetOrCreateRuntimeAnnotations()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _runtimeAnnotations, (object?)null, static _ => new ConcurrentDictionary<string, Annotation>());

    /// <inheritdoc />
    object? IReadOnlyAnnotatable.this[string name]
    {
        [DebuggerStepThrough]
        get => this[name];
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<IAnnotation> IReadOnlyAnnotatable.GetAnnotations()
        => _annotations?.Values.OrderBy(a => a.Name, StringComparer.Ordinal) ?? Enumerable.Empty<Annotation>();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IAnnotation? IReadOnlyAnnotatable.FindAnnotation(string name)
        => FindAnnotation(name);

    /// <inheritdoc />
    IEnumerable<IAnnotation> IAnnotatable.GetRuntimeAnnotations()
        => GetRuntimeAnnotations();

    /// <inheritdoc />
    IAnnotation? IAnnotatable.FindRuntimeAnnotation(string name)
        => FindRuntimeAnnotation(name);

    /// <inheritdoc />
    IAnnotation IAnnotatable.AddRuntimeAnnotation(string name, object? value)
        => AddRuntimeAnnotation(name, value);

    /// <inheritdoc />
    IAnnotation? IAnnotatable.RemoveRuntimeAnnotation(string name)
        => RemoveRuntimeAnnotation(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IAnnotation IAnnotatable.SetRuntimeAnnotation(string name, object? value)
        => SetRuntimeAnnotation(name, value);
}
