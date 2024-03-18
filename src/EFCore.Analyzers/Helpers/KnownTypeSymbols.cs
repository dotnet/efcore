// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

internal sealed class KnownTypeSymbols(Compilation compilation)
{
    public INamedTypeSymbol? IEnumerableOfTType => GetOrResolveType(typeof(IEnumerable<>), ref _IEnumerableOfTType);
    private Option<INamedTypeSymbol?> _IEnumerableOfTType;

    private INamedTypeSymbol? GetOrResolveType(Type type, ref Option<INamedTypeSymbol?> field)
        => GetOrResolveType(type.FullName!, ref field);

    private INamedTypeSymbol? GetOrResolveType(string fullyQualifiedName, ref Option<INamedTypeSymbol?> field)
    {
        if (field.HasValue)
        {
            return field.Value;
        }

        // TODO: What to do if the type is not found
        var type = compilation.GetTypeByMetadataName(fullyQualifiedName)
            ?? throw new InvalidOperationException("Could not find type symbol for: " + fullyQualifiedName);
        field = new(type);
        return type;
    }

    private readonly struct Option<T>(T value)
    {
        public readonly bool HasValue = true;
        public readonly T Value = value;
    }
}
