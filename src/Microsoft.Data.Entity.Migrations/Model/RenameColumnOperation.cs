// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Model
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

        public override void GenerateSql([NotNull] MigrationOperationSqlGenerator generator, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(stringBuilder, "stringBuilder");

            generator.Generate(this, stringBuilder, generateIdempotentSql);
        }

        public override void GenerateCode([NotNull] MigrationCodeGenerator generator, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(stringBuilder, "stringBuilder");

            generator.Generate(this, stringBuilder);
        }
    }
}
