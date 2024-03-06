// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;

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
[DebuggerDisplay("{Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(this), nq}")]
public abstract class TableExpressionBase : Expression, IRelationalQuotableExpression, IPrintableExpression
{
    /// <summary>
    ///     An indexed collection of annotations associated with this table expression.
    /// </summary>
    protected virtual IReadOnlyDictionary<string, IAnnotation>? Annotations { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="TableExpressionBase" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    /// <param name="annotations">A collection of annotations associated with this table expression.</param>
    protected TableExpressionBase(string? alias, IEnumerable<IAnnotation>? annotations = null)
    {
        Alias = alias;

        if (annotations != null)
        {
            var dictionary = new SortedDictionary<string, IAnnotation>();
            foreach (var annotation in annotations)
            {
                dictionary[annotation.Name] = annotation;
            }

            Annotations = dictionary;
        }
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="TableExpressionBase" /> class.
    /// </summary>
    /// <param name="alias">A string alias for the table source.</param>
    /// <param name="annotations">A collection of annotations associated with this expression.</param>
    protected TableExpressionBase(string? alias, IReadOnlyDictionary<string, IAnnotation>? annotations)
    {
        Alias = alias;
        Annotations = annotations;
    }

    /// <summary>
    ///     The alias assigned to this table source.
    /// </summary>
    public virtual string? Alias { get; }

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
    ///     Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <param name="alias">The alias to be used by the cloned table.</param>
    /// <param name="cloningExpressionVisitor">The cloning expression for further visitation of nested nodes.</param>
    /// <returns>A new object that is a copy of this instance.</returns>
    public abstract TableExpressionBase Clone(string? alias, ExpressionVisitor cloningExpressionVisitor);

    /// <summary>
    ///     Returns a copy of the current <see cref="TableExpressionBase" /> with the new provided alias.
    /// </summary>
    /// <param name="newAlias">The alias to apply to the returned <see cref="TableExpressionBase" />.</param>
    public abstract TableExpressionBase WithAlias(string newAlias);

    /// <inheritdoc />
    public abstract Expression Quote();

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
        => Alias?.GetHashCode() ?? 0;

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


        var annotations = new SortedDictionary<string, IAnnotation>();

        if (Annotations is not null)
        {
            foreach (var annotation in Annotations.Values)
            {
                annotations[annotation.Name] = annotation;
            }
        }

        annotations[name] = new Annotation(name, value);

        return WithAnnotations(annotations);
    }

    /// <summary>
    ///     Creates an object like this with specified annotations.
    /// </summary>
    /// <param name="annotations">The annotations to be applied.</param>
    /// <returns>The new expression with given annotations.</returns>
    protected abstract TableExpressionBase WithAnnotations(IReadOnlyDictionary<string, IAnnotation> annotations);

    /// <summary>
    ///     Gets the annotation with the given name, returning <see langword="null" /> if it does not exist.
    /// </summary>
    /// <param name="name">The key of the annotation to find.</param>
    /// <returns>
    ///     The existing annotation if an annotation with the specified name already exists. Otherwise, <see langword="null" />.
    /// </returns>
    public virtual IAnnotation? FindAnnotation(string name)
        => Annotations?.GetValueOrDefault(name);

    /// <summary>
    ///     Gets all annotations on the current object.
    /// </summary>
    public virtual IEnumerable<IAnnotation> GetAnnotations()
        => Annotations?.Values ?? Enumerable.Empty<IAnnotation>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual string GetRequiredAlias()
        => Alias ?? throw new InvalidOperationException(RelationalStrings.NoAliasOnTable(ExpressionPrinter.Print(this)));
}
