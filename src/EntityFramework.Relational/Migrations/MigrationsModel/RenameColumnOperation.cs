// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.MigrationsModel
{
    public class RenameColumnOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly string _columnName;
        private readonly string _newColumnName;

        public RenameColumnOperation(SchemaQualifiedName tableName, [NotNull] string columnName, [NotNull] string newColumnName)
        {
            Check.NotEmpty(columnName, "columnName");
            Check.NotEmpty(newColumnName, "newColumnName");

            _tableName = tableName;
            _columnName = columnName;
            _newColumnName = newColumnName;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual string ColumnName
        {
            get { return _columnName; }
        }

        public virtual string NewColumnName
        {
            get { return _newColumnName; }
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
