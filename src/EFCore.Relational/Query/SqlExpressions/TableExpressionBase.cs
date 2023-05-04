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
public abstract class TableExpressionBase : Expression, IPrintableExpression
{
    private readonly IReadOnlyDictionary<string, IAnnotation>? _annotations;

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
            var dictionary = new SortedDictionary<string, IAnnotation>();
            foreach (var annotation in annotations)
            {
                dictionary[annotation.Name] = annotation;
            }

            _annotations = dictionary;
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
    /// <returns>The new expression with annotation applied to it.</returns>
    public virtual TableExpressionBase AddAnnotation(string name, object? value)
    {
        var oldAnnotation = FindAnnotation(name);
        if (oldAnnotation != null)
        {
            return Equals(oldAnnotation.Value, value)
                ? this
                : throw new InvalidOperationException(CoreStrings.DuplicateAnnotation(name, this.Print()));
        }

        var annotation = new Annotation(name, value);

        return CreateWithAnnotations(new[] { annotation }.Concat(GetAnnotations()));
    }

    /// <summary>
    ///     Creates an object like this with specified annotations.
    /// </summary>
    /// <param name="annotations">The annotations to be applied.</param>
    /// <returns>The new expression with given annotations.</returns>
    protected abstract TableExpressionBase CreateWithAnnotations(IEnumerable<IAnnotation> annotations);

    /// <summary>
    ///     Gets the annotation with the given name, returning <see langword="null" /> if it does not exist.
    /// </summary>
    /// <param name="name">The key of the annotation to find.</param>
    /// <returns>
    ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
    /// </returns>
    public virtual IAnnotation? FindAnnotation(string name)
        => _annotations == null
            ? null
            : _annotations.TryGetValue(name, out var annotation)
                ? annotation
                : null;

    /// <summary>
    ///     Gets all annotations on the current object.
    /// </summary>
    public virtual IEnumerable<IAnnotation> GetAnnotations()
        => _annotations?.Values ?? Enumerable.Empty<IAnnotation>();
}
