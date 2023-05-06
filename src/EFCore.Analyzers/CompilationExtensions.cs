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

    public static INamedTypeSymbol? DatabaseFacadeType(this Compilation compilation)
        => compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade");

    public static INamedTypeSymbol? IEnumerableOfTType(this Compilation compilation)
        => compilation.GetTypeByMetadataName(typeof(IEnumerable<>).FullName);

    public static INamedTypeSymbol? IQueryableOfTType(this Compilation compilation)
        => compilation.GetTypeByMetadataName(typeof(IQueryable<>).FullName);

    public static INamedTypeSymbol? TaskOfTType(this Compilation compilation)
        => compilation.GetTypeByMetadataName(typeof(Task<>).FullName);
}
