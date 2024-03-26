// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public class PrecompiledQuerySafeMarker : Expression
{
    internal static readonly MethodInfo ComposeMethodInfo
        = typeof(PrecompiledQuerySafeMarker).GetTypeInfo().GetDeclaredMethod(nameof(Compose))!;

    public static IQueryable<TSource> Compose<TSource>(IQueryable<TSource> source)
        => source.Provider.CreateQuery<TSource>(
            Call(
                instance: null,
                method: new Func<IQueryable<TSource>, IQueryable<TSource>>(Compose).Method,
                arguments: [source.Expression]));
}
