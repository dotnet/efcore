// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Model
{
    public class AddColumnOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly Column _column;

        public AddColumnOperation(SchemaQualifiedName tableName, [NotNull] Column column)
        {
            Check.NotNull(column, "column");

            _tableName = tableName;
            _column = column;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual Column Column
        {
            get { return _column; }
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
