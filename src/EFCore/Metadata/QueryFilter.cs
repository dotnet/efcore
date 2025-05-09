// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
/// Represents a query filter in a model.
/// </summary>
/// <param name="key">The key of the query filter.</param>
/// <param name="expression">The expression representing the filter.</param>
public class QueryFilter(string? key, LambdaExpression? expression) : IQueryFilter
{
    /// <inheritdoc />
    public virtual LambdaExpression? Expression { get; } = expression;

    /// <inheritdoc />
    public virtual string? Key { get; } = key;

    /// <summary>
    /// The source of the configuration.
    /// </summary>
    internal virtual ConfigurationSource? ConfigurationSource { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryFilter"/> class with the specified filter expression and
    /// configuration source.
    /// </summary>
    /// <param name="expression">The <see cref="LambdaExpression"/> representing the filter logic.</param>
    /// <param name="configurationSource">The source of the configuration for this filter. Defaults to <see cref="ConfigurationSource.Explicit"/>.</param>
    internal QueryFilter(LambdaExpression? expression, ConfigurationSource configurationSource)
        : this(null, expression) => ConfigurationSource = configurationSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryFilter"/> class with the specified filter expression and
    /// configuration source.
    /// </summary>
    /// <param name="key">The key of the query filter.</param>
    /// <param name="expression">The <see cref="LambdaExpression"/> representing the filter logic.</param>
    /// <param name="configurationSource">The source of the configuration for this filter. Defaults to <see cref="ConfigurationSource.Explicit"/>.</param>
    internal QueryFilter(string key, LambdaExpression? expression, ConfigurationSource configurationSource)
        : this(key, expression) => ConfigurationSource = configurationSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryFilter"/> class with the specified filter expression.
    /// </summary>
    /// <param name="expression">A <see cref="LambdaExpression"/> representing the filter criteria.  Can be <see langword="null"/> to indicate no
    /// filter is applied.</param>
    public QueryFilter(LambdaExpression? expression) : this(null, expression)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryFilter"/> class using the specified query filter and
    /// configuration source.
    /// </summary>
    /// <param name="queryFilter">The query filter to initialize this instance with. Must not be <see langword="null"/>.</param>
    /// <param name="fromDataAnnotation">A value indicating whether the configuration source is derived from a data annotation. If <see
    /// langword="true"/>, the configuration source is set to <see cref="ConfigurationSource.DataAnnotation"/>;
    /// otherwise, it is set to <see cref="ConfigurationSource.Convention"/>.</param>
    internal QueryFilter(IQueryFilter queryFilter, bool fromDataAnnotation)
        : this(queryFilter.Key, queryFilter.Expression) => ConfigurationSource = fromDataAnnotation
            ? Metadata.ConfigurationSource.DataAnnotation
            : Metadata.ConfigurationSource.Convention;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryFilter"/> class using the specified query filter and
    /// configuration source.
    /// </summary>
    /// <param name="expression">A <see cref="LambdaExpression"/> representing the filter criteria.  Can be <see langword="null"/> to indicate no
    /// filter is applied.</param>
    /// <param name="fromDataAnnotation">A value indicating whether the configuration source is derived from a data annotation. If <see
    /// langword="true"/>, the configuration source is set to <see cref="ConfigurationSource.DataAnnotation"/>;
    /// otherwise, it is set to <see cref="ConfigurationSource.Convention"/>.</param>
    internal QueryFilter(LambdaExpression? expression, bool fromDataAnnotation)
        : this(expression) => ConfigurationSource = fromDataAnnotation
            ? Metadata.ConfigurationSource.DataAnnotation
            : Metadata.ConfigurationSource.Convention;
}
