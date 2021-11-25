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
public abstract class TableExpressionBase : Expression, IPrintableExpression, ISqlExpressionAnnotatable
{
    private SortedDictionary<string, ISqlExpressionAnnotation>? _annotations;

    /// <summary>
    ///     Creates a new instance of the <see cref="TableExpressionBase" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    protected TableExpressionBase(string? alias)
    {
        Alias = alias;
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
        => HashCode.Combine(Alias);

    /// <inheritdoc />
    public virtual ISqlExpressionAnnotation? FindAnnotation(string name)
        => _annotations == null
            ? null
            : _annotations.TryGetValue(name, out var annotation)
                ? annotation
                : null;

    /// <inheritdoc />
    public virtual IEnumerable<ISqlExpressionAnnotation> GetAnnotations()
        => _annotations?.Values ?? Enumerable.Empty<ISqlExpressionAnnotation>();

    /// <inheritdoc />
    public virtual ISqlExpressionAnnotation? RemoveAnnotation(string name)
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

    /// <inheritdoc />
    public virtual void SetAnnotation(string name, object? value)
    {
        var oldAnnotation = FindAnnotation(name);
        if (oldAnnotation != null
            && Equals(oldAnnotation.Value, value))
        {
            return;
        }

        SetAnnotation(name, new SqlExpressionAnnotation(name, value), oldAnnotation);
    }

    /// <summary>
    ///     Sets the annotation stored under the given key. Overwrites the existing annotation if an
    ///     annotation with the specified name already exists.
    /// </summary>
    /// <param name="name">The key of the annotation to be added.</param>
    /// <param name="annotation">The annotation to be set.</param>
    /// <param name="oldAnnotation">The annotation being replaced.</param>
    /// <returns>The annotation that was set.</returns>
    protected virtual ISqlExpressionAnnotation? SetAnnotation(
        string name,
        ISqlExpressionAnnotation annotation,
        ISqlExpressionAnnotation? oldAnnotation)
    {
        _annotations ??= new SortedDictionary<string, ISqlExpressionAnnotation>(StringComparer.Ordinal);
        _annotations[name] = annotation;

        return annotation;
    }

    /// <summary>
    ///     Annotates this <see cref="TableExpressionBase"/> with annotations in the collection.
    ///     Overwrites the existing annotations if an annotation with the specified name already exists.
    /// </summary>
    /// <param name="annotations">Collection of annotations. </param>
    protected virtual void SetAnnotations(IEnumerable<ISqlExpressionAnnotation> annotations)
    {
        foreach (var annotation in annotations)
        {
            SetAnnotation(annotation.Name, annotation.Value);
        }
    }
}
