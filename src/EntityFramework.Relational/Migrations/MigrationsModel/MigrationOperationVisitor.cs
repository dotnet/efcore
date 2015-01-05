// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Migrations.MigrationsModel
{
    public abstract class MigrationOperationVisitor<TContext>
    {
        public virtual void Visit([NotNull] CreateDatabaseOperation createDatabaseOperation, [NotNull] TContext context)
        {
            VisitDefault(createDatabaseOperation, context);
        }

        public virtual void Visit([NotNull] DropDatabaseOperation dropDatabaseOperation, [NotNull] TContext context)
        {
            VisitDefault(dropDatabaseOperation, context);
        }

        public virtual void Visit([NotNull] CreateSequenceOperation createSequenceOperation, [NotNull] TContext context)
        {
            VisitDefault(createSequenceOperation, context);
        }

        public virtual void Visit([NotNull] DropSequenceOperation dropSequenceOperation, [NotNull] TContext context)
        {
            VisitDefault(dropSequenceOperation, context);
        }

        public virtual void Visit([NotNull] RenameSequenceOperation renameSequenceOperation, [NotNull] TContext context)
        {
            VisitDefault(renameSequenceOperation, context);
        }

        public virtual void Visit([NotNull] MoveSequenceOperation moveSequenceOperation, [NotNull] TContext context)
        {
            VisitDefault(moveSequenceOperation, context);
        }

        public virtual void Visit([NotNull] AlterSequenceOperation alterSequenceOperation, [NotNull] TContext context)
        {
            VisitDefault(alterSequenceOperation, context);
        }

        public virtual void Visit([NotNull] CreateTableOperation createTableOperation, [NotNull] TContext context)
        {
            VisitDefault(createTableOperation, context);
        }

        public virtual void Visit([NotNull] DropTableOperation dropTableOperation, [NotNull] TContext context)
        {
            VisitDefault(dropTableOperation, context);
        }

        public virtual void Visit([NotNull] RenameTableOperation renameTableOperation, [NotNull] TContext context)
        {
            VisitDefault(renameTableOperation, context);
        }

        public virtual void Visit([NotNull] MoveTableOperation moveTableOperation, [NotNull] TContext context)
        {
            VisitDefault(moveTableOperation, context);
        }

        public virtual void Visit([NotNull] AddColumnOperation addColumnOperation, [NotNull] TContext context)
        {
            VisitDefault(addColumnOperation, context);
        }

        public virtual void Visit([NotNull] DropColumnOperation dropColumnOperation, [NotNull] TContext context)
        {
            VisitDefault(dropColumnOperation, context);
        }

        public virtual void Visit([NotNull] AlterColumnOperation alterColumnOperation, [NotNull] TContext context)
        {
            VisitDefault(alterColumnOperation, context);
        }

        public virtual void Visit([NotNull] AddDefaultConstraintOperation addDefaultConstraintOperation, [NotNull] TContext context)
        {
            VisitDefault(addDefaultConstraintOperation, context);
        }

        public virtual void Visit([NotNull] DropDefaultConstraintOperation dropDefaultConstraintOperation, [NotNull] TContext context)
        {
            VisitDefault(dropDefaultConstraintOperation, context);
        }

        public virtual void Visit([NotNull] RenameColumnOperation renameColumnOperation, [NotNull] TContext context)
        {
            VisitDefault(renameColumnOperation, context);
        }

        public virtual void Visit([NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation, [NotNull] TContext context)
        {
            VisitDefault(addPrimaryKeyOperation, context);
        }

        public virtual void Visit([NotNull] DropPrimaryKeyOperation dropPrimaryKeyOperation, [NotNull] TContext context)
        {
            VisitDefault(dropPrimaryKeyOperation, context);
        }

        public virtual void Visit([NotNull] AddUniqueConstraintOperation addUniqueConstraintOperation, [NotNull] TContext context)
        {
            VisitDefault(addUniqueConstraintOperation, context);
        }

        public virtual void Visit([NotNull] DropUniqueConstraintOperation dropUniqueConstraintOperation, [NotNull] TContext context)
        {
            VisitDefault(dropUniqueConstraintOperation, context);
        }

        public virtual void Visit([NotNull] AddForeignKeyOperation addForeignKeyOperation, [NotNull] TContext context)
        {
            VisitDefault(addForeignKeyOperation, context);
        }

        public virtual void Visit([NotNull] DropForeignKeyOperation dropForeignKeyOperation, [NotNull] TContext context)
        {
            VisitDefault(dropForeignKeyOperation, context);
        }

        public virtual void Visit([NotNull] CreateIndexOperation createIndexOperation, [NotNull] TContext context)
        {
            VisitDefault(createIndexOperation, context);
        }

        public virtual void Visit([NotNull] DropIndexOperation dropIndexOperation, [NotNull] TContext context)
        {
            VisitDefault(dropIndexOperation, context);
        }

        public virtual void Visit([NotNull] RenameIndexOperation renameIndexOperation, [NotNull] TContext context)
        {
            VisitDefault(renameIndexOperation, context);
        }

        public virtual void Visit([NotNull] CopyDataOperation copyDataOperation, [NotNull] TContext context)
        {
            VisitDefault(copyDataOperation, context);
        }

        public virtual void Visit([NotNull] SqlOperation sqlOperation, [NotNull] TContext context)
        {
            VisitDefault(sqlOperation, context);
        }

        protected virtual void VisitDefault(MigrationOperation operation, TContext context)
        {
            throw new NotImplementedException();
        }
    }
}
