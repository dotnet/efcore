// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
/// Describes how the parameter expression in a query should be handled.
/// </summary>
public enum ParameterExpressionMode
{
    /// <summary>
    /// Handle as constants.
    /// </summary>
    Constants,

    /// <summary>
    /// Handle as parameter.
    /// </summary>
    Parameter,

    /// <summary>
    /// Handle as multiple parameters.
    /// </summary>
    MultipleParameters,
}
