// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;

namespace Microsoft.Data.Migrations
{
    public abstract class MigrationCodeGenerator
    {
        // TODO: Add main Generate method and revisit to decide what needs to be public vs. protected.

        protected virtual IReadOnlyList<string> GetNamespaces(IEnumerable<MigrationOperation> operations)
        {
            return GetDefaultNamespaces(operations);
        }

        protected virtual IReadOnlyList<string> GetDefaultNamespaces(IEnumerable<MigrationOperation> operations)
        {
            return
                new[]
                    {
                        "System",
                        "Microsoft.Data.Migrations",
                        "Microsoft.Data.Migrations.Builders",
                        "Microsoft.Data.Migrations.Model",
                        "Microsoft.Data.Relational"
                    };
        }

        public abstract void Generate([NotNull] CreateDatabaseOperation createDatabaseOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] DropDatabaseOperation dropDatabaseOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] CreateSequenceOperation createSequenceOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] DropSequenceOperation dropSequenceOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] CreateTableOperation createTableOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] DropTableOperation dropTableOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] RenameTableOperation dropTableOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] MoveTableOperation dropTableOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] AddColumnOperation addColumnOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] DropColumnOperation dropColumnOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] RenameColumnOperation renameColumnOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] AlterColumnOperation alterColumnOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] AddDefaultConstraintOperation addDefaultConstraintOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] DropDefaultConstraintOperation dropDefaultConstraintOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] DropPrimaryKeyOperation dropPrimaryKeyOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] AddForeignKeyOperation addForeignKeyOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] DropForeignKeyOperation dropForeignKeyOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] CreateIndexOperation createIndexOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] DropIndexOperation dropIndexOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] RenameIndexOperation renameIndexOperation, [NotNull] IndentedStringBuilder stringBuilder);
    }
}
