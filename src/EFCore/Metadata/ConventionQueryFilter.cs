// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
/// Represents a query filter defined by a convention, optionally associated with a specific key.
/// </summary>
/// <param name="key">An optional key that identifies the query filter. This can be used to distinguish between multiple filters applied
/// to the same entity type. If <see langword="null"/>, the filter is not associated with a specific key.</param>
/// <param name="expression">A <see cref="LambdaExpression"/> representing the predicate to be applied as the query filter. This expression
/// determines which entities are included in the query results. Can be <see langword="null"/> if no filter is defined.</param>
/// <param name="fromDataAnnotation">A value indicating whether the query filter was configured using data annotations. <see langword="true"/> if the
/// filter was defined via data annotations; otherwise, <see langword="false"/> if it was defined by convention.</param>
public class ConventionQueryFilter(string? key, LambdaExpression? expression, bool fromDataAnnotation = false)
    : QueryFilter(key, expression, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConventionQueryFilter"/> class with the specified filter
    /// expression.
    /// </summary>
    /// <param name="expression">The <see cref="LambdaExpression"/> representing the query filter</param>
    /// <param name="fromDataAnnotation">A value indicating whether the filter is configured using data annotations.  <see langword="true"/> if the
    /// filter is defined via data annotations; otherwise, <see langword="false"/>.</param>
    public ConventionQueryFilter(LambdaExpression? expression, bool fromDataAnnotation = false)
        : this(null, expression, fromDataAnnotation)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConventionQueryFilter"/> class using the specified query filter.
    /// </summary>
    /// <param name="queryFilter">The query filter to be applied.</param>
    /// <param name="fromDataAnnotation">A value indicating whether the filter is configured via a data annotation. <see langword="true"/> if the filter
    /// is defined using a data annotation; otherwise, <see langword="false"/>.</param>
    public ConventionQueryFilter(IQueryFilter queryFilter, bool fromDataAnnotation = false)
        : this(queryFilter.Key, queryFilter.Expression, fromDataAnnotation)
    {
    }
}
