// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Model
{
    public class AddUniqueConstraintOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly string _uniqueConstraintName;
        private readonly IReadOnlyList<string> _columnNames;

        public AddUniqueConstraintOperation(
            SchemaQualifiedName tableName,
            [NotNull] string uniqueConstraintName,
            [NotNull] IReadOnlyList<string> columnNames)
        {
            Check.NotEmpty(uniqueConstraintName, "uniqueConstraintName");
            Check.NotNull(columnNames, "columnNames");

            _tableName = tableName;
            _uniqueConstraintName = uniqueConstraintName;
            _columnNames = columnNames;
        }

        public AddUniqueConstraintOperation([NotNull] UniqueConstraint uniqueConstraint)
        {
            Check.NotNull(uniqueConstraint, "uniqueConstraint");

            _tableName = uniqueConstraint.Table.Name;
            _uniqueConstraintName = uniqueConstraint.Name;
            _columnNames = uniqueConstraint.Columns.Select(c => c.Name).ToArray();
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual string UniqueConstraintName
        {
            get { return _uniqueConstraintName; }
        }

        public virtual IReadOnlyList<string> ColumnNames
        {
            get { return _columnNames; }
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
