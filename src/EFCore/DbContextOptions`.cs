// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     The options to be used by a <see cref="DbContext" />. You normally override
///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or use a <see cref="DbContextOptionsBuilder{TContext}" />
///     to create instances of this class and it is not designed to be directly constructed in your application code.
/// </summary>
/// <typeparam name="TContext">The type of the context these options apply to.</typeparam>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information and examples.
/// </remarks>
public class DbContextOptions<TContext> : DbContextOptions
    where TContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DbContextOptions{TContext}" /> class. You normally override
    ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or use a <see cref="DbContextOptionsBuilder{TContext}" />
    ///     to create instances of this class and it is not designed to be directly constructed in your application code.
    /// </summary>
    public DbContextOptions()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbContextOptions{TContext}" /> class. You normally override
    ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or use a <see cref="DbContextOptionsBuilder{TContext}" />
    ///     to create instances of this class and it is not designed to be directly constructed in your application code.
    /// </summary>
    /// <param name="extensions">The extensions that store the configured options.</param>
    public DbContextOptions(
        IReadOnlyDictionary<Type, IDbContextOptionsExtension> extensions)
        : base(extensions)
    {
    }

    private DbContextOptions(
        ImmutableSortedDictionary<Type, (IDbContextOptionsExtension Extension, int Ordinal)> extensions)
        : base(extensions)
    {
    }

    /// <inheritdoc />
    public override DbContextOptions WithExtension<TExtension>(TExtension extension)
    {
        var type = extension.GetType();
        var ordinal = ExtensionsMap.Count;
        if (ExtensionsMap.TryGetValue(type, out var existingValue))
        {
            ordinal = existingValue.Ordinal;
        }

        return new DbContextOptions<TContext>(ExtensionsMap.SetItem(type, (extension, ordinal)));
    }

    /// <summary>
    ///     The type of context that these options are for (<typeparamref name="TContext" />).
    /// </summary>
    public override Type ContextType
        => typeof(TContext);
}
