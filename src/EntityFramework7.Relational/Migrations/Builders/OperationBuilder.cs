// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Builders
{
    public class OperationBuilder<TOperation> : IAccessor<TOperation>
        where TOperation : MigrationOperation
    {
        public OperationBuilder([NotNull] TOperation operation)
        {
            Check.NotNull(operation, nameof(operation));

            Operation = operation;
        }

        protected virtual TOperation Operation { get; }

        TOperation IAccessor<TOperation>.Service => Operation;

        public virtual OperationBuilder<TOperation> Annotation(
            [NotNull] string name,
            [NotNull] string value)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(value, nameof(value));

            Operation.AddAnnotation(name, value);

            return this;
        }
    }
}
