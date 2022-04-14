// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a table source in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public abstract class TableExpressionBase : Expression, IPrintableExpression, IMutableAnnotatable
{
    private SortedDictionary<string, Annotation>? _annotations;

    /// <summary>
    ///     Creates a new instance of the <see cref="TableExpressionBase" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    protected TableExpressionBase(string? alias)
        : this(alias, annotations: null)
    {
        Alias = alias;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="TableExpressionBase" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    /// <param name="annotations">A collection of annotations associated with this expression.</param>
    protected TableExpressionBase(string? alias, IEnumerable<IAnnotation>? annotations)
    {
        Alias = alias;

        if (annotations != null)
        {
            foreach (var annotation in annotations)
            {
                SetAnnotation(annotation.Name, annotation.Value);
            }
        }
    }

    /// <summary>
    ///     The alias assigned to this table source.
    /// </summary>
    public virtual string? Alias { get; internal set; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    public override Type Type
        => typeof(object);

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

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
    ///     Creates a printable string representation of the given expression using <see cref="ExpressionPrinter" />.
    /// </summary>
    /// <param name="expressionPrinter">The expression printer to use.</param>
    protected abstract void Print(ExpressionPrinter expressionPrinter);

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        => Print(expressionPrinter);

    /// <summary>
    ///     Creates a printable string representation of annotations associated with the given expression using <see cref="ExpressionPrinter" />.
    /// </summary>
    /// <param name="expressionPrinter">The expression printer to use.</param>
    protected virtual void PrintAnnotations(ExpressionPrinter expressionPrinter)
    {
        var annotations = GetAnnotations();
        if (annotations.Any())
        {
            expressionPrinter.Append("[");
            expressionPrinter.Append(annotations.Select(a => a.Name + "=" + a.Value).Join(" | "));
            expressionPrinter.Append("]");
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is TableExpressionBase tableExpressionBase
                && Equals(tableExpressionBase));

    private bool Equals(TableExpressionBase tableExpressionBase)
        => Alias == tableExpressionBase.Alias;

    /// <inheritdoc />
    public override int GetHashCode()
        => 0;

    /// <summary>
    ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The key of the annotation to be added.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The newly added annotation.</returns>
    public virtual Annotation AddAnnotation(string name, object? value)
        => AddAnnotation(name, new(name, value));

    /// <summary>
    ///     Adds an annotation to this object. Throws if an annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The key of the annotation to be added.</param>
    /// <param name="annotation">The annotation to be added.</param>
    /// <returns>The added annotation.</returns>
    protected virtual Annotation AddAnnotation(string name, Annotation annotation)
    {
        if (FindAnnotation(name) != null)
        {
            throw new InvalidOperationException(CoreStrings.DuplicateAnnotation(name, ToString()));
        }

        SetAnnotation(name, annotation, oldAnnotation: null);

        return annotation;
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

        SetAnnotation(name, new(name, value), oldAnnotation);
    }

    /// <summary>
    ///     Sets the annotation stored under the given key. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The key of the annotation to be added.</param>
    /// <param name="annotation">The annotation to be set.</param>
    /// <param name="oldAnnotation">The annotation being replaced.</param>
    /// <returns>The annotation that was set.</returns>
    protected virtual Annotation? SetAnnotation(
        string name,
        Annotation annotation,
        Annotation? oldAnnotation)
    {
        _annotations ??= new SortedDictionary<string, Annotation>(StringComparer.Ordinal);
        _annotations[name] = annotation;

        return annotation;
    }

    /// <summary>
    ///     Removes the given annotation from this object.
    /// </summary>
    /// <param name="name">The annotation to remove.</param>
    /// <returns>The annotation that was removed.</returns>
    public virtual Annotation? RemoveAnnotation(string name)
    {
        var annotation = FindAnnotation(name);
        if (annotation == null)
        {
            return null;
        }

        _annotations!.Remove(name);

        if (_annotations.Count == 0)
        {
            _annotations = null;
        }

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

        return _annotations == null
            ? null
            : _annotations.TryGetValue(name, out var annotation)
                ? annotation
                : null;
    }

    /// <summary>
    ///     Gets all annotations on the current object.
    /// </summary>
    public virtual IEnumerable<IAnnotation> GetAnnotations()
        => _annotations?.Values ?? Enumerable.Empty<Annotation>();

    /// <inheritdoc />
    [DebuggerStepThrough]
    IAnnotation IMutableAnnotatable.AddAnnotation(string name, object? value)
        => AddAnnotation(name, value);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IAnnotation? IMutableAnnotatable.RemoveAnnotation(string name)
        => RemoveAnnotation(name);

    /// <inheritdoc />
    [DebuggerStepThrough]
    void IMutableAnnotatable.SetOrRemoveAnnotation(string name, object? value)
        => this[name] = value;

    /// <inheritdoc />
    [DebuggerStepThrough]
    IAnnotation? IReadOnlyAnnotatable.FindAnnotation(string name)
        => FindAnnotation(name);
}
