// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that manipulates names of database objects for entity types that share a table to avoid clashes.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SqlServerSharedTableConvention : SharedTableConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SqlServerSharedTableConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public SqlServerSharedTableConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <inheritdoc />
    protected override bool IndexesUniqueAcrossTables
        => false;

    /// <inheritdoc />
    protected override bool AreCompatible(IReadOnlyKey key, IReadOnlyKey duplicateKey, in StoreObjectIdentifier storeObject)
        => base.AreCompatible(key, duplicateKey, storeObject)
            && key.AreCompatibleForSqlServer(duplicateKey, storeObject, shouldThrow: false);

    /// <inheritdoc />
    protected override bool AreCompatible(IReadOnlyIndex index, IReadOnlyIndex duplicateIndex, in StoreObjectIdentifier storeObject)
        => base.AreCompatible(index, duplicateIndex, storeObject)
            && index.AreCompatibleForSqlServer(duplicateIndex, storeObject, shouldThrow: false);
}
