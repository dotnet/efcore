// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <inheritdoc/>
public class SqlExpressionAnnotation : ISqlExpressionAnnotation
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlExpressionAnnotation" /> class.
    /// </summary>
    /// <param name="name">The key of this annotation.</param>
    /// <param name="value">The value assigned to this annotation.</param>
    public SqlExpressionAnnotation(string name, object? value)
    {
        Check.NotEmpty(name, nameof(name));

        Name = name;
        Value = value;
    }

    /// <inheritdoc/>
    public virtual string Name { get; }

    /// <inheritdoc/>
    public virtual object? Value { get; }
}
