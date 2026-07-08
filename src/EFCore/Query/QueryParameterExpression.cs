// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Represents a parameter external to the query, which may have different values across different executions of the same query.
///     This is created by <see cref="ExpressionTreeFuncletizer" /> for closure captured variables, and in relational, is translated to
///     <c>SqlParameterExpression</c>
/// </summary>
public class QueryParameterExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="QueryParameterExpression" /> class with associated query provider.
    /// </summary>
    public QueryParameterExpression(string name, Type type)
        : this(name, type, translationMode: null, isNonNullableReferenceType: false)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryParameterExpression" /> class with associated query provider.
    /// </summary>
    public QueryParameterExpression(string name, Type type, ParameterTranslationMode translationMode)
        : this(name, type, translationMode, isNonNullableReferenceType: false)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryParameterExpression" /> class with associated query provider.
    /// </summary>
    [Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
    public QueryParameterExpression(string name, Type type, ParameterTranslationMode? translationMode, bool isNonNullableReferenceType)
    {
        Name = name;
        Type = type;
        TranslationMode = translationMode;
        IsNonNullableReferenceType = isNonNullableReferenceType;
    }

    /// <summary>
    ///     The name of the query parameter.
    /// </summary>
    public virtual string Name { get; }

    /// <summary>
    ///     The static type of the expression that this <see cref="Expression" /> represents.
    /// </summary>
    public override Type Type { get; }

    // TODO: Naming. Also consider changing inline/not-inline to a three-value enum

    /// <summary>
    ///     Whether this query parameter's type is a non-nullable reference type.
    /// </summary>
    [Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
    public virtual bool IsNonNullableReferenceType { get; }

    /// <summary>
    ///     How should the parameter be handled.
    /// </summary>
    public virtual ParameterTranslationMode? TranslationMode { get; }

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <inheritdoc />
    public void Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append("@").Append(Name);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is QueryParameterExpression queryParameterExpression
                && Equals(queryParameterExpression));

    private bool Equals(QueryParameterExpression queryParameterExpression)
        => Name == queryParameterExpression.Name
            && Type == queryParameterExpression.Type
            && TranslationMode == queryParameterExpression.TranslationMode
            && IsNonNullableReferenceType == queryParameterExpression.IsNonNullableReferenceType;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Name, Type, TranslationMode, IsNonNullableReferenceType);
}
