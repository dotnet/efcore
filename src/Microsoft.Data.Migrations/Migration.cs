// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;
using Microsoft.Data.Migrations.Model;

namespace Microsoft.Data.Migrations
{
    public abstract class Migration
    {
        private enum MigrationState { None, Upgrade, Downgrade }

        private readonly Database _sourceDatabase;
        private readonly Database _targetDatabase;
        private readonly List<MigrationOperation> _operations = new List<MigrationOperation>();
        private MigrationState _migrationState;

        protected Migration([CanBeNull] Database sourceDatabase, [CanBeNull] Database targetDatabase)
        {
            _sourceDatabase = sourceDatabase;
            _targetDatabase = targetDatabase;
        }

        public virtual IReadOnlyList<MigrationOperation> Upgrade()
        {
            _migrationState = MigrationState.Upgrade;
            _operations.Clear();

            Up();

            return _operations;
        }

        public virtual IReadOnlyList<MigrationOperation> Downgrade()
        {
            _migrationState = MigrationState.Downgrade;
            _operations.Clear();

            Down();

            return _operations;
        }

        protected abstract void Up();
        protected abstract void Down();

        internal protected virtual void CreateSequence(Sequence sequence)
        {
            _operations.Add(new CreateSequenceOperation(sequence));
        }

        internal protected virtual void DropSequence(SchemaQualifiedName sequenceName)
        {
            switch (_migrationState)
            {
                case MigrationState.Upgrade:
                    _operations.Add(new DropSequenceOperation(
                        _sourceDatabase.GetSequence(sequenceName)));
                    break;
                case MigrationState.Downgrade:
                    _operations.Add(new DropSequenceOperation(
                        _targetDatabase.GetSequence(sequenceName)));
                    break;
            }
        }

        internal protected virtual void CreateTable(Table table)
        {
            _operations.Add(new CreateTableOperation(table));
        }

        internal protected virtual void DropTable(SchemaQualifiedName tableName)
        {
            switch (_migrationState)
            {
                case MigrationState.Upgrade:
                    _operations.Add(new DropTableOperation(
                        _sourceDatabase.GetTable(tableName)));
                    break;
                case MigrationState.Downgrade:
                    _operations.Add(new DropTableOperation(
                        _targetDatabase.GetTable(tableName)));
                    break;
            }
        }

        internal protected virtual void RenameTable(SchemaQualifiedName tableName, string newTableName)
        {
            switch (_migrationState)
            {
                case MigrationState.Upgrade:
                    _operations.Add(new RenameTableOperation(
                        _sourceDatabase.GetTable(tableName), newTableName));
                    break;
                case MigrationState.Downgrade:
                    _operations.Add(new RenameTableOperation(
                        _targetDatabase.GetTable(tableName), newTableName));
                    break;
            }
        }

        internal protected virtual void MoveTable(SchemaQualifiedName tableName, string moveToSchema)
        {
            switch (_migrationState)
            {
                case MigrationState.Upgrade:
                    _operations.Add(new MoveTableOperation(
                        _sourceDatabase.GetTable(tableName), moveToSchema));
                    break;
                case MigrationState.Downgrade:
                    _operations.Add(new MoveTableOperation(
                        _targetDatabase.GetTable(tableName), moveToSchema));
                    break;
            }
        }

        internal protected virtual void AddColumn(Column column, SchemaQualifiedName tableName)
        {
            switch (_migrationState)
            {
                case MigrationState.Upgrade:
                    _operations.Add(new AddColumnOperation(
                        column, _sourceDatabase.GetTable(tableName)));
                    break;
                case MigrationState.Downgrade:
                    _operations.Add(new AddColumnOperation(
                        column, _targetDatabase.GetTable(tableName)));
                    break;
            }
        }

        internal protected virtual void DropColumn(string columnName, SchemaQualifiedName tableName)
        {
            switch (_migrationState)
            {
                case MigrationState.Upgrade:
                    _operations.Add(new DropColumnOperation(
                        _sourceDatabase.GetTable(tableName).GetColumn(columnName)));
                    break;
                case MigrationState.Downgrade:
                    _operations.Add(new DropColumnOperation(
                        _targetDatabase.GetTable(tableName).GetColumn(columnName)));
                    break;
            }
        }

        internal protected virtual void RenameColumn(string columnName, SchemaQualifiedName tableName, string newColumnName)
        {
            switch (_migrationState)
            {
                case MigrationState.Upgrade:
                    _operations.Add(new RenameColumnOperation(
                        _sourceDatabase.GetTable(tableName).GetColumn(columnName), newColumnName));
                    break;
                case MigrationState.Downgrade:
                    _operations.Add(new RenameColumnOperation(
                        _targetDatabase.GetTable(tableName).GetColumn(columnName), newColumnName));
                    break;
            }
        }

        internal protected virtual void AddPrimaryKey(PrimaryKey primaryKey, SchemaQualifiedName tableName)
        {
            switch (_migrationState)
            {
                case MigrationState.Upgrade:
                    _operations.Add(new AddPrimaryKeyOperation(
                        primaryKey, _sourceDatabase.GetTable(tableName)));
                    break;
                case MigrationState.Downgrade:
                    _operations.Add(new AddPrimaryKeyOperation(
                        primaryKey, _targetDatabase.GetTable(tableName)));
                    break;
            }
        }

        internal protected virtual void DropPrimaryKey(SchemaQualifiedName primaryKeyName)
        {
            switch (_migrationState)
            {
                case MigrationState.Upgrade:
                    _operations.Add(new DropPrimaryKeyOperation(
                        _sourceDatabase.GetPrimaryKey(primaryKeyName)));
                    break;
                case MigrationState.Downgrade:
                    _operations.Add(new DropPrimaryKeyOperation(
                        _targetDatabase.GetPrimaryKey(primaryKeyName)));
                    break;
            }
        }
    }
}
