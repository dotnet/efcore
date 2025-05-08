// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
/// Represents a query filter in a model.
/// </summary>
/// <param name="key">The name of the query filter.</param>
/// <param name="expression">The expression representing the filter.</param>
/// <param name="configurationSource">The source of the configuration.</param>
public class QueryFilter(string? key, LambdaExpression? expression, ConfigurationSource configurationSource = ConfigurationSource.Explicit) : IQueryFilter
{
    /// <inheritdoc />
    public virtual LambdaExpression? Expression { get; } = expression;

    /// <inheritdoc />
    public virtual string? Key { get; } = key;

    /// <summary>
    /// The source of the configuration.
    /// </summary>
    public virtual ConfigurationSource ConfigurationSource { get; } = configurationSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryFilter"/> class using the specified query filter and
    /// configuration source.
    /// </summary>
    /// <param name="queryFilter">The query filter to use as the basis for this instance.</param>
    /// <param name="configurationSource">The source of the configuration. Defaults to <see cref="ConfigurationSource.Explicit"/> if not specified.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryFilter"/> is <see langword="null"/>.</exception>
    public QueryFilter(IQueryFilter queryFilter, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        : this(queryFilter.Key, queryFilter.Expression, configurationSource)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryFilter"/> class with the specified filter expression and
    /// configuration source.
    /// </summary>
    /// <param name="expression">The <see cref="LambdaExpression"/> representing the filter logic.</param>
    /// <param name="configurationSource">The source of the configuration for this filter. Defaults to <see cref="ConfigurationSource.Explicit"/>.</param>
    public QueryFilter(LambdaExpression? expression, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        : this(null, expression, configurationSource)
    {
    }
}
