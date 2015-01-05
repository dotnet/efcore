// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.MigrationsModel
{
    public class CreateTableOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly List<Column> _columns = new List<Column>();
        private AddPrimaryKeyOperation _primaryKey;
        private readonly List<AddUniqueConstraintOperation> _uniqueConstraints = new List<AddUniqueConstraintOperation>();
        private readonly List<AddForeignKeyOperation> _foreignKeys = new List<AddForeignKeyOperation>();
        private readonly List<CreateIndexOperation> _indexes = new List<CreateIndexOperation>();

        public CreateTableOperation(SchemaQualifiedName tableName)
        {
            _tableName = tableName;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual List<Column> Columns
        {
            get { return _columns; }
        }

        public virtual AddPrimaryKeyOperation PrimaryKey
        {
            get { return _primaryKey; }
            [param: NotNull] set { _primaryKey = value; }
        }

        public virtual List<AddUniqueConstraintOperation> UniqueConstraints
        {
            get { return _uniqueConstraints; }
        }

        public virtual List<AddForeignKeyOperation> ForeignKeys
        {
            get { return _foreignKeys; }
        }

        public virtual List<CreateIndexOperation> Indexes
        {
            get { return _indexes; }
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
