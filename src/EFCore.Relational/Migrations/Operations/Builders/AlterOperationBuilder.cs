// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

/// <summary>
///     A builder for <see cref="IAlterMigrationOperation" /> operations.
/// </summary>
/// <typeparam name="TOperation">The operation type to build.</typeparam>
public class AlterOperationBuilder<TOperation> : OperationBuilder<TOperation>
    where TOperation : MigrationOperation, IAlterMigrationOperation
{
    /// <summary>
    ///     Constructs a builder for the given <see cref="MigrationOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    public AlterOperationBuilder(TOperation operation)
        : base(operation)
    {
    }

    /// <summary>
    ///     Annotates the <see cref="MigrationOperation" /> with the given name/value pair.
    /// </summary>
    /// <param name="name">The annotation name.</param>
    /// <param name="value">The annotation value.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public new virtual AlterOperationBuilder<TOperation> Annotation(
        string name,
        object? value)
        => (AlterOperationBuilder<TOperation>)base.Annotation(name, value);

    /// <summary>
    ///     Annotates the <see cref="MigrationOperation" /> with the given name/value pair as
    ///     an annotation that was present before the alteration.
    /// </summary>
    /// <param name="name">The annotation name.</param>
    /// <param name="value">The annotation value.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual AlterOperationBuilder<TOperation> OldAnnotation(
        string name,
        object? value)
    {
        Check.NotEmpty(name, nameof(name));

        Operation.OldAnnotations.AddAnnotation(name, value);

        return this;
    }
}
