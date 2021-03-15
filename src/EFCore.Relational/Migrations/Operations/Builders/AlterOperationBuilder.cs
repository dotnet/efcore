// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations.Builders
{
    /// <summary>
    ///     A builder for <see cref="IAlterMigrationOperation" /> operations.
    /// </summary>
    /// <typeparam name="TOperation"> The operation type to build. </typeparam>
    public class AlterOperationBuilder<TOperation> : OperationBuilder<TOperation>
        where TOperation : MigrationOperation, IAlterMigrationOperation
    {
        /// <summary>
        ///     Constructs a builder for the given <see cref="MigrationOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        public AlterOperationBuilder([NotNull] TOperation operation)
            : base(operation)
        {
        }

        /// <summary>
        ///     Annotates the <see cref="MigrationOperation" /> with the given name/value pair.
        /// </summary>
        /// <param name="name"> The annotation name. </param>
        /// <param name="value"> The annotation value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public new virtual AlterOperationBuilder<TOperation> Annotation(
            [NotNull] string name,
            [NotNull] object value)
            => (AlterOperationBuilder<TOperation>)base.Annotation(name, value);

        /// <summary>
        ///     Annotates the <see cref="MigrationOperation" /> with the given name/value pair as
        ///     an annotation that was present before the alteration.
        /// </summary>
        /// <param name="name"> The annotation name. </param>
        /// <param name="value"> The annotation value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
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
