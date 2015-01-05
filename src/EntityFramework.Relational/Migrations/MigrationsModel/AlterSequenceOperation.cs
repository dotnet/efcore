// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.MigrationsModel
{
    public class AlterSequenceOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _sequenceName;
        private readonly int _newIncrementBy;

        public AlterSequenceOperation(SchemaQualifiedName sequenceName, int newIncrementBy)
        {
            Check.NotNull(newIncrementBy, "newIncrementBy");

            _sequenceName = sequenceName;
            _newIncrementBy = newIncrementBy;
        }

        public virtual SchemaQualifiedName SequenceName
        {
            get { return _sequenceName; }
        }

        public virtual int NewIncrementBy
        {
            get { return _newIncrementBy; }
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
