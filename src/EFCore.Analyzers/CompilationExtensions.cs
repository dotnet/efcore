// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore;

internal static class CompilationExtensions
{
    public static INamedTypeSymbol? DbSetType(this Compilation compilation)
        => compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbSet`1");

    public static INamedTypeSymbol? DbContextType(this Compilation compilation)
        => compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbContext");

    public static INamedTypeSymbol? RelationalQueryableExtensionsType(this Compilation compilation)
        => compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.RelationalQueryableExtensions");

    public static INamedTypeSymbol? RelationalDatabaseFacadeExtensionsType(this Compilation compilation)
        => compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions");

    public static IMethodSymbol? FromSqlRawMethod(this Compilation compilation)
    {
        var type = compilation.RelationalQueryableExtensionsType();
        return (IMethodSymbol?)type?.GetMembers("FromSqlRaw").FirstOrDefault(s => s is IMethodSymbol);
    }

    public static IEnumerable<IMethodSymbol> ExecuteSqlRawMethods(this Compilation compilation)
    {
        var type = compilation.RelationalDatabaseFacadeExtensionsType();
        return type?.GetMembers("ExecuteSqlRaw").Where(s => s is IMethodSymbol).Cast<IMethodSymbol>() ?? Array.Empty<IMethodSymbol>();
    }

    public static IEnumerable<IMethodSymbol> ExecuteSqlRawAsyncMethods(this Compilation compilation)
    {
        var type = compilation.RelationalDatabaseFacadeExtensionsType();
        return type?.GetMembers("ExecuteSqlRawAsync").Where(s => s is IMethodSymbol).Cast<IMethodSymbol>() ?? Array.Empty<IMethodSymbol>();
    }

    public static IMethodSymbol? SqlQueryRawMethod(this Compilation compilation)
    {
        var type = compilation.RelationalDatabaseFacadeExtensionsType();
        return (IMethodSymbol?)type?.GetMembers("SqlQueryRaw").FirstOrDefault(s => s is IMethodSymbol);
    }
}
