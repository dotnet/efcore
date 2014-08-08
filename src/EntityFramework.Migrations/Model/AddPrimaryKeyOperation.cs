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
    public class AddPrimaryKeyOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly string _primaryKeyName;
        private readonly IReadOnlyList<string> _columnNames;
        private readonly bool _isClustered = true;

        public AddPrimaryKeyOperation(
            SchemaQualifiedName tableName,
            [NotNull] string primaryKeyName,
            [NotNull] IReadOnlyList<string> columnNames,
            bool isClustered)
        {
            Check.NotEmpty(primaryKeyName, "primaryKeyName");
            Check.NotNull(columnNames, "columnNames");

            _tableName = tableName;
            _primaryKeyName = primaryKeyName;
            _columnNames = columnNames;
            _isClustered = isClustered;
        }

        public AddPrimaryKeyOperation([NotNull] PrimaryKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            _tableName = primaryKey.Table.Name;
            _primaryKeyName = primaryKey.Name;
            _columnNames = primaryKey.Columns.Select(c => c.Name).ToArray();
            _isClustered = primaryKey.IsClustered;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual string PrimaryKeyName
        {
            get { return _primaryKeyName; }
        }

        public virtual IReadOnlyList<string> ColumnNames
        {
            get { return _columnNames; }
        }

        public virtual bool IsClustered
        {
            get { return _isClustered; }
        }

        public override void Accept<TVisitor, TContext>(TVisitor visitor, TContext context)
        {
            Check.NotNull(visitor, "visitor");
            Check.NotNull(context, "context");

            visitor.Visit(this, context);
        }

        public override void GenerateSql(MigrationOperationSqlGenerator generator, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(stringBuilder, "stringBuilder");

            generator.Generate(this, stringBuilder);
        }

        public override void GenerateCode(MigrationCodeGenerator generator, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(stringBuilder, "stringBuilder");

            generator.Generate(this, stringBuilder);
        }
    }
}
