// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations.Builders
{
    public class AlterOperationBuilder<TOperation> : OperationBuilder<TOperation>
        where TOperation : MigrationOperation, IAlterMigrationOperation
    {
        public AlterOperationBuilder([NotNull] TOperation operation)
            : base(operation)
        {
        }

        public new virtual AlterOperationBuilder<TOperation> Annotation(
            [NotNull] string name,
            [NotNull] object value)
            => (AlterOperationBuilder<TOperation>)base.Annotation(name, value);

        public virtual AlterOperationBuilder<TOperation> OldAnnotation(
            [NotNull] string name,
            [NotNull] object value)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(value, nameof(value));

            Operation.OldAnnotations.AddAnnotation(name, value);

            return this;
        }
    }
}
