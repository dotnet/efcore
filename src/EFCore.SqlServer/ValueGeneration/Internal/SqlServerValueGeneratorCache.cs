// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerValueGeneratorCache : ValueGeneratorCache, ISqlServerValueGeneratorCache
{
    private readonly ConcurrentDictionary<string, SqlServerSequenceValueGeneratorState> _sequenceGeneratorCache = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueGeneratorCache" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public SqlServerValueGeneratorCache(ValueGeneratorCacheDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlServerSequenceValueGeneratorState GetOrAddSequenceState(
        IProperty property,
        IRelationalConnection connection)
    {
        var tableIdentifier = StoreObjectIdentifier.Create(property.DeclaringType, StoreObjectType.Table);
        var sequence = tableIdentifier != null
            ? property.FindHiLoSequence(tableIdentifier.Value)
            : property.FindHiLoSequence();

        Check.DebugAssert(sequence != null, "sequence is null");

        return _sequenceGeneratorCache.GetOrAdd(
            GetSequenceName(sequence, connection),
            _ => new SqlServerSequenceValueGeneratorState(sequence));
    }

    private static string GetSequenceName(ISequence sequence, IRelationalConnection connection)
    {
        var dbConnection = connection.DbConnection;

        return dbConnection.Database.ToUpperInvariant()
            + "::"
            + dbConnection.DataSource.ToUpperInvariant()
            + "::"
            + (sequence.Schema == null ? "" : sequence.Schema + ".")
            + sequence.Name;
    }
}
