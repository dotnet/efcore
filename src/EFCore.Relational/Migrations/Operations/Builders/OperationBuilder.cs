// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

/// <summary>
///     A builder for <see cref="MigrationOperation" />s.
/// </summary>
/// <typeparam name="TOperation">The type of <see cref="MigrationOperation" /> to build for.</typeparam>
public class OperationBuilder<TOperation> : IInfrastructure<TOperation>
    where TOperation : MigrationOperation
{
    /// <summary>
    ///     Creates a new builder instance for the given <see cref="MigrationOperation" />.
    /// </summary>
    /// <param name="operation">The <see cref="MigrationOperation" />.</param>
    public OperationBuilder(TOperation operation)
    {
        Check.NotNull(operation, nameof(operation));

        Operation = operation;
    }

    /// <summary>
    ///     The <see cref="MigrationOperation" />.
    /// </summary>
    protected virtual TOperation Operation { get; }

    TOperation IInfrastructure<TOperation>.Instance
        => Operation;

    /// <summary>
    ///     Annotates the operation with the given name/value pair.
    /// </summary>
    /// <param name="name">The annotation name.</param>
    /// <param name="value">The annotation value.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual OperationBuilder<TOperation> Annotation(
        string name,
        object? value)
    {
        Check.NotEmpty(name, nameof(name));

        Operation.AddAnnotation(name, value);

        return this;
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString()
        => base.ToString()!;

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
