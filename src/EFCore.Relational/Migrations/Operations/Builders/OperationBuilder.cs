// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations.Builders
{
    /// <summary>
    ///     A builder for <see cref="MigrationOperation" />s.
    /// </summary>
    /// <typeparam name="TOperation"> The type of <see cref="MigrationOperation" /> to build for. </typeparam>
    public class OperationBuilder<TOperation> : IInfrastructure<TOperation>
        where TOperation : MigrationOperation
    {
        /// <summary>
        ///     Creates a new builder instance for the given <see cref="MigrationOperation" />.
        /// </summary>
        /// <param name="operation"> The <see cref="MigrationOperation" />. </param>
        public OperationBuilder([NotNull] TOperation operation)
        {
            Check.NotNull(operation, nameof(operation));

            Operation = operation;
        }

        /// <summary>
        ///     The <see cref="MigrationOperation" />.
        /// </summary>
        protected virtual TOperation Operation { get; }

        TOperation IInfrastructure<TOperation>.Instance => Operation;

        /// <summary>
        ///     Annotates the operation with the given name/value pair.
        /// </summary>
        /// <param name="name"> The annotation name. </param>
        /// <param name="value"> The annotation value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual OperationBuilder<TOperation> Annotation(
            [NotNull] string name,
            [NotNull] object value)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(value, nameof(value));

            Operation.AddAnnotation(name, value);

            return this;
        }
    }
}
