// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class CreateSequenceOperation : MigrationOperation<Sequence, DropSequenceOperation>
    {
        public CreateSequenceOperation([NotNull] Sequence sequence)
            : base(Check.NotNull(sequence, "sequence"))
        {
        }

        public override void GenerateOperationSql(
            MigrationOperationSqlGenerator migrationOperationSqlGenerator,
            IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            migrationOperationSqlGenerator.Generate(this, stringBuilder, generateIdempotentSql);
        }

        public override DropSequenceOperation Inverse
        {
            get { return new DropSequenceOperation(Target); }
        }
    }
}
