// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     A <see cref="IMigrationsAssembly" /> implementation that supports dynamically compiled migrations
///     in addition to migrations from the original assembly.
/// </summary>
/// <remarks>
///     <para>
///         This service is a decorator that wraps the standard <see cref="MigrationsAssembly" />
///         and merges dynamically registered migrations with statically compiled ones.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public class DynamicMigrationsAssembly : IDynamicMigrationsAssembly
{
    private readonly IMigrationsAssembly _inner;
    private readonly SortedList<string, TypeInfo> _dynamicMigrations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CompiledMigration> _compiledMigrations = new(StringComparer.OrdinalIgnoreCase);
    private ModelSnapshot? _dynamicSnapshot;
    private IReadOnlyDictionary<string, TypeInfo>? _mergedMigrations;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DynamicMigrationsAssembly" /> class.
    /// </summary>
    /// <param name="inner">The inner migrations assembly to wrap.</param>
    public DynamicMigrationsAssembly(IMigrationsAssembly inner)
    {
        _inner = inner;
    }

    /// <inheritdoc />
    public virtual IReadOnlyDictionary<string, TypeInfo> Migrations
    {
        get
        {
            if (_mergedMigrations != null)
            {
                return _mergedMigrations;
            }

            if (_dynamicMigrations.Count == 0)
            {
                return _inner.Migrations;
            }

            // Merge static and dynamic migrations
            var merged = new SortedList<string, TypeInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in _inner.Migrations)
            {
                merged[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in _dynamicMigrations)
            {
                merged[kvp.Key] = kvp.Value;
            }

            _mergedMigrations = merged;
            return _mergedMigrations;
        }
    }

    /// <inheritdoc />
    public virtual ModelSnapshot? ModelSnapshot
        => _dynamicSnapshot ?? _inner.ModelSnapshot;

    /// <inheritdoc />
    public virtual Assembly Assembly
        => _inner.Assembly;

    /// <inheritdoc />
    public virtual string? FindMigrationId(string nameOrId)
    {
        // First check dynamic migrations
        foreach (var id in _dynamicMigrations.Keys)
        {
            if (string.Equals(id, nameOrId, StringComparison.OrdinalIgnoreCase))
            {
                return id;
            }

            // Also check by name (without timestamp prefix)
            var name = id.Length > 15 ? id[15..] : id;
            if (string.Equals(name, nameOrId, StringComparison.OrdinalIgnoreCase))
            {
                return id;
            }
        }

        // Fall back to inner assembly
        return _inner.FindMigrationId(nameOrId);
    }

    /// <inheritdoc />
    public virtual Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
    {
        var migration = (Migration)Activator.CreateInstance(migrationClass.AsType())!;
        migration.ActiveProvider = activeProvider;
        return migration;
    }

    /// <inheritdoc />
    public virtual void RegisterDynamicMigration(CompiledMigration compiledMigration)
    {
        _dynamicMigrations[compiledMigration.MigrationId] = compiledMigration.MigrationTypeInfo;
        _compiledMigrations[compiledMigration.MigrationId] = compiledMigration;

        // Update the snapshot if provided
        if (compiledMigration.SnapshotTypeInfo != null)
        {
            _dynamicSnapshot = (ModelSnapshot)Activator.CreateInstance(compiledMigration.SnapshotTypeInfo.AsType())!;
        }

        // Invalidate merged cache
        _mergedMigrations = null;
    }

    /// <inheritdoc />
    public virtual void ClearDynamicMigrations()
    {
        _dynamicMigrations.Clear();
        _compiledMigrations.Clear();
        _dynamicSnapshot = null;
        _mergedMigrations = null;
    }

    /// <inheritdoc />
    public virtual bool HasDynamicMigrations
        => _dynamicMigrations.Count > 0;

    /// <summary>
    ///     Gets the compiled migration for the specified migration ID, if it was dynamically registered.
    /// </summary>
    /// <param name="migrationId">The migration ID.</param>
    /// <returns>The compiled migration, or <see langword="null" /> if not found.</returns>
    public virtual CompiledMigration? GetCompiledMigration(string migrationId)
        => _compiledMigrations.TryGetValue(migrationId, out var migration) ? migration : null;
}
