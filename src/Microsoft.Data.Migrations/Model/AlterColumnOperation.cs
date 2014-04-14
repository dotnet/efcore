// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class AlterColumnOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly Column _newColumn;
        private readonly bool _isDestructiveChange;

        public AlterColumnOperation(
            SchemaQualifiedName tableName,
            [NotNull] Column newColumn, 
            bool isDestructiveChange)
        {
            Check.NotNull(newColumn, "newColumn");

            _tableName = tableName;
            _newColumn = newColumn;
            _isDestructiveChange = isDestructiveChange;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual Column NewColumn
        {
            get { return _newColumn; }
        }

        public override bool IsDestructiveChange
        {
            get { return _isDestructiveChange; }
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
