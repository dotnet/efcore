// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Represents an expression that is quotable, that is, capable of returning an expression that, when evaluated, would construct an
///     expression identical to this one. Used to generate code for precompiled queries, which reconstructs this expression.
/// </summary>
[Experimental("EF1003")]
public interface IRelationalQuotableExpression
{
    /// <summary>
    ///     Quotes the expression; that is, returns an expression that, when evaluated, would construct an expression identical to this
    ///     one. Used to generate code for precompiled queries, which reconstructs this expression.
    /// </summary>
    Expression Quote();
}
