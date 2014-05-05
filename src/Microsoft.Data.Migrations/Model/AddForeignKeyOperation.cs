// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
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
