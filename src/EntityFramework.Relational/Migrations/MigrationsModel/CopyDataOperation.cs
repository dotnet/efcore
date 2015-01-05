// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.MigrationsModel
{
    public class CopyDataOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _sourceTableName;
        private readonly IReadOnlyList<string> _sourceColumnNames;
        private readonly SchemaQualifiedName _targetTableName;
        private readonly IReadOnlyList<string> _targetColumnNames;

        public CopyDataOperation(
            SchemaQualifiedName sourceTableName, [NotNull] IReadOnlyList<string> sourceColumnNames,
            SchemaQualifiedName targetTableName, [NotNull] IReadOnlyList<string> targetColumnNames)
        {
            Check.NotNull(sourceColumnNames, "sourceColumnNames");
            Check.NotNull(targetColumnNames, "targetColumnNames");

            _sourceTableName = sourceTableName;
            _sourceColumnNames = sourceColumnNames;
            _targetTableName = targetTableName;
            _targetColumnNames = targetColumnNames;
        }

        public virtual SchemaQualifiedName SourceTableName
        {
            get { return _sourceTableName; }
        }

        public virtual IReadOnlyList<string> SourceColumnNames
        {
            get { return _sourceColumnNames; }
        }

        public virtual SchemaQualifiedName TargetTableName
        {
            get { return _targetTableName; }
        }

        public virtual IReadOnlyList<string> TargetColumnNames
        {
            get { return _targetColumnNames; }
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
