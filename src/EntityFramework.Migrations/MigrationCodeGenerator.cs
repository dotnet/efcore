// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations
{
    public abstract class MigrationCodeGenerator
    {
        private readonly ModelCodeGenerator _modelCodeGenerator;

        public MigrationCodeGenerator([NotNull] ModelCodeGenerator modelCodeGenerator)
        {
            Check.NotNull(modelCodeGenerator, "modelCodeGenerator");

            _modelCodeGenerator = modelCodeGenerator;
        }

        public virtual ModelCodeGenerator ModelCodeGenerator
        {
            get { return _modelCodeGenerator; }
        }

        public virtual IReadOnlyList<string> GetNamespaces([NotNull] IEnumerable<MigrationOperation> operations)
        {
            Check.NotNull(operations, "operations");

            return GetDefaultNamespaces();
        }

        public virtual IReadOnlyList<string> GetMetadataNamespaces([NotNull] IMigrationMetadata migration, [NotNull] Type contextType)
        {
            Check.NotNull(migration, "migration");
            Check.NotNull(contextType, "contextType");

            return GetMetadataDefaultNamespaces()
                .Concat(ModelCodeGenerator.GetNamespaces(migration.TargetModel, contextType))
                .ToList();
        }

        public virtual IReadOnlyList<string> GetDefaultNamespaces()
        {
            return new[]
                {
                    "Microsoft.Data.Entity.Migrations",
                    "Microsoft.Data.Entity.Migrations.Builders",
                    "System"
                };
        }

        public virtual IReadOnlyList<string> GetMetadataDefaultNamespaces()
        {
            return new[]
                {
                    "Microsoft.Data.Entity.Migrations.Infrastructure"
                };
        }

        public abstract void GenerateMigrationClass(
            [NotNull] string @namespace,
            [NotNull] string className,
            [NotNull] IMigrationMetadata migration,
            [NotNull] IndentedStringBuilder stringBuilder);

        public abstract void GenerateMigrationMetadataClass(
            [NotNull] string @namespace,
            [NotNull] string className,
            [NotNull] IMigrationMetadata migration,
            [NotNull] Type contextType,
            [NotNull] IndentedStringBuilder stringBuilder);

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
        public abstract void Generate([NotNull] CopyDataOperation copyDataOperation, [NotNull] IndentedStringBuilder stringBuilder);
        public abstract void Generate([NotNull] SqlOperation sqlOperation, [NotNull] IndentedStringBuilder stringBuilder);
    }
}
