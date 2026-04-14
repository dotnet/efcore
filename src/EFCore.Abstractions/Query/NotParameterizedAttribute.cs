// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Signals that custom LINQ operator parameter should not be parameterized during query compilation.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class NotParameterizedAttribute : Attribute;
