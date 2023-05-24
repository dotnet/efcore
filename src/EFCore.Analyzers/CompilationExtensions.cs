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
}
