// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         A placeholder in the template source row of a <see cref="MergeExpression" /> for the value of a member read from each source
///         row. Since the source rows are a query parameter, the template is expanded into the actual rows when the query is executed.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally not used in application code.
///     </para>
/// </summary>
[Experimental(EFDiagnostics.ExecuteMergeExperimental)]
public sealed class MergeSourceMemberExpression : SqlExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="MergeSourceMemberExpression" /> class.
    /// </summary>
    /// <param name="member">The member read from each source row.</param>
    /// <param name="type">The <see cref="System.Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    public MergeSourceMemberExpression(MemberInfo member, Type type, RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        Member = member;
    }

    /// <summary>
    ///     The member read from each source row.
    /// </summary>
    public MemberInfo Member { get; }

    /// <summary>
    ///     Reads the member's value from the given source row.
    /// </summary>
    /// <param name="row">The source row.</param>
    /// <returns>The member's value.</returns>
    public object? GetValue(object? row)
        => Member switch
        {
            PropertyInfo property => property.GetValue(row),
            FieldInfo field => field.GetValue(row),
            _ => throw new UnreachableException()
        };

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    public override Expression Quote()
        => throw new NotSupportedException(RelationalStrings.ExecuteMergeNotSupportedInPrecompiledQueries);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append($"source.{Member.Name}");

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || (obj is MergeSourceMemberExpression mergeSourceMemberExpression
                    && Equals(mergeSourceMemberExpression)));

    private bool Equals(MergeSourceMemberExpression mergeSourceMemberExpression)
        => base.Equals(mergeSourceMemberExpression)
            && Member.Equals(mergeSourceMemberExpression.Member);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Member);
}
