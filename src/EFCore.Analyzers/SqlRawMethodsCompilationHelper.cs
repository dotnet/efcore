// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore;

internal static class SqlRawMethodsCompilationHelper
{
    public static IMethodSymbol? FromSqlRawMethod(Compilation compilation)
    {
        var type = compilation.RelationalQueryableExtensionsType();
        return (IMethodSymbol?)type?.GetMembers("FromSqlRaw").FirstOrDefault(s => s is IMethodSymbol);
    }

    public static IEnumerable<IMethodSymbol> ExecuteSqlRawMethods(Compilation compilation)
    {
        var type = compilation.RelationalDatabaseFacadeExtensionsType();
        return type?.GetMembers("ExecuteSqlRaw").Where(s => s is IMethodSymbol).Cast<IMethodSymbol>() ?? [];
    }

    public static IEnumerable<IMethodSymbol> ExecuteSqlRawAsyncMethods(Compilation compilation)
    {
        var type = compilation.RelationalDatabaseFacadeExtensionsType();
        return type?.GetMembers("ExecuteSqlRawAsync").Where(s => s is IMethodSymbol).Cast<IMethodSymbol>() ?? [];
    }

    public static IMethodSymbol? SqlQueryRawMethod(Compilation compilation)
    {
        var type = compilation.RelationalDatabaseFacadeExtensionsType();
        return (IMethodSymbol?)type?.GetMembers("SqlQueryRaw").FirstOrDefault(s => s is IMethodSymbol);
    }
}
