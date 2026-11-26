// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Wraps an <see cref="IQueryable" /> that was captured in a variable and inlined into the main query by the funcletizer, so that
///     providers can translate it as a named unit (e.g. a common table expression).
/// </summary>
internal static class EmbeddedQueryable
{
    public static readonly MethodInfo MarkerMethod = typeof(EmbeddedQueryable).GetMethod(nameof(Marker))!;

    // Never invoked; only appears in the expression tree between funcletization and translation.
    public static IQueryable<T> Marker<T>(IQueryable<T> source, string name)
        => source;
}
