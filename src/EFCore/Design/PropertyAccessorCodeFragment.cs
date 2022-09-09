// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Represents a property accessor lambda code fragment.
/// </summary>
public class PropertyAccessorCodeFragment
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyAccessorCodeFragment" /> class.
    /// </summary>
    /// <param name="parameter">The name of the lambda's parameter.</param>
    /// <param name="properties">The list of properties represented by the lambda.</param>
    public PropertyAccessorCodeFragment(string parameter, IReadOnlyList<string> properties)
    {
        Parameter = parameter;
        Properties = properties;
    }

    /// <summary>
    ///     Gets the name of the lambda's parameter.
    /// </summary>
    /// <value>The name of the paramenter.</value>
    public virtual string Parameter { get; }

    /// <summary>
    ///     Gets the list of properties represented by the lambda.
    /// </summary>
    /// <value>The list of properties.</value>
    public virtual IReadOnlyList<string> Properties { get; }
}
