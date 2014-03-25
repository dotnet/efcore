// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Migrations.Model
{
    public abstract class MigrationOperationVisitor
    {
        public abstract void Visit([NotNull] CreateDatabaseOperation createDatabaseOperation);
        public abstract void Visit([NotNull] DropDatabaseOperation dropDatabaseOperation);
        public abstract void Visit([NotNull] CreateSequenceOperation createSequenceOperation);
        public abstract void Visit([NotNull] DropSequenceOperation dropSequenceOperation);
        public abstract void Visit([NotNull] CreateTableOperation createTableOperation);
        public abstract void Visit([NotNull] DropTableOperation dropTableOperation);
        public abstract void Visit([NotNull] RenameTableOperation renameTableOperation);
        public abstract void Visit([NotNull] MoveTableOperation moveTableOperation);
        public abstract void Visit([NotNull] AddColumnOperation addColumnOperation);
        public abstract void Visit([NotNull] DropColumnOperation dropColumnOperation);
        public abstract void Visit([NotNull] AddDefaultConstraintOperation addDefaultConstraintOperation);
        public abstract void Visit([NotNull] DropDefaultConstraintOperation dropDefaultConstraintOperation);
        public abstract void Visit([NotNull] AlterColumnOperation alterColumnOperation);
        public abstract void Visit([NotNull] RenameColumnOperation renameColumnOperation);
        public abstract void Visit([NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation);
        public abstract void Visit([NotNull] DropPrimaryKeyOperation dropPrimaryKeyOperation);
        public abstract void Visit([NotNull] AddForeignKeyOperation addForeignKeyOperation);
        public abstract void Visit([NotNull] DropForeignKeyOperation dropForeignKeyOperation);
        public abstract void Visit([NotNull] CreateIndexOperation addIndexOperation);
        public abstract void Visit([NotNull] DropIndexOperation dropIndexOperation);
        public abstract void Visit([NotNull] RenameIndexOperation renameIndexOperation);
    }
}
