// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression representing a scalar extracted from a JSON column with the given path in SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class JsonScalarExpression : SqlExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="JsonScalarExpression" /> class.
    /// </summary>
    /// <param name="json">An expression representing a JSON value.</param>
    /// <param name="type">The <see cref="System.Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <param name="path">A list of path segments leading to the scalar from the root of the JSON stored in the column.</param>
    /// <param name="nullable">A value indicating whether the expression is nullable.</param>
    public JsonScalarExpression(
        SqlExpression json,
        IReadOnlyList<PathSegment> path,
        Type type,
        RelationalTypeMapping? typeMapping,
        bool nullable)
        : base(type, typeMapping)
    {
        Json = json;
        Path = path;
        IsNullable = nullable;
    }

    /// <summary>
    ///     The expression containing the JSON value.
    /// </summary>
    public virtual SqlExpression Json { get; }

    /// <summary>
    ///     The list of path segments leading to the scalar from the root of the JSON stored in the column.
    /// </summary>
    public virtual IReadOnlyList<PathSegment> Path { get; }

    /// <summary>
    ///     The value indicating whether the expression is nullable.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var newJson = (SqlExpression)visitor.Visit(Json);

        var nullable = IsNullable;
        if (newJson is ColumnExpression jsonColumnExpression)
        {
            nullable |= jsonColumnExpression.IsNullable;
        }

        PathSegment[]? newPath = null;

        for (var i = 0; i < Path.Count; i++)
        {
            var segment = Path[i];
            PathSegment newSegment;

            if (segment.PropertyName is not null)
            {
                // PropertyName segments are (currently) constants, nothing to visit.
                newSegment = segment;
            }
            else
            {
                var newArrayIndex = (SqlExpression)visitor.Visit(segment.ArrayIndex)!;
                if (newArrayIndex == segment.ArrayIndex)
                {
                    newSegment = segment;
                }
                else
                {
                    newSegment = new PathSegment(newArrayIndex);

                    if (newPath is null)
                    {
                        newPath = new PathSegment[Path.Count];
                        for (var j = 0; j < i; i++)
                        {
                            newPath[j] = Path[j];
                        }
                    }
                }
            }

            if (newPath is not null)
            {
                newPath[i] = newSegment;
            }
        }

        // TODO Call update: Issue#28887
        return newJson == Json && newPath is null
            ? this
            : new JsonScalarExpression(
                newJson,
                newPath ?? Path,
                Type,
                TypeMapping!,
                nullable);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="json">The <see cref="Json" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual JsonScalarExpression Update(SqlExpression json)
        => json != Json
            ? new JsonScalarExpression(json, Path, Type, TypeMapping!, IsNullable)
            : this;

    /// <inheritdoc />
    public override Expression Quote()
        => New(
            _quotingConstructor ??= typeof(JsonScalarExpression).GetConstructor(
                [typeof(SqlExpression), typeof(IReadOnlyList<PathSegment>), typeof(Type), typeof(RelationalTypeMapping), typeof(bool)])!,
            Json.Quote(),
            NewArrayInit(typeof(PathSegment), initializers: Path.Select(s => s.Quote())),
            Constant(Type),
            RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping),
            Constant(IsNullable));

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Json);
        expressionPrinter
            .Append(" -> ")
            .Append(string.Join(".", Path.Select(e => e.ToString())));
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is JsonScalarExpression jsonScalarExpression
            && Json.Equals(jsonScalarExpression.Json)
            && Path.SequenceEqual(jsonScalarExpression.Path);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Json, Path);
}
