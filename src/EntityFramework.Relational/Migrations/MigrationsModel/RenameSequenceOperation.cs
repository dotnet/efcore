// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.MigrationsModel
{
    // TODO: Consider merging all remove operations into one.
    public class RenameSequenceOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _sequenceName;
        private readonly string _newSequenceName;

        public RenameSequenceOperation(SchemaQualifiedName sequenceName, [NotNull] string newSequenceName)
        {
            Check.NotEmpty(newSequenceName, "newSequenceName");

            _sequenceName = sequenceName;
            _newSequenceName = newSequenceName;
        }

        public virtual SchemaQualifiedName SequenceName
        {
            get { return _sequenceName; }
        }

        public virtual string NewSequenceName
        {
            get { return _newSequenceName; }
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
