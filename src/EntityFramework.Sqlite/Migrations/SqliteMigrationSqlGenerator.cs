// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteMigrationSqlGenerator : MigrationSqlGenerator, ISqliteMigrationSqlGenerator
    {
        private readonly ISqliteSqlGenerator _sql;

        public SqliteMigrationSqlGenerator(
            [NotNull] ISqliteSqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
            _sql = sqlGenerator;
        }

        public override void Generate(DropIndexOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(_sql.DelimitIdentifier(operation.Name));
        }

        public override void Generate(RenameSequenceOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotImplementedException();
        }

        public override void Generate(RenameColumnOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotImplementedException();
        }

        public override void Generate(RenameTableOperation operation, IModel model, SqlBatchBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.NewName != null)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(_sql.DelimitIdentifier(operation.Name))
                    .Append(" RENAME TO ")
                    .Append(_sql.DelimitIdentifier(operation.NewName));
            }
        }

        public override void Generate(RenameIndexOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotImplementedException();
        }

        public override void Generate(AlterColumnOperation operation, IModel model, SqlBatchBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
