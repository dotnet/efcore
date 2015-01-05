// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Migrations.MigrationsModel
{
    public interface IMigrationOperationVisitor<in TContext>
    {
        void Visit([NotNull] CreateDatabaseOperation createDatabaseOperation, [NotNull] TContext context);
        void Visit([NotNull] DropDatabaseOperation dropDatabaseOperation, [NotNull] TContext context);
        void Visit([NotNull] CreateSequenceOperation createSequenceOperation, [NotNull] TContext context);
        void Visit([NotNull] DropSequenceOperation dropSequenceOperation, [NotNull] TContext context);
        void Visit([NotNull] CreateTableOperation createTableOperation, [NotNull] TContext context);
        void Visit([NotNull] DropTableOperation dropTableOperation, [NotNull] TContext context);
        void Visit([NotNull] RenameTableOperation renameTableOperation, [NotNull] TContext context);
        void Visit([NotNull] MoveTableOperation moveTableOperation, [NotNull] TContext context);
        void Visit([NotNull] AddColumnOperation addColumnOperation, [NotNull] TContext context);
        void Visit([NotNull] DropColumnOperation dropColumnOperation, [NotNull] TContext context);
        void Visit([NotNull] AlterColumnOperation alterColumnOperation, [NotNull] TContext context);
        void Visit([NotNull] AddDefaultConstraintOperation addDefaultConstraintOperation, [NotNull] TContext context);
        void Visit([NotNull] DropDefaultConstraintOperation dropDefaultConstraintOperation, [NotNull] TContext context);
        void Visit([NotNull] RenameColumnOperation renameColumnOperation, [NotNull] TContext context);
        void Visit([NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation, [NotNull] TContext context);
        void Visit([NotNull] DropPrimaryKeyOperation dropPrimaryKeyOperation, [NotNull] TContext context);
        void Visit([NotNull] AddForeignKeyOperation addForeignKeyOperation, [NotNull] TContext context);
        void Visit([NotNull] DropForeignKeyOperation dropForeignKeyOperation, [NotNull] TContext context);
        void Visit([NotNull] CreateIndexOperation createIndexOperation, [NotNull] TContext context);
        void Visit([NotNull] DropIndexOperation dropIndexOperation, [NotNull] TContext context);
        void Visit([NotNull] RenameIndexOperation renameIndexOperation, [NotNull] TContext context);
        void Visit([NotNull] SqlOperation sqlOperation, [NotNull] TContext context);
    }
}
