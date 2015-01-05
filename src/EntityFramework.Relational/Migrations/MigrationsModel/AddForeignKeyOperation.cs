// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.MigrationsModel
{
    public class AddForeignKeyOperation : MigrationOperation
    {
        private readonly string _foreignKeyName;
        private readonly SchemaQualifiedName _tableName;
        private readonly SchemaQualifiedName _referencedTableName;
        private readonly IReadOnlyList<string> _columnNames;
        private readonly IReadOnlyList<string> _referencedColumnNames;
        private readonly bool _cascadeDelete;

        public AddForeignKeyOperation(
            SchemaQualifiedName tableName,
            [NotNull] string foreignKeyName,
            [NotNull] IReadOnlyList<string> columnNames,
            SchemaQualifiedName referencedTableName,
            [NotNull] IReadOnlyList<string> referencedColumnNames,
            bool cascadeDelete)
        {
            Check.NotEmpty(foreignKeyName, "foreignKeyName");
            Check.NotNull(columnNames, "columnNames");
            Check.NotNull(referencedColumnNames, "referencedColumnNames");

            _foreignKeyName = foreignKeyName;
            _tableName = tableName;
            _referencedTableName = referencedTableName;
            _columnNames = columnNames;
            _referencedColumnNames = referencedColumnNames;
            _cascadeDelete = cascadeDelete;
        }

        public virtual string ForeignKeyName
        {
            get { return _foreignKeyName; }
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual SchemaQualifiedName ReferencedTableName
        {
            get { return _referencedTableName; }
        }

        public virtual IReadOnlyList<string> ColumnNames
        {
            get { return _columnNames; }
        }

        public virtual IReadOnlyList<string> ReferencedColumnNames
        {
            get { return _referencedColumnNames; }
        }

        public virtual bool CascadeDelete
        {
            get { return _cascadeDelete; }
        }

        public override void Accept<TVisitor, TContext>(TVisitor visitor, TContext context)
        {
            Check.NotNull(visitor, "visitor");
            Check.NotNull(context, "context");

            visitor.Visit(this, context);
        }

        public override void GenerateSql(MigrationOperationSqlGenerator generator, SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(batchBuilder, "batchBuilder");

            generator.Generate(this, batchBuilder);
        }

        public override void GenerateCode(MigrationCodeGenerator generator, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(stringBuilder, "stringBuilder");

            generator.Generate(this, stringBuilder);
        }
    }
}
