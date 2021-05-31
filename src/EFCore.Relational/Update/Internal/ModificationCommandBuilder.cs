// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ModificationCommandBuilder : IModificationCommandBuilder
    {
        private readonly string _tableName;
        private readonly string? _schemaName;
        private readonly Func<string> _generateParameterName;
        private readonly bool _sensitiveLoggingEnabled;
        private readonly IComparer<IUpdateEntry>? _comparer;

        private readonly IModificationCommandFactory _modificationCommandFactory;
        private readonly IColumnModificationFactory _columnModificationFactory;

        private bool _mainEntryAdded;

        private readonly List<IUpdateEntry> _entries = new();

        private readonly IDiagnosticsLogger<DbLoggerCategory.Update>? _logger;

        private IModificationCommand? _resultCommand;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ModificationCommandBuilder(
            string tableName,
            string? schemaName,
            Func<string> generateParameterName,
            bool sensitiveLoggingEnabled,
            IComparer<IUpdateEntry>? comparer,
            IModificationCommandFactory modificationCommandFactory,
            IColumnModificationFactory columnModificationFactory,
            IDiagnosticsLogger<DbLoggerCategory.Update>? logger)
        {
            _tableName = tableName;
            _schemaName = schemaName;

            _generateParameterName = generateParameterName;
            _sensitiveLoggingEnabled = sensitiveLoggingEnabled;
            _comparer = comparer;

            _modificationCommandFactory = modificationCommandFactory;
            _columnModificationFactory = columnModificationFactory;

            _mainEntryAdded = false;

            _logger = logger;

            _resultCommand = null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<IUpdateEntry> Entries
            => _entries;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityState EntityState
        {
            get
            {
                if (_mainEntryAdded)
                {
                    var mainEntry = _entries[0];
                    if (mainEntry.SharedIdentityEntry == null)
                    {
                        return mainEntry.EntityState;
                    }

                    return mainEntry.SharedIdentityEntry.EntityType == mainEntry.EntityType
                        || mainEntry.SharedIdentityEntry.EntityType.GetTableMappings()
                            .Any(m => m.Table.Name == _tableName && m.Table.Schema == _schemaName)
                            ? EntityState.Modified
                            : mainEntry.EntityState;
                }

                return EntityState.Modified;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddEntry(IUpdateEntry entry, bool mainEntry)
        {
            Check.NotNull(entry, nameof(entry));

            // TODO: Transform to runtime check
            Check.DebugAssert(_resultCommand == null, "_resultCommand was created!");

            switch (entry.EntityState)
            {
                case EntityState.Deleted:
                case EntityState.Modified:
                case EntityState.Added:
                    break;
                default:
                    if (_sensitiveLoggingEnabled)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.ModificationCommandInvalidEntityStateSensitive(
                                entry.EntityType.DisplayName(),
                                entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties),
                                entry.EntityState));
                    }

                    throw new InvalidOperationException(
                        RelationalStrings.ModificationCommandInvalidEntityState(
                            entry.EntityType.DisplayName(),
                            entry.EntityState));
            }

            if (mainEntry)
            {
                Check.DebugAssert(!_mainEntryAdded, "Only expected a single main entry");

                for (var i = 0; i < _entries.Count; i++)
                {
                    ValidateState(entry, _entries[i]);
                }

                _mainEntryAdded = true;
                _entries.Insert(0, entry);
            }
            else
            {
                if (_mainEntryAdded)
                {
                    ValidateState(_entries[0], entry);
                }

                _entries.Add(entry);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IModificationCommand GetModificationCommand()
        {
            if(_resultCommand != null)
            {
                return _resultCommand;
            }

            if (_comparer != null)
            {
                _entries.Sort(_comparer);
            }

            _resultCommand = _modificationCommandFactory.CreateModificationCommand(
                new ModificationCommandParameters(
                    _tableName,
                    _schemaName,
                    _generateParameterName,
                    _sensitiveLoggingEnabled,
                    _columnModificationFactory,
                    _entries,
                    EntityState,
                    _logger));

            return _resultCommand;
        }

        private void ValidateState(IUpdateEntry mainEntry, IUpdateEntry entry)
        {
            var mainEntryState = mainEntry.SharedIdentityEntry == null
                ? mainEntry.EntityState
                : EntityState.Modified;
            if (mainEntryState == EntityState.Modified)
            {
                return;
            }

            var entryState = entry.SharedIdentityEntry == null
                ? entry.EntityState
                : EntityState.Modified;
            if (mainEntryState != entryState)
            {
                if (_sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingRowUpdateTypesSensitive(
                            entry.EntityType.DisplayName(),
                            entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties),
                            entryState,
                            mainEntry.EntityType.DisplayName(),
                            mainEntry.BuildCurrentValuesString(mainEntry.EntityType.FindPrimaryKey()!.Properties),
                            mainEntryState));
                }

                throw new InvalidOperationException(
                    RelationalStrings.ConflictingRowUpdateTypes(
                        entry.EntityType.DisplayName(),
                        entryState,
                        mainEntry.EntityType.DisplayName(),
                        mainEntryState));
            }
        }
    }
}
